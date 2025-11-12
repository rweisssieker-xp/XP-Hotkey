using System;
using System.Windows.Forms;
using System.Drawing;

class TestTrayIcon
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            // Create a simple icon
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Blue);
                g.DrawString("XP", new Font("Arial", 12, FontStyle.Bold), Brushes.White, 5, 8);
            }
            IntPtr hIcon = bitmap.GetHicon();
            var icon = Icon.FromHandle(hIcon);

            // Create tray icon
            var notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Text = "Test XP Hotkey",
                Visible = true
            };

            notifyIcon.DoubleClick += (s, e) =>
            {
                MessageBox.Show("Tray Icon funktioniert!");
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Beenden", null, (s, e) => Application.Exit());
            notifyIcon.ContextMenuStrip = contextMenu;

            MessageBox.Show("Tray Icon sollte jetzt sichtbar sein!", "Test");

            Application.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler: {ex.Message}\n\n{ex.StackTrace}", "Fehler");
        }
    }
}
