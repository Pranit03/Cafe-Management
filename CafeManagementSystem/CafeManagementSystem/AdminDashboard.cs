// AdminDashboard.cs

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CafeManagementSystem
{
    public partial class AdminDashboard : Form
    {
        // --- UI Controls ---
        private Panel topPanel, navigationPanel, contentPanel;
        private Label lblHeaderTitle, lblLogo, contentTitle;
        private RoundedButton btnLogout; // Using our custom button
        private Button btnDashboard, btnUserManagement, btnMenuManagement, btnReports;

        // --- Fields ---
        private List<Button> navButtons;
        private FlowLayoutPanel dashboardFlowPanel; // To hold our stat cards

        // --- A Modern Color Palette ---
        private Color colorSidebar = Color.FromArgb(43, 49, 58);
        private Color colorContentBg = Color.FromArgb(239, 241, 246);
        private Color colorTopPanel = Color.White;
        private Color colorAccent = Color.FromArgb(2, 117, 216); // A vibrant blue
        private Color colorFont = Color.FromArgb(220, 220, 220); // Light gray for sidebar text
        private Color colorFontDark = Color.FromArgb(68, 68, 68);

        public AdminDashboard()
        {
            InitializeComponent();
        }

        private Button btnViewOrders;

        private void InitializeComponent()
        {
            // ... (All your existing form properties and top panel code remains the same) ...

            this.Text = "Admin Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1280, 800);
            this.BackColor = colorContentBg;

            // --- Top Panel ---
            topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorTopPanel };
            topPanel.Paint += (s, e) => { e.Graphics.DrawLine(Pens.Gainsboro, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1); };
            this.Controls.Add(topPanel);

            lblHeaderTitle = new Label { Text = "Administrator Dashboard", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = colorFontDark, Location = new Point(20, 18), AutoSize = true };
            topPanel.Controls.Add(lblHeaderTitle);

            btnLogout = new RoundedButton { Text = "Logout", CornerRadius = 10, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Tomato, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Width = 120, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            topPanel.Controls.Add(btnLogout);

            // --- Sidebar ---
            navigationPanel = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = colorSidebar };
            this.Controls.Add(navigationPanel);

            lblLogo = new Label { Text = "CAFÉ ADMIN", ForeColor = colorAccent, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Size = new Size(200, 60), Location = new Point(15, 20), TextAlign = ContentAlignment.MiddleLeft };
            navigationPanel.Controls.Add(lblLogo);

            // --- UPDATED NAVIGATION BUTTONS ---
            btnDashboard = CreateNavButton("🏠   Dashboard", 100);
            btnUserManagement = CreateNavButton("👥   User Management", 150);
            btnMenuManagement = CreateNavButton("📖   Menu Management", 200);
            btnViewOrders = CreateNavButton("🧾   View Orders", 250); // NEW BUTTON ADDED
            btnReports = CreateNavButton("📈   Reports", 300);      // Pushed down

            navButtons = new List<Button> { btnDashboard, btnUserManagement, btnMenuManagement, btnViewOrders, btnReports }; // ADDED TO THE LIST
            navigationPanel.Controls.AddRange(navButtons.ToArray());

            // ... (The rest of your InitializeComponent code for contentPanel, events, etc., remains the same) ...
            contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            contentTitle = new Label { Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = colorFontDark, Location = new Point(20, 20), AutoSize = true };

            dashboardFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(20) };

            btnLogout.Click += (s, e) => { this.Hide(); new LoginForm().Show(); };
            this.FormClosing += (s, e) => { if (e.CloseReason == CloseReason.UserClosing) Application.Exit(); };

            NavButton_Click(btnDashboard, EventArgs.Empty);
        }

        // --- Navigation Methods ---
        private Button CreateNavButton(string text, int top)
        {
            Button button = new Button { Text = text, Top = top, Font = new Font("Segoe UI", 12F), ForeColor = colorFont, BackColor = colorSidebar, FlatStyle = FlatStyle.Flat, Height = 50, Width = navigationPanel.Width, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0), Cursor = Cursors.Hand, };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 61, 70); // Slightly lighter than sidebar
            button.Click += NavButton_Click;
            return button;
        }

        // In AdminDashboard.cs

        // In AdminDashboard.cs

        // In AdminDashboard.cs

        // In AdminDashboard.cs

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            SetActiveButton(clickedButton);

            contentPanel.Controls.Clear();

            if (clickedButton == btnDashboard)
            {
                LoadDashboardView();
            }
            else if (clickedButton == btnUserManagement)
            {
                contentPanel.Controls.Add(new UserManagementControl());
            }
            else if (clickedButton == btnMenuManagement)
            {
                contentPanel.Controls.Add(new MenuManagementControl());
            }
            else if (clickedButton == btnViewOrders)
            {
                contentPanel.Controls.Add(new ViewOrdersControl());
            }
            // --- THIS IS THE CRITICAL FIX ---
            // The "Reports" button must load the ReportsControl (the container with the tabs).
            else if (clickedButton == btnReports)
            {
                contentPanel.Controls.Add(new ReportsControl());
            }
        }
        private void SetActiveButton(Button activeButton)
        {
            foreach (Button button in navButtons)
            {
                if (button == activeButton)
                {
                    button.BackColor = colorAccent;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = colorSidebar;
                    button.ForeColor = colorFont;
                }
            }
        }

        // --- View Loading Methods ---

        private void LoadDashboardView()
        {
            dashboardFlowPanel.Controls.Clear();
            dashboardFlowPanel.Top = 60; // Position below the title
            contentPanel.Controls.Add(dashboardFlowPanel);

            // --- FIX IS APPLIED IN THIS SECTION ---

            // 1. Get the raw object from the database and safely convert it to a decimal
            decimal salesTodayValue = Convert.ToDecimal(GetScalarData("SELECT ISNULL(SUM(total_amount), 0) FROM orders WHERE CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)"));

            // 2. Now that we have a decimal, we can format it to a string with two decimal places.
            string salesToday = salesTodayValue.ToString("0.00");

            // The rest of the calls are correct because they use the parameter-less ToString()
            string menuItems = GetScalarData("SELECT COUNT(*) FROM menu_items").ToString();
            string staffCount = GetScalarData("SELECT COUNT(*) FROM users WHERE role = 'Staff'").ToString();
            string ordersToday = GetScalarData("SELECT COUNT(*) FROM orders WHERE CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)").ToString();

            // --- END OF FIX ---

            // Create and add stat cards
            dashboardFlowPanel.Controls.Add(CreateStatCard($"₹ {salesToday}", "Sales Today", Color.FromArgb(23, 162, 184)));
            dashboardFlowPanel.Controls.Add(CreateStatCard(menuItems, "Menu Items", Color.FromArgb(40, 167, 69)));
            dashboardFlowPanel.Controls.Add(CreateStatCard(staffCount, "Active Staff", Color.FromArgb(255, 193, 7)));
            dashboardFlowPanel.Controls.Add(CreateStatCard(ordersToday, "Orders Today", Color.FromArgb(220, 53, 69)));
        }

        private void LoadUserManagementView()
        {
            UserManagementControl userControl = new UserManagementControl();
            userControl.Top = 60; // Position below the title
            contentPanel.Controls.Add(userControl);
        }

        // --- Helper for creating Stat Cards ---
        private ShadowPanel CreateStatCard(string value, string title, Color accentColor)
        {
            ShadowPanel card = new ShadowPanel { Size = new Size(250, 130), BackColor = Color.White, Margin = new Padding(20) };
            Panel accentPanel = new Panel { BackColor = accentColor, Dock = DockStyle.Left, Width = 7 };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = colorFontDark, Location = new Point(30, 20), AutoSize = true };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(30, 70), AutoSize = true };

            card.Controls.Add(accentPanel);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);

            return card;
        }

        // --- Database Helper Method ---
        private object GetScalarData(string query)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        return cmd.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database query failed: " + ex.Message, "Error");
                        return 0;
                    }
                }
            }
        }
    }
}