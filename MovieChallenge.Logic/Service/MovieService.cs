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
                    result.Episode = result.EpisodeNo;
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
                    result = JsonConvert.DeserializeObject<dynamic>(detailResult);
                    if (!detailResult.Contains("Incorrect IMDb ID")) await AddResultToDbAsync(result);
                }
                else
                {
                    await AddToCacheAsync(id, result);
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
            string entityType = result.Type;
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
                    Year = result.Year,
                    Ratings = GetMovieRatings(result.Ratings)
                });
            }
            else if (entityType == "series")
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
                    totalSeasons = result.totalSeasons,
                    Ratings = GetSerieRatings(result.Ratings)
                });
            }
            else if (entityType == "episode")
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
                    EpisodeNo = result.Episode,
                    Ratings = GetEpisodeRatings(result.Ratings)
                });
            }

            await _context.SaveChangesAsync();
            string id = result.imdbID;
            await AddToCacheAsync(id, result);
        }

        private List<EpisodeRating> GetEpisodeRatings(dynamic ratings)
        {
            var rating = new List<EpisodeRating>();
            foreach (dynamic item in ratings)
            {
                rating.Add(new EpisodeRating { Source = item.Source, Value = item.Value });
            }
            return rating;
        }

        private List<SerieRating> GetSerieRatings(dynamic ratings)
        {
            var rating = new List<SerieRating>();
            foreach (dynamic item in ratings)
            {
                rating.Add(new SerieRating { Source = item.Source, Value = item.Value });
            }
            return rating;
        }

        private List<MovieRating> GetMovieRatings(dynamic ratings)
        {
            var rating = new List<MovieRating>();
            foreach (dynamic item in ratings)
            {
                rating.Add(new MovieRating { Source = item.Source, Value = item.Value });
            }
            return rating;
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

        public void UpdateMovies()
        {
            var movies = _context.Movies.Include(x => x.Ratings).ToList();
            foreach (var m in movies)
            {
                dynamic result = DetailFromOmdbAsync(m.imdbID).Result;

                m.Actors = result.Actors;
                m.Awards = result.Awards;
                m.BoxOffice = result.BoxOffice;
                m.Country = result.Country;
                m.Director = result.Director;
                m.Dvd = result.DVD;
                m.Genre = result.Genre;
                m.imdbRating = result.imdbRating;
                m.imdbVotes = result.imdbVotes;
                m.Language = result.Language;
                m.Metascore = result.Metascore;
                m.Plot = result.Plot;
                m.Poster = result.Poster;
                m.Production = result.Production;
                m.Rated = result.Rated;
                m.Released = result.Released;
                m.Runtime = result.Runtime;
                m.Title = result.Title;
                m.Website = result.Website;
                m.Writer = result.Writer;
                m.Year = result.Year;
                m.Ratings = GetMovieRatings(result.Ratings);

                _context.Movies.Update(m);
            }

            var series = _context.Series.Include(x => x.Ratings).ToList();
            foreach (var s in series)
            {
                dynamic result = DetailFromOmdbAsync(s.imdbID).Result;

                s.Actors = result.Actors;
                s.Awards = result.Awards;
                s.Country = result.Country;
                s.Director = result.Director;
                s.Genre = result.Genre;
                s.imdbID = result.imdbID;
                s.imdbRating = result.imdbRating;
                s.imdbVotes = result.imdbVotes;
                s.Language = result.Language;
                s.Metascore = result.Metascore;
                s.Plot = result.Plot;
                s.Poster = result.Poster;
                s.Rated = result.Rated;
                s.Released = result.Released;
                s.Runtime = result.Runtime;
                s.Title = result.Title;
                s.Writer = result.Writer;
                s.Year = result.Year;
                s.totalSeasons = result.totalSeasons;
                s.Ratings = GetSerieRatings(result.Ratings);

                _context.Series.Update(s);
            }

            var episodes = _context.Episodes.Include(x => x.Ratings).ToList();
            foreach (var e in episodes)
            {
                dynamic result = DetailFromOmdbAsync(e.imdbID).Result;

                e.Actors = result.Actors;
                e.Awards = result.Awards;
                e.Country = result.Country;
                e.Director = result.Director;
                e.Genre = result.Genre;
                e.imdbID = result.imdbID;
                e.imdbRating = result.imdbRating;
                e.imdbVotes = result.imdbVotes;
                e.Language = result.Language;
                e.Metascore = result.Metascore;
                e.Plot = result.Plot;
                e.Poster = result.Poster;
                e.Rated = result.Rated;
                e.Released = result.Released;
                e.Runtime = result.Runtime;
                e.Title = result.Title;
                e.Writer = result.Writer;
                e.Year = result.Year;
                e.Season = result.Season;
                e.seriesID = result.seriesID;
                e.EpisodeNo = result.Episode;
                e.Ratings = GetEpisodeRatings(result.Ratings);

                _context.Episodes.Update(e);
            }

            _context.SaveChanges();
        }

        public async Task<dynamic> DetailFromOmdbAsync(string id)
        {
            var client = new HttpClient();
            var detailResult = await client.GetStringAsync($"http://www.omdbapi.com/?i={id}&apikey={HttpHelper.ApiKey}");
            client.Dispose();
            return JsonConvert.DeserializeObject<dynamic>(detailResult);
        }
    }
}
