using System.Collections.Generic;

namespace ProductShop.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? Age { get; set; }

        public ICollection<Product> SoldProducts { get; set; } = new HashSet<Product>();
        public ICollection<Product> BoughtProducts { get; set; } = new HashSet<Product>();
    }
}
