using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Library.Models;

    public class BookDbContext : DbContext
    {
        public BookDbContext (DbContextOptions<BookDbContext> options)
            : base(options)
        {
        }

        public DbSet<Library.Models.Book> Book { get; set; }
    }
