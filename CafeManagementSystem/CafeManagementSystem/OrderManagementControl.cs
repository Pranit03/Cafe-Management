// OrderManagementControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;

namespace CafeManagementSystem
{
    public class OrderItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Quantity * Price;
    }

    public class MenuItemDisplay
    {
        public int ItemId { get; set; }
        public string DisplayName { get; set; }
        public decimal Price { get; set; }
        public override string ToString() { return DisplayName; }
    }

    public partial class OrderManagementControl : UserControl
    {
        private Panel headerPanel, summaryPanel, leftInputPanel;
        private Label lblTitle, lblSubtotal, lblTax, lblTotal, lblSelectItem, lblQuantity;
        private RoundedButton btnBackToTables, btnProceedToPay, btnPrintBill, btnAddToOrder;
        private SplitContainer splitContainer;
        private ComboBox cmbMenuItems;
        private NumericUpDown numQuantity;
        private DataGridView dgvCurrentOrder;
        private int tableId, loggedInStaffId;
        private string tableName;
        private List<OrderItem> currentOrderItems = new List<OrderItem>();
        private decimal taxRate = 0.05m;
        private int? activeOrderId = null;

        public event EventHandler BackToTablesClicked, OrderCompleted;

        public OrderManagementControl(int tableId, string tableName, int staffId)
        {
            this.tableId = tableId;
            this.tableName = tableName;
            this.loggedInStaffId = staffId;
            InitializeComponent();
            LoadMenuItemsIntoComboBox();
            LoadActiveOrderForTable();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Color.White;
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            lblTitle = new Label { Text = $"Order for {this.tableName}", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(68, 68, 68), Dock = DockStyle.Left, AutoSize = true };
            btnBackToTables = new RoundedButton { Text = "< Back to Tables", CornerRadius = 10, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(108, 117, 125), FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Dock = DockStyle.Right, Cursor = Cursors.Hand };
            btnBackToTables.FlatAppearance.BorderSize = 0;
            headerPanel.Controls.AddRange(new Control[] { lblTitle, btnBackToTables });

            splitContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 350, IsSplitterFixed = true, BorderStyle = BorderStyle.FixedSingle };
            this.Resize += (s, e) => { if (this.ClientSize.Width > 0) splitContainer.SplitterDistance = (int)(this.ClientSize.Width * 0.35); };

            leftInputPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.FromArgb(249, 249, 249) };
            lblSelectItem = new Label { Text = "Select Item", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            cmbMenuItems = new ComboBox { Location = new Point(20, 60), Size = new Size(300, 30), Font = new Font("Segoe UI", 12F), DropDownStyle = ComboBoxStyle.DropDownList };
            lblQuantity = new Label { Text = "Quantity", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 110), AutoSize = true };
            numQuantity = new NumericUpDown { Location = new Point(20, 140), Size = new Size(100, 30), Font = new Font("Segoe UI", 12F), Minimum = 1, Value = 1 };
            btnAddToOrder = new RoundedButton { Text = "Add to Order", CornerRadius = 8, Location = new Point(20, 200), Size = new Size(150, 45), Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(0, 123, 255), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAddToOrder.FlatAppearance.BorderSize = 0;
            leftInputPanel.Controls.AddRange(new Control[] { lblSelectItem, cmbMenuItems, lblQuantity, numQuantity, btnAddToOrder });

            dgvCurrentOrder = new DataGridView();
            dgvCurrentOrder.Dock = DockStyle.Fill; dgvCurrentOrder.BackgroundColor = Color.White; dgvCurrentOrder.BorderStyle = BorderStyle.None; dgvCurrentOrder.AllowUserToAddRows = false; dgvCurrentOrder.AutoGenerateColumns = false; dgvCurrentOrder.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvCurrentOrder.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68), Padding = new Padding(5) }; dgvCurrentOrder.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10), Padding = new Padding(5) }; dgvCurrentOrder.RowTemplate.Height = 35; dgvCurrentOrder.RowHeadersVisible = false;
            var colItem = new DataGridViewTextBoxColumn { HeaderText = "Item", DataPropertyName = "ItemName", FillWeight = 40, ReadOnly = true };
            var colQty = new DataGridViewTextBoxColumn { HeaderText = "Qty", DataPropertyName = "Quantity", FillWeight = 15 };
            var colPrice = new DataGridViewTextBoxColumn { HeaderText = "Price", DataPropertyName = "Price", FillWeight = 20, ReadOnly = true, DefaultCellStyle = { Format = "c2" } };
            var colSub = new DataGridViewTextBoxColumn { HeaderText = "Subtotal", DataPropertyName = "Subtotal", FillWeight = 25, ReadOnly = true, DefaultCellStyle = { Format = "c2" } };
            dgvCurrentOrder.Columns.AddRange(new[] { colItem, colQty, colPrice, colSub });

            summaryPanel = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.White, Padding = new Padding(10) };
            lblSubtotal = new Label { Text = "Subtotal: ₹0.00", Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Height = 25 };
            lblTax = new Label { Text = $"Tax ({taxRate:P0}): ₹0.00", Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Height = 25 };
            lblTotal = new Label { Text = "Total: ₹0.00", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.Black, Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Height = 30 };
            btnProceedToPay = new RoundedButton { Text = "Proceed to Pay", CornerRadius = 10, Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Size = new Size(180, 50), Location = new Point(10, 10), FlatStyle = FlatStyle.Flat };
            btnPrintBill = new RoundedButton { Text = "Print Bill", CornerRadius = 10, Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, Size = new Size(180, 50), Location = new Point(10, 10), FlatStyle = FlatStyle.Flat, Visible = false };
            summaryPanel.Controls.AddRange(new Control[] { btnProceedToPay, btnPrintBill, lblTotal, lblTax, lblSubtotal });

            splitContainer.Panel1.Controls.Add(leftInputPanel);
            splitContainer.Panel2.Controls.AddRange(new Control[] { dgvCurrentOrder, summaryPanel });
            this.Controls.AddRange(new Control[] { splitContainer, headerPanel });

            btnBackToTables.Click += (s, e) => BackToTablesClicked?.Invoke(this, EventArgs.Empty);
            btnAddToOrder.Click += BtnAddToOrder_Click;
            btnProceedToPay.Click += BtnProceedToPay_Click;
            btnPrintBill.Click += BtnPrintBill_Click;
            dgvCurrentOrder.CellValueChanged += DgvCurrentOrder_CellValueChanged;
        }

        private void LoadActiveOrderForTable()
        {
            string findOrderQuery = "SELECT order_id FROM orders WHERE table_id = @TableId AND status = 'Active'";
            object orderIdResult;
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SqlCommand(findOrderQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TableId", this.tableId);
                    con.Open();
                    orderIdResult = cmd.ExecuteScalar();
                }
                if (orderIdResult == null || orderIdResult == DBNull.Value) { return; }
                this.activeOrderId = (int)orderIdResult;

                string detailsQuery = @"SELECT od.item_id, mi.item_name, od.quantity, od.price_at_time_of_order FROM order_details od JOIN menu_items mi ON od.item_id = mi.item_id WHERE od.order_id = @OrderId";
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SqlCommand(detailsQuery, con))
                {
                    cmd.Parameters.AddWithValue("@OrderId", this.activeOrderId.Value);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        currentOrderItems.Clear();
                        while (reader.Read())
                        {
                            currentOrderItems.Add(new OrderItem { ItemId = (int)reader["item_id"], ItemName = reader["item_name"].ToString(), Quantity = (int)reader["quantity"], Price = (decimal)reader["price_at_time_of_order"] });
                        }
                    }
                }
                UpdateOrderGridAndTotals();
            }
            catch (Exception ex) { MessageBox.Show("Failed to load active order: " + ex.Message, "Database Error"); }
        }

        private void LoadMenuItemsIntoComboBox()
        {
            var items = new BindingList<MenuItemDisplay>();
            string query = "SELECT item_id, item_name, price FROM menu_items ORDER BY item_name";
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) { MessageBox.Show("No menu items found in the database.", "Menu Empty"); return; }
                        while (reader.Read())
                        {
                            items.Add(new MenuItemDisplay { ItemId = (int)reader["item_id"], DisplayName = reader["item_name"].ToString(), Price = (decimal)reader["price"] });
                        }
                    }
                }
                cmbMenuItems.DataSource = items;
                cmbMenuItems.DisplayMember = "DisplayName";
                cmbMenuItems.ValueMember = "ItemId";
                cmbMenuItems.SelectedIndex = -1;
                cmbMenuItems.Text = "--- Select an Item ---";
            }
            catch (Exception ex) { MessageBox.Show("Failed to load menu items.\nError: " + ex.Message, "Database Error"); }
        }

        private void BtnAddToOrder_Click(object sender, EventArgs e)
        {
            if (cmbMenuItems.SelectedItem == null) { MessageBox.Show("Please select an item from the list.", "No Item Selected"); return; }
            if (!activeOrderId.HasValue) { CreateNewOrderInDatabase(); }
            if (!activeOrderId.HasValue) { MessageBox.Show("Could not create an order. Please try again.", "Error"); return; }
            var selectedItem = cmbMenuItems.SelectedItem as MenuItemDisplay;
            int quantity = (int)numQuantity.Value;
            var existingItem = currentOrderItems.Find(i => i.ItemId == selectedItem.ItemId);
            if (existingItem != null) { existingItem.Quantity += quantity; }
            else { currentOrderItems.Add(new OrderItem { ItemId = selectedItem.ItemId, ItemName = selectedItem.DisplayName, Price = selectedItem.Price, Quantity = quantity }); }
            AddItemToDatabase(selectedItem.ItemId, quantity, selectedItem.Price);
            UpdateOrderGridAndTotals();
            cmbMenuItems.SelectedIndex = -1; numQuantity.Value = 1;
        }

        private void CreateNewOrderInDatabase()
        {
            using (var con = DatabaseHelper.GetConnection())
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    string orderQuery = "INSERT INTO orders (user_id, total_amount, status, table_id) OUTPUT INSERTED.order_id VALUES (@UserId, 0, 'Active', @TableId)";
                    using (var cmd = new SqlCommand(orderQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@UserId", this.loggedInStaffId);
                        cmd.Parameters.AddWithValue("@TableId", this.tableId);
                        this.activeOrderId = (int)cmd.ExecuteScalar();
                    }
                    string tableQuery = "UPDATE tables SET status = 'Occupied' WHERE table_id = @TableId";
                    using (var cmd = new SqlCommand(tableQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@TableId", this.tableId);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Failed to create new order: " + ex.Message, "Error");
                    this.activeOrderId = null;
                }
            }
        }

        private void AddItemToDatabase(int itemId, int quantity, decimal price)
        {
            string checkQuery = "SELECT quantity FROM order_details WHERE order_id = @OrderId AND item_id = @ItemId";
            int existingQty = 0;
            using (var con = DatabaseHelper.GetConnection())
            {
                con.Open();
                using (var checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@OrderId", activeOrderId.Value);
                    checkCmd.Parameters.AddWithValue("@ItemId", itemId);
                    var result = checkCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) existingQty = (int)result;
                }
                string query = (existingQty > 0) ? "UPDATE order_details SET quantity = @NewQty WHERE order_id = @OrderId AND item_id = @ItemId" : "INSERT INTO order_details (order_id, item_id, quantity, price_at_time_of_order) VALUES (@OrderId, @ItemId, @NewQty, @Price)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@OrderId", activeOrderId.Value);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@NewQty", existingQty + quantity);
                    if (existingQty == 0) cmd.Parameters.AddWithValue("@Price", price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateOrderGridAndTotals()
        {
            dgvCurrentOrder.DataSource = null;
            if (currentOrderItems.Any()) { dgvCurrentOrder.DataSource = currentOrderItems.ToList(); }
            decimal subtotal = currentOrderItems.Sum(item => item.Subtotal);
            decimal tax = subtotal * taxRate;
            decimal total = subtotal + tax;
            lblSubtotal.Text = $"Subtotal: {subtotal:c2}";
            lblTax.Text = $"Tax ({taxRate:P0}): {tax:c2}";
            lblTotal.Text = $"Total: {total:c2}";
        }

        private void DgvCurrentOrder_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvCurrentOrder.Columns["Quantity"].Index) return;
            try
            {
                int newQty = Convert.ToInt32(dgvCurrentOrder.Rows[e.RowIndex].Cells["Quantity"].Value);
                if (newQty <= 0) { currentOrderItems.RemoveAt(e.RowIndex); }
                else { currentOrderItems[e.RowIndex].Quantity = newQty; }
            }
            catch (FormatException) { dgvCurrentOrder.Rows[e.RowIndex].Cells["Quantity"].Value = currentOrderItems[e.RowIndex].Quantity; }
            UpdateOrderGridAndTotals();
        }

        private void BtnProceedToPay_Click(object sender, EventArgs e)
        {
            if (currentOrderItems.Count == 0) { MessageBox.Show("Cannot process an empty order.", "Warning"); return; }
            if (decimal.TryParse(lblTotal.Text.Replace("Total: ", "").Replace("₹", "").Trim(), out decimal totalAmount))
            {
                using (var paymentForm = new PaymentForm(totalAmount))
                {
                    if (paymentForm.ShowDialog() == DialogResult.OK) { FinalizeOrder(paymentForm.SelectedPaymentMethod, totalAmount); }
                }
            }
            else { MessageBox.Show("Could not calculate the total amount.", "Calculation Error"); }
        }

        private void FinalizeOrder(string paymentMethod, decimal totalAmount)
        {
            if (!activeOrderId.HasValue) { MessageBox.Show("No active order to process.", "Error"); return; }
            using (var con = DatabaseHelper.GetConnection())
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    string orderQuery = "UPDATE orders SET total_amount = @Total, status = 'Paid', payment_method = @PaymentMethod WHERE order_id = @OrderId";
                    using (var cmd = new SqlCommand(orderQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Total", totalAmount);
                        cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                        cmd.Parameters.AddWithValue("@OrderId", activeOrderId.Value);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Payment successful! Bill is ready to print.", "Success");

                    btnProceedToPay.Visible = false;
                    btnPrintBill.Visible = true;
                    leftInputPanel.Enabled = false;

                    // --- MODIFIED LINE ---
                    // This line caused the automatic redirection. It is now commented out.
                    // OrderCompleted?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("An error occurred: " + ex.Message, "Transaction Failed");
                }
            }
        }

        private void BtnPrintBill_Click(object sender, EventArgs e)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
            PrintPreviewDialog preview = new PrintPreviewDialog { Document = printDoc };
            preview.ShowDialog(this);
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Courier New", 12), headerFont = new Font("Courier New", 16, FontStyle.Bold);
            int startX = 50, startY = 50, offset = 40;
            g.DrawString("Your Café Name", headerFont, Brushes.Black, startX, startY);
            g.DrawString("----------------------------------------", font, Brushes.Black, startX, startY + offset); offset += 40;
            g.DrawString($"Bill for Table: {tableName}", font, Brushes.Black, startX, startY + offset);
            g.DrawString($"Date: {DateTime.Now:dd-MM-yyyy hh:mm tt}", font, Brushes.Black, startX, startY + offset + 20); offset += 60;
            g.DrawString("Item".PadRight(20) + "Qty".PadRight(5) + "Price".PadRight(10) + "Total", font, Brushes.Black, startX, startY + offset); offset += 20;
            g.DrawString("----------------------------------------", font, Brushes.Black, startX, startY + offset); offset += 30;
            foreach (var item in currentOrderItems) { string itemLine = item.ItemName.PadRight(20) + item.Quantity.ToString().PadRight(5) + item.Price.ToString("c2").PadRight(10) + item.Subtotal.ToString("c2"); g.DrawString(itemLine, font, Brushes.Black, startX, startY + offset); offset += 20; }
            offset += 10; g.DrawString("----------------------------------------", font, Brushes.Black, startX, startY + offset); offset += 30;
            g.DrawString(lblSubtotal.Text.PadLeft(40), font, Brushes.Black, startX, startY + offset); offset += 20;
            g.DrawString(lblTax.Text.PadLeft(40), font, Brushes.Black, startX, startY + offset); offset += 20;
            g.DrawString(lblTotal.Text.PadLeft(40), new Font("Courier New", 12, FontStyle.Bold), Brushes.Black, startX, startY + offset); offset += 40;
            g.DrawString("Thank You! Visit Again!", font, Brushes.Black, startX + 70, startY + offset);
        }
    }
}