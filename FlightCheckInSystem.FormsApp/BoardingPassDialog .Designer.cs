
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.FormsApp.Services;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp
{
    partial class BoardingPassDialog
    {
        private TableLayoutPanel mainLayout;
        private Label lblTitle;
        private Panel pnlBoardingPass;
        private Panel pnlButtons;
        private Button btnPrint;
        private Button btnPreview;
        private Button btnClose;

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Суудлын тасалбар";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(236, 240, 241);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));

            lblTitle = new Label
            {
                Text = "✈ СУУДЛЫН ТАСАЛБАР АМЖИЛТТАЙ ҮҮСЛЭЭ ✈",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            pnlBoardingPass = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(20),
                AutoScroll = false
            };

            pnlButtons = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            btnPrint = new Button
            {
                Text = "🖨 Хэвлэх",
                Size = new Size(140, 60),
                Location = new Point(180, 20),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            btnPreview = new Button
            {
                Text = "👁 Урьдчилан харах",
                Size = new Size(180, 60),
                Location = new Point(340, 20),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnPreview.FlatAppearance.BorderSize = 0;
            btnPreview.Click += BtnPreview_Click;

            btnClose = new Button
            {
                Text = "✓ Дуусгах",
                Size = new Size(140, 60),
                Location = new Point(540, 20),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;

            pnlButtons.Controls.Add(btnPrint);
            pnlButtons.Controls.Add(btnPreview);
            pnlButtons.Controls.Add(btnClose);

            mainLayout.Controls.Add(lblTitle, 0, 0);
            mainLayout.Controls.Add(pnlBoardingPass, 0, 1);
            mainLayout.Controls.Add(pnlButtons, 0, 2);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private void LoadBoardingPassData()
        {
            if (_boardingPass == null)
            {
                return;
            }

            pnlBoardingPass.Controls.Clear();
            
            Panel boardingPassCard = new Panel
            {
                Size = new Size(800, 320),
                Location = new Point((pnlBoardingPass.Width - 800) / 2, (pnlBoardingPass.Height - 320) / 2),
                BackColor = Color.White,
                Anchor = AnchorStyles.None
            };

            boardingPassCard.Paint += (s, e) =>
            {
                DrawBoardingPassCard(e.Graphics, boardingPassCard.ClientRectangle);
            };

            pnlBoardingPass.Controls.Add(boardingPassCard);
            pnlBoardingPass.Resize += (s, e) =>
            {
                boardingPassCard.Location = new Point((pnlBoardingPass.Width - 800) / 2, (pnlBoardingPass.Height - 320) / 2);
            };
            
            Debug.WriteLine($"[BoardingPassDialog] {_boardingPass.PassengerName}-ийн суудлын тасалбарын мэдээлэл ачааллав");
        }

        private void DrawBoardingPassCard(Graphics g, Rectangle bounds)
        {
            if (_boardingPass == null) return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            using (var backgroundBrush = new SolidBrush(Color.White))
            using (var headerBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, bounds.Width, 55),
                Color.FromArgb(41, 128, 185),
                Color.FromArgb(52, 152, 219),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            using (var borderPen = new Pen(Color.FromArgb(189, 195, 199), 2))
            using (var airlineFont = new Font("Arial", 14, FontStyle.Bold))
            using (var subtitleFont = new Font("Arial", 9, FontStyle.Bold))
            using (var labelFont = new Font("Arial", 7, FontStyle.Regular))
            using (var dataFont = new Font("Arial", 10, FontStyle.Bold))
            using (var largeDataFont = new Font("Arial", 12, FontStyle.Bold))
            using (var smallFont = new Font("Arial", 7, FontStyle.Regular))
            using (var whiteBrush = new SolidBrush(Color.White))
            using (var blackBrush = new SolidBrush(Color.Black))
            using (var grayBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                Rectangle shadowRect = new Rectangle(3, 3, bounds.Width - 3, bounds.Height - 3);
                g.FillRectangle(shadowBrush, shadowRect);
                
                g.FillRectangle(backgroundBrush, bounds);
                g.DrawRectangle(borderPen, bounds);
                
                Rectangle headerRect = new Rectangle(0, 0, bounds.Width, 55);
                g.FillRectangle(headerBrush, headerRect);
                
                string airlineName = "MONGOLIAN AIRLINES";
                SizeF airlineSize = g.MeasureString(airlineName, airlineFont);
                float airlineX = (bounds.Width - airlineSize.Width) / 2;
                g.DrawString(airlineName, airlineFont, whiteBrush, airlineX, 8);
                
                string subtitle = "СУУДЛЫН ТАСАЛБАР / BOARDING PASS";
                SizeF subtitleSize = g.MeasureString(subtitle, subtitleFont);
                float subtitleX = (bounds.Width - subtitleSize.Width) / 2;
                g.DrawString(subtitle, subtitleFont, whiteBrush, subtitleX, 32);
                
                float startY = 70;
                float leftCol = 20;
                float middleCol = 200;
                float rightCol = 420;
                float rightCol2 = 580;
                float lineHeight = 32;
                
                float currentY = startY;
                
                g.DrawString("ЗОРЧИГЧ / PASSENGER", labelFont, grayBrush, leftCol, currentY);
                string passengerName = _boardingPass.PassengerName.ToUpper();
                if (passengerName.Length > 20) passengerName = passengerName.Substring(0, 20) + "...";
                g.DrawString(passengerName, largeDataFont, blackBrush, leftCol, currentY + 10);
                
                g.DrawString("НИСЛЭГ / FLIGHT", labelFont, grayBrush, rightCol, currentY);
                g.DrawString(_boardingPass.FlightNumber, largeDataFont, blackBrush, rightCol, currentY + 10);
                
                currentY += lineHeight + 8;
                
                g.DrawString("ХӨДӨЛГӨХ / FROM", labelFont, grayBrush, leftCol, currentY);
                g.DrawString(_boardingPass.DepartureAirport, dataFont, blackBrush, leftCol, currentY + 10);
                
                g.DrawString("ИРЭХ / TO", labelFont, grayBrush, leftCol + 90, currentY);
                g.DrawString(_boardingPass.ArrivalAirport, dataFont, blackBrush, leftCol + 90, currentY + 10);
                
                g.DrawString("СУУДАЛ / SEAT", labelFont, grayBrush, rightCol, currentY);
                g.DrawString(_boardingPass.SeatNumber, largeDataFont, blackBrush, rightCol, currentY + 10);
                
                currentY += lineHeight + 5;
                
                g.DrawString("ОГНОО / DATE", labelFont, grayBrush, leftCol, currentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("dd-MMM-yyyy"), dataFont, blackBrush, leftCol, currentY + 10);
                
                g.DrawString("ЦАГ / TIME", labelFont, grayBrush, middleCol, currentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("HH:mm"), dataFont, blackBrush, middleCol, currentY + 10);
                
                g.DrawString("ХААЛГА / GATE", labelFont, grayBrush, rightCol, currentY);
                g.DrawString("TBD", dataFont, blackBrush, rightCol, currentY + 10);
                
                currentY += lineHeight;
                
                g.DrawString("СУУХ ЦАГ / BOARDING", labelFont, grayBrush, leftCol, currentY);
                g.DrawString(_boardingPass.BoardingTime.ToString("HH:mm"), dataFont, blackBrush, leftCol, currentY + 10);
                
                currentY += lineHeight + 10;
                
                using (var separatorPen = new Pen(Color.FromArgb(189, 195, 199), 1))
                {
                    g.DrawLine(separatorPen, leftCol, currentY, bounds.Width - 20, currentY);
                }
                currentY += 12;
                
                g.DrawString($"ПАСПОРТ: {_boardingPass.PassportNumber}", smallFont, grayBrush, leftCol, currentY);
                string printTime = $"ҮҮССЭН: {DateTime.Now:dd-MMM-yyyy HH:mm}";
                SizeF printSize = g.MeasureString(printTime, smallFont);
                g.DrawString(printTime, smallFont, grayBrush, bounds.Width - 20 - printSize.Width, currentY);
            }
        }

       private void BtnPrint_Click(object sender, EventArgs e)
{
    Debug.WriteLine("[BoardingPassDialog] Хэвлэх товчлуур дарагдсан");
    
    if (_boardingPass == null)
    {
        MessageBox.Show("Хэвлэх тасалбарын мэдээлэл байхгүй байна.", "Хэвлэх алдаа", 
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    try
    {
        // Create a simple print approach that doesn't rely on the printer service
        PrintDocument printDoc = new PrintDocument();
        printDoc.DocumentName = $"BoardingPass_{_boardingPass.FlightNumber}_{_boardingPass.SeatNumber}";
        
        // Set up print page event
        printDoc.PrintPage += (s, args) =>
        {
            try
            {
                DrawBoardingPassForPrint(args.Graphics, args.MarginBounds);
                args.HasMorePages = false;
            }
            catch (Exception printEx)
            {
                Debug.WriteLine($"[BoardingPassDialog] Print page error: {printEx.Message}");
                args.Cancel = true;
            }
        };

        // Show print dialog
        using (PrintDialog printDialog = new PrintDialog())
        {
            printDialog.Document = printDoc;
            printDialog.UseEXDialog = true;
            printDialog.AllowPrintToFile = true;
            printDialog.AllowCurrentPage = false;
            printDialog.AllowSelection = false;
            printDialog.AllowSomePages = false;

            if (printDialog.ShowDialog(this) == DialogResult.OK)
            {
                printDoc.Print();
                MessageBox.Show("Суудлын тасалбарыг амжилттай хэвлэлээ!", 
                    "Хэвлэх амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[BoardingPassDialog] Хэвлэхэд алдаа: {ex.Message}");
        Debug.WriteLine($"[BoardingPassDialog] Stack trace: {ex.StackTrace}");
        
        // Show user-friendly error message
        string errorMessage = "Суудлын тасалбар хэвлэхэд алдаа гарлаа.";
        
        if (ex.Message.Contains("printer") || ex.Message.Contains("Printer"))
        {
            errorMessage += "\n\nХэвлэгчээ шалгаад дахин оролдоно уу.";
        }
        else if (ex.Message.Contains("access") || ex.Message.Contains("Access"))
        {
            errorMessage += "\n\nХэвлэгчийн эрх шалгаад дахин оролдоно уу.";
        }
        else
        {
            errorMessage += $"\n\nАлдааны дэлгэрэнгүй: {ex.Message}";
        }
        
        MessageBox.Show(errorMessage, "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void DrawBoardingPassForPrint(Graphics g, Rectangle bounds)
{
    if (_boardingPass == null) return;

    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    float x = bounds.X;
    float y = bounds.Y;
    float width = bounds.Width;
    float height = bounds.Height;

    // Use simpler drawing approach for printing
    using (var headerFont = new Font("Arial", 16, FontStyle.Bold))
    using (var labelFont = new Font("Arial", 8, FontStyle.Regular))
    using (var dataFont = new Font("Arial", 10, FontStyle.Bold))
    using (var smallFont = new Font("Arial", 7, FontStyle.Regular))
    using (var blackBrush = new SolidBrush(Color.Black))
    using (var grayBrush = new SolidBrush(Color.Gray))
    using (var borderPen = new Pen(Color.Black, 2))
    {
        // Draw border
        g.DrawRectangle(borderPen, x, y, width - 1, Math.Min(height - 1, 250));
        
        float currentY = y + 20;
        float leftMargin = x + 20;
        float rightMargin = x + width - 20;
        
        // Header
        string header = "MONGOLIAN AIRLINES - СУУДЛЫН ТАСАЛБАР";
        var headerSize = g.MeasureString(header, headerFont);
        g.DrawString(header, headerFont, blackBrush, 
            leftMargin + (width - 40 - headerSize.Width) / 2, currentY);
        currentY += 40;
        
        // Passenger
        g.DrawString("ЗОРЧИГЧ / PASSENGER:", labelFont, grayBrush, leftMargin, currentY);
        currentY += 15;
        g.DrawString(_boardingPass.PassengerName?.ToUpper() ?? "N/A", dataFont, blackBrush, leftMargin, currentY);
        currentY += 25;
        
        // Flight and Route
        g.DrawString("НИСЛЭГ / FLIGHT:", labelFont, grayBrush, leftMargin, currentY);
        g.DrawString("ЧИГЛЭЛ / ROUTE:", labelFont, grayBrush, leftMargin + 200, currentY);
        currentY += 15;
        g.DrawString(_boardingPass.FlightNumber ?? "N/A", dataFont, blackBrush, leftMargin, currentY);
        g.DrawString($"{_boardingPass.DepartureAirport} → {_boardingPass.ArrivalAirport}", 
            dataFont, blackBrush, leftMargin + 200, currentY);
        currentY += 25;
        
        // Date and Time
        g.DrawString("ОГНОО / DATE:", labelFont, grayBrush, leftMargin, currentY);
        g.DrawString("ЦАГ / TIME:", labelFont, grayBrush, leftMargin + 150, currentY);
        g.DrawString("СУУДАЛ / SEAT:", labelFont, grayBrush, leftMargin + 300, currentY);
        currentY += 15;
        g.DrawString(_boardingPass.DepartureTime.ToString("dd-MMM-yyyy"), dataFont, blackBrush, leftMargin, currentY);
        g.DrawString(_boardingPass.DepartureTime.ToString("HH:mm"), dataFont, blackBrush, leftMargin + 150, currentY);
        g.DrawString(_boardingPass.SeatNumber ?? "N/A", dataFont, blackBrush, leftMargin + 300, currentY);
        currentY += 25;
        
        // Boarding Time
        g.DrawString("СУУХ ЦАГ / BOARDING:", labelFont, grayBrush, leftMargin, currentY);
        currentY += 15;
        g.DrawString(_boardingPass.BoardingTime.ToString("HH:mm"), dataFont, blackBrush, leftMargin, currentY);
        currentY += 30;
        
        // Footer
        g.DrawString($"ПАСПОРТ: {_boardingPass.PassportNumber ?? "N/A"}", smallFont, grayBrush, leftMargin, currentY);
        string printTime = $"ХЭВЛЭСЭН: {DateTime.Now:dd-MMM-yyyy HH:mm}";
        var printSize = g.MeasureString(printTime, smallFont);
        g.DrawString(printTime, smallFont, grayBrush, rightMargin - printSize.Width, currentY);
    }
}
        private void BtnPreview_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] Урьдчилан харах товчлуур дарагдсан");
            try
            {
                _printer.ShowPrintPreview(_boardingPass);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassDialog] Урьдчилан харахад алдаа: {ex.Message}");
                MessageBox.Show($"Хэвлэх урьдчилан харахад алдаа гарлаа: {ex.Message}",
                    "Урьдчилан харах алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] Хаах товчлуур дарагдсан");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _printer?.Dispose();
            Debug.WriteLine("[BoardingPassDialog] Форм хаагдаж, нөөц чөлөөлөгдлөө");
        }

        public bool StartNewCheckIn => this.DialogResult == DialogResult.Retry;
    }
}