using BooksSpr2026_sec02.Data;
using BooksSpr2026_sec02.Models;
using BooksSpr2026_sec02.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BooksSpr2026_sec02.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly BooksDbContext _dbContext;
        private IWebHostEnvironment _environment;

        public BookController(BooksDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var listOfBooks = _dbContext.Books.Include(b => b.category).ToList();
            return View(listOfBooks);
        }



        // NNEED CREATE IACTION

        [HttpGet]
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> listOfCategories = _dbContext.Categories.ToList().Select(o => new SelectListItem {Text = o.Name, Value = o.CategoryId.ToString() });

            //ViewBag.ListOfCategories = listOfCategories;

            //ViewData["ListOfCategoriesVD"] = listOfCategories;

            BookWithCategoriesVM bookWithCategoriesVMobj = new BookWithCategoriesVM();

            bookWithCategoriesVMobj.Book = new Book();
            bookWithCategoriesVMobj.ListOfCategories = listOfCategories;

            return View(bookWithCategoriesVMobj);

        }



        [HttpPost]
        public IActionResult Create(BookWithCategoriesVM bookWithCategoriesVMobj, IFormFile imgFile)
        {

            if (ModelState.IsValid)
            {
                string wwwrootPath = _environment.WebRootPath;

                if(imgFile !=  null)
                {
                    using (var fileStream = new FileStream(Path.Combine(wwwrootPath, @"Images\" + imgFile.FileName), FileMode.Create))
                    {
                        imgFile.CopyTo(fileStream); //saves to root folder, specified above
                    }
                }
                //save url in book model
                bookWithCategoriesVMobj.Book.ImgUrl = @"\Images\" + imgFile.FileName;


            }
            _dbContext.Books.Add(bookWithCategoriesVMobj.Book);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");

            return View(bookWithCategoriesVMobj); //if model is invalid, the form will be displayed with the appropriate error messages
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Book book = _dbContext.Books.Find(id);

            IEnumerable<SelectListItem> listOfCategories = _dbContext.Categories.ToList().Select(o => new SelectListItem { Text = o.Name, Value = o.CategoryId.ToString() });

            BookWithCategoriesVM bookWithCategoriesVM = new BookWithCategoriesVM();

            bookWithCategoriesVM.Book = book;

            bookWithCategoriesVM.ListOfCategories = listOfCategories;

            return View(bookWithCategoriesVM);
        }
        [HttpPost]
        public IActionResult Edit(BookWithCategoriesVM bookWithCategoriesVMobj, IFormFile imgFile)
        {
            string wwwrootPath = _environment.WebRootPath;

            if (ModelState.IsValid)
            {
                if(imgFile != null)
                {
                    if (!string.IsNullOrEmpty(bookWithCategoriesVMobj.Book.ImgUrl))
                    {
                        var oldImgPath = Path.Combine(wwwrootPath, bookWithCategoriesVMobj.Book.ImgUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);
                        }

                    }

                    using (var fileStream = new FileStream(Path.Combine(wwwrootPath, @"\Images" + imgFile.FileName), FileMode.Create))
                    {
                        imgFile.CopyTo(fileStream);
                    }

                    bookWithCategoriesVMobj.Book.ImgUrl = @"\Images\" + imgFile.FileName;
                }

                _dbContext.Books.Update(bookWithCategoriesVMobj.Book);
                _dbContext.SaveChanges();

                return RedirectToAction("Index");

            }
        }



    }
}
