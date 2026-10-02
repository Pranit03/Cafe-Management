using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public class RoundedButton : Button
    {
        private int _cornerRadius = 10;
        private Color _originalBackColor;

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                Invalidate(); // Redraw the button
            }
        }

        public RoundedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.FromArgb(0, 123, 255);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Cursor = Cursors.Hand;
            _originalBackColor = this.BackColor;

            // Add hover effect
            this.MouseEnter += (s, e) => this.BackColor = ControlPaint.Dark(this.BackColor, 0.2f);
            this.MouseLeave += (s, e) => this.BackColor = _originalBackColor;
        }

        // This is needed to update the hover color if the BackColor is changed programmatically
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            _originalBackColor = this.BackColor;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();
            int radius = _cornerRadius * 2;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            if (radius > 0)
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            }
            else
            {
                this.Region = new Region(rect);
            }
        }
    }
}