using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace FlightCheckInSystem.FormsApp.Services
{
    public class BoardingPassPrinter
    {
        private BoardingPass _boardingPass;
        private Font _headerFont;
        private Font _regularFont;
        private Font _boldFont;
        private Font _smallFont;
        private Brush _blackBrush;
        private Brush _blueBrush;
        
        public BoardingPassPrinter()
        {
            InitializeFonts();
        }

        private void InitializeFonts()
        {
            _headerFont = new Font("Arial", 16, FontStyle.Bold);
            _regularFont = new Font("Arial", 10, FontStyle.Regular);
            _boldFont = new Font("Arial", 10, FontStyle.Bold);
            _smallFont = new Font("Arial", 8, FontStyle.Regular);
            _blackBrush = new SolidBrush(Color.Black);
            _blueBrush = new SolidBrush(Color.Blue);
        }

        public void PrintBoardingPass(BoardingPass boardingPass)
        {
            if (boardingPass == null)
            {
                MessageBox.Show("No boarding pass data to print.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    Debug.WriteLine($"[BoardingPassPrinter] Successfully printed boarding pass for {boardingPass.PassengerName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BoardingPassPrinter] Error printing boarding pass: {ex.Message}");
                    MessageBox.Show($"Error printing boarding pass: {ex.Message}", "Print Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void ShowPrintPreview(BoardingPass boardingPass, IWin32Window owner = null)
        {
            if (boardingPass == null)
            {
                MessageBox.Show(owner, "No boarding pass data to preview.", "Preview Error", 
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
                    Text = $"Boarding Pass - {boardingPass.FlightNumber} - {boardingPass.PassengerName}",
                    ShowIcon = true,
                    ShowInTaskbar = true,
                    UseAntiAlias = true
                };

                                var printButton = new ToolStripButton("Print", null, (s, e) =>
                {
                    try
                    {
                        PrintBoardingPass(boardingPass);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(previewDialog, $"Error printing: {ex.Message}", 
                            "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                Debug.WriteLine($"[BoardingPassPrinter] Showing print preview for {boardingPass.PassengerName}");
                previewDialog.ShowDialog(owner);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] Error in ShowPrintPreview: {ex}");
                MessageBox.Show(owner, $"Error showing boarding pass preview: {ex.Message}", 
                    "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                                RectangleF bounds = e.MarginBounds;
                float x = bounds.Left;
                float y = bounds.Top;
                float width = bounds.Width;

                                DrawBoardingPassContent(g, x, y, width);
                
                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassPrinter] Error in PrintDocument_PrintPage: {ex}");
                e.Cancel = true;
                throw;
            }
        }

        private void DrawBoardingPassContent(Graphics g, float x, float y, float width)
        {
            float currentY = y;
            float lineHeight = _regularFont.Height;
            float sectionSpacing = 20;
            float padding = 15f;
            float boxWidth = width - (padding * 2);
            
                                    using (var bgBrush = new SolidBrush(Color.FromArgb(245, 245, 245)))
            using (var borderPen = new Pen(Color.FromArgb(200, 200, 200), 1.5f))
            {
                RectangleF borderRect = new RectangleF(x, y, width, 500);
                g.FillRectangle(bgBrush, borderRect);
                g.DrawRectangle(borderPen, Rectangle.Round(borderRect));
                
                                x += padding;
                currentY += padding;
                width -= padding * 2;
            }

                        using (var headerBrush = new LinearGradientBrush(
                new RectangleF(x, currentY, width, 40),
                Color.FromArgb(0, 0, 128),                  Color.FromArgb(30, 144, 255),                 LinearGradientMode.Horizontal))
            {
                g.FillRectangle(headerBrush, x, currentY, width, 40);
                
                string header = "BOARDING PASS";
                var headerSize = g.MeasureString(header, _headerFont);
                float headerX = x + (width - headerSize.Width) / 2;
                g.DrawString(header, _headerFont, Brushes.White, headerX, currentY + 5);
                
                currentY += 45;             }

                        try
            {
                var airlineIcon = SystemIcons.Information;
                g.DrawIcon(airlineIcon, (int)x, (int)currentY);
            }
            catch
            {
                            }

                        using (var passengerTitleFont = new Font(_boldFont.FontFamily, 11, FontStyle.Bold))
            {
                g.DrawString("PASSENGER", passengerTitleFont, _blackBrush, x, currentY);
                currentY += lineHeight;
                
                using (var passengerNameFont = new Font(_regularFont.FontFamily, 14, FontStyle.Bold))
                {
                    g.DrawString(_boardingPass.PassengerName.ToUpper(), passengerNameFont, _blackBrush, x, currentY);
                    currentY += lineHeight * 1.5f;
                }
                
                                if (!string.IsNullOrEmpty(_boardingPass.PassportNumber))
                {
                    g.DrawString($"PASSPORT: {_boardingPass.PassportNumber}", _smallFont, _blackBrush, x, currentY);
                    currentY += lineHeight * 0.8f;
                }
            }

                        g.DrawString("FLIGHT DETAILS", _boldFont, _blackBrush, x, currentY);
            currentY += lineHeight;

                        float col1X = x + 20;
            float col2X = x + width / 2;

            g.DrawString($"Flight: {_boardingPass.FlightNumber}", _regularFont, _blackBrush, col1X, currentY);
            g.DrawString($"Seat: {_boardingPass.SeatNumber}", _regularFont, _blackBrush, col2X, currentY);
            currentY += lineHeight;

            g.DrawString($"From: {_boardingPass.DepartureAirport}", _regularFont, _blackBrush, col1X, currentY);
            g.DrawString($"To: {_boardingPass.ArrivalAirport}", _regularFont, _blackBrush, col2X, currentY);
            currentY += lineHeight + sectionSpacing;

                        g.DrawString("TIME DETAILS", _boldFont, _blackBrush, x, currentY);
            currentY += lineHeight;

            g.DrawString($"Departure: {_boardingPass.DepartureTime:MMM dd, yyyy HH:mm}", _regularFont, _blackBrush, col1X, currentY);
            g.DrawString($"Boarding: {_boardingPass.BoardingTime:MMM dd, yyyy HH:mm}", _regularFont, _blackBrush, col2X, currentY);
            currentY += lineHeight + sectionSpacing;

                        g.DrawString($"Passport: {_boardingPass.PassportNumber}", _regularFont, _blackBrush, x + 20, currentY);
            currentY += lineHeight + sectionSpacing;

                        g.DrawLine(new Pen(Color.Gray, 1), x, currentY, x + width - 20, currentY);
            currentY += 10;

                        g.DrawString("IMPORTANT NOTICES:", _boldFont, _blackBrush, x, currentY);
            currentY += lineHeight;

            string[] notices = {
                "â€¢ Please arrive at the gate 30 minutes before boarding time",
                "â€¢ Valid photo ID and boarding pass required",
                "â€¢ Check airline policies for baggage restrictions",
                "â€¢ Gate information may change - check displays"
            };

            foreach (string notice in notices)
            {
                g.DrawString(notice, _smallFont, _blackBrush, x + 10, currentY);
                currentY += 15;
            }

            currentY += 10;

                        string footer = $"Generated on: {DateTime.Now:MMM dd, yyyy HH:mm} | Have a pleasant flight!";
            SizeF footerSize = g.MeasureString(footer, _smallFont);
            float footerX = x + (width - footerSize.Width) / 2;
            g.DrawString(footer, _smallFont, _blackBrush, footerX, currentY);

                    }

        public void Dispose()
        {
            _headerFont?.Dispose();
            _regularFont?.Dispose();
            _boldFont?.Dispose();
            _smallFont?.Dispose();
            _blackBrush?.Dispose();
            _blueBrush?.Dispose();
        }
    }
}