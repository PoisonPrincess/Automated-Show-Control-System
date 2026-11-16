using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace ShowAutomation.UI.ViewModels.Media
{
    public class SongWaveformGenerator
    {
        public event EventHandler<EventArgs> UpdateData;
        public String songPath;
        public Dispatcher UIDisaptcher;
        public List<float>[] waves;

        public void getWaveData() {
            waves = new List<float>[3] { new List<float>(), new List<float>(), new List<float>() };
            waves[0] = getWaveData(4); // low detail
            UIDisaptcher.BeginInvoke((ThreadStart)(() => RaiseUpdateEvent(waves)));
            waves[1] = getWaveData(10); // mid detail
            UIDisaptcher.BeginInvoke((ThreadStart)(() => RaiseUpdateEvent(waves)));
            waves[2] = getWaveData(20); // high detail
            UIDisaptcher.BeginInvoke((ThreadStart)(() => RaiseUpdateEvent(waves)));
        }

        public List<float> getWaveData(int samplesPerSec) {
            //uses NAudio library to read song data and make waveform float[] form it
            if (!File.Exists((String)songPath))
            {
                return null;
            }
            AudioFileReader reader = new AudioFileReader(songPath);
            int estimatedFloatsPerSec = reader.WaveFormat.AverageBytesPerSecond / sizeof(float);
            List<float> wave = new List<float>();
            int count = 0;
            int endOfFile;
            float highPeak = 0, lowPeak = 0;
            //reads data out of file one float at a time to find end of file
            do
            {
                count++;
                float[] num = new float[reader.WaveFormat.BlockAlign / 2];
                endOfFile = reader.Read(num, 0, reader.WaveFormat.BlockAlign/2);
                if (num[0] > highPeak)
                {
                    highPeak = num[0];
                }else if (num[0] < lowPeak)
                {
                    lowPeak = num[0];
                }
                if (count % (estimatedFloatsPerSec / samplesPerSec) == 0 || endOfFile == 0)
                {
                    try
                    {
                        wave.Add(highPeak);
                        wave.Add(lowPeak);
                    }
                    catch (OutOfMemoryException e) {
                        Console.WriteLine(e);
                        return new float[10].ToList();
                    }
                    highPeak = 0;
                    lowPeak = 0;
                }
                //file ends endOfFile = 0
            } while (endOfFile != 0);
            reader.Close();
            
            return wave;
        }

        private void RaiseUpdateEvent(List<float>[] waves)
        {
            UpdateData?.Invoke(waves, EventArgs.Empty);
        }
    }
}
