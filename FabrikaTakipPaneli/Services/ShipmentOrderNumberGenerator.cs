using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Services;

public static class ShipmentOrderNumberGenerator
{
    public static async Task<string> GenerateAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        var year = DateTime.Now.Year;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT IGNORE INTO ShipmentSequences (Year, LastNumber) VALUES ({year}, 0)",
            cancellationToken);

        var sequence = await context.ShipmentSequences
            .FromSqlInterpolated($"SELECT * FROM ShipmentSequences WHERE Year = {year} FOR UPDATE")
            .SingleAsync(cancellationToken);

        sequence.LastNumber++;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return $"SVK-{year}-{sequence.LastNumber:D4}";
    }
}
