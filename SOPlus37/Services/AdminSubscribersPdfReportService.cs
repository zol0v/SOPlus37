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
    public class AdminSubscribersPdfReportService
    {
        public void GenerateSubscribersReport(
            string statusFilterText,
            string clientTypeFilterText,
            string adminFullName,
            IList<AdminSubscriberItem> subscribers,
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Некорректный путь к файлу.", nameof(filePath));

            var dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                throw new DirectoryNotFoundException("Папка для сохранения PDF не найдена.");

            var ru = CultureInfo.GetCultureInfo("ru-RU");
            var count = subscribers?.Count ?? 0;

            var doc = new Document();
            doc.Info.Title = "Отчет по абонентам";
            doc.Info.Subject = "Список абонентов (с фильтрами)";

            var style = doc.Styles["Normal"];
            style.Font.Name = "Segoe UI";
            style.Font.Size = 9;

            var section = doc.AddSection();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.0);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.0);

            var title = section.AddParagraph("Отчет по абонентам");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            section.AddParagraph($"Фильтр по статусу: {statusFilterText ?? "—"}");
            section.AddParagraph($"Фильтр по типу: {clientTypeFilterText ?? "—"}");
            section.AddParagraph($"Количество абонентов в отчёте: {count}");
            section.AddParagraph($"Сформировал администратор: {(!string.IsNullOrWhiteSpace(adminFullName) ? adminFullName : "—")}");
            section.AddParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
            section.AddParagraph().AddLineBreak();

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn(Unit.FromCentimeter(3.2));
            table.AddColumn(Unit.FromCentimeter(2.2));
            table.AddColumn(Unit.FromCentimeter(2.3));
            table.AddColumn(Unit.FromCentimeter(6.0));
            table.AddColumn(Unit.FromCentimeter(3.2));
            table.AddColumn(Unit.FromCentimeter(2.2));
            table.AddColumn(Unit.FromCentimeter(3.7));
            table.AddColumn(Unit.FromCentimeter(1.6));
            table.AddColumn(Unit.FromCentimeter(1.6));
            table.AddColumn(Unit.FromCentimeter(1.6));

            var header = table.AddRow();
            header.Shading.Color = Colors.LightGray;
            header.Format.Font.Bold = true;
            header.Format.Font.Size = 9;

            header.Cells[0].AddParagraph("Номер");
            header.Cells[1].AddParagraph("Статус");
            header.Cells[2].AddParagraph("Тип");
            header.Cells[3].AddParagraph("ФИО/Орг.");
            header.Cells[4].AddParagraph("Паспорт/ОГРН");
            header.Cells[5].AddParagraph("Баланс");
            header.Cells[6].AddParagraph("Тариф");
            header.Cells[7].AddParagraph("Услуги");
            header.Cells[8].AddParagraph("Звонки");
            header.Cells[9].AddParagraph("SMS");

            if (subscribers != null)
            {
                foreach (var s in subscribers)
                {
                    var row = table.AddRow();
                    row.Format.Font.Size = 9;

                    row.Cells[0].AddParagraph(s.PhoneNumber ?? "");
                    row.Cells[1].AddParagraph(s.Status ?? "");
                    row.Cells[2].AddParagraph(s.ClientType ?? "");
                    row.Cells[3].AddParagraph(s.NameOrOrg ?? "");
                    row.Cells[4].AddParagraph(s.Doc ?? "");
                    row.Cells[5].AddParagraph(string.Format(ru, "{0:N2} ₽", s.Balance));
                    row.Cells[6].AddParagraph(s.TariffName ?? "");
                    row.Cells[7].AddParagraph(s.ServicesCount.ToString());
                    row.Cells[8].AddParagraph(s.CallsCount.ToString());
                    row.Cells[9].AddParagraph(s.SmsCount.ToString());
                }
            }

            section.AddParagraph().AddLineBreak();
            var copyright = section.AddParagraph("© 2025 SOPlus37. Все права защищены.");
            copyright.Format.Font.Size = 9;
            copyright.Format.Font.Color = Colors.Gray;
            copyright.Format.Alignment = ParagraphAlignment.Center;

            var renderer = new PdfDocumentRenderer(unicode: true) { Document = doc };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }
    }
}
