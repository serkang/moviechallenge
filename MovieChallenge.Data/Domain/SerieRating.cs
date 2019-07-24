using System.ComponentModel.DataAnnotations.Schema;

namespace MovieChallenge.Data.Domain
{
    [Table("SerieRatings", Schema = "dbo")]
    public class SerieRating : BaseRating
    {
        public Serie Serie { get; set; }
    }
}
