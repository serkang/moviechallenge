using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MovieChallenge.Common;
using MovieChallenge.Logic.Interface;
using StackExchange.Redis;

namespace MovieChallenge.Logic.Service
{
    public class MovieService : IMovieService
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        public MovieService(IDistributedCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
        }
        public async Task<string> SearchAsync(string keyword)
        {
            var client = new HttpClient();
            var searchResult = await client.GetStringAsync($"http://www.omdbapi.com/?s={keyword}&apikey={HttpHelper.ApiKey}");
            client.Dispose();
            return searchResult;
        }

        public async Task<string> DetailAsync(string id)
        {
            var client = new HttpClient();
            var detailResult = await client.GetStringAsync($"http://www.omdbapi.com/?i={id}&apikey={HttpHelper.ApiKey}");
            client.Dispose();
            return detailResult;
        }

        public async Task ClearCache()
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(_configuration["Redis"] + ",allowAdmin=true");
            var endPoints = redis.GetEndPoints();
            var server = redis.GetServer(endPoints[0]);
            await server.FlushAllDatabasesAsync();
        }
    }
}
