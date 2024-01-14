using Microsoft.EntityFrameworkCore;

namespace api.Model
{
    public class TodosAppContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Todo> Todos { get; set; }

        public TodosAppContext(DbContextOptions<TodosAppContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            var userGuid = Guid.NewGuid();

            modelBuilder.Entity<User>().HasData(
            new User()
            {
                Id = userGuid,
                Username = "wladyslaw",
                Password = "123123123"
            });

            modelBuilder.Entity<Todo>().HasData(
            new Todo()
            {
                Id = Guid.NewGuid(),
                Content = "Zrob pranie",
                IsCompleted = false,
                UserId = userGuid
            },
            new Todo()
            {
                Id = Guid.NewGuid(),
                Content = "Wyrzuc smieci",
                IsCompleted = true,
                UserId = userGuid
            }
            );
        }
    }
}
