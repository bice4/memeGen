using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemeGen.ApiService.Persistent;

public static class DbInitializer
{
    public static void MigrateAndSeed(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.Migrate();


        if (db.Persons.Any()) return;

        db.Persons.AddRange(
            new Person("Саня"),
            new Person("Никита")
        );
        db.SaveChanges();
    }
}