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
        //private readonly DistributedCacheEntryOptions _cacheOptions;
        
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {            
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
        public async Task<JsonResult> Detail(string id)
        {
            return Json(await _movieService.DetailAsync(id));
        }

        [Route("api/clearcache")]
        [HttpGet]
        public async Task ClearCache()
        {
            await _movieService.ClearCache();
        }
    }
}