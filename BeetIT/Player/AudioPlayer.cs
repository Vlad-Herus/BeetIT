using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Player
{
    public class AudioPlayer
    {
        private WaveInEvent recorder;
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveOut player;

        private int VirtualOutDeviceNumber
        {
            get
            {
                for (int n = -1; n < WaveOut.DeviceCount; n++)
                {
                    var caps = WaveOut.GetCapabilities(n);
                    if (caps.ProductName.StartsWith("CABLE"))
                    {
                        return n;
                    }
                }

                throw new Exception("Failed to find VB Virtual Cable Input device");
            }
        }

        public void StartRecording(int microphoneDeviceNumber = 0)
        {
            // set up the recorder
            recorder = new WaveInEvent();
            recorder.DeviceNumber = microphoneDeviceNumber; // Note that device number 0 represents the default device
            recorder.DataAvailable += RecorderOnDataAvailable;

            // set up our signal chain
            bufferedWaveProvider = new BufferedWaveProvider(recorder.WaveFormat);

            // set up playback
            player = new WaveOut();
            player.DeviceNumber = VirtualOutDeviceNumber;
            player.Init(bufferedWaveProvider);

            // begin playback & record
            player.Play();
            recorder.StartRecording();
        }

        public void StopRecording()
        {
            // stop recording
            recorder.StopRecording();
            // stop playback
            player.Stop();
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        public async Task PlaySoundAsync(string path, int? physicalOutDeviceNumber = null)
        {
            using (var file = File.OpenRead(path))
            {
                await PlaySoundAsync(file, physicalOutDeviceNumber);
            }
        }

        public async Task PlaySoundAsync(Stream audioStream, int? physicalOutDeviceNumber = null)
        {
            using (var rdr = new Mp3FileReader(audioStream))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var virtualWaveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            {
                virtualWaveOut.DeviceNumber = VirtualOutDeviceNumber;
                virtualWaveOut.Init(baStream);
                virtualWaveOut.Play();

                if (physicalOutDeviceNumber.HasValue)
                {
                    waveOut.DeviceNumber = physicalOutDeviceNumber.Value;
                    waveOut.Init(baStream);
                    waveOut.Play();
                }

                while (virtualWaveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
