namespace P01_BillsPaymentSystem.Models
{
    using System.Collections.Generic;

    public class User
    {
        public User()
        {
            this.PaymentMethods = new List<PaymentMethod>();
        }

        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int PaymentMethodId { get; set; }

        public ICollection<PaymentMethod> PaymentMethods { get; set; }
    }
}
