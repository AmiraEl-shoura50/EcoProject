using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;



namespace ECommerce.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation (علاقة اختيارية 1-to-1 مع Customer أو Seller)
        public Customer? Customer { get; set; }
        public Seller? Seller { get; set; }
    }
}
