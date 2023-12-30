using Microsoft.EntityFrameworkCore;
using Todo.Entities;

namespace Todo.Contexts
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<TodoItem>(entity =>
        {
            _ = entity.Property(static t => t.Name).HasMaxLength(1000);
            _ = entity.Property(static t => t.Project).HasMaxLength(1000);
            _ = entity.Property(static t => t.Context).HasMaxLength(1000);
            _ = entity.Property(static t => t.Secret).HasMaxLength(1000);
        });
    }
}
