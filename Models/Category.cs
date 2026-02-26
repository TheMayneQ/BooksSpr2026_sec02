using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BooksSpr2026_sec02.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        //[DisplayName("Category Name: "), Required(ErrorMessage = "Category Name MUST be provided")]
        public string Name { get; set; }
        //[DisplayName("Category Description: "), Required(ErrorMessage = "Category Description MUST be provided"), MaxLength(20, ErrorMessage = "TOO LONG")]
        public string? Description { get; set; }

    }

}
