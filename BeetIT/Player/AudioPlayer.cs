using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace Player
{
    public class AudioPlayer
    {
        private int? _outDeviceId;
        private int OutDeviceId
        {
            get
            {

                if (_outDeviceId == null)
                {
                    for (int n = -1; n < WaveOut.DeviceCount; n++)
                    {
                        var caps = WaveOut.GetCapabilities(n);
                        if (caps.ProductName.StartsWith("VoiceMeeter Input"))
                        {
                            _outDeviceId = n;
                            break;
                        }
                    }

                    if (_outDeviceId == null)
                        throw new Exception("Failed to find VoiceMeeter Input device");
                }

                return _outDeviceId.Value;
            }
        }


        public void PlaySound(string path)
        {
            using (var file = File.OpenRead(path))
            {
                PlaySound(file);
            }
        }

        public void PlaySound(Stream audioStream)
        {
            using (var rdr = new Mp3FileReader(audioStream))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            {
                waveOut.DeviceNumber = OutDeviceId;
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
