using System;
using System.Windows.Forms;

namespace OneDayOneBing
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (MainTask())
            {

            }
            ShowForm();
        }

        public static bool MainTask()
        {
            throw new NotImplementedException();
        }

        private static void ShowForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
