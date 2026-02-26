using System.Diagnostics;
using BooksSpr2026_sec02.Data;
using BooksSpr2026_sec02.Models;
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
    }
}
