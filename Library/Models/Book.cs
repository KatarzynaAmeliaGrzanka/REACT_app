using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Display(Name = "Author")]
        public string Author { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Publisher")]
        public string Publisher { get; set; }

        [Display(Name = "Date")]
        public int date { get; set; }

        public string? User { get; set; }

        public DateTime? Reserved { get; set; }

        public DateTime? Leased { get; set; }

    }

}
