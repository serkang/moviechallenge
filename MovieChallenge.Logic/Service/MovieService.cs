using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MovieChallenge.Common;
using MovieChallenge.Data;
using MovieChallenge.Data.Domain;
using MovieChallenge.Logic.Interface;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieChallenge.Logic.Service
{
    public class MovieService : IMovieService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public MovieService(AppDbContext context, IDistributedCache cache, IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(12)
            };
        }
        public async Task<string> SearchAsync(string keyword)
        {
            var client = new HttpClient();
            var searchResult = await client.GetStringAsync($"http://www.omdbapi.com/?s={keyword}&apikey={HttpHelper.ApiKey}");
            client.Dispose();
            return searchResult;
        }

        public async Task<object> DetailAsync(string id)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            dynamic result = null;

            var itemString = await _cache.GetStringAsync(id);

            if (string.IsNullOrEmpty(itemString))
            {

                var movieResult = await _context.Movies.Include(x => x.Ratings).Where(x => x.imdbID == id).FirstOrDefaultAsync();

                if (movieResult != null)
                {
                    result = movieResult;
                    result.type = "movie";
                    sw.Stop();
                    Debug.WriteLine($"From db with cost of {sw.ElapsedMilliseconds} ms");
                }

                var serieResult = await _context.Series.Include(x => x.Ratings).Where(x => x.imdbID == id).FirstOrDefaultAsync();
                if (serieResult != null)
                {
                    result = serieResult;
                    result.type = "series";
                    sw.Stop();
                    Debug.WriteLine($"From db with cost of {sw.ElapsedMilliseconds} ms");
                }

                var episodeResult = await _context.Series.Include(x => x.Ratings).Where(x => x.imdbID == id).FirstOrDefaultAsync();
                if (episodeResult != null)
                {
                    result = episodeResult;
                    result.type = "episode";
                    sw.Stop();
                    Debug.WriteLine($"From db with cost of {sw.ElapsedMilliseconds} ms");
                }

                if (result == null)
                {
                    var client = new HttpClient();
                    var detailResult = await client.GetStringAsync($"http://www.omdbapi.com/?i={id}&apikey={HttpHelper.ApiKey}");
                    client.Dispose();
                    sw.Stop();
                    Debug.WriteLine($"From omdb with cost of {sw.ElapsedMilliseconds} ms");
                    if (!detailResult.Contains("Incorrect IMDb ID")) await AddResultToDbAsync(result);
                    result = JsonConvert.DeserializeObject<dynamic>(detailResult);
                }
            }
            else
            {
                sw.Stop();
                Debug.WriteLine($"From cache with cost of {sw.ElapsedMilliseconds} ms");
                result = JsonConvert.DeserializeObject<dynamic>(itemString);
            }

            return result;
        }

        private async Task AddResultToDbAsync(dynamic result)
        {
            string entityType = result.type;
            if (entityType == "movie")
            {
                await _context.Movies.AddAsync(new Movie
                {
                    Actors = result.Actors,
                    Awards = result.Awards,
                    BoxOffice = result.BoxOffice,
                    Country = result.Country,
                    Director = result.Director,
                    Dvd = result.DVD,
                    Genre = result.Genre,
                    imdbID = result.imdbID,
                    imdbRating = result.imdbRating,
                    imdbVotes = result.imdbVotes,
                    Language = result.Language,
                    Metascore = result.Metascore,
                    Plot = result.Plot,
                    Poster = result.Poster,
                    Production = result.Production,
                    Rated = result.Rated,
                    Released = result.Released,
                    Runtime = result.Runtime,
                    Title = result.Title,
                    Website = result.Website,
                    Writer = result.Writer,
                    Year = result.Year
                });
            }
            else if(entityType == "series")
            {
                await _context.Series.AddAsync(new Serie
                {
                    Actors = result.Actors,
                    Awards = result.Awards,
                    Country = result.Country,
                    Director = result.Director,
                    Genre = result.Genre,
                    imdbID = result.imdbID,
                    imdbRating = result.imdbRating,
                    imdbVotes = result.imdbVotes,
                    Language = result.Language,
                    Metascore = result.Metascore,
                    Plot = result.Plot,
                    Poster = result.Poster,
                    Rated = result.Rated,
                    Released = result.Released,
                    Runtime = result.Runtime,
                    Title = result.Title,
                    Writer = result.Writer,
                    Year = result.Year,
                    totalSeasons = result.totalSeasons
                });
            } else if(entityType == "episode")
            {
                await _context.Episodes.AddAsync(new Episode
                {
                    Actors = result.Actors,
                    Awards = result.Awards,
                    Country = result.Country,
                    Director = result.Director,
                    Genre = result.Genre,
                    imdbID = result.imdbID,
                    imdbRating = result.imdbRating,
                    imdbVotes = result.imdbVotes,
                    Language = result.Language,
                    Metascore = result.Metascore,
                    Plot = result.Plot,
                    Poster = result.Poster,
                    Rated = result.Rated,
                    Released = result.Released,
                    Runtime = result.Runtime,
                    Title = result.Title,
                    Writer = result.Writer,
                    Year = result.Year,
                    Season = result.Season,
                    seriesID = result.seriesID,
                    EpisodeNo = result.Episode
                });
            }
        }

            private async Task AddToCacheAsync(string id, object o)
            {
                await _cache.SetStringAsync(id, JsonConvert.SerializeObject(o), _cacheOptions);
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
