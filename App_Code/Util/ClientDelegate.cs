using System.Net.Http;

namespace Wayfinder {
public interface ClientDelegate {

HttpClient GetClient();

string PageUrl(string baseUrl, int page, int count);
}
}
