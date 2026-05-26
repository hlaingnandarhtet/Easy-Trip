using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace DotNet8.EasyTrip.App.Client.Services
{
    public sealed class ReportExportService
    {
        private readonly IJSRuntime _js;

        public ReportExportService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task DownloadExcelAsync(string title, string fileName, IEnumerable<string[]> rows)
        {
            var html = BuildExcelHtml(title, rows);
            await DownloadAsync($"{fileName}.xls", "application/vnd.ms-excel", Encoding.UTF8.GetBytes(html));
        }

        public async Task DownloadPdfAsync(string title, string fileName, IEnumerable<string> summaryLines, IEnumerable<string[]> rows)
        {
            var pdf = BuildSimplePdf(title, summaryLines, rows);
            await DownloadAsync($"{fileName}.pdf", "application/pdf", pdf);
        }

        public static string Money(decimal value) => $"{value:N0} mmk";

        public static string Money(double value) => $"{value:N0} mmk";

        private async Task DownloadAsync(string fileName, string contentType, byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("easyTripDownloads.downloadBase64File", fileName, contentType, base64);
        }

        private static string BuildExcelHtml(string title, IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset=\"utf-8\"></head><body>");
            sb.Append("<h2>").Append(Html(title)).AppendLine("</h2>");
            sb.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"6\">");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                foreach (var cell in row)
                    sb.Append("<td>").Append(Html(cell)).AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private static byte[] BuildSimplePdf(string title, IEnumerable<string> summaryLines, IEnumerable<string[]> rows)
        {
            var textLines = new List<string> { title, string.Empty };
            textLines.AddRange(summaryLines);
            textLines.Add(string.Empty);
            textLines.AddRange(rows.Select(r => string.Join(" | ", r)));

            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 18 Tf");
            content.AppendLine($"50 800 Td ({Pdf(title)}) Tj");
            content.AppendLine("/F1 10 Tf");

            var yMove = -24;
            foreach (var line in textLines.Skip(1).Take(55))
            {
                content.AppendLine($"0 {yMove.ToString(CultureInfo.InvariantCulture)} Td ({Pdf(line)}) Tj");
                yMove = -15;
            }

            content.AppendLine("ET");
            var contentText = content.ToString();

            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(contentText)} >>\nstream\n{contentText}endstream"
            };

            var pdf = new StringBuilder();
            var offsets = new List<int> { 0 };
            pdf.AppendLine("%PDF-1.4");

            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.AppendLine($"{i + 1} 0 obj");
                pdf.AppendLine(objects[i]);
                pdf.AppendLine("endobj");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.AppendLine("xref");
            pdf.AppendLine($"0 {objects.Count + 1}");
            pdf.AppendLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
                pdf.AppendLine($"{offset:0000000000} 00000 n ");

            pdf.AppendLine("trailer");
            pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            pdf.AppendLine("startxref");
            pdf.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
            pdf.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private static string Html(string value) =>
            (value ?? string.Empty)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");

        private static string Pdf(string value) =>
            (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\r", " ")
                .Replace("\n", " ");
    }
}
