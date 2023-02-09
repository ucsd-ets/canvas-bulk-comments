using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Wayfinder.Canvas {
public class Client : ClientDelegate {

protected HttpClientHandler handler;
protected HttpClient client;

public HttpClient GetClient()
	{
	if(client == null)
		{
		handler = new HttpClientHandler();
		handler.AllowAutoRedirect = false;
		client = new HttpClient(handler, true);

		client.DefaultRequestHeaders.Accept.Clear();
		SetClientAuthorization();
		}

	return client;
	}

public virtual void SetClientAuthorization()
	{
	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Constants.CanvasToken);
	}

public virtual string PageUrl(string baseUrl, int page = 0, int count = 100)
	{
	var url = new UriBuilder(baseUrl); 

	var nameValues = HttpUtility.ParseQueryString(url.Query);
	nameValues.Set("per_page", Convert.ToString(count));
	nameValues.Set("page", Convert.ToString(page+1)); // Canvas uses 1-indexed pages

	// Because NameValuePairCollection.ToString() doesn't break out repeated values as Canvas requires
	List<string> pairs = new List<string>();
	char[] commaSep = new char[]{','};

	foreach(var key in nameValues.AllKeys)
		{
		string[] values = nameValues[key].Split(commaSep);

		foreach(var val in values)
			{
			pairs.Add(String.Format("{0}={1}", key, HttpUtility.UrlEncode(val)));
			}
		}

	url.Query = String.Join("&", pairs.ToArray());
	return url.Uri.ToString();
	}

}

}
