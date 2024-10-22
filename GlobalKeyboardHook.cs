using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace AutoClicker
{    
    public class GlobalKeyboardHook
    {
        // Define constants for the Windows API function
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        // Declare the hook handle
        private IntPtr _hookID = IntPtr.Zero;

        // Define the delegate for the hook procedure
        private LowLevelKeyboardProc _proc;

        public GlobalKeyboardHook()
        {
            _proc = HookCallback;
            // Set the hook
            _hookID = SetHook(_proc);
        }

        ~GlobalKeyboardHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // Set the hook
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // Callback function to handle key events
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    OnKeyDown?.Invoke(this, new KeyEventArgs(key));
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    OnKeyUp?.Invoke(this, new KeyEventArgs(key));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public event EventHandler<KeyEventArgs> OnKeyDown;
        public event EventHandler<KeyEventArgs> OnKeyUp;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class KeyEventArgs : EventArgs
    {
        public Key Key { get; private set; }

        public KeyEventArgs(Key key)
        {
            Key = key;
        }
    }

}
