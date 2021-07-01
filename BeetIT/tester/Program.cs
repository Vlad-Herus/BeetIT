using Player;
using System;
using System.IO;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            new AudioPlayer().PlaySound(@"D:\Music\Lustra - Scotty Doesn't Know_q.mp3");
        }
    }
}
