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
    public class BoardingPassPrinter : IDisposable
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
        private bool _disposed = false;

        public BoardingPassPrinter()
        {
            InitializeFonts();
        }

        private void InitializeFonts()
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] Error initializing fonts: {ex.Message}");
                throw;
            }
        }

        public void PrintBoardingPass(BoardingPass boardingPass)
        {
            if (boardingPass == null)
            {
                MessageBox.Show("Хэвлэх тасалбарын мэдээлэл байхгүй байна.", "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _boardingPass = boardingPass;
            PrintDocument printDocument = null;

            try
            {
                printDocument = new PrintDocument();
                printDocument.DocumentName = $"BoardingPass_{boardingPass.FlightNumber}_{boardingPass.SeatNumber}";

                // Set up print settings
                printDocument.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);

                printDocument.PrintPage += PrintDocument_PrintPage;

                // Create print dialog with proper settings
                using (var printDialog = new PrintDialog())
                {
                    printDialog.Document = printDocument;
                    printDialog.UseEXDialog = true;
                    printDialog.AllowCurrentPage = false;
                    printDialog.AllowPrintToFile = true;
                    printDialog.AllowSelection = false;
                    printDialog.AllowSomePages = false;

                    // Show print dialog
                    DialogResult result = printDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Debug.WriteLine($"[BoardingPassPrinter] Starting print job for {boardingPass.PassengerName}");

                        // Print with error handling
                        printDocument.Print();

                        Debug.WriteLine($"[BoardingPassPrinter] Print job completed successfully for {boardingPass.PassengerName}");
                        MessageBox.Show("Суудлын тасалбарыг амжилттай хэвлэлээ!",
                            "Хэвлэх амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (InvalidPrinterException ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] Invalid printer error: {ex.Message}");
                MessageBox.Show($"Хэвлэгчийн алдаа: {ex.Message}\n\nХэвлэгчээ шалгаад дахин оролдоно уу.",
                    "Хэвлэгчийн алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] Print error: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show($"Суудлын тасалбар хэвлэхэд алдаа гарлаа: {ex.Message}",
                    "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Clean up print document
                if (printDocument != null)
                {
                    printDocument.PrintPage -= PrintDocument_PrintPage;
                    printDocument.Dispose();
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

                // Add custom print button to toolbar
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

                // Find and add button to toolbar
                var toolStrip = previewDialog.Controls.OfType<ToolStrip>().FirstOrDefault();
                if (toolStrip != null)
                {
                    toolStrip.Items.Add(new ToolStripSeparator());
                    toolStrip.Items.Add(printButton);
                }

                // Handle cleanup when preview dialog closes
                previewDialog.FormClosing += (s, e) =>
                {
                    try
                    {
                        if (printDocument != null)
                        {
                            printDocument.PrintPage -= PrintDocument_PrintPage;
                            printDocument.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[BoardingPassPrinter] Error during preview cleanup: {ex.Message}");
                    }
                };

                Debug.WriteLine($"[BoardingPassPrinter] Showing print preview for {boardingPass.PassengerName}");
                previewDialog.ShowDialog(owner);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] ShowPrintPreview error: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show(owner, $"Суудлын тасалбар урьдчилан харахад алдаа гарлаа: {ex.Message}",
                    "Урьдчилан харах алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Clean up on error
                try
                {
                    printDocument?.Dispose();
                    previewDialog?.Dispose();
                }
                catch (Exception cleanupEx)
                {
                    Debug.WriteLine($"[BoardingPassPrinter] Error during error cleanup: {cleanupEx.Message}");
                }
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_boardingPass == null)
            {
                Debug.WriteLine("[BoardingPassPrinter] PrintDocument_PrintPage: _boardingPass is null, cancelling print");
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
                Debug.WriteLine("[BoardingPassPrinter] PrintDocument_PrintPage completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] PrintDocument_PrintPage error: {ex.Message}\nStack trace: {ex.StackTrace}");
                e.Cancel = true;
                throw;
            }
        }

        private void DrawModernBoardingPass(Graphics g, float x, float y, float width)
        {
            if (_disposed)
            {
                Debug.WriteLine("[BoardingPassPrinter] Cannot draw - object is disposed");
                return;
            }

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
                // Draw shadow
                RectangleF shadowRect = new RectangleF(mainRect.X + 3, mainRect.Y + 3, mainRect.Width, mainRect.Height);
                g.FillRectangle(shadowBrush, shadowRect);

                // Draw main background
                g.FillRectangle(backgroundBrush, mainRect);
                g.DrawRectangle(borderPen, Rectangle.Round(mainRect));

                // Draw header
                RectangleF headerRect = new RectangleF(mainRect.X, mainRect.Y, mainRect.Width, 60);
                g.FillRectangle(blueBrush, headerRect);

                // Draw airline name
                string airlineName = "MONGOLIAN AIRLINES";
                SizeF airlineSize = g.MeasureString(airlineName, _headerFont);
                float airlineX = mainRect.X + (mainRect.Width - airlineSize.Width) / 2;
                g.DrawString(airlineName, _headerFont, _whiteBrush, airlineX, mainRect.Y + 8);

                // Draw subtitle
                string subtitle = "СУУДЛЫН ТАСАЛБАР / BOARDING PASS";
                SizeF subtitleSize = g.MeasureString(subtitle, _dataFont);
                float subtitleX = mainRect.X + (mainRect.Width - subtitleSize.Width) / 2;
                g.DrawString(subtitle, _dataFont, _whiteBrush, subtitleX, mainRect.Y + 32);

                // Draw content
                float contentY = mainRect.Y + 80;
                float leftCol = mainRect.X + padding;
                float rightCol = mainRect.X + mainRect.Width * 0.7f;
                float rowHeight = 35;

                // Passenger name
                g.DrawString("ЗОРЧИГЧ / PASSENGER", _labelFont, _grayBrush, leftCol, contentY);
                string passengerName = _boardingPass.PassengerName?.ToUpper() ?? "N/A";
                if (passengerName.Length > 25) passengerName = passengerName.Substring(0, 25) + "...";
                g.DrawString(passengerName, _largeFont, _blackBrush, leftCol, contentY + 12);
                contentY += rowHeight + 10;

                // Route and flight
                g.DrawString("ХӨДӨЛГӨХ / FROM", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.DepartureAirport ?? "N/A", _dataFont, _blackBrush, leftCol, contentY + 12);

                g.DrawString("ИРЭХ / TO", _labelFont, _grayBrush, leftCol + 120, contentY);
                g.DrawString(_boardingPass.ArrivalAirport ?? "N/A", _dataFont, _blackBrush, leftCol + 120, contentY + 12);

                g.DrawString("НИСЛЭГ / FLIGHT", _labelFont, _grayBrush, rightCol, contentY);
                g.DrawString(_boardingPass.FlightNumber ?? "N/A", _largeFont, _blackBrush, rightCol, contentY + 12);
                contentY += rowHeight;

                // Date, time, and seat
                g.DrawString("ОГНОО / DATE", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("dd-MMM-yyyy"), _dataFont, _blackBrush, leftCol, contentY + 12);

                g.DrawString("ЦАГ / TIME", _labelFont, _grayBrush, leftCol + 120, contentY);
                g.DrawString(_boardingPass.DepartureTime.ToString("HH:mm"), _dataFont, _blackBrush, leftCol + 120, contentY + 12);

                g.DrawString("СУУДАЛ / SEAT", _labelFont, _grayBrush, rightCol, contentY);
                g.DrawString(_boardingPass.SeatNumber ?? "N/A", _largeFont, _blackBrush, rightCol, contentY + 12);
                contentY += rowHeight;

                // Boarding time and gate
                g.DrawString("СУУХ ЦАГ / BOARDING TIME", _labelFont, _grayBrush, leftCol, contentY);
                g.DrawString(_boardingPass.BoardingTime.ToString("HH:mm"), _dataFont, _blackBrush, leftCol, contentY + 12);

                g.DrawString("ХААЛГА / GATE", _labelFont, _grayBrush, leftCol + 150, contentY);
                g.DrawString("TBD", _dataFont, _blackBrush, leftCol + 150, contentY + 12);
                contentY += rowHeight + 15;

                // Separator line
                using (var separatorPen = new Pen(Color.FromArgb(189, 195, 199), 1))
                {
                    g.DrawLine(separatorPen, leftCol, contentY, mainRect.Right - padding, contentY);
                }
                contentY += 15;

                // Footer info
                g.DrawString($"ПАСПОРТ: {_boardingPass.PassportNumber ?? "N/A"}", _smallFont, _grayBrush, leftCol, contentY);
                string printTime = $"ХЭВЛЭСЭН: {DateTime.Now:dd-MMM-yyyy HH:mm}";
                SizeF printSize = g.MeasureString(printTime, _smallFont);
                g.DrawString(printTime, _smallFont, _grayBrush, mainRect.Right - padding - printSize.Width, contentY);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[BoardingPassPrinter] Error disposing resources: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BoardingPassPrinter()
        {
            Dispose(false);
        }
    }
}