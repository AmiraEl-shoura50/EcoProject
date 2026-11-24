using ClosedXML.Excel;
using ECommerce.Application.DTOs.Settlement;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class ExcelReportService : IExcelReportService
{
    private const string MoneyFormat = "#,##0.00";
    private const string DateTimeFormat = "yyyy-mm-dd hh:mm";
    private static readonly XLColor HeaderFill = XLColor.FromHtml("#1A1A1A");
    private static readonly XLColor HeaderFont = XLColor.FromHtml("#C9A24B");

    public byte[] BuildSettlementReport(SettlementReportData d)
    {
        using var workbook = new XLWorkbook();

        // ---------- الملخص ----------
        var summary = workbook.Worksheets.Add("الملخص");
        summary.RightToLeft = true;

        var line = 1;
        void Pair(string label, object? value)
        {
            summary.Cell(line, 1).Value = label;
            summary.Cell(line, 1).Style.Font.Bold = true;
            SetCell(summary.Cell(line, 2), value);
            line++;
        }

        Pair("رقم التسوية", d.SettlementId);
        Pair("المتجر", d.StoreName);
        Pair("صاحب المتجر", d.OwnerName);
        Pair("البريد الإلكتروني", d.OwnerEmail);
        Pair("الهاتف", d.OwnerPhone ?? "-");
        Pair("طريقة التحويل", PayoutLabel(d.PayoutMethod));
        Pair("حساب التحويل", d.PayoutAccount);
        Pair("رقم عملية التحويل", d.Reference ?? "-");
        Pair("تاريخ التحويل", d.CreatedLocal);
        Pair("الطلبات المتوصّلة حتى يوم", d.CutoffDay.ToString("yyyy-MM-dd"));
        line++;
        Pair("عدد الطلبات", d.OrdersCount);
        Pair("إجمالي المبيعات", d.GrossAmount);
        Pair($"عمولة الموقع ({d.CommissionPercent:0.##}%)", d.CommissionAmount);
        Pair("تعديل يدوي", d.AdjustmentAmount);
        Pair("سبب التعديل", d.AdjustmentNote ?? "-");
        Pair("الصافي المحوَّل للبائع", d.NetAmount);

        summary.Cell(line - 1, 1).Style.Font.FontSize = 13;
        summary.Cell(line - 1, 2).Style.Font.Bold = true;
        summary.Cell(line - 1, 2).Style.Font.FontSize = 13;
        summary.Columns().AdjustToContents();

        // ---------- الطلبات ----------
        var orders = workbook.Worksheets.Add("الطلبات");
        orders.RightToLeft = true;

        WriteTable(
            orders,
            new[] { "رقم الطلب", "تاريخ الطلب", "تاريخ التوصيل", "طريقة الدفع", "عدد القطع", "إجمالي الطلب", "العمولة", "الصافي" },
            d.Orders.Select(o => new object?[]
            {
                o.OrderId, o.CreatedLocal, o.DeliveredLocal, o.PaymentMethod, o.ItemsCount, o.Amount, o.Commission, o.Net
            }));

        var totalRow = d.Orders.Count + 2;
        orders.Cell(totalRow, 1).Value = "الإجمالي";
        SetCell(orders.Cell(totalRow, 6), d.Orders.Sum(o => o.Amount));
        SetCell(orders.Cell(totalRow, 7), d.Orders.Sum(o => o.Commission));
        SetCell(orders.Cell(totalRow, 8), d.Orders.Sum(o => o.Net));
        orders.Range(totalRow, 1, totalRow, 8).Style.Font.Bold = true;
        orders.Columns().AdjustToContents();

        // ---------- المنتجات ----------
        var items = workbook.Worksheets.Add("المنتجات");
        items.RightToLeft = true;

        WriteTable(
            items,
            new[] { "رقم الطلب", "المنتج", "الكمية", "سعر القطعة", "الإجمالي" },
            d.Items.Select(i => new object?[] { i.OrderId, i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal }));
        items.Columns().AdjustToContents();

        return Save(workbook);
    }

    public byte[] BuildPeriodReport(PeriodReportData d)
    {
        using var workbook = new XLWorkbook();

        // ---------- ملخص البائعين ----------
        var sellers = workbook.Worksheets.Add("ملخص البائعين");
        sellers.RightToLeft = true;

        sellers.Cell(1, 1).Value = $"تقرير الطلبات المتوصّلة من {d.From:yyyy-MM-dd} إلى {d.To:yyyy-MM-dd}";
        sellers.Cell(1, 1).Style.Font.Bold = true;
        sellers.Cell(1, 1).Style.Font.FontSize = 13;

        WriteTable(
            sellers,
            new[]
            {
                "المتجر", "صاحب المتجر", "البريد الإلكتروني", "الهاتف", "طريقة التحويل", "حساب التحويل",
                "نسبة العمولة %", "عدد الطلبات", "إجمالي المبيعات", "العمولة", "الصافي", "الصافي غير المحوَّل", "ملاحظة"
            },
            d.Sellers.Select(s => new object?[]
            {
                s.StoreName, s.OwnerName, s.OwnerEmail, s.OwnerPhone ?? "-", PayoutLabel(s.PayoutMethod), s.PayoutAccount,
                s.CommissionPercent.HasValue ? s.CommissionPercent.Value.ToString("0.##") : "غير محددة",
                s.OrdersCount, s.GrossAmount, s.CommissionAmount, s.NetAmount, s.UnsettledNet, s.Note
            }),
            startRow: 3);

        var totalRow = d.Sellers.Count + 4;
        sellers.Cell(totalRow, 1).Value = "الإجمالي";
        SetCell(sellers.Cell(totalRow, 8), d.Sellers.Sum(s => s.OrdersCount));
        SetCell(sellers.Cell(totalRow, 9), d.Sellers.Sum(s => s.GrossAmount));
        SetCell(sellers.Cell(totalRow, 10), d.Sellers.Sum(s => s.CommissionAmount));
        SetCell(sellers.Cell(totalRow, 11), d.Sellers.Sum(s => s.NetAmount));
        SetCell(sellers.Cell(totalRow, 12), d.Sellers.Sum(s => s.UnsettledNet));
        sellers.Range(totalRow, 1, totalRow, 13).Style.Font.Bold = true;
        sellers.Columns().AdjustToContents();

        // ---------- الطلبات ----------
        var orders = workbook.Worksheets.Add("الطلبات");
        orders.RightToLeft = true;

        WriteTable(
            orders,
            new[] { "رقم الطلب", "المتجر", "تاريخ الطلب", "تاريخ التوصيل", "طريقة الدفع", "عدد القطع", "إجمالي الطلب", "نسبة العمولة %", "العمولة", "الصافي", "حالة التسوية" },
            d.Orders.Select(o => new object?[]
            {
                o.OrderId, o.StoreName, o.CreatedLocal, o.DeliveredLocal, o.PaymentMethod, o.ItemsCount, o.Amount,
                o.CommissionPercent.HasValue ? o.CommissionPercent.Value.ToString("0.##") : "غير محددة",
                o.Commission, o.Net, o.Status
            }));
        orders.Columns().AdjustToContents();

        return Save(workbook);
    }

    // ---------- مساعدات ----------

    private static void WriteTable(IXLWorksheet sheet, string[] headers, IEnumerable<object?[]> rows, int startRow = 1)
    {
        for (var c = 0; c < headers.Length; c++)
            sheet.Cell(startRow, c + 1).Value = headers[c];

        var header = sheet.Range(startRow, 1, startRow, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = HeaderFont;
        header.Style.Fill.BackgroundColor = HeaderFill;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var r = startRow + 1;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Length; c++)
                SetCell(sheet.Cell(r, c + 1), row[c]);
            r++;
        }

        sheet.SheetView.FreezeRows(startRow);
    }

    private static void SetCell(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case string text:
                cell.Value = Safe(text);
                break;
            case decimal money:
                cell.Value = money;
                cell.Style.NumberFormat.Format = MoneyFormat;
                break;
            case int number:
                cell.Value = number;
                break;
            case DateTime date:
                cell.Value = date;
                cell.Style.DateFormat.Format = DateTimeFormat;
                break;
            default:
                cell.Value = Safe(value.ToString() ?? string.Empty);
                break;
        }
    }

    // حماية من حقن الصيغ: أي نص بيبدأ بـ = + - @ بنحط قبله علامة ' عشان Excel ميعتبرهوش معادلة
    private static string Safe(string text)
        => text.Length > 0 && "=+-@".Contains(text[0]) ? "'" + text : text;

    private static string PayoutLabel(string method) => method switch
    {
        "VodafoneCash" => "فودافون كاش",
        "InstaPay" => "إنستاباي",
        "BankAccount" => "حساب بنكي",
        _ => "غير محدد"
    };

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}