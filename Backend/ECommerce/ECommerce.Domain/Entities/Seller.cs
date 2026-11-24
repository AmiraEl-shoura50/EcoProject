using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Seller
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string StoreName { get; set; } = string.Empty;
        public float Rating { get; set; }
        public decimal? CommissionPercent { get; set; }   // null = لسه متحددتش (التسوية ممنوعة)
        public PayoutMethod? PayoutMethod { get; set; }
        public string? PayoutAccount { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
