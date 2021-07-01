using Player;
using System;
using System.Threading.Tasks;

namespace tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Conditions to work:
            // - Need to set the Slack/Zoom microphone to be the VB Virtual Cable Input/Speaker.
            //   - This condition can be reduced impact if we set our application to run in the background at all times.
            // - Default system microphone should be set to the microphone you'd normally use for Slack/Zoom.
            //   - Can be fixed with a UI that provide an option

            var player = new AudioPlayer();

            Console.WriteLine("Start Recording");

            player.StartRecording();
            
            await player.PlaySoundAsync(@"./assets/applause.mp3", 0);
            await player.PlaySoundAsync(@"./assets/laugh-evil-1.mp3", 0);

            Console.WriteLine("Sounds has finished playing. Press enter to close the application...");
            Console.ReadLine();

            player.StopRecording();
        }
    }
}
