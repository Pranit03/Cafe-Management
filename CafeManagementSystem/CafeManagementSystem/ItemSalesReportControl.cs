// ItemSalesReportControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Text;

namespace CafeManagementSystem
{
    public partial class ItemSalesReportControl : UserControl
    {
        private Panel filterPanel;
        private Label lblStartDate, lblEndDate;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private RoundedButton btnGenerate, btnExport, btnPrint;
        private DataGridView dgvReport;

        public ItemSalesReportControl()
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
            btnPrint = new RoundedButton { Text = "Print", CornerRadius = 8, Location = new Point(800, 15), Size = new Size(100, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(92, 107, 123), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnExport = new RoundedButton { Text = "Export (CSV)", CornerRadius = 8, Location = new Point(910, 15), Size = new Size(130, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 167, 69), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            filterPanel.Controls.AddRange(new Control[] { lblStartDate, dtpStartDate, lblEndDate, dtpEndDate, btnGenerate, btnPrint, btnExport });

            dgvReport = new DataGridView();
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.BackgroundColor = Color.White; dgvReport.BorderStyle = BorderStyle.None; dgvReport.AllowUserToAddRows = false; dgvReport.AutoGenerateColumns = false; dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvReport.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68) }; dgvReport.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10) }; dgvReport.RowTemplate.Height = 40; dgvReport.RowHeadersVisible = false;

            var colSrNo = new DataGridViewTextBoxColumn { HeaderText = "Sr. No.", Name = "SrNo", FillWeight = 10 };
            var colItemName = new DataGridViewTextBoxColumn { HeaderText = "Item Name", DataPropertyName = "item_name", FillWeight = 35 };
            var colCategory = new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "category", FillWeight = 25 };
            var colQtySold = new DataGridViewTextBoxColumn { HeaderText = "Quantity Sold", DataPropertyName = "TotalQuantity", FillWeight = 15 };
            var colTotalRevenue = new DataGridViewTextBoxColumn { HeaderText = "Total Revenue", DataPropertyName = "TotalRevenue", FillWeight = 15, DefaultCellStyle = { Format = "c2" } };
            dgvReport.Columns.AddRange(new[] { colSrNo, colItemName, colCategory, colQtySold, colTotalRevenue });

            this.Controls.Add(dgvReport);
            this.Controls.Add(filterPanel);

            btnGenerate.Click += BtnGenerate_Click;
            btnPrint.Click += BtnPrint_Click;
            btnExport.Click += BtnExport_Click;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddTicks(-1);

            string query = @"
                SELECT 
                    mi.item_name, 
                    mi.category, 
                    SUM(od.quantity) AS TotalQuantity, 
                    SUM(od.quantity * od.price_at_time_of_order) AS TotalRevenue
                FROM order_details od
                JOIN menu_items mi ON od.item_id = mi.item_id
                JOIN orders o ON od.order_id = o.order_id
                WHERE o.status = 'Paid' AND o.order_date BETWEEN @StartDate AND @EndDate
                GROUP BY mi.item_name, mi.category
                ORDER BY TotalRevenue DESC"; // Show best-selling items first

            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                da.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                var dt = new DataTable();
                da.Fill(dt);
                dgvReport.DataSource = dt;

                // Add Sr. No. manually after data binding
                for (int i = 0; i < dgvReport.Rows.Count; i++)
                {
                    dgvReport.Rows[i].Cells["SrNo"].Value = i + 1;
                }
            }
        }

        // --- The Print and Export methods are very similar to SalesReportControl ---
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0) { MessageBox.Show("There is no data to export.", "No Data"); return; }
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "CSV File (*.csv)|*.csv", Title = "Save Item-wise Sales Report", FileName = $"ItemSalesReport_{DateTime.Now:yyyyMMdd}.csv" };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();
                    var headers = new List<string>();
                    foreach (DataGridViewColumn col in dgvReport.Columns) { headers.Add($"\"{col.HeaderText}\""); }
                    sb.AppendLine(string.Join(",", headers));
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        var cells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string cellValue = (cell.FormattedValue ?? string.Empty).ToString().Replace("\"", "\"\"");
                            cells.Add($"\"{cellValue}\"");
                        }
                        sb.AppendLine(string.Join(",", cells));
                    }
                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully!", "Success");
                }
                catch (Exception ex) { MessageBox.Show("Error exporting report: " + ex.Message, "Export Error"); }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0) { MessageBox.Show("There is no data to print.", "No Data"); return; }
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
            g.DrawString("Item-wise Sales Report", headerFont, Brushes.Black, startX, startY);
            string dateRange = $"From: {dtpStartDate.Value:dd-MMM-yyyy} To: {dtpEndDate.Value:dd-MMM-yyyy}";
            g.DrawString(dateRange, font, Brushes.Black, startX, startY + 30); offset += 60;
            int[] colWidths = { 60, 250, 150, 150, 150 };
            int currentX = startX;
            for (int i = 0; i < dgvReport.Columns.Count; i++) { g.DrawString(dgvReport.Columns[i].HeaderText, new Font(font, FontStyle.Bold), Brushes.Black, currentX, startY + offset); currentX += colWidths[i]; }
            offset += 30;
            foreach (DataGridViewRow row in dgvReport.Rows)
            {
                currentX = startX;
                for (int i = 0; i < row.Cells.Count; i++) { g.DrawString(row.Cells[i].FormattedValue.ToString(), font, Brushes.Black, currentX, startY + offset); currentX += colWidths[i]; }
                offset += 25;
                if (offset > e.MarginBounds.Height) { e.HasMorePages = true; return; }
            }
            e.HasMorePages = false;
        }
    }
}