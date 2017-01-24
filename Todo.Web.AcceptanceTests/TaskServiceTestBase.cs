using System;
using RestSharp;

namespace Todo.Web.AcceptanceTests
{
    public class TaskServiceTestBase
    {
        readonly string _baseurl = 
            Environment.GetEnvironmentVariable("todo-web-url") ?? 
            "http://qa-todo-web.azurewebsites.net/Tasks/";

        protected readonly IRestClient Client;
        protected readonly string User = Guid.NewGuid().ToString("N") + "@test.com";
        protected const string OtherUser = "othertest@test.com";

        protected TaskServiceTestBase()
        {
            Client = new RestClient(_baseurl);
        }
    }
}
