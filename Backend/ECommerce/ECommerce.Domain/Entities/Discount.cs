using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Discount
    {
        public int Id { get; set; }

        public int DiscountTargetId { get; set; }
        public DiscountTarget DiscountTarget { get; set; } = null!;

        public DiscountType DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
