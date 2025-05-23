using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZhiganshinaMilana420_MarryMe.DB;
using static System.Net.WebRequestMethods;

namespace ZhiganshinaMilana420_MarryMe.Classes
{
    public class RestaurantsApiService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:44357/api/restaurants";



        public async Task<List<Restaurant>> GetWorkoutsAsync()
        {
            var response = await _client.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Restaurant>>(content);
        }


        public RestaurantsApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
