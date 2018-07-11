namespace BookShop
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Models;
    using Models.Enums;

    public class StartUp
    {
        public static void Main()
        {
            //NOTE: The methods should be public in order to get the maximum points in judge

            var command = Console.ReadLine().ToLower();
            //var year = int.Parse(Console.ReadLine());
            //var length = int.Parse(Console.ReadLine());

            using (var db = new BookShopContext())
            {
                //DbInitializer.ResetDatabase(db); DO NOT SUBMIT THIS IN JUDGE OR YOU WILL GET COMPILE TIME ERROR
                Console.Write(GetMostRecentBooks(db));
            }
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var sb = new StringBuilder();

            var commandType = (AgeRestriction)Enum.Parse(typeof(AgeRestriction), CapitalizeCommand(command));

            context
               .Books
               .Where(b => b.AgeRestriction == commandType)
               .Select(b => b.Title)
               .OrderBy(t => t)
               .ToList()
               .ForEach(t => sb.AppendLine(t));

            return sb.ToString();
        }

        public static string CapitalizeCommand(string command)
        {
            return char.ToUpper(command[0]) + command.Substring(1);
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.EditionType == EditionType.Gold && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList()
                .ForEach(t => sb.AppendLine(t));

            return sb.ToString();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .OrderByDescending(b => b.Price)
                .ToList()
                .ForEach(t => sb.AppendLine($"{t.Title} - ${t.Price:f2}"));

            return sb.ToString();
        }

        public static string GetBooksNotRealeasedIn(BookShopContext context, int year)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList()
                .ForEach(t => sb.AppendLine(t));

            return sb.ToString();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var sb = new StringBuilder();

            var categories = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            context
                .Books
                .Where(b => b.BookCategories
                    .Any(c => categories.Contains(c.Category.Name.ToLower())))
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToList()
                .ForEach(b => sb.AppendLine(b));

            return sb.ToString();
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.ReleaseDate < DateTime.Parse(date, CultureInfo.CurrentCulture))
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                })
                .ToList()
                .ForEach(t => sb.AppendLine($"{t.Title} - {t.EditionType} - ${t.Price:f2}"));

            return sb.ToString();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var sb = new StringBuilder();

            context
                .Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    FullName = $"{a.FirstName} {a.LastName}"
                })
                .ToList()
                .ForEach(t => sb.AppendLine(t.FullName));

            return sb.ToString();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.Title.Contains(input))
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToList()
                .ForEach(t => sb.AppendLine(t));

            return sb.ToString();
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var sb = new StringBuilder();

            context
                .Books
                .Where(b => b.Author.LastName.StartsWith(input))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    AuthorFullName = $"{b.Author.FirstName} {b.Author.LastName}"
                })
                .ToList()
                .ForEach(b => sb.AppendLine($"{b.Title} ({b.AuthorFullName})"));

            return sb.ToString();
        }

        public static string CountBooks(BookShopContext context, int lengthCheck)
        {
            var booksCounts = context
                .Books
                .Count(b => b.Title.Length > lengthCheck);

            var result = $"There are {booksCounts} books with longer title than {lengthCheck} symbols";

            return result;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var sb = new StringBuilder();

            context
                .Authors
                .Select(b => new
                {
                    AuthorFullName = $"{b.FirstName} {b.LastName}",
                    BooksCopies = b.Books
                        .Select(c => c.Copies)
                        .Sum()
                })
                .OrderByDescending(b => b.BooksCopies)
                .ToList()
                .ForEach(a => sb.AppendLine($"{a.AuthorFullName} - {a.BooksCopies}"));

            return sb.ToString();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var sb = new StringBuilder();

            context
                .Categories
                .Select(c => new
                {
                    c.Name,
                    TotalProfit = c.CategoryBooks.Sum(b => b.Book.Copies * b.Book.Price)
                })
                .OrderByDescending(b => b.TotalProfit)
                .ThenBy(b => b.Name)
                .ToList()
                .ForEach(c => sb.AppendLine($"{c.Name} ${c.TotalProfit:f2}"));

            return sb.ToString();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context
                .Categories
                .Select(c => new
                {
                    c.Name,
                    BookCount = c
                        .CategoryBooks
                        .Count,
                    RecentBooks = c
                        .CategoryBooks
                        .Select(b => b.Book)
                        .OrderByDescending(b => b.ReleaseDate.Value.Year)
                        .Take(3)
                })
                .OrderBy(c => c.Name)
                .ToList();

            return string.Join(Environment.NewLine, categories
                .Select(x => $"--{x.Name}" + Environment.NewLine + string.Join(Environment.NewLine, x.RecentBooks
                                 .Select(y => $"{y.Title} ({y.ReleaseDate.Value.Year})"))));
        }

        public static void IncreasePrices(BookShopContext context)
        {
            context
                .Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList()
                .ForEach(p => p.Price += 5);

            context.SaveChanges();
        }

        public static string RemoveBooks(BookShopContext context)
        {
            var booksToDelete = context
                .Books
                .Where(b => b.Copies < 4200)
                .ToList();

            var countOfBooks = booksToDelete.Count;

            context.Books.RemoveRange(booksToDelete);

            context.SaveChanges();

            return $"{countOfBooks} books were deleted";
        }
    }
}
