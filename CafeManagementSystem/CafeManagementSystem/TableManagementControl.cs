// TableManagementControl.cs

using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    // NOTE: The duplicate 'TableSelectedEventArgs' class definition has been REMOVED from this file.
    // This class now correctly USES the definition from the 'TableSelectedEventArgs.cs' file.

    public partial class TableManagementControl : UserControl
    {
        private FlowLayoutPanel flowLayoutTables;
        public event EventHandler<TableSelectedEventArgs> TableSelected;

        public TableManagementControl()
        {
            InitializeComponent();
            LoadTables();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(239, 241, 246);
            flowLayoutTables = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(flowLayoutTables);
        }

        public void LoadTables()
        {
            flowLayoutTables.Controls.Clear();
            string query = "SELECT table_id, table_name, status FROM tables ORDER BY table_id";
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(query, con))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        flowLayoutTables.Controls.Add(CreateTableButton(reader));
                    }
                }
            }
        }

        private RoundedButton CreateTableButton(SqlDataReader reader)
        {
            int tableId = Convert.ToInt32(reader["table_id"]);
            string tableName = reader["table_name"].ToString();
            string status = reader["status"].ToString();

            RoundedButton button = new RoundedButton
            {
                Text = tableName,
                Tag = new Tuple<int, string>(tableId, status), // Store both ID and Status
                Size = new Size(150, 120),
                Margin = new Padding(15),
                CornerRadius = 10,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;

            if (status.Equals("Available", StringComparison.OrdinalIgnoreCase)) { button.BackColor = Color.FromArgb(40, 167, 69); } // Green
            else { button.BackColor = Color.FromArgb(220, 53, 69); } // Red

            button.Click += TableButton_Click;
            return button;
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as RoundedButton;
            if (clickedButton == null) return;

            var tagData = clickedButton.Tag as Tuple<int, string>;
            int tableId = tagData.Item1;
            string status = tagData.Item2;
            string tableName = clickedButton.Text;

            // This logic is now in the OrderManagementControl, so we simplify this.
            // We ALWAYS raise the event and let the next screen decide what to do.
            TableSelected?.Invoke(this, new TableSelectedEventArgs(tableId, tableName));
        }
    }
}