using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using aspNetCoreMvc.Models;

namespace aspNetCoreMvc.Data
{
    public class aspNetCoreMvcContext : DbContext
    {
        public aspNetCoreMvcContext(DbContextOptions<aspNetCoreMvcContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; }
    }
}
