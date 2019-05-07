using Jacobi.Vst.Framework;
using System;
using System.IO;
using System.Text;

namespace MultimodSynth
{
    static class Utilities
    {

        public static void ReadParameters(Stream stream, VstParameterCollection activeParameters)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                foreach (var param in activeParameters)
                {
                    var name = reader.ReadString();
                    var normalizedValue = reader.ReadSingle();
                    activeParameters[name].NormalizedValue = normalizedValue;
                }
            }
        }

        public static void WriteParameters(Stream stream, VstParameterCollection activeParameters)
        {
            using (var writer = new BinaryWriter(stream, Encoding.Default, true))
            {
                foreach (var param in activeParameters)
                {
                    writer.Write(param.Info.Name);
                    writer.Write(param.NormalizedValue);
                }
            }
        }

        private const double DefaultSampleRate = 44100;

        public static double MidiNoteToFrequency(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0);
        }

        static Utilities()
        {
            GetOrGenerateWaveTables();
        }

        private static void GetOrGenerateWaveTables()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = directory + @"\waveTables.bin";

            try
            {
                using (var file = new FileStream(path, FileMode.Open))
                {
                    WaveTables = new WaveTableOscillator[6];
                    for (int i = 0; i < WaveTables.Length; ++i)
                        WaveTables[i] = WaveTableOscillator.Deserialize(file);
                }
            }
            catch (IOException)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var file = new FileStream(path, FileMode.Create))
                {
                    WaveTables = new[]
                    {
                        new WaveTableOscillator(SineGenerator, 1, 1),
                        new WaveTableOscillator(TriangleGenerator, 20, DefaultSampleRate / 2),
                        new WaveTableOscillator(SawGenerator, 20, DefaultSampleRate / 2),
                        new WaveTableOscillator(SquareGenerator, 20, DefaultSampleRate / 2),
                        new WaveTableOscillator(HalfSquareGenerator, 20, DefaultSampleRate / 2),
                        new WaveTableOscillator(QuarterSquareGenerator, 20, DefaultSampleRate / 2),
                    };
                    foreach (var vt in WaveTables)
                        WaveTableOscillator.Serialize(file, vt);
                }
            }
        }

        public static WaveTableOscillator[] WaveTables;

        private static double SineGenerator(double phase, double freq, double maxFreq) => 
            Math.Sin(2 * Math.PI * phase);

        private static double TriangleGenerator(double phase, double freq, double maxFreq)
        {
            int harmonicsCount = (int)(maxFreq / freq) / 2;
            double res = 0;
            for (int i = 0; i < harmonicsCount; ++i)
                res += Math.Pow(-1, i) * Math.Sin(2 * Math.PI * (2 * i + 1) * phase) / Math.Pow(2 * i + 1, 2);
            return res;
        }
        
        private static double SawGenerator(double phase, double freq, double maxFreq)
        {
            int harmonicsCount = (int)(maxFreq / freq);
            double res = 0;
            for (int i = 1; i <= harmonicsCount; ++i)
                res += Math.Pow(-1, i + 1) * Math.Sin(2 * Math.PI * i * phase) / i;
            return res;
        }

        private static double SquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.5, freq, maxFreq);

        private static double HalfSquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.75, freq, maxFreq);

        private static double QuarterSquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.875, freq, maxFreq);
    }
}
