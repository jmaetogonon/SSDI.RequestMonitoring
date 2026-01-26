using OfficeOpenXml;
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
        byte[]? bannerBytes = null,
        IProgress<int>? progress = null)
    {
        ExcelPackage.License.SetNonCommercialPersonal("SSDI");

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add($"{type} Requests");

        // ======================
        // DOCUMENT PROPERTIES
        // ======================
        package.Workbook.Properties.Title = $"{type} Request List";
        package.Workbook.Properties.Author = "SSDI";
        package.Workbook.Properties.Subject = "System Requests";
        package.Workbook.Properties.Created = DateTime.Now;

        const int totalColumns = 7;
        int currentRow = 1;

        // ======================
        // BANNER SECTION
        // ======================
        try
        {
            if (bannerBytes != null && bannerBytes.Length > 0)
            {
                using var bannerStream = new MemoryStream(bannerBytes);

                // Get banner dimensions
                int bannerWidth = 600; // Default width for banner
                int bannerHeight = 90; // Default height for banner

                // Add banner to worksheet
                var banner = ws.Drawings.AddPicture("CompanyBanner", bannerStream);

                // Position banner at the top, centered
                banner.SetPosition(0, 0, 0, 0);
                banner.SetSize(bannerWidth, bannerHeight);

                // Calculate how many rows the banner will occupy
                int bannerRows = (int)Math.Ceiling(bannerHeight / 15.0); // Approx 15 pixels per Excel row
                bannerRows = Math.Max(2, Math.Min(bannerRows, 3)); // Between 2-4 rows

                // Merge cells for banner area
                ws.Cells[currentRow, 1, currentRow + bannerRows - 1, 3].Merge = true;

                // Set row heights for banner
                for (int i = 0; i < bannerRows; i++)
                {
                    ws.Row(currentRow + i).Height = bannerHeight / bannerRows;
                }

                currentRow += bannerRows;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding banner: {ex.Message}");
        }

        // ======================
        // REPORT TITLE
        // ======================
        ws.Cells[currentRow, 1, currentRow, totalColumns].Merge = true;
        ws.Cells[currentRow, 1].Value = $"{type.ToUpper()} REQUEST LIST";
        ws.Cells[currentRow, 1].Style.Font.Size = 16;
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Row(currentRow).Height = 32;
        currentRow++;

        // ======================
        // GENERATED DATE
        // ======================
        ws.Cells[currentRow, 1, currentRow, totalColumns].Merge = true;
        ws.Cells[currentRow, 1].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
        ws.Cells[currentRow, 1].Style.Font.Size = 10;
        ws.Cells[currentRow, 1].Style.Font.Italic = true;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(currentRow).Height = 20;
        currentRow++;

        // ======================
        // FILTERS
        // ======================
        ws.Cells[currentRow, 1].Value = "Status Filter:";
        ws.Cells[currentRow, 2].Value = statusFilter;
        ws.Cells[currentRow, 2, currentRow, 3].Merge = true;

        ws.Cells[currentRow, 5].Value = "Priority Filter:";
        ws.Cells[currentRow, 6].Value = priorityFilter;
        ws.Cells[currentRow, 6, currentRow, 7].Merge = true;

        for (int col = 1; col <= totalColumns; col++)
        {
            ws.Cells[currentRow, col].Style.Font.Size = 10;
            if (col == 1 || col == 5)
            {
                ws.Cells[currentRow, col].Style.Font.Bold = true;
            }
        }
        ws.Row(currentRow).Height = 22;
        currentRow++;

        ws.Cells[currentRow, 1].Value = "Request Count:";
        ws.Cells[currentRow, 2].Value = data.Count;
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        ws.Cells[currentRow, 1, currentRow, 2].Style.Font.Size = 10;

        ws.Row(currentRow).Height = 22;
        currentRow++;

        // ======================
        // SPACER
        // ======================
        ws.Row(currentRow).Height = 10;
        currentRow++;

        // ======================
        // TABLE HEADERS
        // ======================
        int headerRow = currentRow;

        string[] headers =
        {
            "Series No",
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
            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(64, 64, 64));

            // FIX: Set border style first, then color
            cell.Style.Border.Top.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Medium;

            // Now set colors
            cell.Style.Border.Top.Color.SetColor(Color.Black);
            cell.Style.Border.Bottom.Color.SetColor(Color.Black);
            cell.Style.Border.Left.Color.SetColor(Color.Black);
            cell.Style.Border.Right.Color.SetColor(Color.Black);
        }

        ws.Row(headerRow).Height = 22;
        currentRow++;

        // ======================
        // DATA ROWS - SIMPLE BORDERS (NO STRIPES)
        // ======================
        for (int i = 0; i < data.Count; i++)
        {
            int rowIndex = currentRow;
            var r = data[i];

            ws.Cells[rowIndex, 1].Value = r.SeriesNo;
            ws.Cells[rowIndex, 2].Value = r.RequestedBy;
            ws.Cells[rowIndex, 3].Value = r.NatureOfRequest;
            ws.Cells[rowIndex, 4].Value = r.DivisionDepartment;
            ws.Cells[rowIndex, 5].Value = r.Priority;
            ws.Cells[rowIndex, 6].Value = r.Status;
            ws.Cells[rowIndex, 7].Value = r.DateRequested;

            // Add vertical alignment
            //ws.Cells[rowIndex, 1, rowIndex, totalColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            currentRow++;
            progress?.Report((i + 1) * 100 / data.Count);
            await Task.Yield();
        }

        // ======================
        // ADD BORDERS TO DATA AREA (CLEAN APPROACH)
        // ======================
        if (data.Count > 0)
        {
            int dataStartRow = headerRow + 1;
            int dataEndRow = headerRow + data.Count;

            // Create data range
            var dataRange = ws.Cells[dataStartRow, 1, dataEndRow, totalColumns];

            // Add outer border to entire data area
            dataRange.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

            // Add inner horizontal borders (between rows)
            for (int row = dataStartRow; row <= dataEndRow; row++)
            {
                ws.Cells[row, 1, row, totalColumns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // Add inner vertical borders (between columns)
            for (int col = 1; col <= totalColumns; col++)
            {
                ws.Cells[dataStartRow, col, dataEndRow, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Remove last vertical border (rightmost)
            ws.Cells[dataStartRow, totalColumns, dataEndRow, totalColumns].Style.Border.Right.Style = ExcelBorderStyle.None;
        }

        // ======================
        // FINAL FORMATTING
        // ======================
        ws.Cells[1, 1, currentRow - 1, totalColumns].AutoFitColumns();

        // Set column widths
        ws.Column(1).Width = Math.Max(ws.Column(1).Width, 15);
        ws.Column(2).Width = Math.Max(ws.Column(2).Width, 25);
        ws.Column(3).Width = Math.Max(ws.Column(3).Width, 35);
        ws.Column(4).Width = Math.Max(ws.Column(4).Width, 25);
        ws.Column(5).Width = Math.Max(ws.Column(5).Width, 15);
        ws.Column(6).Width = Math.Max(ws.Column(6).Width, 15);
        ws.Column(7).Width = Math.Max(ws.Column(7).Width, 15);

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