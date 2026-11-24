using ECommerce.Application.DTOs.Settlement;

namespace ECommerce.Application.Interfaces.Services;

public interface IExcelReportService
{
    byte[] BuildSettlementReport(SettlementReportData data);
    byte[] BuildPeriodReport(PeriodReportData data);
}