using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using RentManage.Models;


namespace RentManage.Services;

public class ReceiptService
{
    public async Task<string> GenerateReceiptPdfAsync(Payment payment)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Receipt_{payment.Id}";

        var page = document.AddPage();
        page.Width = XUnit.FromMillimeter(80); // Receipt paper width
        page.Height = XUnit.FromMillimeter(120);

        using var gfx = XGraphics.FromPdfPage(page);

        // Fonts
        var titleFont = new XFont("Arial", 12, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 9, XFontStyle.Bold);
        var bodyFont = new XFont("Arial", 8, XFontStyle.Regular);

        // Header
        gfx.DrawString("RENT PAYMENT RECEIPT", titleFont, XBrushes.DarkBlue, new XRect(0, 10, page.Width, 20), XStringFormats.TopCenter);
        gfx.DrawLine(XPens.LightGray, 10, 32, page.Width - 10, 32);

        // Receipt Details
        int y = 40;
        int lineSpacing = 14;

        gfx.DrawString($"Receipt No: #{payment.Id:D5}", headerFont, XBrushes.Black, 10, y);
        y += lineSpacing;
        gfx.DrawString($"Date: {payment.PaymentDate:yyyy-MM-dd HH:mm}", bodyFont, XBrushes.DarkGray, 10, y);
        y += lineSpacing + 4;

        gfx.DrawString($"Tenant: {payment.TenantName}", bodyFont, XBrushes.Black, 10, y);
        y += lineSpacing;
        gfx.DrawString($"Unit: {payment.UnitNumber}", bodyFont, XBrushes.Black, 10, y);
        y += lineSpacing;
        gfx.DrawString($"Payment Method: {payment.PaymentMethod}", bodyFont, XBrushes.Black, 10, y);
        y += lineSpacing + 6;

        gfx.DrawLine(XPens.LightGray, 10, y, page.Width - 10, y);
        y += 8;

        // Total Amount
        gfx.DrawString("AMOUNT PAID:", headerFont, XBrushes.Black, 10, y);
        gfx.DrawString($"${payment.AmountPaid:F2}", titleFont, XBrushes.Green, page.Width - 70, y);
        y += lineSpacing + 10;

        gfx.DrawString("Thank you for your payment!", bodyFont, XBrushes.Gray, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);

        // Save PDF to App Storage
        string fileName = $"Receipt_{payment.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        document.Save(filePath);
        return filePath;
    }
}