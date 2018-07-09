namespace P01_BillsPaymentSystem.App
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StartUp
    {
        public static void Main()
        {
            using (var db = new BillsPaymentSystemContext())
            {
                db.Database.EnsureDeleted();
                db.Database.Migrate();
                Database.Seed(db);

                Console.WriteLine("Enter UserId: ");
                var userId = int.Parse(Console.ReadLine());

                Database.UserDetails(userId, db);

                Console.WriteLine("Do You Want To Withdraw Sum ? Y/N");
                var input = Console.ReadLine().ToLower();

                if (input == "y")
                {
                    Console.WriteLine("Enter sum to withdraw");
                    PayBills(userId, decimal.Parse(Console.ReadLine()), db);
                }
            }
        }

        private static void PayBills(int userId, decimal amount, BillsPaymentSystemContext db)
        {
            using (db = new BillsPaymentSystemContext())
            {
                var user = db.Users
                    .Include(x => x.PaymentMethods)
                    .FirstOrDefault(x => x.UserId == userId);

                var bankAccounts = (from a in db.BankAccounts from b in user.PaymentMethods where a.BankAccountId == b.BankAccountId select a).ToList();

                var creditCards = (from a in db.CreditCards from b in user.PaymentMethods where a.CreditCardId == b.CreditCardId select a).ToList();

                try
                {
                    foreach (var ba in bankAccounts)
                    {
                        ba.Withdraw(amount);
                    }
                    foreach (var cc in creditCards)
                    {
                        cc.Withdraw(amount);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
        }
    }
}
