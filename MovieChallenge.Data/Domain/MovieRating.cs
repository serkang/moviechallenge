using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("MovieRatings", Schema = "dbo")]
    public class MovieRating : BaseRating
    {
        public Movie Movie { get; set; }
    }
}
