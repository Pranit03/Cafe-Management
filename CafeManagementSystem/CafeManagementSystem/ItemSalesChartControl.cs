// ItemSalesChartControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Required for charts

namespace CafeManagementSystem
{
    public partial class ItemSalesChartControl : UserControl
    {
        private Panel filterPanel;
        private Label lblStartDate, lblEndDate;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private RoundedButton btnGenerate;
        private Chart salesChart; // The main chart control

        public ItemSalesChartControl()
        {
            InitializeComponent();
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpEndDate.Value = DateTime.Now;
            btnGenerate.PerformClick();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // --- Filter Panel (same as other reports) ---
            filterPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10), BackColor = Color.FromArgb(239, 241, 246) };
            lblStartDate = new Label { Text = "Start Date:", Location = new Point(10, 25), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpStartDate = new DateTimePicker { Location = new Point(90, 22), Font = new Font("Segoe UI", 10F) };
            lblEndDate = new Label { Text = "End Date:", Location = new Point(320, 25), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpEndDate = new DateTimePicker { Location = new Point(400, 22), Font = new Font("Segoe UI", 10F) };
            btnGenerate = new RoundedButton { Text = "Generate Chart", CornerRadius = 8, Location = new Point(630, 15), Size = new Size(150, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(0, 123, 255), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            filterPanel.Controls.AddRange(new Control[] { lblStartDate, dtpStartDate, lblEndDate, dtpEndDate, btnGenerate });

            // --- Chart Control ---
            salesChart = new Chart();
            salesChart.Dock = DockStyle.Fill;

            // Define a Chart Area (the plotting area)
            ChartArea chartArea = new ChartArea("MainChartArea");
            chartArea.AxisX.Interval = 1; // Show a label for every item
            chartArea.AxisX.MajorGrid.Enabled = false; // Cleaner look
            chartArea.AxisY.Title = "Revenue (₹)";
            chartArea.AxisY.LabelStyle.Format = "c0"; // Format Y-axis as currency
            salesChart.ChartAreas.Add(chartArea);

            // Define a Series (the actual bars)
            Series series = new Series("ItemSales");
            series.ChartType = SeriesChartType.Column; // Bar chart
            series.IsValueShownAsLabel = true; // Show the value on top of each bar
            series.LabelFormat = "c0";
            series.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            series.Color = Color.FromArgb(23, 162, 184);
            salesChart.Series.Add(series);

            // Add a title to the chart
            salesChart.Titles.Add(new Title("Top Selling Items by Revenue", Docking.Top, new Font("Segoe UI", 16, FontStyle.Bold), Color.FromArgb(68, 68, 68)));

            this.Controls.Add(salesChart);
            this.Controls.Add(filterPanel);

            btnGenerate.Click += BtnGenerate_Click;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddTicks(-1);

            // Same query as Item-wise report, but we'll take the Top 10
            string query = @"
                SELECT TOP 10
                    mi.item_name, 
                    SUM(od.quantity * od.price_at_time_of_order) AS TotalRevenue
                FROM order_details od
                JOIN menu_items mi ON od.item_id = mi.item_id
                JOIN orders o ON od.order_id = o.order_id
                WHERE o.status = 'Paid' AND o.order_date BETWEEN @StartDate AND @EndDate
                GROUP BY mi.item_name
                ORDER BY TotalRevenue DESC";

            var dt = new DataTable();
            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                da.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                da.Fill(dt);
            }

            // --- Bind Data to Chart ---
            salesChart.Series["ItemSales"].Points.Clear(); // Clear old data

            foreach (DataRow row in dt.Rows)
            {
                string itemName = row["item_name"].ToString();
                double totalRevenue = Convert.ToDouble(row["TotalRevenue"]);
                // Add each item as a data point to the chart
                salesChart.Series["ItemSales"].Points.AddXY(itemName, totalRevenue);
            }
        }
    }
}