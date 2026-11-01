namespace ECommerce.Application.Pricing;

// بيانات خفيفة للمنتج تكفي لحساب سعره بعد الخصم من غير ما نحمّل المنتج كله
public record ProductPricingInfo(int Id, int CategoryId, int SellerId, decimal Price, float Rating);