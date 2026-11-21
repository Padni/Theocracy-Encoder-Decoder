using System;
using System.IO;
using System.Windows.Forms;

namespace TheocracyEncDecGUI
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            var form = new Form1();

            if (args != null && args.Length > 0 && File.Exists(args[0]))
            {
                form.LoadSingleFile(args[0]);
            }

            Application.Run(form);
        }
    }
}
