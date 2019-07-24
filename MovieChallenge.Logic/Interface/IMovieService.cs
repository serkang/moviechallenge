using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovieChallenge.Logic.Interface
{
    public interface IMovieService
    {
        Task<string> SearchAsync(string keyword);
        Task<string> DetailAsync(string id);
        Task ClearCache();
    }
}
