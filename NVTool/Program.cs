/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : Program.cs
* date      : 2023/7/7 10:13:13
* author    : jinlong.wang
* brief     : main function
* section Modification History
* - 1.0 : Initial version (2023/7/7 10:13:13) - jinlong.wang
***************************************************************************************************/

#define ForTest
using NVTool.UI;
using System;
using System.Windows.Forms;

namespace NVTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
//#if DEBUG
  
            Application.Run(new MainForm());
//#else
//            FormLogin login = new FormLogin();
//            login.StartPosition = FormStartPosition.CenterScreen;
//            if (login.ShowDialog() == DialogResult.OK)
//            { 
//                Application.Run(new MainForm());
//            }
//            else
//            {
//                Application.Exit();
//            }
//#endif
        }
    }
}
