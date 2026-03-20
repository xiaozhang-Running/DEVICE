using System;
using System.Threading.Tasks;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsumableChecker
{
    class Program
    {
        static async Task Main()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DeviceWarehouse;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            using var context = new ApplicationDbContext(options);
            {
                var lowStockConsumables = await context.Consumables
                    .Where(c => c.RemainingQuantity <= 10)
                    .ToListAsync();

                Console.WriteLine("低库存耗材:");
                foreach (var consumable in lowStockConsumables)
                {
                    Console.WriteLine($"{consumable.Name} - 剩余: {consumable.RemainingQuantity}");
                }
            }
        }
    }
}