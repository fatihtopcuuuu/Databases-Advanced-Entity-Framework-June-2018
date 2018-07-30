namespace FastFood.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using AutoMapper;
    using Dto.Import;
    using Data;
    using Models;
    using Models.Enums;
    using Newtonsoft.Json;
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

                var position = FindOrCreatePosition(context, employeeDto.Position);
                var employee = Mapper.Map<Employee>(employeeDto);
                employee.Position = position;

                validEmployees.Add(employee);
                sb.AppendLine(string.Format(SuccessMessage, employeeDto.Name));
            }

            context.Employees.AddRange(validEmployees);
            context.SaveChanges();

            return sb.ToString();
        }

        private static Position FindOrCreatePosition(FastFoodDbContext context, string employeeDtoPosition)
        {
            var position = context
                .Positions
                .SingleOrDefault(e => e.Name == employeeDtoPosition);
            if (position == null)
            {
                position = new Position
                {
                    Name = employeeDtoPosition
                };

                context.Positions.Add(position);
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

                var category = FindOrCreateCategory(context, itemDto.Category);
                var item = Mapper.Map<Item>(itemDto);
                item.Category = category;

                validItems.Add(item);
                sb.AppendLine(string.Format(SuccessMessage, itemDto.Name));
            }

            context.Items.AddRange(validItems);
            context.SaveChanges();

            return sb.ToString();
        }

        private static Category FindOrCreateCategory(FastFoodDbContext context, string itemDtoCategoryName)
        {
            var category = context
                .Categories
                .SingleOrDefault(c => c.Name == itemDtoCategoryName);
            if (category == null)
            {
                category = new Category
                {
                    Name = itemDtoCategoryName
                };

                context.Categories.Add(category);
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
                var employee = context
                    .Employees
                    .SingleOrDefault(e => e.Name == orderDto.Employee); // If the order’s employee doesn’t exist, do not import the order
                if (!IsValid(orderDto) || !IsValid(orderDto.Items) || employee == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var validItems = true;
                var validOrderItems = new List<OrderItem>();
                foreach (var itemDto in orderDto.Items)
                {
                    var item = context
                        .Items
                        .SingleOrDefault(i => i.Name == itemDto.Name); // If any of the order’s items do not exist, do not import the order

                    if (item == null)
                    {
                        validItems = false;
                        break;
                    }

                    var orderItem = new OrderItem
                    {
                        Item = item,
                        Quantity = itemDto.Quantity,
                    };

                    validOrderItems.Add(orderItem);
                }

                if (!validItems)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var date = DateTime.ParseExact(orderDto.DateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                var type = OrderType.ForHere;
                if (orderDto.Type != null)
                {
                    type = Enum.Parse<OrderType>(orderDto.Type);
                }

                var order = new Order
                {
                    Customer = orderDto.Customer,
                    DateTime = date,
                    Employee = employee,
                    Type = type,
                    OrderItems = validOrderItems,
                };

                validOrders.Add(order);
                sb.AppendLine($"Order for {orderDto.Customer} on {orderDto.DateTime} added");
            }

            context.Orders.AddRange(validOrders);
            context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            return isValid;
        }
    }
}