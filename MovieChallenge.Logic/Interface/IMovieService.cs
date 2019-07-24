using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovieChallenge.Logic.Interface
{
    public interface IMovieService
    {
        Task<string> SearchAsync(string keyword);
        Task<object> DetailAsync(string id);
        Task<dynamic> DetailFromOmdbAsync(string id);
        Task ClearCache();
        void UpdateMovies();
    }
}
