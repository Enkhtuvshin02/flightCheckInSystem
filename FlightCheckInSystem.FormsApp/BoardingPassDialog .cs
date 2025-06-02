using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.FormsApp.Services;

namespace FlightCheckInSystem.FormsApp
{
    public partial class BoardingPassDialog : Form
    {
        private BoardingPass _boardingPass;
        private BoardingPassPrinter _printer;

        public BoardingPassDialog(BoardingPass boardingPass)
        {
            _boardingPass = boardingPass;
            _printer = new BoardingPassPrinter();
            InitializeComponent();
            LoadBoardingPassData();
            Debug.WriteLine($"[BoardingPassDialog] Initialized for passenger {boardingPass?.PassengerName}");
        }
    }
}