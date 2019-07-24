using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("Series", Schema = "dbo")]
    public class Serie : BaseEntity
    {
        public string totalSeasons { get; set; }
        public List<SerieRating> Ratings { get; set; }
    }
}
