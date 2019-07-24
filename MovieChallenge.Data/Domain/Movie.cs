using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("Movies", Schema = "dbo")]
    public class Movie : BaseEntity
    {
        public string Dvd { get; set; }
        public string BoxOffice { get; set; }
        public string Production { get; set; }
        public string Website { get; set; }
        public List<MovieRating> Ratings { get; set; }
    }
}
