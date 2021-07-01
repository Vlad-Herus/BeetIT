using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace Player
{
    public class AudioPlayer
    {

        public void PlaySound(string path)
        {
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                Console.WriteLine($"{n}: {caps.ProductName}");
            }
            //using (var file = System.IO.File.OpenRead(path))
            //{
            //    PlaySound(file);
            //}
        }

        public void PlaySound(Stream audioStream)
        {
            using (var rdr = new Mp3FileReader(audioStream))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            {
                waveOut.Init(baStream);
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }
        }


    }
}
