namespace FastFood.DataProcessor
{
    using AutoMapper;
    using Data;
    using Dtos.Import;
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Models.Enums;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public static class Deserializer
    {
        private const string FailureMessage = "Invalid data format.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportEmployees(FastFoodDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedEmployees = JsonConvert.DeserializeObject<EmployeeDto[]>(jsonString);

            var validEmployees = new List<Employee>();
            foreach (var employeeDto in deserializedEmployees)
            {
                if (!IsValid(employeeDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var employee = Mapper.Map<Employee>(employeeDto);

                Position position = FindOrCreatePosition(context, employeeDto.Position);
                employee.Position = position;

                sb.AppendLine(string.Format(SuccessMessage, employeeDto.Name));

                validEmployees.Add(employee);
            }
            context.Employees.AddRange(validEmployees);
            context.SaveChanges();

            return sb.ToString();
        }

        private static Position FindOrCreatePosition(FastFoodDbContext context, string wantedPosition)
        {
            var position = context.Positions.SingleOrDefault(p => p.Name == wantedPosition);
            if (position == null)
            {
                var pos = new Position
                {
                    Name = wantedPosition
                };
                context.Positions.Add(pos);
                context.SaveChanges();

            }

            return position;
        }

        public static string ImportItems(FastFoodDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedItems = JsonConvert.DeserializeObject<ItemDto[]>(jsonString);

            var validItems = new List<Item>();
            foreach (var itemDto in deserializedItems)
            {
                if (!IsValid(itemDto) || validItems.Any(i => i.Name == itemDto.Name))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var item = Mapper.Map<Item>(itemDto);

                var category = FindOrCreateCategory(context, itemDto);

                item.Category = category;

                validItems.Add(item);
                sb.AppendLine(string.Format(SuccessMessage, item.Name));
            }
            context.Items.AddRange(validItems);
            context.SaveChanges();

            return sb.ToString();
        }

        private static Category FindOrCreateCategory(FastFoodDbContext context, ItemDto itemDto)
        {
            var category = context.Categories.SingleOrDefault(c => c.Name == itemDto.Category);
            if (category == null)
            {
                var categ = new Category
                {
                    Name = itemDto.Category
                };

                context.Categories.Add(categ);
                context.SaveChanges();

            }

            return category;
        }

        public static string ImportOrders(FastFoodDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(OrderDto[]), new XmlRootAttribute("Orders"));
            var deserializedOrders = (OrderDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validOrders = new List<Order>();
            foreach (var orderDto in deserializedOrders)
            {
                if (!IsValid(orderDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isOrderEmployeeExist = context.Employees.Any(e => e.Name == orderDto.Employee); //•If the order’s employee doesn’t exist, do not import the order.
                if (!isOrderEmployeeExist)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var items = orderDto
                    .Items
                    .ToArray();
                var validItems = items.All(i => IsValid(items)); //•If any of the order’s items do not exist, do not import the order.
                var itemsExist = orderDto
                    .Items
                    .All(s => context.Items.Any(i => i.Name == s.Name)); //•If any of the order’s items do not exist, do not import the order.
                if (!validItems || !itemsExist)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var date = DateTime.ParseExact(orderDto.DateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                var type = OrderType.ForHere;
                if (orderDto.Type != null)
                {
                    type = (OrderType)Enum.Parse(typeof(OrderType), orderDto.Type, true);
                }

                var orderItems = new List<OrderItem>();
                foreach (var itemDto in orderDto.Items)
                {
                    var item = context.Items.First(i => i.Name == itemDto.Name);
                    var orderItem = new OrderItem
                    {
                        Item = item,
                        Quantity = itemDto.Quantity
                    };

                    orderItems.Add(orderItem);
                }

                var employee = context.Employees.FirstOrDefault(e => e.Name == orderDto.Employee);
                var order = new Order
                {
                    Employee = employee,
                    Type = type,
                    DateTime = date,
                    Customer = orderDto.Customer,
                    OrderItems = orderItems
                };

                validOrders.Add(order);
                sb.AppendLine($"Order for {orderDto.Customer} on {date.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} added");
            }

            context.Orders.AddRange(validOrders);
            context.SaveChanges();


            return sb.ToString();
        }

        private static bool IsValid(object obj)
        {
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, context, results, true);

            return isValid;
        }
    }
}