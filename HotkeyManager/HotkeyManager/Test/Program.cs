using Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Key.HotKeyManager.RegisterHotKey(Keys.PageUp, KeyModifiers.None, new Action<Keys, KeyModifiers>((key, mod) =>
            {
                Console.WriteLine("aaaa");
            }));

            Key.HotKeyManager.RegisterHotKey(Keys.PageUp, KeyModifiers.None, new Action<Keys, KeyModifiers>((key, mod) =>
            {
                Console.WriteLine("aaaa");
            }));

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
