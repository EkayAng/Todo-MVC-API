using Microsoft.EntityFrameworkCore;
using TodoApi.Model;

namespace Todo_MVC_API.Repository
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}
