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
    public class CallsPdfReportService
    {
        public void GenerateCallsReport(string phoneNumber, DateTime from, DateTime to, IList<CallItem> calls, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Некорректный путь к файлу.", nameof(filePath));

            var dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                throw new DirectoryNotFoundException("Папка для сохранения PDF не найдена.");

            var ru = CultureInfo.GetCultureInfo("ru-RU");

            var doc = new Document();
            doc.Info.Title = "Детализация звонков";
            doc.Info.Subject = "Отчет по звонкам за период";
            doc.Info.Author = "SOPlus37";

            var style = doc.Styles["Normal"];
            style.Font.Name = "Segoe UI";
            style.Font.Size = 10;

            var section = doc.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            var title = section.AddParagraph("Детализация звонков");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            section.AddParagraph($"Абонент: {phoneNumber}");
            section.AddParagraph($"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}");
            section.AddParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
            section.AddParagraph().AddLineBreak();

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn(Unit.FromCentimeter(3.4));
            table.AddColumn(Unit.FromCentimeter(3.4));
            table.AddColumn(Unit.FromCentimeter(3.0));
            table.AddColumn(Unit.FromCentimeter(3.6));
            table.AddColumn(Unit.FromCentimeter(2.4));

            var header = table.AddRow();
            header.Shading.Color = Colors.LightGray;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Вызываемый абонент");
            header.Cells[1].AddParagraph("Дата и время");
            header.Cells[2].AddParagraph("Длительность (мин)");
            header.Cells[3].AddParagraph("Тип");
            header.Cells[4].AddParagraph("Стоимость звонка");

            foreach (var c in calls)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(c.CalledNumber);
                row.Cells[1].AddParagraph(c.CallDateTime.ToString("dd.MM.yyyy HH:mm", ru));
                row.Cells[2].AddParagraph(c.DurationMinutes.ToString(ru));
                row.Cells[3].AddParagraph(c.CallTypeText);
                row.Cells[4].AddParagraph(c.Cost.ToString("N2", ru) + " ₽");
            }

            section.AddParagraph().AddLineBreak();

            decimal sum = 0m;
            foreach (var c in calls) sum += c.Cost;

            var total = section.AddParagraph($"Итого звонков: {calls.Count}, на сумму: {sum.ToString("N2", ru)} ₽");
            total.Format.Font.Bold = true;

            section.AddParagraph().AddLineBreak();
            var copyright = section.AddParagraph("© 2025 SOPlus37. Все права защищены.");
            copyright.Format.Font.Size = 9;
            copyright.Format.Font.Color = Colors.Gray;
            copyright.Format.Alignment = ParagraphAlignment.Center;

            // Рендер
            var renderer = new PdfDocumentRenderer(unicode: true)
            {
                Document = doc
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }
    }
}
