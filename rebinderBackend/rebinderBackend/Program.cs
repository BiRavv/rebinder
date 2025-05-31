
using System;
using System.Threading;
using System.Windows.Forms;
using rebinderBackend.FrontendConnection;
using rebinderBackend.RebindControls;

namespace rebinderBackend
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {

            // Capture the main thread context. Do not even ask!
            CaptureMain();
            
            Scenario sc = new Scenario("one");
            sc.AddBind(new StringMap(Keys.A, "test string 😎 "));
            sc.AddBind(new KeyMap(Keys.B, Keys.C));
            
            Application.Run();
        }

        public static void CaptureMain()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Force the installation of WinForms synchronization context
            var context = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            Fetch.Init(context);
        }
    }
}