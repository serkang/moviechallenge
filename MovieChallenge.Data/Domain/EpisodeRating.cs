using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("EpisodeRatings", Schema = "dbo")]
    public class EpisodeRating : BaseRating
    {
        public Episode Episode { get; set; }
    }
}
