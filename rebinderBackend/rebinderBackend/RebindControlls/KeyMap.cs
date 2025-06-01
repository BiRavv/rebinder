using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using rebinderBackend.RebindControls;

public class KeyMap : IBind
{
    private readonly Keys fromKey;
    private readonly Keys[] toKeys;
    private IntPtr hookId = IntPtr.Zero;
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc proc;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    public KeyMap(Keys fromKey, Keys[] toKeys)
    {
        this.fromKey = fromKey;
        this.toKeys = toKeys;
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

    public string ToFrontendData()
    {
        return "0&"+ (int)fromKey +">"+String.Join(";", toKeys);
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
                SendVirtualKeys(toKeys);
                return (IntPtr)1; // Block original key
            }
        }
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }
    
    // This is helper a method, helps press keys more low level.
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    private const int KEYEVENTF_KEYUP = 0x0002;
    private void SendVirtualKeys(Keys[] keys)
    {
        foreach (Keys key in keys)
        {
            byte vk = (byte)key;
            keybd_event(vk, 0, 0, 0); // Key down
        }
        foreach (Keys key in keys)
        {
            byte vk = (byte)key;
            keybd_event(vk, 0, KEYEVENTF_KEYUP, 0); // Key up
        }
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
