using System.Diagnostics;
using System.Security.Claims;
using BooksSpr2026_sec02.Data;
using BooksSpr2026_sec02.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksSpr2026_sec02.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private BooksDbContext _dbcontext;
        private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public HomeController(BooksDbContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public IActionResult Index()
        {
            var listOfBooks = _dbcontext.Books.Include(c => c.category);
            return View(listOfBooks.ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(int id)
        {
            var book = _dbcontext.Books.Include(c => c.category).FirstOrDefault(b => b.BookId == id);

            var cartItem = new CartItem()
            {
                BookId = id,
                Book = book,
                Quantity = 1
            };

            return View(cartItem);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddToCart(CartItem cartItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            cartItem.UserId = userId;

            CartItem existingCart = _dbcontext.CartItems.FirstOrDefault(c => c.UserId == userId && c.BookId == cartItem.BookId);

            if (existingCart != null) //cartItem exists already
            {
                existingCart.Quantity += cartItem.Quantity;
                _dbcontext.CartItems.Update(existingCart);
            }

            else //cart doesnt exist, add as new cartItem
            {
                _dbcontext.CartItems.Add(cartItem);
            }

            _dbcontext.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
