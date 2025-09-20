using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace TruckSlip.Report
{
    public class OrderReportService
    {
        private readonly Jobsite _jobsite;
        private readonly Company _company;
        private readonly Order _order;
        private readonly IList<OrderItemsQuery> _dataSource;
        private readonly string tempFile = Path.Combine(FileSystem.CacheDirectory, "temp.pdf");

        public OrderReportService(Company company, Jobsite jobsite, Order order, IList<OrderItemsQuery> orderItems)
        {
            _jobsite = jobsite;
            _company = company;
            _order = order;
            _dataSource = orderItems;
            _dataSource = [.. _dataSource.OrderBy(x => x.TaskCode)];
        }

        public void GeneratePdfAndShow() //main method to generate PDF
        {
            //Create MigraDoc document
            var document = new Document();
            BuildDocument(document);

            var pdfRenderer = new PdfDocumentRenderer
            {
                Document = document
            };
            pdfRenderer.RenderDocument();

            pdfRenderer.PdfDocument.Save(tempFile);

            Launcher.Default.OpenAsync(new OpenFileRequest("Open PDF", new ReadOnlyFile(tempFile)));
        }

        private void BuildDocument(Document document)
        {
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.TopMargin = Unit.FromInch(0.75);
            section.PageSetup.BottomMargin = Unit.FromInch(0.75);
            section.PageSetup.LeftMargin = Unit.FromInch(0.75);
            section.PageSetup.RightMargin = Unit.FromInch(0.75);
            
            CreateHeader(document);
            CreateFooter(document);
            CreateOrderHeader(document);
            CreateOrderDetails(document);
            CreateNotesSection(document);
        }

        private void CreateHeader(Document document)
        {
            var header = document.LastSection.Headers.Primary;
            var table = header.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromInch(3.5));
            table.AddColumn(Unit.FromInch(3.5));
            table.Format.SpaceAfter = Unit.FromInch(4);

            var row = table.AddRow();
            row.Height = Unit.FromInch(0.75);
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[1].Format.Alignment = ParagraphAlignment.Right;

            var image = row.Cells[0].AddImage(GetLogo());
            image.LockAspectRatio = true;
            image.Height = Unit.FromInch(0.6);

            // Add title
            var title = row.Cells[1].AddParagraph("TRUCK TICKET");
            title.Format.Font.Size = 14;
            title.Format.Font.Bold = true;

            document.LastSection.AddParagraph().Format.SpaceAfter = Unit.FromInch(0.25);
        }

        private static void CreateFooter(Document document)
        {
            var paragraph = new Paragraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.AddText("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();

            var footer = document.LastSection.Footers.Primary;
            footer.Add(paragraph);
        }

        private void CreateOrderHeader(Document document)
        {
            var table = document.LastSection.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromInch(3)); // Jobsite
            table.AddColumn(Unit.FromInch(4)); // Order

            var row = table.AddRow();
            row.Cells[0].Elements.Add(CreateJobsiteTable());
            row.Cells[1].Elements.Add(CreateOrderTable());

            document.LastSection.AddParagraph().Format.SpaceAfter = Unit.FromInch(0.1);
        }

        private Table CreateJobsiteTable()
        { 
            var jobSite = $"{_jobsite.Number}\r\n" +
                $"{_jobsite.Name}\r\n" +
                $"{_jobsite.Address}\r\n" +
                $"{_jobsite.Address2}\r\n" +
                $"{_jobsite.Contact}";
            
            var table = new Table();
            table.Borders.Width = 0;
            table.Format.Font.Size = 12;
            table.AddColumn(Unit.FromInch(3));

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("JOBSITE:");

            var row = table.AddRow();
            row.Cells[0].AddParagraph(jobSite);
            row.Cells[0].Format.Font.Size = 12;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

            return table;
        }

        private Table CreateOrderTable()
        {
            bool isDelivery = Convert.ToBoolean(_order.OrderTypeId);
            bool isPickup = !isDelivery;

            var table = new Table();
            table.Borders.Width = 0;
            table.Format.Font.Size = 12;
            table.AddColumn(Unit.FromInch(0.75)); // Blank column
            table.AddColumn(Unit.FromInch(1.25)); // Delivery
            table.AddColumn(Unit.FromInch(0.75)); // Blank column
            table.AddColumn(Unit.FromInch(1.25)); // Pickup

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("ORDER:");

            var row1 = table.AddRow();
            row1.Height = Unit.FromPoint(18F);
            row1.Cells[0].AddParagraph("Date");
            row1.Cells[1].AddParagraph(_order.Date);

            var row2 = table.AddRow();
            row2.Height = Unit.FromPoint(18F);
            row2.HeadingFormat = true;
            row2.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row2.Cells[0].Borders.Bottom.Width = 0.5;
            row2.Cells[0].AddParagraph(GetBooleanString(isDelivery));
            row2.Cells[1].AddParagraph("DELIVERY");
            
            row2.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row2.Cells[2].Borders.Bottom.Width = 0.5;
            row2.Cells[2].AddParagraph(GetBooleanString(isPickup));
            row2.Cells[3].AddParagraph("PICKUP");

            var row3 = table.AddRow();
            row3.Height = Unit.FromPoint(18F);
            row3.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[0].Borders.Bottom.Width = 0.5;
            row3.Cells[1].AddParagraph("LOW BOY");

            row3.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[2].Borders.Bottom.Width = 0.5;
            row3.Cells[3].AddParagraph("FLATBED");

            var row4 = table.AddRow();
            row4.Height = Unit.FromPoint(18F);
            row4.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[0].Borders.Bottom.Width = 0.5;
            row4.Cells[1].AddParagraph("TIME IN");

            row4.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[2].Borders.Bottom.Width = 0.5;
            row4.Cells[3].AddParagraph("TIME OUT");

            return table;
        }

        private void CreateOrderDetails(Document document)
        {
            var orderTable = document.LastSection.AddTable();
            orderTable.Borders.Width = 0.5;
            orderTable.Format.Font.Size = 12;
            
            // Define columns
            orderTable.AddColumn(Unit.FromInch(.625)).Format.Alignment = ParagraphAlignment.Right; // Qty
            orderTable.AddColumn(Unit.FromInch(1.25)); // Unit
            orderTable.AddColumn(Unit.FromInch(1.125)); // Task Code
            orderTable.AddColumn(Unit.FromInch(4)); // Item Description
            // Add header row
            var headerRow = orderTable.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Qty");
            headerRow.Cells[1].AddParagraph("Unit");
            headerRow.Cells[2].AddParagraph("Cost Code");
            headerRow.Cells[3].AddParagraph("Item Description");
            // Add data rows
            foreach (var item in _dataSource)
            {
                var row = orderTable.AddRow();
                row.Height = Unit.FromPoint(18);
                row.Cells[0].AddParagraph(item.Quantity.ToString());
                row.Cells[1].AddParagraph(item.UnitName);
                row.Cells[2].AddParagraph(item.TaskCode);
                row.Cells[3].AddParagraph(item.Name);
            }
        }

        private void CreateNotesSection(Document document)
        {
            var notes = _order.Notes ?? string.Empty;

            var paragraph = document.LastSection.AddParagraph("NOTES:");
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;

            var table = document.LastSection.AddTable();
            table.Borders.Width = 0.5;
            table.Format.Font.Size = 12;
            table.AddColumn(Unit.FromInch(7)); // Single column for notes
            var row = table.AddRow();
            row.Height = Unit.FromInch(1);
            row.Cells[0].Elements.Add(GetNotes(notes));
        }

        private string GetLogo()
        {
            if (_company.Logo == null) return string.Empty;
            string base64Image = Convert.ToBase64String(_company.Logo);
            return $"base64:{base64Image}" ;
        }

        private static string GetBooleanString(bool value)
            => value ? "X" : string.Empty;

        private static Paragraph GetNotes(string value)
        {
            var paragraph = new Paragraph();
            paragraph.Format.Font.Size = 12;

            if (string.IsNullOrEmpty(value))
            {
                paragraph.AddText("There are no special instructions for this order.");
                return paragraph;
            }

            var lines = value.Replace("\r", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    paragraph.AddLineBreak();
                paragraph.AddText(lines[i]);
            }
            return paragraph;
        }
    }
}
