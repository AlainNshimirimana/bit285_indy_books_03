using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndyBooks.Models;
using IndyBooks.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndyBooks.Controllers
{
    public class AdminController : Controller
    {
        private IndyBooksDataContext _db;
        public AdminController(IndyBooksDataContext db) { _db = db; }

        /***
         * CREATE
         */
        [HttpGet]
        public IActionResult CreateBook()
        {
            //TODO: Populate a new AddBookViewModel object with a complete set of Writers
            //      and send it on to the View "AddBook"
            AddBookViewModel addBookViewModel = new AddBookViewModel
            {
                Writers = _db.Writers
            };

            return View("AddBook", addBookViewModel);
        }
        [HttpPost]
        public IActionResult CreateBook(AddBookViewModel bookVM)
        {
            //TODO: Build the Writer object using the parameter
            Writer author = new Writer();
            if(bookVM.AuthorId != 0)
            {
                author = _db.Writers.Single(a => a.Id == bookVM.Id);
            }
            else
            {
                author.Name = bookVM.Name;
            }
            

            //TODO: Build the Book using the parameter data and your newly created author.
            Book book;
            Book newBook = new Book
            {
                Title = bookVM.Title,
                SKU = bookVM.SKU,
                Price = bookVM.Price,
                Author = author
            };

            //TODO: Add author and book to their DbSets; SaveChanges
            _db.Books.Add(newBook);
            _db.SaveChanges();
           

            //Shows the book using the Index View 
            return RedirectToAction("Index", new { id = bookVM.Id });
        }
        /***
         * READ       
         */
        [HttpGet]
        public IActionResult Index(long id)
        {
            IQueryable<Book> books = _db.Books.Include(b => b.Author);
            //TODO: filter books by the id (if passed an id as its Route Parameter),
            //     otherwise use the entire collection of Books, ordered by SKU.
            if(id != 0)
            {
                var book = _db.Books.Single(b => b.Id == id);
            }
            else
            {
                var book = _db.Books.OrderBy(b => b.SKU);
            }


            return View("SearchResults", books);
        }
        /***
         * UPDATE
         */
         //TODO: Write a method to take a book id, and load book and author info
         //      into the ViewModel for the AddBook View
         [HttpGet]

        /***
         * DELETE
         */
        [HttpGet]
        public IActionResult DeleteBook(long id)
        {
            //TODO: Remove the Book associated with the given id number; Save Changes
            var book = _db.Books.Single(b => b.Id == id);
            _db.Books.Remove(book);
            _db.SaveChanges();



            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Search() { return View(); }
        [HttpPost]
        public IActionResult Search(SearchViewModel search)
        {
            //Full Collection Search
            IQueryable<Book> foundBooks = _db.Books; // start with entire collection

            //Partial Title Search
            if (search.Title != null)
            {
                foundBooks = foundBooks
                            .Where(b => b.Title.Contains(search.Title))
                            .OrderBy(b => b.Author.Name)
                            ;
            }

            //Author's Last Name Search
            if (search.AuthorName != null)
            {
                //Use the Name property of the Book's Author entity
                foundBooks = foundBooks
                            .Include(b => b.Author)
                            .Where(b => b.Author.Name.Contains(search.AuthorName, StringComparison.CurrentCulture))
                            ;
            }
            //Priced Between Search (min and max price entered)
            if (search.MinPrice > 0 && search.MaxPrice > 0)
            {
                foundBooks = foundBooks
                            .Where(b => b.Price >= search.MinPrice && b.Price <= search.MaxPrice)
                            .OrderByDescending(b=>b.Price)
                            ;
            }
            //Highest Priced Book Search (only max price entered)
            if (search.MinPrice == 0 && search.MaxPrice > 0)
            {
                decimal max = _db.Books.Max(b => b.Price);
                foundBooks = foundBooks
                            .Where(b => b.Price == max)
                            ;
            }
            //Composite Search Results
            return View("SearchResults", foundBooks);
        }

    }
}
