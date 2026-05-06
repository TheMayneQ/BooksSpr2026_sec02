using BooksSpr2026_sec02.Data;
using Microsoft.AspNetCore.Mvc;

namespace BooksSpr2026_sec02.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly BooksDbContext _dbContext;

        public OrderController(BooksDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            IEnumerable<Order> listOfOrders = _dbContext.Orders.Include(o => o.ApplicationUser);

            return View(listOfOrders);
        }
    }
}
