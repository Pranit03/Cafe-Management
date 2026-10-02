// AddTableForm.cs

using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class AddTableForm : Form
    {
        private Label lblTitle, lblTableName, lblStatus;
        private TextBox txtTableName;
        private ComboBox cmbStatus;
        private RoundedButton btnSave, btnCancel;
        private readonly int? tableId;

        public AddTableForm(int? id = null)
        {
            this.tableId = id;
            InitializeComponent();
            if (tableId.HasValue)
            {
                lblTitle.Text = "Edit Table";
                LoadTableData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Table Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(350, 280);
            this.BackColor = Color.White;

            lblTitle = new Label { Text = "Add New Table", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = Color.FromArgb(68, 68, 68) };
            lblTableName = new Label { Text = "Table Name (e.g., T-13)", Location = new Point(30, 80), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtTableName = new TextBox { Location = new Point(30, 105), Size = new Size(290, 25), Font = new Font("Segoe UI", 10F) };

            lblStatus = new Label { Text = "Status", Location = new Point(30, 150), Font = new Font("Segoe UI", 10F), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(30, 175), Size = new Size(150, 25), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Available", "Occupied" });
            cmbStatus.SelectedItem = "Available";

            btnSave = new RoundedButton { Text = "Save", CornerRadius = 10, Location = new Point(130, 220), Size = new Size(100, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCancel = new RoundedButton { Text = "Cancel", CornerRadius = 10, Location = new Point(240, 220), Size = new Size(100, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            this.Controls.AddRange(new Control[] { lblTitle, lblTableName, txtTableName, lblStatus, cmbStatus, btnSave, btnCancel });

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        }

        private void LoadTableData()
        {
            string query = "SELECT table_name, status FROM tables WHERE table_id = @TableId";
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TableId", this.tableId.Value);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTableName.Text = reader["table_name"].ToString();
                            cmbStatus.SelectedItem = reader["status"].ToString();
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load table data: " + ex.Message); }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTableName.Text) || cmbStatus.SelectedItem == null) { MessageBox.Show("Please fill all fields.", "Validation Error"); return; }

            string query = tableId.HasValue
                ? "UPDATE tables SET table_name = @TableName, status = @Status WHERE table_id = @TableId"
                : "INSERT INTO tables (table_name, status) VALUES (@TableName, @Status)";

            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TableName", txtTableName.Text.Trim());
                cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem.ToString());
                if (tableId.HasValue) { cmd.Parameters.AddWithValue("@TableId", tableId.Value); }
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (SqlException ex) when (ex.Number == 2627) { MessageBox.Show("This table name already exists.", "Error"); }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Database Error"); }
            }
        }
    }
}