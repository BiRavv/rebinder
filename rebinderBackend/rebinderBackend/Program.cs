
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
            // Start listening for the frontend
            Fetch.Listen();
            // Implements Listeners
            ImplementListeners();
            
            
            Scenario sc = new Scenario("one");
            sc.AddBind(new StringMap(Keys.A, "test string 😎 "));
            sc.AddBind(new KeyMap(Keys.B, [Keys.LWin, Keys.D]));
            //sc.SetActive(true);

            
            Application.Run();
        }

        private static void ImplementListeners()
        {
            // Listen for new scenarios
            Fetch.AddListener(body =>
            {
                if (!body.StartsWith("add_scenario@") || 
                    body.Split(new [] {'@'},3)[1].Length == 0) return null;
                
                Scenario scenario = new Scenario(body.Split(new [] {'@'},3)[1]);
                return "add_scenario@"+scenario.Name;
            });
        }
        private static void CaptureMain()
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