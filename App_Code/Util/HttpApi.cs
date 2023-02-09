using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net; 
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wayfinder {

public class HttpApi<T> {
private ClientDelegate clientDelegate;

public HttpApi(ClientDelegate cd)
	{
	clientDelegate = cd;
	}

public async Task<List<U>> GetAll<U>(string baseUrl, int pageSize = 100)
	{
	var results = new List<U>();

	string pageUrl = clientDelegate.PageUrl(baseUrl, 0, pageSize);
	while(true)
		{
		ApiResult<List<U>> result = (await GetAsyncResult(pageUrl)) as ApiResult<List<U>>;
		List<U> page_courses = result.Result;

		if(page_courses == null)
			break;

		results.AddRange(page_courses);

		// Prepare next page
		var pageLinks = result.PageLinks;
		if(!pageLinks.ContainsKey("next"))
			break;

		pageUrl = pageLinks["next"];
		}

	return results;
	}

public async Task<T> GetAsync(string baseUrl, HttpMethod method = null, HttpContent request_content = null)
	{
	ApiResult<T> result = (await GetAsyncResult(baseUrl, method, request_content));
	return result.Result;
	}

public async Task<ApiResult<T>> GetAsyncResult(string baseUrl, HttpMethod method = null, HttpContent request_content = null)
	{
	ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

	if(method == null) method = HttpMethod.Get;

	Console.WriteLine(baseUrl);
	
	HttpResponseMessage response = null;
	try {
		//we do not want to allow auto-redirect, this will cause issues if we are uploading files using the Canvas API
		HttpClientHandler handler = new HttpClientHandler();
		handler.AllowAutoRedirect = false;
		if(Wayfinder.Constants.Proxy != null)
			{
			handler.Proxy = Wayfinder.Constants.Proxy;
			}

		if(method == HttpMethod.Put)
			response = await clientDelegate.GetClient().PutAsync(baseUrl, request_content);
		else if(method == HttpMethod.Post)
			response = await clientDelegate.GetClient().PostAsync(baseUrl, request_content);
		else
			response = await clientDelegate.GetClient().GetAsync(baseUrl);

		}
	catch (Exception err)
		{
		Console.WriteLine("[EXCEPTION] Failed to execute HTTP GET command: [" + baseUrl + "]");
		throw err;
		}

	T result = default(T);

	if (response != null && response.IsSuccessStatusCode)
		{
		Console.WriteLine("Success!");
		var content = await response.Content.ReadAsStringAsync();
		Console.WriteLine(content);
		result = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
			{
			DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
			DateTimeZoneHandling = DateTimeZoneHandling.Local
			});
		}
	else
		{
		Console.WriteLine("Failure! {0}\nHeaders:", response.StatusCode);
		foreach(var header in response.Headers)
			{
			Console.WriteLine(" * '{0}'='{1}'", header.Key, String.Join("\n", header.Value));
			}
		var content = await response.Content.ReadAsStringAsync();
		Console.WriteLine(content);
		}

	return new ApiResult<T> { Result = result, Response = response};
	}

}

public class ApiResult<T> {
public T Result {get; set;}
public HttpResponseMessage Response {get; set;}

public Dictionary<string, string> PageLinks
	{
	get {
		var header = Response.Headers.GetValues("Link").FirstOrDefault();
		if(header != null) return HttpResponseHelper.PageLinks(header);
		return null;
		}
	}
}

public class HttpResponseHelper {
public static Dictionary<string, string> PageLinks(string header)
	{
	var links = new Dictionary<string, string>();
	var linkPat = new Regex(@"<([^>]+)>;\s*rel=""([^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	foreach(var link in header.Split(new Char[] {','}))
		{
		var matches = linkPat.Matches(link);
		foreach(Match match in matches)
			{
			links[match.Groups[2].Value] = match.Groups[1].Value;
			}
		}
	return links;
	}
}
}


