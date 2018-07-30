namespace FastFood.DataProcessor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Dto.Export;
    using Microsoft.EntityFrameworkCore;
    using Models.Enums;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportOrdersByEmployee(FastFoodDbContext context, string employeeName, string orderType)
        {
            var type = Enum.Parse<OrderType>(orderType);

            var orders = context
                .Orders
                .Where(o => o.Employee.Name == employeeName && o.Type == type)
                .Select(o => new
                {
                    o.Customer,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.Item.Name,
                        oi.Item.Price,
                        oi.Quantity,
                    }).ToArray(),
                    TotalPrice = o.OrderItems.Sum(i => i.Item.Price * i.Quantity),
                })
                .OrderByDescending(o => o.TotalPrice)
                .ThenByDescending(o => o.Items.Length)
                .ToList();

            var orderItems = new
            {
                Name = employeeName,
                Orders = orders,
                TotalMade = orders.Sum(o => o.TotalPrice)
            };

            var json = JsonConvert.SerializeObject(orderItems, Formatting.Indented);

            return json;

        }

        public static string ExportCategoryStatistics(FastFoodDbContext context, string categoriesString)
        {
            var sb = new StringBuilder();
            var categoryNames = categoriesString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var categories = context
                .Categories
                .Include(c => c.Items)
                .ThenInclude(oi => oi.OrderItems)
                .Where(c => categoryNames.Contains(c.Name))
                .ToArray()
                .Select(c => new
                {
                    c.Name,
                    MostPopularItem = c.Items.Aggregate((i, j) => i.OrderItems.Sum(oi => oi.Quantity * i.Price) >
                                                                  j.OrderItems.Sum(oi => oi.Quantity * j.Price) ? i : j)
                })
                .Select(c => new CategoryDto
                {
                    Name = c.Name,
                    MostPopularItem = new MostPopularItemDto
                    {
                        Name = c.MostPopularItem.Name,
                        TotalMade = c.MostPopularItem.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity),
                        TimesSold = c.MostPopularItem.OrderItems.Sum(oi => oi.Quantity)
                    }
                })
                .OrderByDescending(c => c.MostPopularItem.TotalMade)
                .ThenByDescending(c => c.MostPopularItem.TimesSold)
                .ToArray();

            //Second Way
            //var categories = context
            //    .Categories
            //    .Where(c => categoryNames.Contains(c.Name))
            //    .Select(c => new
            //    {
            //        c.Name,
            //        MostPopularItem = c.Items
            //        .OrderByDescending(i => i.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity))
            //        .FirstOrDefault()
            //    })
            //    .Select(c => new CategoryDto
            //    {
            //        Name = c.Name,
            //        MostPopularItem = new MostPopularItemDto
            //        {
            //            Name = c.MostPopularItem.Name,
            //            TotalMade = c.MostPopularItem.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity),
            //            TimesSold = c.MostPopularItem.OrderItems.Sum(oi => oi.Quantity)
            //        }
            //    })
            //    .OrderByDescending(c => c.MostPopularItem.TotalMade)
            //    .ThenByDescending(c => c.MostPopularItem.TimesSold)
            //    .ToArray();

            var serializer = new XmlSerializer(typeof(CategoryDto[]), new XmlRootAttribute("Categories"));
            serializer.Serialize(new StringWriter(sb),
                categories,
                new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty, }));

            return sb.ToString();
        }
    }
}