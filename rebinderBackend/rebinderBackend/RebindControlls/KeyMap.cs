using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using rebinderBackend.RebindControls;

public class KeyMap : IBind
{
    private readonly Keys fromKey;
    private readonly string toKeyChar;
    private IntPtr hookId = IntPtr.Zero;
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc proc;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    public KeyMap(Keys fromKey, Keys toKey)
    {
        this.fromKey = fromKey;
        this.toKeyChar = toKey.ToString().ToLower();
        this.proc = HookCallback;
    }

    public void Start()
    {
        if (hookId != IntPtr.Zero) return;
        hookId = SetHook(proc);
        Application.Run();
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
                SendKeys.SendWait(toKeyChar);
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
