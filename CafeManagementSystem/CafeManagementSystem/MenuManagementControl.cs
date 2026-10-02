// MenuManagementControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class MenuManagementControl : UserControl
    {
        private DataGridView dgvMenuItems;
        private RoundedButton btnAddItem;
        private Panel buttonPanel;
        private Label lblTitle;

        public MenuManagementControl()
        {
            InitializeComponent();
            LoadMenuItems();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            // --- Header Layout ---
            lblTitle = new Label { Text = "Menu Management", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(68, 68, 68), Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 10), TextAlign = ContentAlignment.MiddleLeft };
            buttonPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(0, 0, 0, 10) };
            btnAddItem = new RoundedButton { Text = "+ Add Item", CornerRadius = 10, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(0, 123, 255), FlatStyle = FlatStyle.Flat, Size = new Size(120, 40), Dock = DockStyle.Right, Cursor = Cursors.Hand };
            btnAddItem.FlatAppearance.BorderSize = 0;
            buttonPanel.Controls.Add(btnAddItem);

            // --- DataGridView ---
            dgvMenuItems = new DataGridView();
            dgvMenuItems.Dock = DockStyle.Fill;
            dgvMenuItems.BackgroundColor = Color.White;
            dgvMenuItems.BorderStyle = BorderStyle.None;
            dgvMenuItems.AllowUserToAddRows = false;
            dgvMenuItems.AutoGenerateColumns = false;
            dgvMenuItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMenuItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMenuItems.EnableHeadersVisualStyles = false;
            dgvMenuItems.RowHeadersVisible = false;
            dgvMenuItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMenuItems.RowTemplate.Height = 40;

            // Header Style
            dgvMenuItems.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(5), SelectionBackColor = Color.FromArgb(220, 223, 226) };
            // Row Style
            dgvMenuItems.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10), Padding = new Padding(5), Alignment = DataGridViewContentAlignment.MiddleLeft, SelectionBackColor = Color.FromArgb(204, 232, 255), SelectionForeColor = Color.FromArgb(68, 68, 68) };

            // --- Columns ---
            var colSrNo = new DataGridViewTextBoxColumn { HeaderText = "Sr. No.", Name = "SrNo", FillWeight = 10 };
            var colItemName = new DataGridViewTextBoxColumn { HeaderText = "Item Name", Name = "ItemName", FillWeight = 35 };
            var colCategory = new DataGridViewTextBoxColumn { HeaderText = "Category", Name = "Category", FillWeight = 25 };
            var colPrice = new DataGridViewTextBoxColumn { HeaderText = "Price", Name = "Price", FillWeight = 15, DefaultCellStyle = { Format = "c2" } }; // Format as currency
            var colEdit = new DataGridViewButtonColumn { HeaderText = "Action", Name = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 10, FlatStyle = FlatStyle.Flat };
            colEdit.DefaultCellStyle.BackColor = Color.FromArgb(40, 167, 69); colEdit.DefaultCellStyle.ForeColor = Color.White; colEdit.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 136, 56);
            var colDelete = new DataGridViewButtonColumn { HeaderText = "", Name = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 10, FlatStyle = FlatStyle.Flat };
            colDelete.DefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69); colDelete.DefaultCellStyle.ForeColor = Color.White; colDelete.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 35, 51);
            var colItemId = new DataGridViewTextBoxColumn { Name = "ItemId", Visible = false };

            dgvMenuItems.Columns.AddRange(new DataGridViewColumn[] { colSrNo, colItemName, colCategory, colPrice, colEdit, colDelete, colItemId });

            // --- Control Layout ---
            this.Controls.Add(dgvMenuItems);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(lblTitle);

            // --- Event Handlers ---
            btnAddItem.Click += BtnAddItem_Click;
            dgvMenuItems.CellContentClick += DgvMenuItems_CellContentClick;
        }

        private void LoadMenuItems()
        {
            string query = "SELECT item_id, item_name, category, price FROM menu_items";
            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                var dt = new DataTable();
                da.Fill(dt);

                dgvMenuItems.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    dgvMenuItems.Rows.Add(i + 1, row["item_name"], row["category"], row["price"], "Edit", "Delete", row["item_id"]);
                }
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditMenuItemForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadMenuItems(); // Refresh grid
                }
            }
        }

        private void DgvMenuItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int itemId = Convert.ToInt32(dgvMenuItems.Rows[e.RowIndex].Cells["ItemId"].Value);

            if (dgvMenuItems.Columns[e.ColumnIndex].Name == "Edit")
            {
                using (var form = new AddEditMenuItemForm(itemId))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadMenuItems(); // Refresh grid
                    }
                }
            }

            if (dgvMenuItems.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this menu item?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string query = "DELETE FROM menu_items WHERE item_id = @ItemId";
                    using (var con = DatabaseHelper.GetConnection())
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                        try
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            LoadMenuItems(); // Refresh grid
                        }
                        catch (Exception ex) { MessageBox.Show("Failed to delete item. " + ex.Message, "Error"); }
                    }
                }
            }
        }
    }
}