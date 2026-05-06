using BooksSpr2026_sec02.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksSpr2026_sec02.Data
{
    public class BooksDbContext : IdentityDbContext
    {

        public BooksDbContext(DbContextOptions<BooksDbContext> options) : base (options)
        {


        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(

                new Category { CategoryId = 1, Name = "Travel", Description = "goin outside" },
                new Category { CategoryId = 2, Name = "Fiction", Description = "fake but dope" }



                );

            modelBuilder.Entity<Book>().HasData(

                new Book
                {
                    BookId = 4,
                    BookTitle = "1984",
                    Author = "George Orwell",
                    Description = "schizo doesn't trust the government",
                    Price = 12.99m,
                    CategoryID = 2,
                    ImgUrl = ""
                },

                new Book
                {
                    BookId = 2,
                    BookTitle = "1974",
                    Author = "George Orwell",
                    Description = "simple-minded guy trusts the government",
                    Price = 6.99m,
                    CategoryID = 2,
                    ImgUrl = ""

                }


                );
        }

    }
}
