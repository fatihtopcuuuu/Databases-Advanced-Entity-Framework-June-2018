using FastFood.Data;
using System;
using System.IO;

namespace FastFood.DataProcessor
{
    using Dtos.Export;
    using Microsoft.EntityFrameworkCore;
    using Models.Enums;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportOrdersByEmployee(FastFoodDbContext context, string employeeName, string orderType)
        {
            var type = (OrderType)Enum.Parse(typeof(OrderType), orderType);

            var orders = context
                .Employees
                .Include(e => e.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(e => e.Name == employeeName && e.Orders.Any(o => o.Type == type))
                .ToArray()
                .Select(e => new
                {
                    e.Name,
                    Orders = e.Orders.ToArray()
                })
                .Select(e => new
                {
                    e.Name,
                    Orders = e.Orders.Select(o => new
                    {
                        o.Customer,
                        Items = o.OrderItems.Select(oi => new
                        {
                            oi.Item.Name,
                            oi.Item.Price,
                            oi.Quantity
                        }).ToArray(),
                        TotalPrice = o.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity)
                    })
                        .OrderByDescending(o => o.TotalPrice)
                        .ToArray()
                })
                .Select(e => new
                {
                    e.Name,
                    Orders = e.Orders
                        .OrderByDescending(o => o.TotalPrice)
                        .ThenByDescending(i => i.Items.Length),
                    TotalMade = e.Orders.Sum(o => o.TotalPrice)
                })
                .SingleOrDefault();


            var json = JsonConvert.SerializeObject(orders);

            return json;
        }

        public static string ExportCategoryStatistics(FastFoodDbContext context, string categoriesString)
        {
            var categoryNames = categoriesString.Split(',');

            var categories = context
                .Categories
                .Where(c => categoryNames.Any(cn => cn == c.Name))
                .Select(c => new
                {
                    c.Name,
                    MostPopularItem = c.Items
                        .OrderByDescending(i => i.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity))
                        .FirstOrDefault()
                })
                .Select(c => new CategoryDto
                {
                    Name = c.Name,
                    MostPopularItem = new ItemDto
                    {
                        Name = c.MostPopularItem.Name,
                        TotalMade = c.MostPopularItem.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity),
                        TimesSold = c.MostPopularItem.OrderItems.Sum(oi => oi.Quantity)
                    }
                })
                .OrderByDescending(c => c.MostPopularItem.TotalMade)
                .ThenByDescending(c => c.MostPopularItem.TimesSold)
                .ToArray();

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(CategoryDto[]), new XmlRootAttribute("Categories"));
            serializer.Serialize(new StringWriter(sb),
                categories,
                new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));

            return sb.ToString();
        }
    }
}