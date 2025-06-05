using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using System.Diagnostics;
using System.Linq;
using System.Drawing.Drawing2D;

namespace FlightCheckInSystem.FormsApp.Services
{
    public class BoardingPassPrinter
    {
        private BoardingPass _boardingPass;
        private Font _headerFont;
        private Font _labelFont;
        private Font _dataFont;
        private Font _largeFont;
        private Font _smallFont;
        private Brush _blackBrush;
        private Brush _whiteBrush;
        private Brush _grayBrush;
        private Brush _blueBrush;
        
        public BoardingPassPrinter()
        {
            InitializeFonts();
        }

        private void InitializeFonts()
        {
            _headerFont = new Font("Arial", 18, FontStyle.Bold);
            _labelFont = new Font("Arial", 9, FontStyle.Regular);
            _dataFont = new Font("Arial", 12, FontStyle.Bold);
            _largeFont = new Font("Arial", 16, FontStyle.Bold);
            _smallFont = new Font("Arial", 8, FontStyle.Regular);
            _blackBrush = new SolidBrush(Color.Black);
            _whiteBrush = new SolidBrush(Color.White);
            _grayBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
            _blueBrush = new SolidBrush(Color.FromArgb(41, 128, 185));
        }

        public void PrintBoardingPass(BoardingPass boardingPass)
        {
            if (boardingPass == null)
            {
                MessageBox.Show("Хэвлэх тасалбарын мэдээлэл байхгүй байна.", "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _boardingPass = boardingPass;

            PrintDialog printDialog = new PrintDialog();
            PrintDocument printDocument = new PrintDocument();
            
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDialog.Document = printDocument;

            printDocument.DocumentName = $"BoardingPass_{boardingPass.FlightNumber}_{boardingPass.SeatNumber}";

            DialogResult result = printDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    printDocument.Print();
                    Debug.WriteLine($"[BoardingPassPrinter] {boardingPass.PassengerName}-ийн суудлын тасалбарыг амжилттай хэвлэлээ");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BoardingPassPrinter] Суудлын тасалбар хэвлэхэд алдаа: {ex.Message}");
                    MessageBox.Show($"Суудлын тасалбар хэвлэхэд алдаа гарлаа: {ex.Message}", "Хэвлэх алдаа", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void ShowPrintPreview(BoardingPass boardingPass, IWin32Window owner = null)
        {
            if (boardingPass == null)
            {
                MessageBox.Show(owner, "Урьдчилан харах тасалбарын мэдээлэл байхгүй байна.", "Урьдчилан харах алдаа", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _boardingPass = boardingPass;
            PrintPreviewDialog previewDialog = null;
            PrintDocument printDocument = null;

            try
            {
                printDocument = new PrintDocument
                {
                    DocumentName = $"BoardingPass_{boardingPass.FlightNumber}_{boardingPass.SeatNumber}",
                    DefaultPageSettings = {
                        Margins = new Margins(50, 50, 50, 50)
                    }
                };
                
                printDocument.PrintPage += PrintDocument_PrintPage;

                previewDialog = new PrintPreviewDialog
                {
                    Document = printDocument,
                    WindowState = FormWindowState.Maximized,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = $"Суудлын тасалбар - {boardingPass.FlightNumber} - {boardingPass.PassengerName}",
                    ShowIcon = true,
                    ShowInTaskbar = true,
                    UseAntiAlias = true
                };

                var printButton = new ToolStripButton("Хэвлэх", null, (s, e) =>
                {
                    try
                    {
                        PrintBoardingPass(boardingPass);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(previewDialog, $"Хэвлэхэд алдаа: {ex.Message}", 
                            "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                })
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    TextImageRelation = TextImageRelation.Overlay
                };

                var toolStrip = previewDialog.Controls.OfType<ToolStrip>().FirstOrDefault();
                if (toolStrip != null)
                {
                    toolStrip.Items.Add(new ToolStripSeparator());
                    toolStrip.Items.Add(printButton);
                }

                previewDialog.FormClosing += (s, e) =>
                {
                    printDocument.PrintPage -= PrintDocument_PrintPage;
                    printDocument.Dispose();
                    previewDialog.Dispose();
                };

                Debug.WriteLine($"[BoardingPassPrinter] {boardingPass.PassengerName}-ийн хэвлэх урьдчилан харах харуулж байна");
                previewDialog.ShowDialog(owner);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] ShowPrintPreview алдаа: {ex}");
                MessageBox.Show(owner, $"Суудлын тасалбар урьдчилан харахад алдаа гарлаа: {ex.Message}", 
                    "Урьдчилан харах алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                printDocument?.Dispose();
                previewDialog?.Dispose();
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_boardingPass == null)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                RectangleF bounds = e.MarginBounds;
                float x = bounds.Left;
                float y = bounds.Top;
                float width = bounds.Width;

                DrawModernBoardingPass(g, x, y, width);
                
                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] PrintDocument_PrintPage алдаа: {ex}");
                e.Cancel = true;
                throw;
            }
        }

        private void DrawModernBoardingPass(Graphics g, float x, float y, float width)
        {
            float padding = 25;
            float bpWidth = Math.Min(650, width - 40);
            float bpHeight = 300;
            
            RectangleF mainRect = new RectangleF(x + 20, y, bpWidth, bpHeight);
            
            using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
            using (var backgroundBrush = new SolidBrush(Color.White))
            using (var blueBrush = new LinearGradientBrush(
                new RectangleF(mainRect.X, mainRect.Y, mainRect.Width, 60),
                Color.FromArgb(41, 128, 185),
                Color.FromArgb(52, 152, 219),
                LinearGradientMode.Horizontal))
            using (var borderPen = new Pen(Color.FromArgb(189, 195, 199), 2))
            {
                RectangleF shadowRect = new RectangleF(mainRect.X + 3, mainRect.Y + 3, mainRect.Width, mainRect.Height);
                g.FillRectangle(shadowBrush, shadowRect);
                
                g.FillRectangle(backgroundBrush, mainRect);
                g.DrawRectangle(borderPen, Rectangle.Round(mainRect));
                
                RectangleF headerRect = new RectangleF(mainRect.X, mainRect.Y, mainRect.Width, 60);
                g.FillRectangle(blueBrush, headerRect);
                
                string airlineName = "MONGOLIAN AIRLINES";
                SizeF airlineSize = g.MeasureString(airlineName, _headerFont);
                float airlineX = mainRect.X + (mainRect.Width - airlineSize.Width) / 2;
                g.DrawString(airlineName, _headerFont, _whiteBrush, airlineX, mainRect.Y + 8);
                
                string subtitle = "СУУДЛЫН ТАСАЛБАР / BOARDING PASS";
                SizeF subtitleSize = g.MeasureString(subtitle, _dataFont);
                float subtitleX = mainRect.X + (mainRect.Width - subtitleSize.Width) / 2;
                g.DrawString(subtitle, _dataFont, _whiteBrush, subtitleX, mainRect.Y + 32);
                
                float contentY = mainRect.Y + 80;
                float leftCol = mainRect.X + padding;
                float rightCol = mainRect.X + mainRect.Width * 0.7f;
                float rowHeight = 35;
                
                g.DrawString("ЗОРЧИГЧ / PASSENGER", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.PassengerName.ToUpper(), _largeFont, _blackBrush, leftCol, contentY + 12);
                contentY += rowHeight + 10;
                
                g.DrawString("ХӨДӨЛГӨХ / FROM", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.DepartureAirport, _dataFont, _blackBrush, leftCol, contentY + 12);
                
                g.DrawString("ИРЭХ / TO", _labelFont, _grayBrush, leftCol + 120, contentY);
                g.DrawString(_boardingPass.ArrivalAirport, _dataFont, _blackBrush, leftCol + 120, contentY + 12);
                
                g.DrawString("НИСЛЭГ / FLIGHT", _labelFont, _grayBrush, rightCol, contentY);
                g.DrawString(_boardingPass.FlightNumber, _largeFont, _blackBrush, rightCol, contentY + 12);
                contentY += rowHeight;
                
                g.DrawString("ОГНОО / DATE", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("dd-MMM-yyyy"), _dataFont, _blackBrush, leftCol, contentY + 12);
                
                g.DrawString("ЦАГ / TIME", _labelFont, _grayBrush, leftCol + 120, contentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("HH:mm"), _dataFont, _blackBrush, leftCol + 120, contentY + 12);
                
                g.DrawString("СУУДАЛ / SEAT", _labelFont, _grayBrush, rightCol, contentY);
                g.DrawString(_boardingPass.SeatNumber, _largeFont, _blackBrush, rightCol, contentY + 12);
                contentY += rowHeight;
                
                g.DrawString("СУУХ ЦАГ / BOARDING TIME", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.BoardingTime.ToString("HH:mm"), _dataFont, _blackBrush, leftCol, contentY + 12);
                
                g.DrawString("ХААЛГА / GATE", _labelFont, _grayBrush, leftCol + 150, contentY);
                g.DrawString("TBD", _dataFont, _blackBrush, leftCol + 150, contentY + 12);
                contentY += rowHeight + 15;
                
                using (var separatorPen = new Pen(Color.FromArgb(189, 195, 199), 1))
                {
                    g.DrawLine(separatorPen, leftCol, contentY, mainRect.Right - padding, contentY);
                }
                contentY += 15;
                
                g.DrawString($"ПАСПОРТ: {_boardingPass.PassportNumber}", _smallFont, _grayBrush, leftCol, contentY);
                string printTime = $"ХЭВЛЭСЭН: {DateTime.Now:dd-MMM-yyyy HH:mm}";
                SizeF printSize = g.MeasureString(printTime, _smallFont);
                g.DrawString(printTime, _smallFont, _grayBrush, mainRect.Right - padding - printSize.Width, contentY);
            }
        }

        public void Dispose()
        {
            _headerFont?.Dispose();
            _labelFont?.Dispose();
            _dataFont?.Dispose();
            _largeFont?.Dispose();
            _smallFont?.Dispose();
            _blackBrush?.Dispose();
            _whiteBrush?.Dispose();
            _grayBrush?.Dispose();
            _blueBrush?.Dispose();
        }
    }
}