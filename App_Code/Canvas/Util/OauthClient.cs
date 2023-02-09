using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Wayfinder.Canvas {

public class OauthClient: Wayfinder.Canvas.Client {
public static int RefreshPadding = 60;

protected OauthToken _Token;
public OauthToken Token {
	get { 
		if(_Token == null)
			_Token = Repo.GetUsernameToken(Username);

		if(_Token != null && _Token.expires < DateTime.Now && _Token.refresh_token != null)
			{
			try {
				RefreshToken(_Token);
				}
			catch(OauthClientException except)
				{
				_Token.token = null;
				_Token.refresh_token = null;
				_Token.expires = null;
				}
			finally
				{
				// if it was refreshed or nulled, persist it
				Repo.UpdateUsernameToken(_Token); 
				}
			}
		return _Token;
		}
	set {
		_Token = value;
		Repo.UpdateUsernameToken(_Token); 
		}
	}

public OauthTokenRepo Repo;
public string Username;

public bool HasToken 
	{
	get { return Token != null; }
	}

public bool HasCurrentToken 
	{
	get { return Token != null && Token.expires != null && Token.expires > DateTime.Now; }
	}

public bool NeedsAuthorization
	{
	get { return (Token == null || Token.refresh_token == null || Token.token == null || Token.expires == null); }
	}

public OauthClient(string username, OauthTokenRepo repo)
	{
	Username = username;
	Repo = repo;
	}

public override string PageUrl(string baseUrl, int page = 0, int count = 100)
	{
	var token = Token;
	return base.PageUrl(baseUrl, page, count);
	}

public string AuthorizationUrl(string returnToUrl)
	{
	return String.Format("{0}/login/oauth2/auth?client_id={1}&response_type=code&state=TEST1234&redirect_uri={2}",
		ConfigurationManager.AppSettings["Canvas.Base"],
		ConfigurationManager.AppSettings["Canvas.Oauth.ClientId"],
		returnToUrl
		);
	}

public OauthToken AuthorizeToken(string state, string code, string redirect_url)
	{
	if(String.IsNullOrEmpty(code)) 
		throw new ArgumentException("Authorization requires a valid post-back code");

	var api = new HttpApi<TokenResponse>(new Wayfinder.Canvas.Client());
	string baseUrl = String.Format("{0}/login/oauth2/token", ConfigurationManager.AppSettings["Canvas.Base"]);

	var content = new List<KeyValuePair<string,string>>
			{
			new KeyValuePair<string, string>("grant_type","authorization_code"),
			new KeyValuePair<string, string>("client_id",ConfigurationManager.AppSettings["Canvas.Oauth.ClientId"]),
			new KeyValuePair<string, string>("client_secret",ConfigurationManager.AppSettings["Canvas.Oauth.Key"]),
			new KeyValuePair<string, string>("redirect_url",redirect_url),
			};

	content.Add(new KeyValuePair<string, string>("code",code));

	TokenResponse response = api.GetAsync(baseUrl, HttpMethod.Post, new FormUrlEncodedContent(content)).Result;

	if(response == null)
		{
		var exception = new OauthClientException("Could not authorize user token!");
		exception.Content = content;
		throw exception;
		}

	var token = Token;
	token.token = response.access_token;
	token.expires = DateTime.Now.AddSeconds(Convert.ToInt32(response.expires_in) - OauthClient.RefreshPadding);
	token.canvas_user_id = response.user.id;
	token.canvas_name = response.user.name;

	if(!String.IsNullOrEmpty(response.refresh_token))
		token.refresh_token = response.refresh_token;

	Repo.UpdateUsernameToken(token);

	return token;
	}

public OauthToken RefreshToken(OauthToken token)
	{
	if(token.refresh_token == null) throw new InvalidOperationException("The user token must have a refresh_token in order to refresh it.");

	Console.WriteLine("Refreshing token for username {0}", token.username);
	var api = new HttpApi<TokenResponse>(new Wayfinder.Canvas.Client());
	string baseUrl = String.Format("{0}/login/oauth2/token", ConfigurationManager.AppSettings["Canvas.Base"]);

	var content = new List<KeyValuePair<string,string>>
			{
			new KeyValuePair<string, string>("grant_type","refresh_token"),
			new KeyValuePair<string, string>("client_id",ConfigurationManager.AppSettings["Canvas.Oauth.ClientId"]),
			new KeyValuePair<string, string>("client_secret",ConfigurationManager.AppSettings["Canvas.Oauth.Key"]),
			new KeyValuePair<string, string>("refresh_token",token.refresh_token),
			};

	TokenResponse response = api.GetAsync(baseUrl, HttpMethod.Post, new FormUrlEncodedContent(content)).Result;

	if(response == null)
		{
		var exception = new OauthClientException("Could not refresh user token!");
		exception.Content = content;
		throw exception;
		}
		
	token.token = response.access_token;
	token.expires = DateTime.Now.AddSeconds(Convert.ToInt32(response.expires_in) - OauthClient.RefreshPadding);
	Console.WriteLine(" * Refreshed to token = {0},expires = {1}", token.token, token.expires);

	Repo.UpdateUsernameToken(token);
	return token;
	}

public override void SetClientAuthorization()
	{
	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.token);
	}
}

public class OauthClientException : Exception {
public List<KeyValuePair<string,string>> Content {get; set;}

public OauthClientException()
    {
    }

public OauthClientException(string message) : base(message)
    {
    }

public OauthClientException(string message, Exception inner) : base(message, inner)
    {
    }
}

}
