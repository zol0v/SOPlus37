using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SmsPdfReportService
    {
        public void GenerateSmsReport(string phoneNumber, DateTime from, DateTime to, IList<SmsItem> sms, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Некорректный путь к файлу.", nameof(filePath));

            var dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                throw new DirectoryNotFoundException("Папка для сохранения PDF не найдена.");

            var ru = CultureInfo.GetCultureInfo("ru-RU");

            var doc = new Document();
            doc.Info.Title = "Детализация SMS";
            doc.Info.Subject = "Отчет по SMS за период";
            doc.Info.Author = "SOPlus37";

            var style = doc.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 10;

            var section = doc.AddSection();

            var title = section.AddParagraph("Детализация SMS");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            section.AddParagraph($"Абонент: {phoneNumber}");
            section.AddParagraph($"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}");
            section.AddParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
            section.AddParagraph().AddLineBreak();

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn(Unit.FromCentimeter(5.0));
            table.AddColumn(Unit.FromCentimeter(6.0));
            table.AddColumn(Unit.FromCentimeter(3.0));

            var header = table.AddRow();
            header.Shading.Color = Colors.LightGray;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Получатель");
            header.Cells[1].AddParagraph("Дата и время");
            header.Cells[2].AddParagraph("Стоимость");

            foreach (var s in sms)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(s.SmsedNumber);
                row.Cells[1].AddParagraph(s.SmsDateTime.ToString("dd.MM.yyyy HH:mm", ru));
                row.Cells[2].AddParagraph(s.Cost.ToString("N2", ru) + " ₽");
            }

            section.AddParagraph().AddLineBreak();

            decimal sum = 0m;
            foreach (var s in sms) sum += s.Cost;

            var total = section.AddParagraph($"Итого SMS: {sms.Count}, на сумму: {sum.ToString("N2", ru)} ₽");
            total.Format.Font.Bold = true;

            section.AddParagraph().AddLineBreak();
            var copyright = section.AddParagraph("© 2025 SOPlus37. Все права защищены.");
            copyright.Format.Font.Size = 9;
            copyright.Format.Font.Color = Colors.Gray;
            copyright.Format.Alignment = ParagraphAlignment.Center;

            var renderer = new PdfDocumentRenderer(unicode: true)
            {
                Document = doc
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }
    }
}
