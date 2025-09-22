using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemeGen.ApiService.Persistent;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Photo> Photos => Set<Photo>();
    
    public DbSet<Person> Persons => Set<Person>();
    
    public DbSet<QuoteItem> Quotes => Set<QuoteItem>();
}