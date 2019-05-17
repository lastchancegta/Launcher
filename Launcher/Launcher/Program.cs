using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*
Copyright 2019 Sushi

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

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
