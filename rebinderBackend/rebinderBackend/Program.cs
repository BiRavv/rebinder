
using System;
using System.Threading;
using System.Windows.Forms;
using rebinderBackend.FrontendConnection;
using rebinderBackend.RebindControls;

namespace rebinderBackend
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            
            Console.WriteLine("Remapping A → D. Press Ctrl+C to exit.");
            
            Fetch.Listen("bind", "ab", () =>
            {
                Console.WriteLine("AB");
                KeyMap keyMap = new KeyMap(Keys.A, Keys.B);
                keyMap.Start();

                return 0;
            });
            
            Fetch.Listen("bind", "vf", () =>
            {
                Console.WriteLine("VF");
                KeyMap keyMap = new KeyMap(Keys.V, Keys.F);
                keyMap.Start();

                return 0;
            });
            
            Application.Run();
        }
    }
}