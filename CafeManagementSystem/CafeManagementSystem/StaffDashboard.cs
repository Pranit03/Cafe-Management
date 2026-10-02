// StaffDashboard.cs

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CafeManagementSystem
{
    public partial class StaffDashboard : Form
    {
        private Panel topPanel, navigationPanel, contentPanel;
        private Label lblHeaderTitle, lblLogo;
        private RoundedButton btnLogout;
        private Button btnDashboard, btnTables, btnActiveOrders;
        private List<Button> navButtons;
        private FlowLayoutPanel dashboardFlowPanel;

        private int loggedInStaffId;
        private string loggedInStaffName;
        
        private Color colorSidebar = Color.FromArgb(43, 49, 58), colorContentBg = Color.FromArgb(239, 241, 246), colorTopPanel = Color.White, colorAccent = Color.FromArgb(23, 162, 184), colorFont = Color.FromArgb(220, 220, 220), colorFontDark = Color.FromArgb(68, 68, 68);

        public StaffDashboard(int staffId, string staffFullName)
        {
            this.loggedInStaffId = staffId;
            this.loggedInStaffName = staffFullName;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Staff Dashboard"; this.StartPosition = FormStartPosition.CenterScreen; this.WindowState = FormWindowState.Maximized; this.MinimumSize = new Size(1280, 800); this.BackColor = colorContentBg;
            topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorTopPanel }; topPanel.Paint += (s, e) => { e.Graphics.DrawLine(Pens.Gainsboro, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1); }; this.Controls.Add(topPanel);
            lblHeaderTitle = new Label { Text = "Staff Portal | Logged In: " + this.loggedInStaffName, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = colorFontDark, Location = new Point(20, 18), AutoSize = true }; topPanel.Controls.Add(lblHeaderTitle);
            btnLogout = new RoundedButton { Text = "Logout", CornerRadius = 10, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Tomato, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Width = 120, Cursor = Cursors.Hand }; btnLogout.FlatAppearance.BorderSize = 0; topPanel.Controls.Add(btnLogout);
            
            navigationPanel = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = colorSidebar }; this.Controls.Add(navigationPanel);
            lblLogo = new Label { Text = "CAFÉ STAFF", ForeColor = colorAccent, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Size = new Size(200, 60), Location = new Point(15, 20), TextAlign = ContentAlignment.MiddleLeft }; navigationPanel.Controls.Add(lblLogo);
            
            btnDashboard = CreateNavButton("🏠   Dashboard", 100);
            btnTables = CreateNavButton("📋   Tables", 150);
            btnActiveOrders = CreateNavButton("🍽️   Active Orders", 200);

            navButtons = new List<Button> { btnDashboard, btnTables, btnActiveOrders };
            navigationPanel.Controls.AddRange(navButtons.ToArray());

            contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            this.Controls.Add(contentPanel); contentPanel.BringToFront();

            dashboardFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(20) };

            btnLogout.Click += (s, e) => { this.Hide(); new LoginForm().Show(); };
            this.FormClosing += (s, e) => { if (e.CloseReason == CloseReason.UserClosing) Application.Exit(); };

            NavButton_Click(btnDashboard, EventArgs.Empty);
        }
        
        private void NavButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            SetActiveButton(clickedButton);
            
            contentPanel.Controls.Clear();
            if (clickedButton == btnDashboard) LoadDashboardView();
            else if (clickedButton == btnTables) contentPanel.Controls.Add(new StaffTablesControl());
            else if (clickedButton == btnActiveOrders) ShowActiveOrdersView();
        }

        private void LoadDashboardView()
        {
            contentPanel.Controls.Clear();
            dashboardFlowPanel.Controls.Clear();
            contentPanel.Controls.Add(dashboardFlowPanel);

            var staffIdParam = new SqlParameter("@StaffId", this.loggedInStaffId);
            string totalTables = GetScalarData("SELECT COUNT(*) FROM tables").ToString();
            string myOrdersToday = GetScalarData("SELECT COUNT(*) FROM orders WHERE user_id = @StaffId AND CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)", new[] { staffIdParam }).ToString();
            
            var staffIdParam2 = new SqlParameter("@StaffId", this.loggedInStaffId);
            decimal mySalesTodayValue = Convert.ToDecimal(GetScalarData("SELECT ISNULL(SUM(total_amount), 0) FROM orders WHERE user_id = @StaffId AND CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)", new[] { staffIdParam2 }));
            string mySalesToday = mySalesTodayValue.ToString("c2");

            dashboardFlowPanel.Controls.Add(CreateStatCard(totalTables, "Total Tables", Color.FromArgb(23, 162, 184)));
            dashboardFlowPanel.Controls.Add(CreateStatCard(myOrdersToday, "My Orders Today", Color.FromArgb(40, 167, 69)));
            dashboardFlowPanel.Controls.Add(CreateStatCard(mySalesToday, "My Sales Today", Color.FromArgb(255, 193, 7)));
        }
        
        private void ShowActiveOrdersView()
        {
            contentPanel.Controls.Clear();
            var tableControl = new TableManagementControl();
            tableControl.TableSelected += OnTableSelected;
            contentPanel.Controls.Add(tableControl);
        }

        // --- THE MISSING METHOD IS NOW ADDED HERE ---
        // In StaffDashboard.cs

        // In StaffDashboard.cs

        // In StaffDashboard.cs

        private void OnTableSelected(object sender, TableSelectedEventArgs e)
        {
            contentPanel.Controls.Clear();
            var orderControl = new OrderManagementControl(e.TableId, e.TableName, this.loggedInStaffId);

            // This connects the "Back" button to the ShowActiveOrdersView method
            orderControl.BackToTablesClicked += (s, args) => ShowActiveOrdersView();

            // --- THIS IS THE CRUCIAL CONNECTION ---
            // This connects the 'OrderCompleted' event (raised after payment)
            // to the ShowActiveOrdersView method, forcing a refresh.
            orderControl.OrderCompleted += (s, args) => ShowActiveOrdersView();

            contentPanel.Controls.Add(orderControl);
        }
        private Button CreateNavButton(string text, int top)
        {
            Button button = new Button { Text = text, Top = top, Font = new Font("Segoe UI", 12F), ForeColor = colorFont, BackColor = colorSidebar, FlatStyle = FlatStyle.Flat, Height = 50, Width = navigationPanel.Width, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0), Cursor = Cursors.Hand, };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 61, 70);
            button.Click += NavButton_Click;
            return button;
        }
        
        private void SetActiveButton(Button activeButton)
        {
            foreach (Button button in navButtons)
            {
                button.BackColor = (button == activeButton) ? colorAccent : colorSidebar;
                button.ForeColor = (button == activeButton) ? Color.White : colorFont;
            }
        }
        
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

        private object GetScalarData(string query, SqlParameter[] parameters = null)
        {
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
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