using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MovieChallenge.Logic.Interface;
using Newtonsoft.Json;

namespace MovieChallenge.Api.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class MovieController : Controller
    {
        private readonly DistributedCacheEntryOptions _cacheOptions;
        
        private readonly IMovieService _movieService;
        private readonly IDistributedCache _cache;

        public MovieController(IDistributedCache cache, IMovieService movieService)
        {
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(12)
            };
            _movieService = movieService;
        }

        [Route("api/search")]
        [HttpGet]
        public async Task<string> Search(string keyword)
        {
            return await _movieService.SearchAsync(keyword);
        }

        [Route("api/detail")]
        [HttpGet]
        public async Task<string> Detail(string id)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var cacheResult = await _cache.GetStringAsync(id);

            if (string.IsNullOrEmpty(cacheResult))
            {
                var movieDetail = await _movieService.DetailAsync(id);
                if(!movieDetail.Contains("Error:")) await _cache.SetStringAsync(id, movieDetail, _cacheOptions);
                sw.Stop();
                Debug.WriteLine($"From omdb with cost of {sw.ElapsedMilliseconds} ms");
                return movieDetail;
            }
            
            sw.Stop();
            Debug.WriteLine($"From cache with cost of {sw.ElapsedMilliseconds} ms");
            return cacheResult;
        }

        [Route("api/clearcache")]
        [HttpGet]
        public async Task ClearCache()
        {
            await _movieService.ClearCache();
        }
    }
}