using System.ComponentModel.DataAnnotations;

namespace MovieChallenge.Data.Domain
{
    public class BaseRating
    {
        [Key]
        public int Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
        public string imdbId { get; set; }
    }
}
