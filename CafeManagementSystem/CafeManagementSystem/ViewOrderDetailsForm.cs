// ViewOrderDetailsForm.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class ViewOrderDetailsForm : Form
    {
        private readonly int orderId;
        private DataGridView dgvOrderDetails;
        private Label lblTitle, lblTotal;
        private RoundedButton btnClose;

        public ViewOrderDetailsForm(int orderId)
        {
            this.orderId = orderId;
            InitializeComponent();
            LoadOrderDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Order Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(500, 450);
            this.BackColor = Color.White;

            lblTitle = new Label { Text = $"Details for Order #{this.orderId}", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = Color.FromArgb(68, 68, 68) };

            dgvOrderDetails = new DataGridView();
            dgvOrderDetails.Location = new Point(20, 70);
            dgvOrderDetails.Size = new Size(460, 280);
            dgvOrderDetails.BackgroundColor = Color.White;
            dgvOrderDetails.BorderStyle = BorderStyle.Fixed3D;
            dgvOrderDetails.AllowUserToAddRows = false;
            dgvOrderDetails.ReadOnly = true;
            dgvOrderDetails.AutoGenerateColumns = false;
            dgvOrderDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrderDetails.RowHeadersVisible = false;
            dgvOrderDetails.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(242, 242, 242) };
            dgvOrderDetails.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9) };

            var colItemName = new DataGridViewTextBoxColumn { HeaderText = "Item Name", FillWeight = 40, DataPropertyName = "item_name" };
            var colQty = new DataGridViewTextBoxColumn { HeaderText = "Quantity", FillWeight = 20, DataPropertyName = "quantity" };
            var colPrice = new DataGridViewTextBoxColumn { HeaderText = "Price", FillWeight = 20, DataPropertyName = "price_at_time_of_order", DefaultCellStyle = { Format = "c2" } };
            var colSubtotal = new DataGridViewTextBoxColumn { HeaderText = "Subtotal", FillWeight = 20, DataPropertyName = "Subtotal", DefaultCellStyle = { Format = "c2" } };
            dgvOrderDetails.Columns.AddRange(new DataGridViewColumn[] { colItemName, colQty, colPrice, colSubtotal });

            lblTotal = new Label { Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(250, 360), Size = new Size(230, 30), TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(68, 68, 68) };

            btnClose = new RoundedButton { Text = "Close", CornerRadius = 10, Location = new Point(380, 400), Size = new Size(100, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnClose.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, dgvOrderDetails, lblTotal, btnClose });
            btnClose.Click += (s, e) => this.Close();
        }

        private void LoadOrderDetails()
        {
            string detailsQuery = @"
                SELECT 
                    mi.item_name, 
                    od.quantity, 
                    od.price_at_time_of_order,
                    (od.quantity * od.price_at_time_of_order) AS Subtotal
                FROM order_details od
                JOIN menu_items mi ON od.item_id = mi.item_id
                WHERE od.order_id = @OrderId";

            string totalQuery = "SELECT total_amount FROM orders WHERE order_id = @OrderId";

            using (var con = DatabaseHelper.GetConnection())
            {
                try
                {
                    con.Open();
                    using (var da = new SqlDataAdapter(detailsQuery, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@OrderId", this.orderId);
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvOrderDetails.DataSource = dt;
                    }
                    using (var cmd = new SqlCommand(totalQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", this.orderId);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            decimal totalAmount = Convert.ToDecimal(result);
                            lblTotal.Text = $"Grand Total: {totalAmount:c2}";
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load order details. " + ex.Message, "Error"); }
            }
        }
    }
}