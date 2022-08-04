using Microsoft.EntityFrameworkCore;
using Idea.Models;

namespace Idea.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) {}
    public DbSet<TestModel>? Models { get; set; }
    public DbSet<User>? Users { get; set; }
}