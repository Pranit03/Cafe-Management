// ViewOrdersControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class ViewOrdersControl : UserControl
    {
        private DataGridView dgvOrders;
        private Label lblTitle;

        public ViewOrdersControl()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            lblTitle = new Label { Text = "Order History", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(68, 68, 68), Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 10), TextAlign = ContentAlignment.MiddleLeft };

            dgvOrders = new DataGridView();
            dgvOrders.Dock = DockStyle.Fill;
            // --- UI STYLE MATCHING OTHER MODULES ---
            dgvOrders.BackgroundColor = Color.White; dgvOrders.BorderStyle = BorderStyle.None; dgvOrders.AllowUserToAddRows = false; dgvOrders.AutoGenerateColumns = false; dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; dgvOrders.EnableHeadersVisualStyles = false; dgvOrders.RowHeadersVisible = false; dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvOrders.RowTemplate.Height = 40;
            dgvOrders.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(5), SelectionBackColor = Color.FromArgb(220, 223, 226) };
            dgvOrders.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10), Padding = new Padding(5), Alignment = DataGridViewContentAlignment.MiddleLeft, SelectionBackColor = Color.FromArgb(204, 232, 255), SelectionForeColor = Color.FromArgb(68, 68, 68) };

            // --- COLUMNS ---
            var colOrderId = new DataGridViewTextBoxColumn { HeaderText = "Order ID", Name = "OrderId", FillWeight = 10 };
            var colDate = new DataGridViewTextBoxColumn { HeaderText = "Date", Name = "Date", FillWeight = 25 };
            var colStaff = new DataGridViewTextBoxColumn { HeaderText = "Staff Name", Name = "Staff", FillWeight = 20 };
            var colTable = new DataGridViewTextBoxColumn { HeaderText = "Table Name", Name = "Table", FillWeight = 15 };
            var colTotal = new DataGridViewTextBoxColumn { HeaderText = "Total Amount", Name = "Total", FillWeight = 15, DefaultCellStyle = { Format = "c2" } };
            var colPayment = new DataGridViewTextBoxColumn { HeaderText = "Payment", Name = "Payment", FillWeight = 15 };
            var colDetails = new DataGridViewButtonColumn { HeaderText = "Action", Name = "Details", Text = "View Details", UseColumnTextForButtonValue = true, FillWeight = 15, FlatStyle = FlatStyle.Flat };
            colDetails.DefaultCellStyle.BackColor = Color.FromArgb(23, 162, 184); // Teal color
            colDetails.DefaultCellStyle.ForeColor = Color.White;
            colDetails.DefaultCellStyle.SelectionBackColor = Color.FromArgb(19, 129, 148);

            dgvOrders.Columns.AddRange(new DataGridViewColumn[] { colOrderId, colDate, colStaff, colTable, colTotal, colPayment, colDetails });

            this.Controls.Add(dgvOrders);
            this.Controls.Add(lblTitle);

            dgvOrders.CellContentClick += DgvOrders_CellContentClick;
        }

        private void LoadOrders()
        {
            // This query joins all necessary tables to get comprehensive order information
            string query = @"
                SELECT 
                    o.order_id, 
                    o.order_date, 
                    u.full_name, 
                    t.table_name,
                    o.total_amount,
                    o.payment_method
                FROM orders o
                JOIN users u ON o.user_id = u.user_id
                LEFT JOIN tables t ON o.table_id = t.table_id
                WHERE o.status = 'Paid'
                ORDER BY o.order_date DESC";

            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                var dt = new DataTable();
                da.Fill(dt);

                dgvOrders.Rows.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    dgvOrders.Rows.Add(
                        row["order_id"],
                        ((DateTime)row["order_date"]).ToString("dd-MMM-yyyy hh:mm tt"),
                        row["full_name"],
                        row["table_name"],
                        row["total_amount"],
                        row["payment_method"]
                    );
                }
            }
        }

        private void DgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvOrders.Columns[e.ColumnIndex].Name != "Details") return;

            int orderId = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["OrderId"].Value);
            using (var form = new ViewOrderDetailsForm(orderId))
            {
                form.ShowDialog();
            }
        }
    }
}