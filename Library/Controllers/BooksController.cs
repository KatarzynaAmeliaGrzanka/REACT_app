using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Library.Migrations.BookDb;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Collections.Immutable;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookDbContext _context;

        public BooksController(BookDbContext context)
        {
            _context = context;
            cancelreservation();
        }

        public void cancelreservation()
        {
            DateTime currentday = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            var books = from s in _context.Book.Where(a => a.Reserved != null)
                        select s;
            foreach (var book in books)
            {
                if (book.Reserved <= currentday)
                {
                    book.Reserved = null;
                    book.User = null;
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }

        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string searchString2, string reserved)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            var books = from s in _context.Book
                        select s;

            if (books == null)
            {
                return Problem("Entity set 'BookContext'  is null.");
            }

            if (sortOrder == "title_desc") 
            {
                books = books.OrderByDescending(s => s.Title);
            }
            else
                books = books.OrderBy(s => s.Title);

            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title!.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(searchString2))
            {
                books = books.Where(s => s.Author!.Contains(searchString2));
            }

            if (!String.IsNullOrEmpty(reserved))
            {
                books = books.Where(s => s.Reserved!=null);
            }

            return View(await books.AsNoTracking().ToListAsync());
        }

        // GET: Reserved Books
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Index2(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            var books = from s in _context.Book.Where(a => a.User == this.User.Identity.Name && a.Reserved != null)
                        select s;

            if (sortOrder == "title_desc")
            {
                books = books.OrderByDescending(s => s.Title);
            }
            else
                books = books.OrderBy(s => s.Title);

            return View(await books.AsNoTracking().ToListAsync());
        }

        //GET : Rented books
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Index3(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            var books = from s in _context.Book.Where(a => a.Leased != null)
                        select s;

            if (sortOrder == "title_desc")
            {
                books = books.OrderByDescending(s => s.Title);
            }
            else
                books = books.OrderBy(s => s.Title);

            return View(await books.AsNoTracking().ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Reserve/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Reserve(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            if (book.User != null)
            {
                TempData["AlertMessage"] = "This book is not avaiable - you cannot reserve it.";
            }

            return View(book);
        }

        // POST: Books/Reserve/5
        [HttpPost, ActionName("Reserve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int id)
        {
            if (_context.Book == null)
            {
                return Problem("Entity set 'BookDbContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var UserName = currentUser.Identity.Name;
                if (UserName != null) {
                    book.Reserved = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1);
                    book.User = UserName;
                }
            }

            try
            {
               await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/DeleteReservation/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Book.Where(a => a.User == this.User.Identity.Name && a.User != null) == null)
            {
                return NotFound();
            }

            var book = await _context.Book.Where(a => a.User == this.User.Identity.Name && a.User != null)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/DeleteReservation/5
        [HttpPost, ActionName("DeleteReservation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Book.Where(a => a.User == this.User.Identity.Name && a.User != null) == null)
            {
                return Problem("Entity set 'BookDbContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                book.Reserved = null;
                book.User = null;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }

            return RedirectToAction(nameof(Index2));
        }

        // POST: Books/Rent/5
        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Rent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int id)
        {
            if (_context.Book == null)
            {
                return Problem("Entity set 'BookDbContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null && book.User != null)
            {

                book.Leased= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                book.Reserved= null;
            }
            else
            {
                TempData["AlertMessage"] = "This book is not reserved - it cannot be leased.";
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }

            return RedirectToAction(nameof(Index), new {reserved = "reserved"});
        }


        // POST: Books/Return/5
        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            if (_context.Book == null)
            {
                return Problem("Entity set 'BookDbContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null && book.User != null)
            {

                book.Leased = null;
                book.User = null;
            }
            else
            {
                TempData["AlertMessage"] = "This book is not reserved - it cannot be leased.";
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }

            return RedirectToAction(nameof(Index3));
        }
        private bool BookExists(int id)
        {
          return _context.Book.Any(e => e.Id == id);
        }
    }
}
