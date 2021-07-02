using Key;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Runner
{
    class Program
    {
        static string _soundPath = @".\assets\{0}";
        static IEnumerable<string> _sounds = new List<string>
        {
            "applause.mp3", // 1
            "laugh-evil-1.mp3", // 2
            "Hello darkness.mp3", // 3
            "Sad Violin.mp3", // 4
            "cricketsounds090613.mp3", // 5
            "Woo Hoo.mp3", // 6
            "Sad Trombone Wah Wah Wah Fail Sound Effect.mp3" // 7
        };

        static void Main(string[] args)
        {
            var player = new AudioPlayer();

            var hotkeyThread = new Thread(() =>
            {
                if (_sounds.Count() > 9)
                    throw new Exception("too many sounds");

                for (int i = 0; i < _sounds.Count(); i++)
                {
                    int j = i;
                    Keys soundKey = Keys.D1 + i;
                    HotKeyManager.RegisterHotKey(soundKey, KeyModifiers.Alt, new Action<Keys, KeyModifiers>((key, mod) =>
                    {
                        var soundThread = new Thread(() =>
                        {
                            try
                            {
                                player.PlaySoundAsync(string.Format(_soundPath, _sounds.ElementAt(j)), 0).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        });
                        soundThread.Start();

                        var virtualSoundThread = new Thread(() =>
                        {
                            try
                            {
                                player.PlaySoundOnVirtualCableAsync(string.Format(_soundPath, _sounds.ElementAt(j))).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        });
                        virtualSoundThread.Start();
                    }));
                }
            });
            hotkeyThread.Start();

            player.StartRecording();

            Console.ReadLine();

            player.StopRecording();
        }
    }
}
