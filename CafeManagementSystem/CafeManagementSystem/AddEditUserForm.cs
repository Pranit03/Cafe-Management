// AddEditUserForm.cs

using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    // This is a Form class, which is a special type of class designed to be a window.
    public partial class AddEditUserForm : Form
    {
        private Label lblTitle, lblFullName, lblUsername, lblPassword, lblRole;
        private TextBox txtFullName, txtUsername, txtPassword;
        private ComboBox cmbRole;
        private RoundedButton btnSave, btnCancel;

        // This field determines if we are adding a new user (null) or editing an existing one.
        private readonly int? userId;

        public AddEditUserForm(int? id = null)
        {
            this.userId = id;
            InitializeComponent();

            // If an ID was passed, we are in "Edit" mode.
            if (userId.HasValue)
            {
                lblTitle.Text = "Edit User";
                LoadUserData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "User Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(400, 400);
            this.BackColor = Color.White;

            lblTitle = new Label { Text = "Add New User", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = Color.FromArgb(68, 68, 68) };

            lblFullName = new Label { Text = "Full Name", Location = new Point(30, 80), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(30, 105), Size = new Size(340, 25), Font = new Font("Segoe UI", 10F) };

            lblUsername = new Label { Text = "Username", Location = new Point(30, 150), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(30, 175), Size = new Size(340, 25), Font = new Font("Segoe UI", 10F) };

            lblPassword = new Label { Text = "Password", Location = new Point(30, 220), Font = new Font("Segoe UI", 10F), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(30, 245), Size = new Size(160, 25), Font = new Font("Segoe UI", 10F), PasswordChar = '●' };

            lblRole = new Label { Text = "Role", Location = new Point(210, 220), Font = new Font("Segoe UI", 10F), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(210, 245), Size = new Size(160, 25), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Admin", "Staff" });

            btnSave = new RoundedButton { Text = "Save", CornerRadius = 10, Location = new Point(160, 320), Size = new Size(100, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;

            btnCancel = new RoundedButton { Text = "Cancel", CornerRadius = 10, Location = new Point(270, 320), Size = new Size(100, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, lblFullName, txtFullName, lblUsername, txtUsername, lblPassword, txtPassword, lblRole, cmbRole, btnSave, btnCancel });

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        }

        private void LoadUserData()
        {
            // This method fills the form with data when editing an existing user.
            if (!userId.HasValue) return;
            string query = "SELECT full_name, username, role FROM users WHERE user_id = @UserId";
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", this.userId.Value);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtFullName.Text = reader["full_name"].ToString();
                            txtUsername.Text = reader["username"].ToString();
                            cmbRole.SelectedItem = reader["role"].ToString();
                            // We don't load the password for security.
                            // The label below indicates that leaving it blank will not change it.
                            lblPassword.Text = "Password (leave blank to keep unchanged)";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load user data. " + ex.Message, "Error");
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // --- Validation ---
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!userId.HasValue && string.IsNullOrWhiteSpace(txtPassword.Text)) // Password is required for new users
            {
                MessageBox.Show("Password is required for new users.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query;
            if (userId.HasValue) // UPDATE logic
            {
                query = string.IsNullOrWhiteSpace(txtPassword.Text)
                    ? "UPDATE users SET full_name = @FullName, username = @Username, role = @Role WHERE user_id = @UserId"
                    : "UPDATE users SET full_name = @FullName, username = @Username, password_hash = @Password, role = @Role WHERE user_id = @UserId";
            }
            else // INSERT logic
            {
                query = "INSERT INTO users (full_name, username, password_hash, role) VALUES (@FullName, @Username, @Password, @Role)";
            }

            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                if (userId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                }
                if (!string.IsNullOrWhiteSpace(txtPassword.Text) || !userId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                }

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK; // Signal success to the calling control
                    this.Close();
                }
                catch (SqlException ex) when (ex.Number == 2627) // Unique username constraint violation
                {
                    MessageBox.Show("This username already exists. Please choose another.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}