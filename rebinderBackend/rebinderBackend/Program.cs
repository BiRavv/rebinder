
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
                Remap remap = new Remap(Keys.A, Keys.B);
                remap.Start();

                return 0;
            });
            
            Fetch.Listen("bind", "vf", () =>
            {
                Console.WriteLine("VF");
                Remap remap = new Remap(Keys.V, Keys.F);
                remap.Start();

                return 0;
            });
            
            Application.Run();
        }
    }
}