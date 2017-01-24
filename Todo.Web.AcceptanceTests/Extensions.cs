using RestSharp;

namespace Todo.Web.AcceptanceTests
{
    public static class Extensions
    {
        public static T Post<T>(this IRestClient client, string url, object request) where T : new()
        {
            var req = new RestRequest(url + "/") { RequestFormat = DataFormat.Json };
            req.AddBody(request);
            return client.Post<T>(req).Data;
        }

        public static T Put<T>(this IRestClient client, string url, object request) where T : new()
        {
            var req = new RestRequest(url + "/") { RequestFormat = DataFormat.Json };
            req.AddBody(request);
            return client.Put<T>(req).Data;
        }

        public static T Get<T>(this IRestClient client, string url) where T : new() =>
            client.Get<T>(new RestRequest(url + "/")).Data;

        public static void Delete(this IRestClient client, string url) =>
            client.Delete(new RestRequest(url + "/"));
    }
}
