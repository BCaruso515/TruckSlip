using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Border = iText.Layout.Borders.Border;
using Cell = iText.Layout.Element.Cell;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = iText.Layout.Properties.VerticalAlignment;
using Image = iText.Layout.Element.Image;

namespace TruckSlip.Helpers
{
    public static class iTextPdfHelper
    {
        public static Cell BottomBorder(string content)
        {
            Cell cell = new Cell().Add(new Paragraph(content));
            cell.SetBorderLeft(Border.NO_BORDER);
            cell.SetBorderRight(Border.NO_BORDER);
            cell.SetBorderTop(Border.NO_BORDER);
            return cell;
        }

        public static Cell TopBorder(string content)
        {
            Cell cell = new Cell().Add(new Paragraph(content));
            cell.SetBorderLeft(Border.NO_BORDER);
            cell.SetBorderRight(Border.NO_BORDER);
            cell.SetBorderBottom(Border.NO_BORDER);
            return cell;
        }

        public static Cell RightBorder(string content)
        {
            Cell cell = new Cell().Add(new Paragraph(content));
            cell.SetBorderLeft(Border.NO_BORDER);
            cell.SetBorderBottom(Border.NO_BORDER);
            cell.SetBorderTop(Border.NO_BORDER);
            return cell;
        }

        public static Cell LeftBorder(string content)
        {
            Cell cell = new Cell().Add(new Paragraph(content));
            cell.SetBorderBottom(Border.NO_BORDER);
            cell.SetBorderRight(Border.NO_BORDER);
            cell.SetBorderTop(Border.NO_BORDER);
            return cell;
        }

        public static Cell NoBorder(string content)
        {
            Cell cell = new Cell().Add(new Paragraph(content));
            cell.SetBorder(Border.NO_BORDER);
            return cell;
        }

        public static Cell NoBorder(Table table)
        {
            Cell cell = new Cell().Add(table);
            cell.SetBorder(Border.NO_BORDER);
            return cell;
        }

        public static Cell NoBorder(List list)
        {
            Cell cell = new Cell().Add(list);
            cell.SetBorder(Border.NO_BORDER);
            return cell;
        }

        public static Cell NoBorder(Image image)
        {
            Cell cell = new Cell().Add(image);
            cell.SetBorder(Border.NO_BORDER);
            return cell;
        }

        public static async void DisplayPdf(string FileName) =>
            await Launcher.Default.OpenAsync(new OpenFileRequest("Open document...", new ReadOnlyFile(FileName)));

        public static void AddPageFooterWithDate(PdfDocument pdfDoc)
        {
            Document doc = new(pdfDoc);

            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (int i = 1; i <= numberOfPages; i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                Paragraph pageXofY = new($"Page {i} of {numberOfPages}");
                Paragraph date = new(DateTime.Now.ToShortDateString());
                float x = pageSize.GetWidth() - 65;
                float y = pageSize.GetBottom() + 25;

                doc.ShowTextAligned(pageXofY, x, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                doc.ShowTextAligned(date, 65, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
            }
            doc.Close();
        }

        public static void AddPageFooter(PdfDocument pdfDoc)
        {
            Document doc = new(pdfDoc);

            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (int i = 1; i <= numberOfPages; i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                Paragraph p = new($"Page {i} of {numberOfPages}");
                float x = pageSize.GetWidth() / 2 - 6;
                float y = pageSize.GetBottom() + 30;
                doc.ShowTextAligned(p, x, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
            }
            doc.Close();
        }
    }
}
