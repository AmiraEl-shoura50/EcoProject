using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class DiscountTarget
    {
        public int Id { get; set; }
        public int TargetId { get; set; }   // ده ممكن يبقى Product.Id أو Category.Id
        public TargetType Target { get; set; }

        public Discount? Discount { get; set; }
    }

}
