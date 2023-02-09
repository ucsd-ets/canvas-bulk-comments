using System.Configuration;
using System.Net;

namespace Wayfinder {
public class Constants {

public static string ProxyUrl = ConfigurationManager.AppSettings["proxy"];

public static WebProxy _Proxy;
public static WebProxy Proxy
	{
	get{ 
		if(_Proxy == null && ProxyUrl != null)
			_Proxy = new WebProxy(ProxyUrl);
		return _Proxy;
		}
	}

public static string CanvasApiBase = ConfigurationManager.AppSettings["Canvas.APIBase"];
public static string CanvasToken = ConfigurationManager.AppSettings["Canvas.Token"];
public static string DBDSN = ConfigurationManager.AppSettings["Database.DSN"];
}
}
