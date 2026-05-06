using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksSpr2026_sec02.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; } //naviga prop

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; } //MISSING APPLICATIONUSER MODEL WATCH CLASS CONTENT

        public int Quantity { get; set; }

    }
}
