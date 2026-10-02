// PaymentForm.cs

using System;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class PaymentForm : Form
    {
        private Label lblTitle, lblTotal, lblPaymentMethod;
        private RadioButton rbCash, rbCard, rbUPI;
        private RoundedButton btnConfirm;

        public string SelectedPaymentMethod { get; private set; }

        public PaymentForm(decimal totalAmount)
        {
            InitializeComponent();
            lblTotal.Text = $"Total Payable: {totalAmount:c2}";
        }

        private void InitializeComponent()
        {
            this.Text = "Process Payment";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(350, 300);
            this.BackColor = Color.White;

            lblTitle = new Label { Text = "Confirm Payment", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            lblTotal = new Label { Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 70), AutoSize = true, ForeColor = Color.FromArgb(0, 123, 255) };

            lblPaymentMethod = new Label { Text = "Select Payment Method:", Font = new Font("Segoe UI", 10F), Location = new Point(25, 120), AutoSize = true };

            rbCash = new RadioButton { Text = "Cash", Location = new Point(30, 150), Font = new Font("Segoe UI", 12F), AutoSize = true, Checked = true };
            rbCard = new RadioButton { Text = "Card", Location = new Point(130, 150), Font = new Font("Segoe UI", 12F), AutoSize = true };
            rbUPI = new RadioButton { Text = "UPI", Location = new Point(230, 150), Font = new Font("Segoe UI", 12F), AutoSize = true };

            btnConfirm = new RoundedButton { Text = "Confirm Payment", CornerRadius = 10, Location = new Point(170, 220), Size = new Size(160, 50), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnConfirm.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, lblTotal, lblPaymentMethod, rbCash, rbCard, rbUPI, btnConfirm });
            btnConfirm.Click += BtnConfirm_Click;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (rbCash.Checked) SelectedPaymentMethod = "Cash";
            else if (rbCard.Checked) SelectedPaymentMethod = "Card";
            else if (rbUPI.Checked) SelectedPaymentMethod = "UPI";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}