using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // ✅ جديد: البائع اللي أنشأ القسم (null لأقسام الأدمن)
        public int? CreatedBySellerId { get; set; }
        public bool IsHiddenBySeller { get; set; }   // أخفاه صاحبه
        public bool IsHiddenByAdmin { get; set; }    // أخفته الإدارة (البائع مش بيقدر يلغيه)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
