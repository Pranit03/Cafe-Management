// StaffTablesControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class StaffTablesControl : UserControl
    {
        private DataGridView dgvTables;
        private RoundedButton btnAddTable;
        private Label lblTitle;
        private Panel headerPanel;

        public StaffTablesControl()
        {
            InitializeComponent();
            LoadTables();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Color.White; this.Padding = new Padding(10);
            lblTitle = new Label { Text = "Manage Tables", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(68, 68, 68), Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 10), TextAlign = ContentAlignment.MiddleLeft };
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(0, 0, 0, 10) };
            btnAddTable = new RoundedButton { Text = "+ Add Table", CornerRadius = 10, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(0, 123, 255), FlatStyle = FlatStyle.Flat, Size = new Size(120, 40), Dock = DockStyle.Right, Cursor = Cursors.Hand };
            btnAddTable.FlatAppearance.BorderSize = 0;
            headerPanel.Controls.Add(btnAddTable);

            dgvTables = new DataGridView();
            // --- UI STYLE MATCHING ADMIN SIDE ---
            dgvTables.Dock = DockStyle.Fill; dgvTables.BackgroundColor = Color.White; dgvTables.BorderStyle = BorderStyle.None; dgvTables.AllowUserToAddRows = false; dgvTables.AutoGenerateColumns = false; dgvTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvTables.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; dgvTables.EnableHeadersVisualStyles = false; dgvTables.RowHeadersVisible = false; dgvTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvTables.RowTemplate.Height = 40;
            dgvTables.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(5), SelectionBackColor = Color.FromArgb(220, 223, 226) };
            dgvTables.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10), Padding = new Padding(5), Alignment = DataGridViewContentAlignment.MiddleLeft, SelectionBackColor = Color.FromArgb(204, 232, 255), SelectionForeColor = Color.FromArgb(68, 68, 68) };

            // --- UPDATED COLUMNS WITH ACTION BUTTONS ---
            var colSrNo = new DataGridViewTextBoxColumn { HeaderText = "Sr. No.", Name = "SrNo", FillWeight = 10 };
            var colTableName = new DataGridViewTextBoxColumn { HeaderText = "Table Name", Name = "TableName", FillWeight = 40 };
            var colStatus = new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 25 };
            var colEdit = new DataGridViewButtonColumn { HeaderText = "Action", Name = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 12, FlatStyle = FlatStyle.Flat };
            colEdit.DefaultCellStyle.BackColor = Color.FromArgb(40, 167, 69); colEdit.DefaultCellStyle.ForeColor = Color.White; colEdit.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 136, 56);
            var colDelete = new DataGridViewButtonColumn { HeaderText = "", Name = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 13, FlatStyle = FlatStyle.Flat };
            colDelete.DefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69); colDelete.DefaultCellStyle.ForeColor = Color.White; colDelete.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 35, 51);
            var colTableId = new DataGridViewTextBoxColumn { Name = "TableId", Visible = false }; // Hidden column for the ID

            dgvTables.Columns.AddRange(new DataGridViewColumn[] { colSrNo, colTableName, colStatus, colEdit, colDelete, colTableId });

            this.Controls.Add(dgvTables); this.Controls.Add(headerPanel); this.Controls.Add(lblTitle);

            btnAddTable.Click += BtnAddTable_Click;
            dgvTables.CellContentClick += DgvTables_CellContentClick;
        }

        private void LoadTables()
        {
            string query = "SELECT table_id, table_name, status FROM tables ORDER BY table_id";
            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                var dt = new DataTable();
                da.Fill(dt);

                dgvTables.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    // Add data to all columns, including the hidden ID and the button text
                    dgvTables.Rows.Add(i + 1, row["table_name"], row["status"], "Edit", "Delete", row["table_id"]);
                }
            }
        }

        private void BtnAddTable_Click(object sender, EventArgs e)
        {
            // Open the form in "Add" mode by not passing an ID
            using (var form = new AddTableForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadTables();
                }
            }
        }

        private void DgvTables_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Ignore clicks on the header
            int tableId = Convert.ToInt32(dgvTables.Rows[e.RowIndex].Cells["TableId"].Value);

            // --- EDIT BUTTON LOGIC ---
            if (dgvTables.Columns[e.ColumnIndex].Name == "Edit")
            {
                // Open the form in "Edit" mode by passing the table's ID
                using (var form = new AddTableForm(tableId))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadTables();
                    }
                }
            }

            // --- DELETE BUTTON LOGIC ---
            if (dgvTables.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this table?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string query = "DELETE FROM tables WHERE table_id = @TableId";
                    using (var con = DatabaseHelper.GetConnection())
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TableId", tableId);
                        try
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            LoadTables(); // Refresh the grid after deletion
                        }
                        catch (Exception ex) { MessageBox.Show("Failed to delete table. " + ex.Message, "Error"); }
                    }
                }
            }
        }
    }
}