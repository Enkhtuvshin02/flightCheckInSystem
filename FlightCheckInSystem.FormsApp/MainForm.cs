using System; // Only keep essentials for main navigation
using System.Drawing;
using System.Windows.Forms;

namespace FlightCheckInSystem.FormsApp
{
    public partial class MainForm : Form
    {
        private Form _currentForm;
        private Button _activeButton;
        private Color _defaultButtonColor;
        private Color _activeButtonColor = Color.FromArgb(0, 90, 158);

        public MainForm()
        {
            InitializeComponent();

            // Set up button text
            btnBooking.Text = "Booking";
            btnCheckIn.Text = "Check-In";
            btnFlightManagement.Text = "Flight Management";

            // Store default button color
            _defaultButtonColor = btnBooking.BackColor;

            // Wire up button click events
            btnBooking.Click += BtnBooking_Click;
            btnCheckIn.Click += BtnCheckIn_Click;
            btnFlightManagement.Click += BtnFlightManagement_Click;
            
            // Show default form - start with Booking form
            BtnBooking_Click(btnBooking, EventArgs.Empty);
        }

        private void BtnBooking_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new BookingForm());
            lblStatus.Text = "Booking management mode active";
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new CheckInForm());
            lblStatus.Text = "Check-in management mode active";
        }

        private void BtnFlightManagement_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new FlightManagementForm());
            lblStatus.Text = "Flight management mode active";
        }
        
        private void ActivateButton(Button button)
        {
            if (_activeButton != null)
            {
                // Reset previous button
                _activeButton.BackColor = _defaultButtonColor;
            }
            
            // Highlight new button
            _activeButton = button;
            _activeButton.BackColor = _activeButtonColor;
        }

        private void ShowForm(Form form)
        {            
            // Close the current form if it exists
            if (_currentForm != null)
            {
                _currentForm.Close();
                _currentForm.Dispose();
            }

            // Set up the new form
            _currentForm = form;
            _currentForm.TopLevel = false;
            _currentForm.FormBorderStyle = FormBorderStyle.None;
            _currentForm.Dock = DockStyle.Fill;

            // Add to panel and show
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(_currentForm);
            _currentForm.Show();
        }
    }
}
