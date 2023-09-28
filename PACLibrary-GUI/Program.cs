using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PACLibrary;

namespace PACLibrary_GUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try { SetDefault(); } catch (Exception) { /* Might fail on some Wine installs */ }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0) { Application.Run(new MainForm(args[0])); }
            else { Application.Run(new MainForm()); }
        }

        /// <summary>
        /// Sets PACTool as the default application for .one files.
        /// </summary>
        public static void SetDefault()
        {
            // Navigate to Computer\HKEY_CURRENT_USER\Software\Classes\
            var classesKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

            // Create an entry for ONE files.
            var oneKey = classesKey.CreateSubKey(".pac");

            // Gets the path of the executable and the command string.
            string myExecutable = Assembly.GetEntryAssembly().Location;
            string command = $"\"{myExecutable}\" \"%1\"";

            // Create default key for .one\shell\Open\command\
            var commandKey = oneKey.CreateSubKey("shell\\Open\\command");
            commandKey.SetValue("", command);
        }
    }
}
