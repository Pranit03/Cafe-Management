// AddEditMenuItemForm.cs

using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class AddEditMenuItemForm : Form
    {
        private Label lblTitle, lblItemName, lblCategory, lblPrice;
        private TextBox txtItemName, txtPrice;
        private ComboBox cmbCategory;
        private RoundedButton btnSave, btnCancel;

        // Determines if we are in Add (null) or Edit mode.
        private readonly int? itemId;

        public AddEditMenuItemForm(int? id = null)
        {
            this.itemId = id;
            InitializeComponent();

            if (itemId.HasValue)
            {
                lblTitle.Text = "Edit Menu Item";
                LoadItemData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Menu Item Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(400, 350);
            this.BackColor = Color.White;

            lblTitle = new Label { Text = "Add New Menu Item", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = Color.FromArgb(68, 68, 68) };

            lblItemName = new Label { Text = "Item Name", Location = new Point(30, 80), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtItemName = new TextBox { Location = new Point(30, 105), Size = new Size(340, 25), Font = new Font("Segoe UI", 10F) };

            lblCategory = new Label { Text = "Category", Location = new Point(30, 150), Font = new Font("Segoe UI", 10F), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(30, 175), Size = new Size(160, 25), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            // Pre-defined categories for consistency
            cmbCategory.Items.AddRange(new object[] { "Coffee", "Tea", "Beverages", "Snacks", "Desserts" });

            lblPrice = new Label { Text = "Price (₹)", Location = new Point(210, 150), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(210, 175), Size = new Size(160, 25), Font = new Font("Segoe UI", 10F) };

            btnSave = new RoundedButton { Text = "Save", CornerRadius = 10, Location = new Point(160, 260), Size = new Size(100, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;

            btnCancel = new RoundedButton { Text = "Cancel", CornerRadius = 10, Location = new Point(270, 260), Size = new Size(100, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, lblItemName, txtItemName, lblCategory, cmbCategory, lblPrice, txtPrice, btnSave, btnCancel });

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        }

        private void LoadItemData()
        {
            if (!itemId.HasValue) return;
            string query = "SELECT item_name, category, price FROM menu_items WHERE item_id = @ItemId";
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@ItemId", this.itemId.Value);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtItemName.Text = reader["item_name"].ToString();
                            cmbCategory.SelectedItem = reader["category"].ToString();
                            txtPrice.Text = reader["price"].ToString();
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load item data. " + ex.Message, "Error"); }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text) || cmbCategory.SelectedItem == null || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid, non-negative price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = itemId.HasValue
                ? "UPDATE menu_items SET item_name = @ItemName, category = @Category, price = @Price WHERE item_id = @ItemId"
                : "INSERT INTO menu_items (item_name, category, price) VALUES (@ItemName, @Category, @Price)";

            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@ItemName", txtItemName.Text.Trim());
                cmd.Parameters.AddWithValue("@Category", cmbCategory.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@Price", price);
                if (itemId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ItemId", itemId.Value);
                }

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
                {
                    MessageBox.Show("This item name already exists. Please choose another.", "Duplicate Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}