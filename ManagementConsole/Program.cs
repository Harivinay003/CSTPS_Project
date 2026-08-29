using System;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualEMS.DataServices;

namespace ManagementConsole
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // To Show login dialog before opening main window
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    var user = login.LoggedInUser;

                    Application.Run(new Main(user));
                }
            }
            //Application.Run(new Main());
        }
    }
}