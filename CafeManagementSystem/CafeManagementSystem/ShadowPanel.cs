// ShadowPanel.cs
using System.Drawing;
using System.Windows.Forms;

public class ShadowPanel : Panel
{
    public ShadowPanel()
    {
        this.BorderStyle = BorderStyle.None;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Draw shadow
        using (var pen = new Pen(Color.FromArgb(50, 0, 0, 0), 6))
        {
            e.Graphics.DrawRectangle(pen, new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 2, ClientRectangle.Height - 2));
        }
        // Draw the main rectangle
        using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
        {
            e.Graphics.DrawRectangle(pen, new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 3, ClientRectangle.Height - 3));
        }
    }
}