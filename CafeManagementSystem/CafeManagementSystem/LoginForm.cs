// LoginForm.cs

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net; // Required for WebClient
using System.IO;   // Required for MemoryStream

namespace CafeManagementSystem
{
    public partial class LoginForm : Form
    {
        // UI controls
        private Panel loginPanel;
        private Label titleLabel;
        private Label usernameLabel;
        private TextBox usernameTextBox;
        private Label passwordLabel;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button exitButton;

        public LoginForm()
        {
            InitializeComponent();
            this.AcceptButton = loginButton;
        }

        private void InitializeComponent()
        {
            // --- Form Properties ---
            this.Text = "Café Management System - Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Resize += LoginForm_Resize;

            // --- Load Background Image From URL ---
            string imageUrl = "https://images.pexels.com/photos/1695052/pexels-photo-1695052.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2";
            Image backgroundImage = LoadImageFromUrl(imageUrl);

            if (backgroundImage != null)
            {
                this.BackgroundImage = backgroundImage;
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                // Fallback if the image fails to load (e.g., no internet)
                this.BackColor = Color.FromArgb(30, 30, 30); // Dark gray
            }

            // --- Login Panel ---
            loginPanel = new Panel();
            loginPanel.Size = new Size(400, 500);
            loginPanel.BackColor = Color.FromArgb(180, 0, 0, 0); // Semi-transparent black
            CenterLoginPanel();
            loginPanel.Anchor = AnchorStyles.None;
            this.Controls.Add(loginPanel);

            // ... (The rest of your UI controls code is exactly the same) ...

            // --- Title Label ---
            titleLabel = new Label { Text = "CAFÉ LOGIN", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White, Size = new Size(300, 50), TextAlign = ContentAlignment.MiddleCenter, Location = new Point((loginPanel.Width - 300) / 2, 40) };
            loginPanel.Controls.Add(titleLabel);

            // --- Username Controls ---
            usernameLabel = new Label { Text = "Username", Font = new Font("Segoe UI", 12), ForeColor = Color.White, Location = new Point(50, 150), Size = new Size(100, 30) };
            usernameTextBox = new TextBox { Font = new Font("Segoe UI", 12), Size = new Size(300, 30), Location = new Point(50, 180) };
            loginPanel.Controls.Add(usernameLabel);
            loginPanel.Controls.Add(usernameTextBox);

            // --- Password Controls ---
            passwordLabel = new Label { Text = "Password", Font = new Font("Segoe UI", 12), ForeColor = Color.White, Location = new Point(50, 240), Size = new Size(100, 30) };
            passwordTextBox = new TextBox { Font = new Font("Segoe UI", 12), Size = new Size(300, 30), Location = new Point(50, 270), PasswordChar = '●' };
            loginPanel.Controls.Add(passwordLabel);
            loginPanel.Controls.Add(passwordTextBox);

            // --- Login Button ---
            loginButton = new Button { Text = "LOGIN", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(217, 83, 79), FlatStyle = FlatStyle.Flat, Size = new Size(300, 50), Location = new Point(50, 350), Cursor = Cursors.Hand };
            loginButton.FlatAppearance.BorderSize = 0;
            loginPanel.Controls.Add(loginButton);

            // --- Exit Button ---
            exitButton = new Button { Text = "X", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Size = new Size(40, 40), Location = new Point(this.ClientSize.Width - 40, 0), Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand };
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.FlatAppearance.MouseOverBackColor = Color.Red;
            this.Controls.Add(exitButton);
            exitButton.BringToFront();

            // --- Event Handlers ---
            loginButton.Click += LoginButton_Click;
            exitButton.Click += ExitButton_Click;
        }

        // --- NEW HELPER METHOD TO LOAD IMAGE FROM A URL ---
        private Image LoadImageFromUrl(string url)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(url);
                    using (MemoryStream memStream = new MemoryStream(data))
                    {
                        return Image.FromStream(memStream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load background image. Please check your internet connection.\nError: {ex.Message}", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        // --- (The rest of the event handler methods are the same) ---

        // In LoginForm.cs

        // In LoginForm.cs

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // FIX #1: The SQL query MUST select the user_id.
                string query = "SELECT user_id, role, full_name FROM users WHERE username = @username AND password_hash = @password AND is_active = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        // FIX #2: Read the user_id from the database.
                        int userId = Convert.ToInt32(reader["user_id"]);
                        string role = reader["role"].ToString();
                        string fullName = reader["full_name"].ToString();

                        this.Hide();

                        if (role == "Admin")
                        {
                            AdminDashboard adminForm = new AdminDashboard();
                            adminForm.Show();
                        }
                        else if (role == "Staff")
                        {
                            // FIX #3: Pass BOTH the ID and the name to the constructor.
                            StaffDashboard staffForm = new StaffDashboard(userId, fullName);
                            staffForm.Show();
                        }
                        else
                        {
                            MessageBox.Show("Invalid user role assigned. Please contact administrator.", "Role Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Show();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A database error occurred: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        //private void LoginButton_Click(object sender, EventArgs e)
        //{
        //    string username = usernameTextBox.Text.Trim();
        //    string password = passwordTextBox.Text;

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    using (SqlConnection con = DatabaseHelper.GetConnection())
        //    {
        //        string query = "SELECT role, full_name FROM users WHERE username = @username AND password_hash = @password AND is_active = 1";
        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@username", username);
        //        cmd.Parameters.AddWithValue("@password", password);

        //        try
        //        {
        //            con.Open();
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            // In LoginForm.cs -> LoginButton_Click method

        //            if (reader.HasRows)
        //            {
        //                reader.Read();
        //                string role = reader["role"].ToString();
        //                // We still read the full name, but we don't use it anymore.
        //                string fullName = reader["full_name"].ToString();

        //                this.Hide(); // Hide the login form

        //                if (role == "Admin")
        //                {
        //                    // THE FIX: We are now calling the default constructor without any arguments.
        //                    AdminDashboard adminForm = new AdminDashboard();
        //                    adminForm.Show();
        //                }
        //                else if (role == "Staff")
        //                {
        //                    // IMPORTANT: You must also change your StaffDashboard constructor back to default
        //                    // to avoid the same error for staff logins.
        //                    StaffDashboard staffForm = new StaffDashboard(fullName);
        //                    staffForm.Show();
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Invalid user role assigned. Please contact administrator.", "Role Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                    this.Show();
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("A database error occurred: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        //        }
        //    }
        //}

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CenterLoginPanel()
        {
            loginPanel.Location = new Point((this.ClientSize.Width - loginPanel.Width) / 2, (this.ClientSize.Height - loginPanel.Height) / 2);
        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            CenterLoginPanel();
        }
    }
}