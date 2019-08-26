using Microsoft.EntityFrameworkCore;
namespace ActivityCenter.Models
{
    public class MyContext :DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
        public DbSet<User> Users {get;set;}
        public DbSet<ActivityEvent> Activites {get;set;}
        public DbSet <Guest> Links {get;set;}
    }
}