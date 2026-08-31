using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using FinancialManagement.Api.Data;
using FinancialManagement.Api.Exceptions;
using FinancialManagement.Api.Models;
using FinancialManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinancialManagement.Api.Services.Impl;

public class ExportService : IExportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        AppDbContext context,
        ILogger<ExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> ExportTransactionsCsvAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId)
    {
        _logger.LogInformation("Membuat export CSV transaksi untuk UserId {UserId}", userId);

        var transactions = await GetFilteredTransactionsAsync(userId, startDate, endDate, type, categoryId, walletId);

        var sb = new StringBuilder();
        // CSV Header
        sb.AppendLine("ID,Tanggal,Tipe,Kategori,Dompet,Nominal,Deskripsi,Bukti_Nota");

        var idCulture = new CultureInfo("id-ID");

        foreach (var t in transactions)
        {
            var id = t.Id;
            var date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm");
            var txType = t.Type;
            var category = EscapeCsv(t.Category?.Name ?? "-");
            var wallet = EscapeCsv(t.Wallet?.Name ?? "-");
            var amount = t.Amount.ToString("0.00", CultureInfo.InvariantCulture);
            var desc = EscapeCsv(t.Description ?? "");
            var receipt = EscapeCsv(t.ReceiptUrl ?? "");

            sb.AppendLine($"{id},{date},{txType},{category},{wallet},{amount},{desc},{receipt}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportTransactionsExcelAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId)
    {
        _logger.LogInformation("Membuat export Excel transaksi untuk UserId {UserId}", userId);

        var transactions = await GetFilteredTransactionsAsync(userId, startDate, endDate, type, categoryId, walletId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Riwayat Transaksi");

        // Title
        worksheet.Cell(1, 1).Value = "LAPORAN RIWAYAT TRANSAKSI KEUANGAN";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 7).Merge();

        var filterInfo = $"Periode: {(startDate.HasValue ? startDate.Value.ToString("dd/MM/yyyy") : "Semua")} s/d {(endDate.HasValue ? endDate.Value.ToString("dd/MM/yyyy") : "Sekarang")}";
        worksheet.Cell(2, 1).Value = filterInfo;
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Range(2, 1, 2, 7).Merge();

        // Table Headers
        var headers = new[] { "ID", "Tanggal", "Tipe", "Kategori", "Dompet", "Nominal (IDR)", "Deskripsi" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int row = 5;
        decimal totalIncome = 0;
        decimal totalExpense = 0;

        foreach (var t in transactions)
        {
            worksheet.Cell(row, 1).Value = t.Id;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(row, 2).Value = t.TransactionDate.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var typeCell = worksheet.Cell(row, 3);
            typeCell.Value = t.Type;
            typeCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            if (t.Type == "Income")
            {
                typeCell.Style.Font.FontColor = XLColor.FromHtml("#10B981");
                totalIncome += t.Amount;
            }
            else
            {
                typeCell.Style.Font.FontColor = XLColor.FromHtml("#EF4444");
                totalExpense += t.Amount;
            }

            worksheet.Cell(row, 4).Value = t.Category?.Name ?? "-";
            worksheet.Cell(row, 5).Value = t.Wallet?.Name ?? "-";

            var amountCell = worksheet.Cell(row, 6);
            amountCell.Value = t.Amount;
            amountCell.Style.NumberFormat.Format = "#,##0";
            amountCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, 7).Value = t.Description ?? "-";

            // Row striping
            if (row % 2 == 0)
            {
                worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
            }

            row++;
        }

        // Summary Rows
        row++;
        worksheet.Cell(row, 5).Value = "Total Pemasukan:";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        var incTotalCell = worksheet.Cell(row, 6);
        incTotalCell.Value = totalIncome;
        incTotalCell.Style.Font.Bold = true;
        incTotalCell.Style.Font.FontColor = XLColor.FromHtml("#10B981");
        incTotalCell.Style.NumberFormat.Format = "#,##0";

        row++;
        worksheet.Cell(row, 5).Value = "Total Pengeluaran:";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        var expTotalCell = worksheet.Cell(row, 6);
        expTotalCell.Value = totalExpense;
        expTotalCell.Style.Font.Bold = true;
        expTotalCell.Style.Font.FontColor = XLColor.FromHtml("#EF4444");
        expTotalCell.Style.NumberFormat.Format = "#,##0";

        row++;
        worksheet.Cell(row, 5).Value = "Selisih Bersih (Net):";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        var netCell = worksheet.Cell(row, 6);
        netCell.Value = totalIncome - totalExpense;
        netCell.Style.Font.Bold = true;
        netCell.Style.NumberFormat.Format = "#,##0";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportTransactionsPdfAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId)
    {
        _logger.LogInformation("Membuat export PDF transaksi untuk UserId {UserId}", userId);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        var transactions = await GetFilteredTransactionsAsync(userId, startDate, endDate, type, categoryId, walletId);

        decimal totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
        decimal totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
        decimal netAmount = totalIncome - totalExpense;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);

                void ComposeHeader(IContainer headerContainer)
                {
                    headerContainer.Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RIWAYAT TRANSAKSI KEUANGAN")
                                    .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                                col.Item().Text($"Pengguna: {user?.FullName ?? "User"} ({user?.Email})")
                                    .FontSize(10).FontColor(Colors.Grey.Darken1);
                                col.Item().Text($"Periode: {(startDate.HasValue ? startDate.Value.ToString("dd MMM yyyy") : "Awal")} - {(endDate.HasValue ? endDate.Value.ToString("dd MMM yyyy") : "Sekarang")}")
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                            });

                            row.ConstantItem(120).Column(col =>
                            {
                                col.Item().AlignRight().Text("Financial App").FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                                col.Item().AlignRight().Text($"Dicetak: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });

                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });
                }

                void ComposeContent(IContainer contentContainer)
                {
                    contentContainer.PaddingTop(10).Column(column =>
                    {
                        // Summary Cards
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text("Total Pemasukan").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Rp {totalIncome:N0}").FontSize(12).Bold().FontColor(Colors.Green.Medium);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text("Total Pengeluaran").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Rp {totalExpense:N0}").FontSize(12).Bold().FontColor(Colors.Red.Medium);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text("Selisih Bersih (Net)").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Rp {netAmount:N0}").FontSize(12).Bold().FontColor(netAmount >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            });
                        });

                        column.Item().PaddingTop(15);

                        // Transactions Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);  // No
                                columns.ConstantColumn(80);  // Tanggal
                                columns.ConstantColumn(55);  // Tipe
                                columns.RelativeColumn(2);   // Kategori
                                columns.RelativeColumn(2);   // Dompet
                                columns.RelativeColumn(2);   // Nominal
                                columns.RelativeColumn(3);   // Deskripsi
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("#").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Tanggal").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Tipe").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Kategori").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Dompet").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Nominal").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Deskripsi").FontColor(Colors.White).Bold();
                            });

                            int idx = 1;
                            foreach (var t in transactions)
                            {
                                var bgColor = idx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bgColor).Padding(4).Text(idx.ToString());
                                table.Cell().Background(bgColor).Padding(4).Text(t.TransactionDate.ToString("dd/MM/yy HH:mm"));

                                var typeColor = t.Type == "Income" ? Colors.Green.Medium : Colors.Red.Medium;
                                table.Cell().Background(bgColor).Padding(4).Text(t.Type).FontColor(typeColor).Bold();

                                table.Cell().Background(bgColor).Padding(4).Text(t.Category?.Name ?? "-");
                                table.Cell().Background(bgColor).Padding(4).Text(t.Wallet?.Name ?? "-");
                                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"Rp {t.Amount:N0}").FontColor(typeColor);
                                table.Cell().Background(bgColor).Padding(4).Text(t.Description ?? "-");

                                idx++;
                            }
                        });
                    });
                }

                void ComposeFooter(IContainer footerContainer)
                {
                    footerContainer.Row(row =>
                    {
                        row.RelativeItem().Text("Dokumen ini dibuat otomatis oleh Financial Management API.").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Halaman ").FontSize(8).FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            text.Span(" dari ").FontSize(8).FontColor(Colors.Grey.Medium);
                            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                }
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportFinancialReportPdfAsync(
        int userId,
        int? month,
        int? year,
        int? walletId,
        DateTime? startDate,
        DateTime? endDate)
    {
        _logger.LogInformation("Membuat export PDF laporan keuangan bulanan untuk UserId {UserId}", userId);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        DateTime reportStart = startDate ?? new DateTime(targetYear, targetMonth, 1);
        DateTime reportEnd = endDate ?? reportStart.AddMonths(1).AddTicks(-1);

        var query = _context.Transactions.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Wallet)
            .Where(t => t.UserId == userId && t.TransactionDate >= reportStart && t.TransactionDate <= reportEnd);

        if (walletId.HasValue && walletId.Value > 0)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        var transactions = await query.OrderByDescending(t => t.Amount).ToListAsync();

        var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
        var netSavings = totalIncome - totalExpense;
        var savingsRate = totalIncome > 0 ? (netSavings / totalIncome) * 100 : 0;

        var expenseByCategory = transactions
            .Where(t => t.Type == "Expense")
            .GroupBy(t => t.Category?.Name ?? "Lain-lain")
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount),
                Count = g.Count(),
                Percentage = totalExpense > 0 ? (g.Sum(x => x.Amount) / totalExpense) * 100 : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var incomeByCategory = transactions
            .Where(t => t.Type == "Income")
            .GroupBy(t => t.Category?.Name ?? "Lain-lain")
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount),
                Count = g.Count(),
                Percentage = totalIncome > 0 ? (g.Sum(x => x.Amount) / totalIncome) * 100 : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var topExpenses = transactions.Where(t => t.Type == "Expense").Take(5).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LAPORAN KEUANGAN BULANAN").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().Text($"Periode: {reportStart:dd MMMM yyyy} - {reportEnd:dd MMMM yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"Pengguna: {user?.FullName ?? "User"} ({user?.Email})").FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        r.ConstantItem(120).Column(c =>
                        {
                            c.Item().AlignRight().Text("Financial Report").FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                            c.Item().AlignRight().Text($"Generated: {DateTime.Now:dd/MM/yyyy}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Ringkasan Keuangan (Executive Summary)
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Green.Lighten3).Background(Colors.Green.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("TOTAL PEMASUKAN").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text($"Rp {totalIncome:N0}").FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Red.Lighten3).Background(Colors.Red.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("TOTAL PENGELUARAN").FontSize(8).Bold().FontColor(Colors.Red.Darken2);
                            c.Item().Text($"Rp {totalExpense:N0}").FontSize(14).Bold().FontColor(Colors.Red.Darken2);
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("TABUNGAN BERSIH / NET").FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Rp {netSavings:N0}").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Savings Rate: {savingsRate:F1}%").FontSize(8).FontColor(Colors.Blue.Darken1);
                        });
                    });

                    col.Item().PaddingTop(15);

                    // Category Breakdown Table
                    col.Item().Text("Rincian Pengeluaran per Kategori").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);  // Kategori
                            columns.ConstantColumn(60); // Jml Tx
                            columns.RelativeColumn(2);  // Nominal
                            columns.ConstantColumn(80); // Persentase
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Kategori").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignCenter().Text("Frekuensi").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Total Nominal").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Porsi").FontColor(Colors.White).Bold();
                        });

                        int i = 0;
                        foreach (var cat in expenseByCategory)
                        {
                            var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(cat.Category);
                            table.Cell().Background(bg).Padding(4).AlignCenter().Text($"{cat.Count}x");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"Rp {cat.Total:N0}");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"{cat.Percentage:F1}%").Bold();
                            i++;
                        }

                        if (!expenseByCategory.Any())
                        {
                            table.Cell().ColumnSpan(4).Padding(8).AlignCenter().Text("Tidak ada transaksi pengeluaran pada periode ini.").FontColor(Colors.Grey.Medium);
                        }
                    });

                    col.Item().PaddingTop(15);

                    // Top 5 Expenses
                    col.Item().Text("5 Pengeluaran Terbesar").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80); // Tanggal
                            columns.RelativeColumn(2);  // Kategori
                            columns.RelativeColumn(3);  // Deskripsi
                            columns.RelativeColumn(2);  // Nominal
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Tanggal").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Kategori").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Deskripsi").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Grey.Darken3).Padding(5).AlignRight().Text("Nominal").FontColor(Colors.White).Bold();
                        });

                        int j = 0;
                        foreach (var tx in topExpenses)
                        {
                            var bg = j % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(tx.TransactionDate.ToString("dd/MM/yyyy"));
                            table.Cell().Background(bg).Padding(4).Text(tx.Category?.Name ?? "-");
                            table.Cell().Background(bg).Padding(4).Text(tx.Description ?? "-");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"Rp {tx.Amount:N0}").FontColor(Colors.Red.Medium).Bold();
                            j++;
                        }

                        if (!topExpenses.Any())
                        {
                            table.Cell().ColumnSpan(4).Padding(8).AlignCenter().Text("Tidak ada data transaksi.").FontColor(Colors.Grey.Medium);
                        }
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("Financial Management Report — Laporan Resmi").FontSize(8).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Halaman ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task<List<Transaction>> GetFilteredTransactionsAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        int? categoryId,
        int? walletId)
    {
        var query = _context.Transactions.AsNoTracking()
            .Include(t => t.Wallet)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (walletId.HasValue && walletId.Value > 0)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        if (startDate.HasValue)
        {
            var startOfDay = startDate.Value.Date;
            query = query.Where(t => t.TransactionDate >= startOfDay);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endOfDay);
        }

        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
