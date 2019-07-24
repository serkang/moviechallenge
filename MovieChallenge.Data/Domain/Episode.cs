using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("Episodes", Schema = "dbo")]
    public class Episode : BaseEntity
    {
        public string Season { get; set; }
        public string EpisodeNo { get; set; }
        public string seriesID { get; set; }
        public List<EpisodeRating> Ratings { get; set; }
    }
}
