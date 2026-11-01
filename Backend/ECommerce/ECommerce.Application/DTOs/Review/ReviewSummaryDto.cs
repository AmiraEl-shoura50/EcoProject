namespace ECommerce.Application.DTOs.Review;

public class ReviewSummaryDto
{
    public double Average { get; set; }
    public int TotalCount { get; set; }

    // 5 عناصر: Distribution[0] = عدد تقييمات نجمة واحدة ... Distribution[4] = عدد تقييمات 5 نجوم
    public int[] Distribution { get; set; } = new int[5];
}