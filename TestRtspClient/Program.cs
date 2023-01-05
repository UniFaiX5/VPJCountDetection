using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace VPJCountDetection
{
    class Program
    {
        public static int Boxnm { get; private set; }
        private static string appGuid = "c0a09b5a-12ab-45c5-b9d9-d693fc96e7b9";

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Another instance of this application is already running, Try closing or end-tasking the previous instance before opening the app.", "Parallel Application Instance", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                Application.Run(new VPJCountDetection());
            }

















            //   Thread trdfrm = new Thread(Formrunner);

            // trdfrm.Start();


        }



        private static void Formrunner(object obj)
        {
           


        }





    }
}

