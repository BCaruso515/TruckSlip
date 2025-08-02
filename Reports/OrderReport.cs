using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Cell = iText.Layout.Element.Cell;
using Image = iText.Layout.Element.Image;

namespace TruckSlip.Reports
{
    public class OrderReport
    {
        private readonly Jobsite _jobsite;
        private readonly Order _order;
        private readonly IList<OrderItemsQuery> _dataSource;
        private readonly string source = Path.Combine(FileSystem.CacheDirectory, "temp1.pdf");
        private readonly string destination = Path.Combine(FileSystem.CacheDirectory, "temp.pdf");

        public OrderReport(Jobsite jobsite, Order order, IList<OrderItemsQuery> orderItems)
        {
            _jobsite = jobsite;
            _order = order;
            _dataSource = orderItems;
            _dataSource = [.. _dataSource.OrderBy(x => x.TaskCode)];
        }

        public void GeneratePdfAndShow() //main method to generate PDF
        {
            PdfWriter writer = new(source);
            PdfDocument pdf = new(writer);
            Document document = new(pdf);

            //Add logo and items to the page top here

            document.Add(ComposeHeaderTable());
            document.Add(ComposeOrderDetailsTable());
            document.Add(ComposeFooterTable());
            document.Close();
            pdf = new PdfDocument(new PdfReader(source), new PdfWriter(destination));
            iTextPdfHelper.AddPageFooterWithDate(pdf);
            iTextPdfHelper.DisplayPdf(destination);
        }

        private Table ComposeHeaderTable()
        {
            float[] columnWidths = [220F, 340F];
            Table table = new Table(columnWidths)
                .SetKeepTogether(true)
                .SetMarginBottom(5)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

            var logo = GetLogo();
            if (logo == null)
                table.AddCell(iTextPdfHelper.NoBorder("")); // If no logo, add an empty cell
            else
                table.AddCell(iTextPdfHelper.NoBorder(logo));

            table.AddCell(iTextPdfHelper.NoBorder("TRUCK SLIP")
                .SetFontSize(14).SimulateBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

            table.AddCell(iTextPdfHelper.NoBorder(ComposeJobsiteTable()));
            table.AddCell(iTextPdfHelper.NoBorder(ComposeOrderTable()));
            return table;
        }

        private Table ComposeJobsiteTable()
        {
            string jobSite = $"{_jobsite.Number}\r\n"+
                $"{_jobsite.Name}\r\n" +
                $"{_jobsite.Address}\r\n" +
                $"{_jobsite.Address2}\r\n" +
                $"{_jobsite.Contact}";

            Table table = new Table(1)
                .SetKeepTogether(true)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT)
                .SetMarginTop(10)
                .SetFontSize(12);
            table.AddHeaderCell(iTextPdfHelper.NoBorder("JOBSITE:"));
            table.AddCell(iTextPdfHelper.NoBorder(jobSite));
            return table;
        }

        private Table ComposeOrderTable()
        {
            bool isDelivery = Convert.ToBoolean(_order.OrderTypeId);
            bool isPickup = !isDelivery;
            float[] columnWidths = [50F, 100F, 50F, 100F];
            Table table = new Table(columnWidths)
                .SetKeepTogether(true)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT)
                .SetMarginTop(10)
                .SetFontSize(12);

            table.AddHeaderCell(new Cell(1, 4)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("ORDER:")));
            table.AddHeaderCell(iTextPdfHelper.NoBorder("Date"));
            table.AddHeaderCell(iTextPdfHelper.NoBorder(_order.Date));
            table.AddCell(iTextPdfHelper.BottomBorder(GetBooleanString(isDelivery))
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            table.AddCell(iTextPdfHelper.NoBorder("DELIVERY"));
            table.AddCell(iTextPdfHelper.BottomBorder(GetBooleanString(isPickup))
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            table.AddCell(iTextPdfHelper.NoBorder("PICKUP"));
            table.AddCell(iTextPdfHelper.BottomBorder(""));
            table.AddCell(iTextPdfHelper.NoBorder("LOW BOY"));
            table.AddCell(iTextPdfHelper.BottomBorder(""));
            table.AddCell(iTextPdfHelper.NoBorder("FLATBED"));
            table.AddCell(iTextPdfHelper.BottomBorder(""));
            table.AddCell(iTextPdfHelper.NoBorder("TIME IN"));
            table.AddCell(iTextPdfHelper.BottomBorder(""));
            table.AddCell(iTextPdfHelper.NoBorder("TIME OUT"));
            return table;
        }

        private Table ComposeOrderDetailsTable()
        {
            float[] columnWidths = [40F, 80F, 100F, 400F];
            Table table = new Table(columnWidths)
                .SetKeepTogether(true);
            table.AddHeaderCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetPaddingRight(5)
                    .Add(new Paragraph("Qty")));
            table.AddHeaderCell("Unit");
            table.AddHeaderCell("Task Code");
            table.AddHeaderCell("Item");

            foreach (var result in _dataSource)
            {
                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetPaddingRight(5)
                    .Add(new Paragraph(result.Quantity.ToString())));
                table.AddCell(result.UnitName);
                table.AddCell(result.TaskCode);
                table.AddCell(result.Name);
            }

            return table;
        }

        private Table ComposeFooterTable()
        {
            float[] columnWidths = [560F];
            Table table = new Table(columnWidths)
                .SetKeepTogether(true)
                .SetMinHeight(80);
            table.AddHeaderCell(iTextPdfHelper.NoBorder("NOTES:"));
            table.AddCell(GetNotes(_order.Notes));
            return table;
        }

        private Image? GetLogo()
        {
            if (_jobsite.Logo == null) return null;
            ImageData imageData = ImageDataFactory.Create(_jobsite.Logo);
            Image image = new Image(imageData)
                .SetWidth(182F)
                .SetHeight(42F)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT);
            return image;
        }

        private static string GetBooleanString(bool value)
            => value ? "X" : string.Empty;

        private static string GetNotes(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "There are no special instructions for this order.";
            return value;
        }
    }
}
