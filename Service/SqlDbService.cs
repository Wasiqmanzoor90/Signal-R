using Microsoft.EntityFrameworkCore;
using MyApiProject.Model;

namespace MyApiProject.Service
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users{ get; set; }
       

    }


}