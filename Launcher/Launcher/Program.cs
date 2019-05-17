using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Launcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            PrivateFontCollection font = new PrivateFontCollection();
            int fontLength = Properties.Resources.Lato_Regular.Length;
            byte[] fontData = Properties.Resources.Lato_Regular;
            IntPtr data = Marshal.AllocCoTaskMem(fontLength);
            Marshal.Copy(fontData, 0, data, fontLength);
            font.AddMemoryFont(data, fontLength);
            Application.Run(new Launcher());
        }
    }
}
