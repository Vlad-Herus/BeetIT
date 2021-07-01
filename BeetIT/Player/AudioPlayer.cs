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
        private SavingWaveProvider savingWaveProvider;
        private WaveOut player;
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
                        if (caps.ProductName.StartsWith("CABLE"))
                        {
                            _outDeviceId = n;
                            break;
                        }
                    }

                    if (_outDeviceId == null)
                        throw new Exception("Failed to find VB Virtual Cable Input device");
                }

                return _outDeviceId.Value;
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
            savingWaveProvider = new SavingWaveProvider(bufferedWaveProvider, "temp.wav");

            // set up playback
            player = new WaveOut();
            player.DeviceNumber = OutDeviceId;
            player.Init(savingWaveProvider);

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
            // finalise the WAV file
            savingWaveProvider.Dispose();
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        public async Task PlaySoundAsync(string path)
        {
            using (var file = File.OpenRead(path))
            {
                await PlaySoundAsync(file);
            }
        }

        public async Task PlaySoundAsync(Stream audioStream)
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
                    await Task.Delay(100);
                }
            }
        }
    }
}
