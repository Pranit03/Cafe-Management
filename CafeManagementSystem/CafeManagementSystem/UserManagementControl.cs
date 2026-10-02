// UserManagementControl.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class UserManagementControl : UserControl
    {
        private DataGridView dgvUsers;
        private RoundedButton btnAddUser;
        private Panel buttonPanel;
        private Label lblTitle;

        public UserManagementControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            // --- HIERARCHICAL LAYOUT CONTROLS ---
            lblTitle = new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68),
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 0, 10)
            };

            btnAddUser = new RoundedButton
            {
                Text = "+ Add User",
                CornerRadius = 10,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand
            };
            btnAddUser.FlatAppearance.BorderSize = 0;
            buttonPanel.Controls.Add(btnAddUser);

            // --- DataGridView ---
            dgvUsers = new DataGridView();
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.BackgroundColor = Color.White;
            // ... (rest of DataGridView properties are the same) ...
            dgvUsers.BorderStyle = BorderStyle.None;
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.RowHeadersVisible = false;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.RowTemplate.Height = 40;

            // Header Style
            dgvUsers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(220, 223, 226), ForeColor = Color.FromArgb(68, 68, 68), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(5), SelectionBackColor = Color.FromArgb(220, 223, 226) };
            // Row Style
            dgvUsers.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10), Padding = new Padding(5), Alignment = DataGridViewContentAlignment.MiddleLeft, SelectionBackColor = Color.FromArgb(204, 232, 255), SelectionForeColor = Color.FromArgb(68, 68, 68) };

            // Define Columns
            var colSrNo = new DataGridViewTextBoxColumn { HeaderText = "Sr. No.", Name = "SrNo", FillWeight = 10 };
            var colFullName = new DataGridViewTextBoxColumn { HeaderText = "Full Name", Name = "FullName", FillWeight = 35 };
            var colUsername = new DataGridViewTextBoxColumn { HeaderText = "Username", Name = "Username", FillWeight = 25 };
            var colRole = new DataGridViewTextBoxColumn { HeaderText = "Role", Name = "Role", FillWeight = 15 };
            var colEdit = new DataGridViewButtonColumn { HeaderText = "Action", Name = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 10, FlatStyle = FlatStyle.Flat };
            colEdit.DefaultCellStyle.BackColor = Color.FromArgb(40, 167, 69); colEdit.DefaultCellStyle.ForeColor = Color.White; colEdit.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 136, 56);
            var colDelete = new DataGridViewButtonColumn { HeaderText = "", Name = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 10, FlatStyle = FlatStyle.Flat };
            colDelete.DefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69); colDelete.DefaultCellStyle.ForeColor = Color.White; colDelete.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 35, 51);
            var colUserId = new DataGridViewTextBoxColumn { Name = "UserId", Visible = false };
            dgvUsers.Columns.AddRange(new DataGridViewColumn[] { colSrNo, colFullName, colUsername, colRole, colEdit, colDelete, colUserId });


            // --- THE FIX: Change the order of adding controls for correct docking behavior ---
            // 1. Add the control that fills the space FIRST.
            this.Controls.Add(dgvUsers);
            // 2. Add the panel for the button SECOND. It will dock to the top of the remaining space.
            this.Controls.Add(buttonPanel);
            // 3. Add the main title LAST. It will dock to the very top of the form, pushing everything else down.
            this.Controls.Add(lblTitle);


            // --- Event Handlers ---
            btnAddUser.Click += BtnAddUser_Click;
            dgvUsers.CellContentClick += DgvUsers_CellContentClick;
            dgvUsers.CellFormatting += DgvUsers_CellFormatting;
        }

        // --- The rest of your methods (LoadUsers, DgvUsers_CellFormatting, etc.) remain unchanged ---
        private void LoadUsers()
        {
            string query = "SELECT user_id, full_name, username, role FROM users";
            using (var con = DatabaseHelper.GetConnection())
            using (var da = new SqlDataAdapter(query, con))
            {
                var dt = new DataTable();
                da.Fill(dt);

                dgvUsers.Rows.Clear();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    int n = dgvUsers.Rows.Add();
                    dgvUsers.Rows[n].Cells["SrNo"].Value = i + 1;
                    dgvUsers.Rows[n].Cells["FullName"].Value = row["full_name"];
                    dgvUsers.Rows[n].Cells["Username"].Value = row["username"];
                    dgvUsers.Rows[n].Cells["Role"].Value = row["role"];
                    dgvUsers.Rows[n].Cells["UserId"].Value = row["user_id"];
                }
            }
        }

        private void DgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsers.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                e.CellStyle.Padding = new Padding(3);
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditUserForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }

        private void DgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int userId = Convert.ToInt32(dgvUsers.Rows[e.RowIndex].Cells["UserId"].Value);

            if (dgvUsers.Columns[e.ColumnIndex].Name == "Edit")
            {
                using (var form = new AddEditUserForm(userId))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadUsers();
                    }
                }
            }

            if (dgvUsers.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this user?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string query = "DELETE FROM users WHERE user_id = @UserId";
                    using (var con = DatabaseHelper.GetConnection())
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        try
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            LoadUsers();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete user. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}