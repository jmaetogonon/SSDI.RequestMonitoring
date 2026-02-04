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

        // Updated column count: Added Business Unit (8) and Amount (9)
        const int totalColumns = 9;
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

        // Updated headers with Business Unit and Amount
        string[] headers =
        {
            "Series No",
            "Requested By",
            "Nature Of Request",
            "Division / Department",
            "Business Unit", // NEW COLUMN
            "Priority",
            "Status",
            "Amount", // NEW COLUMN
            "Date Requested"
        };

        for (int col = 0; col < headers.Length; col++)
        {
            var cell = ws.Cells[headerRow, col + 1];
            cell.Value = headers[col];

            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.Black);
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Dark gray background for headers
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(225, 233, 241));

            // Set border style
            cell.Style.Border.Top.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Medium;

            // Set border colors
            cell.Style.Border.Top.Color.SetColor(Color.Black);
            cell.Style.Border.Bottom.Color.SetColor(Color.Black);
            cell.Style.Border.Left.Color.SetColor(Color.Black);
            cell.Style.Border.Right.Color.SetColor(Color.Black);
        }

        ws.Row(headerRow).Height = 22;
        currentRow++;

        // ======================
        // DATA ROWS
        // ======================
        decimal totalAmount = 0;

        for (int i = 0; i < data.Count; i++)
        {
            int rowIndex = currentRow;
            var r = data[i];

            ws.Cells[rowIndex, 1].Value = r.SeriesNo;
            ws.Cells[rowIndex, 2].Value = r.RequestedBy;
            ws.Cells[rowIndex, 3].Value = r.NatureOfRequest;
            ws.Cells[rowIndex, 4].Value = r.DivisionDepartment;
            ws.Cells[rowIndex, 5].Value = r.BusinessUnit; // NEW: Business Unit
            ws.Cells[rowIndex, 6].Value = r.Priority;
            ws.Cells[rowIndex, 7].Value = r.Status;

            // Amount column (format as currency)
            if (r.TotalAmount.HasValue && r.TotalAmount > 0)
            {
                ws.Cells[rowIndex, 8].Value = r.TotalAmount.Value;
                ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0.00";
                totalAmount += r.TotalAmount.Value;
            }
            else
            {
                ws.Cells[rowIndex, 8].Value = "-";
                ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            ws.Cells[rowIndex, 9].Value = r.DateRequested;

            // Format date column
            ws.Cells[rowIndex, 9].Style.Numberformat.Format = "mmmm dd, yyyy";
            ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Center align for certain columns
            ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Series No
            ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Priority
            ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Status

            currentRow++;
            progress?.Report((i + 1) * 100 / data.Count);
            await Task.Yield();
        }

        // ======================
        // ADD BORDERS TO DATA AREA
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
        // GRAND TOTAL ROW
        // ======================
        if (totalAmount > 0)
        {
            int totalRow = currentRow + 1;

            // Merge cells for "Grand Total" label
            ws.Cells[totalRow, 1, totalRow, 7].Merge = true;
            ws.Cells[totalRow, 1].Value = "GRAND TOTAL";
            ws.Cells[totalRow, 1].Style.Font.Bold = true;
            ws.Cells[totalRow, 1].Style.Font.Size = 11;
            ws.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[totalRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Total amount cell
            ws.Cells[totalRow, 8].Value = totalAmount;
            ws.Cells[totalRow, 8].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[totalRow, 8].Style.Font.Bold = true;
            ws.Cells[totalRow, 8].Style.Font.Color.SetColor(Color.DarkGreen);
            ws.Cells[totalRow, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[totalRow, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(235, 247, 235)); // Light green

            // Formatting for total row
            ws.Cells[totalRow, 8].Style.Border.Top.Style = ExcelBorderStyle.Double;
            ws.Cells[totalRow, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
            ws.Cells[totalRow, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            ws.Cells[totalRow, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            ws.Cells[totalRow, 8].Style.Border.Top.Color.SetColor(Color.DarkGray);
            ws.Cells[totalRow, 8].Style.Border.Bottom.Color.SetColor(Color.DarkGray);

            // Empty cell for date column
            ws.Cells[totalRow, 9].Value = "";

            // Set row height for total row
            ws.Row(totalRow).Height = 22;
        }

        // ======================
        // FINAL FORMATTING
        // ======================
        ws.Cells[1, 1, currentRow - 1, totalColumns].AutoFitColumns();

        // Set column widths
        ws.Column(1).Width = Math.Max(ws.Column(1).Width, 15);  // Series No
        ws.Column(2).Width = Math.Max(ws.Column(2).Width, 25);  // Requested By
        ws.Column(3).Width = Math.Max(ws.Column(3).Width, 35);  // Nature Of Request
        ws.Column(4).Width = Math.Max(ws.Column(4).Width, 25);  // Division/Dept
        ws.Column(5).Width = Math.Max(ws.Column(5).Width, 20);  // Business Unit
        ws.Column(6).Width = Math.Max(ws.Column(6).Width, 15);  // Priority
        ws.Column(7).Width = Math.Max(ws.Column(7).Width, 15);  // Status
        ws.Column(8).Width = Math.Max(ws.Column(8).Width, 18);  // Amount
        ws.Column(9).Width = Math.Max(ws.Column(9).Width, 18);  // Date Requested

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