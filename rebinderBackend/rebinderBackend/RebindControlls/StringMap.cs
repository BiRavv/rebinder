namespace rebinderBackend.RebindControls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using rebinderBackend.RebindControls;
    
    public class StringMap : IBind
    {
          private readonly Keys fromKey;
        private readonly string textToPaste;
        private IntPtr hookId = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc proc;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public StringMap(Keys fromKey, string textToPaste)
        {
            this.fromKey = fromKey;
            this.textToPaste = textToPaste;
            this.proc = HookCallback;
        }

        public void Start()
        {
            if (hookId != IntPtr.Zero) return;
            hookId = SetHook(proc);
        }

        public void Stop()
        {
            if (hookId == IntPtr.Zero) return;
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == fromKey)
                {
                    try
                    {
                        // Backup original clipboard
                        string originalText = null;
                        bool hadText = Clipboard.ContainsText();
                        if (hadText)
                            originalText = Clipboard.GetText();

                        // Set clipboard to desired text
                        Clipboard.SetText(textToPaste);

                        // Simulate Ctrl+V
                        SendKeys.SendWait("^v");

                        // Optional: small delay to ensure paste completes
                        System.Threading.Thread.Sleep(50);

                        // Restore original clipboard
                        if (hadText)
                            Clipboard.SetText(originalText);
                        else
                            Clipboard.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Clipboard error: " + ex.Message);
                    }

                    return (IntPtr)1; // Block original key
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
        
        // P/Invoke
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}