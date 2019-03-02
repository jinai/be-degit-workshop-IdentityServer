using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MvcClient.Services
{
    public class MvcHttpService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static HttpClient _httpClient;

        public MvcHttpService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var baseAddress = new Uri("http://localhost:5001");

            _httpClient = new HttpClient
            {
                BaseAddress = baseAddress
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(requestMessage);

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, string content)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage);

            return response;
        }
    }
}
