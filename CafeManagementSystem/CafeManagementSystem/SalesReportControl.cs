// SalesReportControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Text; // Required for UTF-8 Encoding

namespace CafeManagementSystem
{
    public partial class SalesReportControl : UserControl
    {
        private Panel filterPanel, bottomSummaryPanel;
        private Label lblStartDate, lblEndDate, lblTotalRevenue, lblTotalOrders;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private RoundedButton btnGenerate, btnExport, btnPrint;
        private DataGridView dgvSalesReport;

        private Color colorFontDark = Color.FromArgb(68, 68, 68);

        public SalesReportControl()
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

            filterPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10), BackColor = Color.FromArgb(239, 241, 246) };
            lblStartDate = new Label { Text = "Start Date:", Location = new Point(10, 25), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpStartDate = new DateTimePicker { Location = new Point(90, 22), Font = new Font("Segoe UI", 10F) };
            lblEndDate = new Label { Text = "End Date:", Location = new Point(320, 25), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            dtpEndDate = new DateTimePicker { Location = new Point(400, 22), Font = new Font("Segoe UI", 10F) };
            btnGenerate = new RoundedButton { Text = "Generate Report", CornerRadius = 8, Location = new Point(630, 15), Size = new Size(150, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(0, 123, 255), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnPrint = new RoundedButton { Text = "Print", CornerRadius = 8, Location = new Point(800, 15), Size = new Size(100, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(92, 107, 123), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnExport = new RoundedButton { Text = "Export (CSV)", CornerRadius = 8, Location = new Point(910, 15), Size = new Size(130, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 167, 69), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnExport.FlatAppearance.BorderSize = 0;
            filterPanel.Controls.AddRange(new Control[] { lblStartDate, dtpStartDate, lblEndDate, dtpEndDate, btnGenerate, btnPrint, btnExport });

            bottomSummaryPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            bottomSummaryPanel.Paint += (s, e) => { e.Graphics.DrawLine(Pens.Gainsboro, 0, 0, bottomSummaryPanel.Width, 0); };
            lblTotalRevenue = new Label { Text = "Total Revenue: ₹0.00", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = colorFontDark, Dock = DockStyle.Right, AutoSize = true, Padding = new Padding(20, 0, 20, 0), TextAlign = ContentAlignment.MiddleRight };
            lblTotalOrders = new Label { Text = "Total Orders: 0", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = colorFontDark, Dock = DockStyle.Right, AutoSize = true, Padding = new Padding(20, 0, 20, 0), TextAlign = ContentAlignment.MiddleRight };
            bottomSummaryPanel.Controls.Add(lblTotalRevenue);
            bottomSummaryPanel.Controls.Add(lblTotalOrders);

            dgvSalesReport = new DataGridView();
            dgvSalesReport.Dock = DockStyle.Fill;
            dgvSalesReport.BackgroundColor = Color.White; dgvSalesReport.BorderStyle = BorderStyle.None; dgvSalesReport.AllowUserToAddRows = false; dgvSalesReport.AutoGenerateColumns = false; dgvSalesReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvSalesReport.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68) }; dgvSalesReport.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10) }; dgvSalesReport.RowTemplate.Height = 40; dgvSalesReport.RowHeadersVisible = false;
            var colOrderId = new DataGridViewTextBoxColumn { HeaderText = "Order ID", DataPropertyName = "order_id", FillWeight = 10 };
            var colDate = new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "order_date", FillWeight = 25, DefaultCellStyle = { Format = "dd-MMM-yyyy hh:mm tt" } };
            var colStaff = new DataGridViewTextBoxColumn { HeaderText = "Staff Name", DataPropertyName = "full_name", FillWeight = 20 };
            var colTable = new DataGridViewTextBoxColumn { HeaderText = "Table", DataPropertyName = "table_name", FillWeight = 15 };
            var colTotal = new DataGridViewTextBoxColumn { HeaderText = "Total Amount", DataPropertyName = "total_amount", FillWeight = 15, DefaultCellStyle = { Format = "c2" } };
            var colPayment = new DataGridViewTextBoxColumn { HeaderText = "Payment Method", DataPropertyName = "payment_method", FillWeight = 15 };
            dgvSalesReport.Columns.AddRange(new[] { colOrderId, colDate, colStaff, colTable, colTotal, colPayment });

            this.Controls.Add(dgvSalesReport);
            this.Controls.Add(bottomSummaryPanel);
            this.Controls.Add(filterPanel);

            btnGenerate.Click += BtnGenerate_Click;
            btnPrint.Click += BtnPrint_Click;
            btnExport.Click += BtnExport_Click;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddTicks(-1);

            string detailsQuery = @"SELECT o.order_id, o.order_date, u.full_name, t.table_name, o.total_amount, o.payment_method FROM orders o JOIN users u ON o.user_id = u.user_id LEFT JOIN tables t ON o.table_id = t.table_id WHERE o.status = 'Paid' AND o.order_date BETWEEN @StartDate AND @EndDate ORDER BY o.order_date DESC";
            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(detailsQuery, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                da.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                var dt = new DataTable();
                da.Fill(dt);
                dgvSalesReport.DataSource = dt;
            }

            string baseSummaryQuery = "FROM orders WHERE status = 'Paid' AND order_date BETWEEN @StartDate AND @EndDate";
            var p1 = new SqlParameter("@StartDate", startDate); var p2 = new SqlParameter("@EndDate", endDate);
            decimal totalRevenue = Convert.ToDecimal(GetScalarData($"SELECT ISNULL(SUM(total_amount), 0) {baseSummaryQuery}", new[] { p1, p2 }));
            var p3 = new SqlParameter("@StartDate", startDate); var p4 = new SqlParameter("@EndDate", endDate);
            int totalOrders = Convert.ToInt32(GetScalarData($"SELECT COUNT(*) {baseSummaryQuery}", new[] { p3, p4 }));

            lblTotalRevenue.Text = $"Total Revenue: {totalRevenue:c2}";
            lblTotalOrders.Text = $"Total Orders: {totalOrders}";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvSalesReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Sales Report",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();

                    var headers = new List<string>();
                    foreach (DataGridViewColumn col in dgvSalesReport.Columns)
                    {
                        headers.Add($"\"{col.HeaderText}\"");
                    }
                    sb.AppendLine(string.Join(",", headers));

                    foreach (DataGridViewRow row in dgvSalesReport.Rows)
                    {
                        var cells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string cellValue = (cell.FormattedValue ?? string.Empty).ToString().Replace("\"", "\"\"");
                            cells.Add($"\"{cellValue}\"");
                        }
                        sb.AppendLine(string.Join(",", cells));
                    }

                    // Write the file with UTF-8 Encoding to fix the Rupee symbol issue.
                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting report: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (dgvSalesReport.Rows.Count == 0) { MessageBox.Show("There is no data to print.", "No Data"); return; }
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintReport_PrintPage;
            PrintPreviewDialog previewDialog = new PrintPreviewDialog { Document = printDoc };
            previewDialog.ShowDialog(this);
        }

        private void PrintReport_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Courier New", 10), headerFont = new Font("Courier New", 14, FontStyle.Bold);
            int startX = 50, startY = 50, offset = 40;
            g.DrawString("Sales Report", headerFont, Brushes.Black, startX, startY);
            string dateRange = $"From: {dtpStartDate.Value:dd-MMM-yyyy} To: {dtpEndDate.Value:dd-MMM-yyyy}";
            g.DrawString(dateRange, font, Brushes.Black, startX, startY + 30); offset += 60;
            int[] colWidths = { 60, 180, 150, 100, 120, 100 };
            int currentX = startX;
            for (int i = 0; i < dgvSalesReport.Columns.Count; i++) { g.DrawString(dgvSalesReport.Columns[i].HeaderText, new Font(font, FontStyle.Bold), Brushes.Black, currentX, startY + offset); currentX += colWidths[i]; }
            offset += 30;
            foreach (DataGridViewRow row in dgvSalesReport.Rows)
            {
                currentX = startX;
                for (int i = 0; i < row.Cells.Count; i++) { g.DrawString(row.Cells[i].FormattedValue.ToString(), font, Brushes.Black, currentX, startY + offset); currentX += colWidths[i]; }
                offset += 25;
                if (offset > e.MarginBounds.Height) { e.HasMorePages = true; return; }
            }
            e.HasMorePages = false;
        }

        private object GetScalarData(string query, SqlParameter[] parameters = null)
        {
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                if (parameters != null) { cmd.Parameters.AddRange(parameters); }
                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return (result == DBNull.Value) ? 0 : result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database query failed: " + ex.Message, "Error");
                    return 0;
                }
            }
        }
    }
}