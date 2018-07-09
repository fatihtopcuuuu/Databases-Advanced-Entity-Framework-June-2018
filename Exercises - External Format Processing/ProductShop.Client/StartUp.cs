using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProductShop.Client
{
    public class StartUp
    {
        public static void Main()
        {
            Console.WriteLine(ImportUsersFromJson());
            Console.WriteLine(ImportCategoriesFromJson());
            Console.WriteLine(ImportProductsFromJson());
        }

        private static void SetCategories()
        {
            using (var context = new ProductShopContext())
            {
                var productIds = context
                    .Products
                    .Select(p => p.Id)
                    .ToList();
                var categoryIds = context
                    .Categories
                    .Select(c => c.Id)
                    .ToList();

                var categoryProducts = new List<CategoryProduct>();

                var random = new Random();

                foreach (var product in productIds)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int categoryIndex = random.Next(0, categoryIds.Count);

                        var categoryProduct = new CategoryProduct
                        {
                            ProductId = product,
                            CategoryId = categoryIds[categoryIndex]
                        };

                        categoryProducts.Add(categoryProduct);
                    }
                }

            }
        }

        private static string ImportProductsFromJson()
        {
            var path = "Files/products.json";
            var products = ImportJson<Product>(path);

            var random = new Random();

            using (var context = new ProductShopContext())
            {
                var userIds = context
                    .Users
                    .Select(u => u.Id)
                    .ToList();

                foreach (var product in products)
                {
                    int index = random.Next(0, userIds.Count);
                    int sellerId = userIds[index];

                    int? buyerId = sellerId;
                    while (buyerId == sellerId)
                    {
                        int buyerIndex = random.Next(0, userIds.Count);
                        buyerId = userIds[buyerIndex];
                    }

                    if (buyerId - sellerId < 10 && buyerId - sellerId > 0)
                    {
                        buyerId = null;
                    }

                    product.SellerId = sellerId;
                    product.BuyerId = buyerId;
                }

                context.Products.AddRange(products);
                context.SaveChanges();
            }

            return $"{products.Length} products were imported from file: {path}";
        }

        private static string ImportUsersFromJson()
        {
            var path = "Files/users.json";
            User[] users = ImportJson<User>(path);

            using (var context = new ProductShopContext())
            {
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            return $"{users.Length} users were imported from: {path}";
        }

        private static T[] ImportJson<T>(string path)
        {
            var jsonString = File.ReadAllText(path);

            var objects = JsonConvert.DeserializeObject<T[]>(jsonString);

            return objects;
        }

        private static string ImportCategoriesFromJson()
        {
            var path = "Files/categories.json";

            var categories = ImportJson<Category>(path);

            using (var context = new ProductShopContext())
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            return $"{categories.Length} categories have been imported from: {path}";
        }

        private static void InitializeMapper()
        {
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<Product, ProductDto>()
            //        .ForMember(dto => dto.SellerName,
            //            opt => opt.MapFrom(src => $"{src.Seller.FirstName} {src.Seller.LastName}"));
            //    cfg.CreateMap<Product, SoldProductDto>()
            //        .ForMember(dto => dto.BuyerFirstName,
            //            opt => opt.MapFrom(src => src.Buyer.FirstName))
            //        .ForMember(dto => dto.BuyerLastName,
            //            opt => opt.MapFrom(src => src.Buyer.LastName));

            //    cfg.CreateMap<Product, UserSoldProductsDto>()
            //        .ForMember(dto => dto.FirstName,
            //            opt => opt.MapFrom(src => src.Seller.FirstName))
            //        .ForMember(dto => dto.LastName,
            //            opt => opt.MapFrom(src => src.Seller.LastName));

            //    cfg.CreateMap<CategoryDto, CategoryProductsDto>()
            //        .ForMember(dto => dto.ProductsCount,
            //            opt => opt.MapFrom(src => src.Products.Count))
            //        .ForMember(dto => dto.AveragePrice,
            //            opt => opt.MapFrom(src => src.Products.Average(p => p.Price)))
            //        .ForMember(dto => dto.TotalRevenue,
            //            opt => opt.MapFrom(src => src.Products.Sum(p => p.Price)));
            //});
        }
    }
}
