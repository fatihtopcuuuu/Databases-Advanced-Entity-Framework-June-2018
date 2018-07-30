using FastFood.Data;

namespace FastFood.DataProcessor
{
    using System.Linq;

    public static class Bonus
    {
        public static string UpdatePrice(FastFoodDbContext context, string itemName, decimal newPrice)
        {
            var item = context
                .Items
                .SingleOrDefault(i => i.Name == itemName);

            if (item == null)
            {
                return $"Item {itemName} not found!";
            }

            var oldPrice = item.Price;

            item.Price = newPrice;

            context.SaveChanges();

            return $"{item.Name} Price updated from ${oldPrice:f2} to ${item.Price:f2}";
        }
    }
}
