using Nancy;
using RestSharp;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DotNet.Perf
{
    public class HttpClientModule : NancyModule
    {
        public HttpClientModule(Program.Options options)
        {
            Get["Test HttpClient Multiple", "/clients/httpclient/multiple"] = HttpClientMultiple;
            Get["Test HttpClient Single", "/clients/httpclient/single"] = HttpClientSingle;
            Get["Test HttpClient Leaks", "/clients/httpclient/leaks"] = HttpClientLeaks;

            Get["Test RestSharp Multiple", "/clients/restsharp/multiple"] = RestSharpMultiple;

            Get["Test Flurl Multiple", "/clients/flurl/multiple"] = FlurlMultiple;

            this.options = options;
        }

        #region HttpClient

        private Response HttpClientMultiple(dynamic parameters)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.BaseAddress = new Uri(BaseUrl);
                var result = httpClient.GetAsync("servers/default").Result;

                return Response.AsJson(result.Content.ReadAsStringAsync());
            }
        }

        private Response HttpClientSingle(dynamic parameters)
        {
            var httpClient = GetSingleHttpClient();
            var result = httpClient.GetAsync("servers/default").Result;

            return Response.AsJson(result.Content.ReadAsStringAsync());
        }

        private Response HttpClientLeaks(dynamic parameters)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.BaseAddress = new Uri(BaseUrl);
            var result = httpClient.GetAsync("servers/default").Result;

            return Response.AsJson(result.Content.ReadAsStringAsync());
        }

        private HttpClient GetSingleHttpClient()
        {
            if (SingleHttpClient == null)
            {
                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.BaseAddress = new Uri(BaseUrl);

                SingleHttpClient = httpClient;
            }

            return SingleHttpClient;
        }

        #endregion

        #region RestSharp

        private Response RestSharpMultiple(dynamic parameters)
        {
            var httpClient = new RestClient();
            httpClient.BaseUrl = new Uri(BaseUrl);

            var request = new RestRequest("servers/default");
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");

            var response = httpClient.Execute(request);

            return Response.AsJson(response.Content);
        }

        #endregion

        #region Flurl

        private Response FlurlMultiple(dynamic parameters)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.BaseAddress = new Uri(BaseUrl);
                var result = httpClient.GetAsync("servers/default").Result;

                return Response.AsJson(result.Content.ReadAsStringAsync());
            }
        }

        #endregion

        private static HttpClient SingleHttpClient;
        private static readonly string BaseUrl = "http://localhost:9002/api/v1/";
        private readonly Program.Options options;
    }
}
