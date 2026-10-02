// ReportsControl.cs

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CafeManagementSystem
{
    public partial class ReportsControl : UserControl
    {
        private Panel headerPanel, subNavPanel, reportContentPanel;
        private Label lblTitle;
        private Button btnSalesReport, btnItemReport, btnCharts; // New button for charts
        private List<Button> reportNavButtons;

        public ReportsControl()
        {
            InitializeComponent();
            ReportNavButton_Click(btnSalesReport, EventArgs.Empty);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Color.White; this.Padding = new Padding(10);

            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            lblTitle = new Label { Text = "Reports", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(68, 68, 68), Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 10), TextAlign = ContentAlignment.MiddleLeft };
            headerPanel.Controls.Add(lblTitle);

            subNavPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            subNavPanel.Paint += (s, e) => { e.Graphics.DrawLine(Pens.Gainsboro, 0, subNavPanel.Height - 1, subNavPanel.Width, subNavPanel.Height - 1); };

            // --- UPDATED TABS ---
            btnSalesReport = CreateReportNavButton("Sales Report", 0);
            btnItemReport = CreateReportNavButton("Item-Item Sales", btnSalesReport.Width);
            btnCharts = CreateReportNavButton("Itemmi-Chart Sales", btnSalesReport.Width + btnItemReport.Width); // Add the new button

            reportNavButtons = new List<Button> { btnSalesReport, btnItemReport, btnCharts };
            subNavPanel.Controls.AddRange(reportNavButtons.ToArray());

            reportContentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(239, 241, 246), Padding = new Padding(10) };

            this.Controls.Add(reportContentPanel);
            this.Controls.Add(subNavPanel);
            this.Controls.Add(headerPanel);
        }

        private Button CreateReportNavButton(string text, int left)
        {
            Button button = new Button { Text = text, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(left, 0), Height = 50, AutoSize = true, Padding = new Padding(10, 0, 10, 0), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            button.Click += ReportNavButton_Click;
            return button;
        }

        private void SetActiveReportButton(Button activeButton)
        {
            foreach (var button in reportNavButtons)
            {
                button.ForeColor = (button == activeButton) ? Color.FromArgb(0, 122, 204) : Color.Gray;
            }
        }

        private void ReportNavButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            SetActiveReportButton(clickedButton);
            reportContentPanel.Controls.Clear();

            if (clickedButton == btnSalesReport)
            {
                reportContentPanel.Controls.Add(new SalesReportControl());
            }
            else if (clickedButton == btnItemReport)
            {
                reportContentPanel.Controls.Add(new ItemSalesReportControl());
            }
            // --- NEW NAVIGATION LOGIC ---
            else if (clickedButton == btnCharts)
            {
                reportContentPanel.Controls.Add(new ItemSalesChartControl());
            }
        }
    }
}