using Player;
using System;
using System.IO;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            new AudioPlayer().PlaySound(@"./assets/applause.mp3");
        }
    }
}
