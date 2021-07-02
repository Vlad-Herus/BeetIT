using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Key
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }

    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    public static class HotKeyManager
    {
        private static int THREAD_VARIABLE_ID = 0;

        #region Imports

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        #region HiddenForm

        private static volatile MessageWindow _wnd;
        private static volatile IntPtr _hwnd;
        private static ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);

        private class MessageWindow : Form
        {
            public MessageWindow()
            {
                _wnd = this;
                _hwnd = this.Handle;
                _windowReadyEvent.Set();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
                    HotKeyManager.OnHotKeyPressed(e);
                }
                else
                    base.WndProc(ref m);
            }

            protected override void SetVisibleCore(bool value)
            {
                // Ensure the window never becomes visible
                base.SetVisibleCore(false);
            }

            private const int WM_HOTKEY = 0x312;
        }

        #endregion

        #region Signature

        private class Signature
        {
            public Keys Key { get; private set; }
            public KeyModifiers Modifiers { get; private set; }
            public int HotkeyId { get; private set; }
            public Guid UniqueId { get; private set; }

            public Signature(Keys key, KeyModifiers modifiers, int hotkeyId)
            {
                Key = key;
                Modifiers = modifiers;
                HotkeyId = hotkeyId;
                UniqueId = Guid.NewGuid();
            }

            public bool Match(Signature other)
            {
                return Match(other.Key, other.Modifiers);
            }

            public bool Match(Keys key, KeyModifiers modifiers)
            {
                if (Key == key && Modifiers == modifiers)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        static Dictionary<Signature, Action<Keys, KeyModifiers>> HotkeyHandlers = new Dictionary<Signature, Action<Keys, KeyModifiers>>();

        static HotKeyManager()
        {
            Thread messageLoop = new Thread(delegate ()
            {
                Application.Run(new MessageWindow());
            });
            messageLoop.Name = "MessageLoopThread";
            messageLoop.IsBackground = true;
            messageLoop.Start();
        }

        #region Public 

        public static Guid RegisterHotKey(Keys key, KeyModifiers modifiers, Action<Keys, KeyModifiers> action)
        {
            _windowReadyEvent.WaitOne();

            int hotkeyId = -1;
            bool foundMatch = false;
            foreach (var sig in HotkeyHandlers)
                if (sig.Key.Match(key, modifiers))
                {
                    hotkeyId = sig.Key.HotkeyId;
                    foundMatch = true;
                    break;
                }

            if (!foundMatch)
                hotkeyId = System.Threading.Interlocked.Increment(ref THREAD_VARIABLE_ID);

            var newSig = new Signature(key, modifiers, hotkeyId);

            if (!foundMatch)
                _wnd.Invoke(new Action(() =>
               RegisterHotKeyInternal(_hwnd, newSig.HotkeyId, (uint)modifiers, (uint)key)
               ));

            HotkeyHandlers.Add(newSig, action);
            return newSig.UniqueId;
        }

        public static void UnregisterHotKey(Guid uniqueId)
        {
            var sig = HotkeyHandlers.FirstOrDefault(hnd => hnd.Key.UniqueId == uniqueId).Key;
            if (sig != null)
            {
                if (!HotkeyHandlers.Where(toup => toup.Key != sig)
                    .Any(hnd => hnd.Key.Match(sig)))
                {
                    _wnd.Invoke(new Action(() =>
                        UnRegisterHotKeyInternal(_hwnd, sig.HotkeyId)
                        ));
                }
                HotkeyHandlers.Remove(sig);
            }
        }

        #endregion

        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        {
            RegisterHotKey(hwnd, id, modifiers, key);
        }

        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
        {
            UnregisterHotKey(_hwnd, id);
        }

        private static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            foreach (var hnd in HotkeyHandlers.ToArray())
            {
                if (hnd.Key.Match(e.Key, e.Modifiers))
                    hnd.Value(e.Key, e.Modifiers);
            }
        }
    }
}
