using BooksSpr2026_sec02.Data;
using BooksSpr2026_sec02.Models;
using BooksSpr2026_sec02.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BooksSpr2026_sec02.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]

    public class CartItemController : Controller
    {
        private readonly BooksDbContext _dbContext;

        public CartItemController(BooksDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItemsList = _dbContext.CartItems.Where(c => c.UserId == userId).Include(c => c.Book);

            ShoppingCartVM shoppingCartVM = new ShoppingCartVM
            {
                CartItems = cartItemsList,
                OrderTotal = 0


            };



            return View(shoppingCartVM);
        }

        public IActionResult ReviewOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _dbContext.ApplicationUsers.Find(userId);

            var cartItemsList = _dbContext.CartItems.Where(c => c.UserId == userId).Include(c => c.Book);

            ShoppingCartVM shoppingCartVM = new ShoppingCartVM
            {
                CartItems = cartItemsList,
                OrderTotal = new Order()
                {
                    CustomerName = user.Name,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    Phone = user.Phone
                }
            };

            //calculate and add the order total to the Order part of the ShoppingCartVM

            foreach(var cartItem in shoppingCartVM.CartItems)
            {
                cartItem.SubTotal = cartItem.Book.Price * cartItem.Quantity;

                shoppingCartVM.Order.OrderToal += cartItem.SubTotal;
            }
            
            return View();

            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReviewOrder(ShoppingCartVM shoppingCartVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItemsList = _dbContext.CartItems.Where(c => c.UserId == userId).Include(c => c.Book);

            shoppingCartVM.CartItems = cartItemsList;

            if (!ModelState.IsValid)
            {

            }

            foreach (var cartItem in shoppingCartVM.CartItems)
            {
                cartItem.SubTotal = cartItem.Book.Price * cartItem.Quantity;

                shoppingCartVM.Order.OrderToal += cartItem.SubTotal;
            }

            shoppingCartVM.Order.ApplicationUserId = userId;
            shoppingCartVM.Order.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            shoppingCartVM.Order.OrderStatus = "Pending";
            shoppingCartVM.Order.PaymentStatus = "Pending";

            return RedirectToAction("Index");
        }
    }
}
