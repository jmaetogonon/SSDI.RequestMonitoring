using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using SSDI.RequestMonitoring.UI.Models.DTO;
using System.Drawing;

namespace SSDI.RequestMonitoring.UI.Helpers.Export;

public class ExportRequest
{
    public async Task<byte[]> Export(
        List<RequestExportRow> data,
        string statusFilter = "All",
        string priorityFilter = "All",
        string type = "Purchase",
        IProgress<int>? progress = null)
    {
        ExcelPackage.License.SetNonCommercialPersonal("SSDI");

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add($"{type} Requests");

        // ======================
        // DOCUMENT PROPERTIES
        // ======================
        package.Workbook.Properties.Title = $"{type} Requests Report";
        package.Workbook.Properties.Author = "SSDI";
        package.Workbook.Properties.Subject = "System Requests";
        package.Workbook.Properties.Created = DateTime.Now;

        const int totalColumns = 7;

        // ======================
        // ROW 1 — REPORT TITLE
        // ======================
        ws.Cells[1, 1, 1, totalColumns].Merge = true;
        ws.Cells[1, 1].Value = $"{type.ToUpper()} REQUESTS REPORT";
        ws.Cells[1, 1].Style.Font.Size = 16;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Row(1).Height = 32;

        // ======================
        // ROW 2 — STATUS
        // ======================
        ws.Cells[2, 1].Value = "Status:";
        ws.Cells[2, 2].Value = statusFilter;
        ws.Cells[2, 2, 2, 3].Merge = true;

        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1, 2, totalColumns].Style.Font.Size = 10;

        // ======================
        // ROW 3 — PRIORITY
        // ======================
        ws.Cells[3, 1].Value = "Priority:";
        ws.Cells[3, 2].Value = priorityFilter;
        ws.Cells[3, 2, 3, 3].Merge = true;

        ws.Cells[3, 1].Style.Font.Bold = true;
        ws.Cells[3, 1, 3, totalColumns].Style.Font.Size = 10;

        // ======================
        // ROW 4 — SPACER
        // ======================
        ws.Row(4).Height = 10;

        // ======================
        // ROW 5 — TABLE HEADERS
        // ======================
        int headerRow = 5;

        string[] headers =
        {
            "Request No",
            "Requested By",
            "Nature Of Request",
            "Division / Department",
            "Priority",
            "Status",
            "Date Requested"
        };

        for (int col = 0; col < headers.Length; col++)
        {
            var cell = ws.Cells[headerRow, col + 1];
            cell.Value = headers[col];

            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(64, 64, 64));

            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        ws.Row(headerRow).Height = 22;

        // ======================
        // DATA ROWS
        // ======================
        for (int i = 0; i < data.Count; i++)
        {
            int rowIndex = headerRow + 1 + i;
            var r = data[i];

            ws.Cells[rowIndex, 1].Value = r.RequestNo;
            ws.Cells[rowIndex, 2].Value = r.RequestedBy;
            ws.Cells[rowIndex, 3].Value = r.NatureOfRequest;
            ws.Cells[rowIndex, 4].Value = r.DivisionDepartment;
            ws.Cells[rowIndex, 5].Value = r.Priority;
            ws.Cells[rowIndex, 6].Value = r.Status;
            ws.Cells[rowIndex, 7].Value = r.DateRequested;

            ws.Cells[rowIndex, 1, rowIndex, totalColumns]
                .Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

            progress?.Report((i + 1) * 100 / data.Count);
            await Task.Yield();
        }

        // ======================
        // FINAL FORMATTING
        // ======================
        ws.Cells[ws.Dimension.Address].AutoFitColumns();

        // Freeze table header
        ws.View.FreezePanes(headerRow + 1, 1);

        // Print settings
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.RepeatRows = ws.Cells[$"{headerRow}:{headerRow}"];

        return package.GetAsByteArray();
    }
}
