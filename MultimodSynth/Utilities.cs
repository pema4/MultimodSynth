using Jacobi.Vst.Framework;
using System;
using System.IO;
using System.Text;

namespace MultimodSynth
{
    /// <summary>
    /// Класс, содержащий различные вспомогательные методы.
    /// </summary>
    static class Utilities
    {
        /// <summary>
        /// Стандартная частота дискретизации.
        /// </summary>
        private const double DefaultSampleRate = 44100;

        /// <summary>
        /// Метод, используемый для считывания значений параметров из файла.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="activeParameters"></param>
        public static void ReadParameters(Stream stream, VstParameterCollection activeParameters)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                foreach (var param in activeParameters)
                {
                    var name = reader.ReadString();
                    if (!activeParameters.Contains(name))
                        throw new ArgumentException("File contains wrong values.");
                    var normalizedValue = reader.ReadSingle();
                    if (normalizedValue < 0 || normalizedValue > 1)
                        throw new ArgumentException("File contains wrong values.");
                    activeParameters[name].NormalizedValue = normalizedValue;
                }
            }
        }

        /// <summary>
        /// Метод, используемый для записи значений параметров в файл.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="activeParameters"></param>
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
        
        /// <summary>
        /// Метод, переводящий номер клавиши в частоту.
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static double MidiNoteToFrequency(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0);
        }

        /// <summary>
        /// Статический конструктор, считывающий сгенерированные таблицы сэмплов из файла.
        /// </summary>
        static Utilities()
        {
            GetOrGenerateWaveTables();
        }

        /// <summary>
        /// Метод, обеспечивающий создание таблиц сэмплов.
        /// </summary>
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

        /// <summary>
        /// Массив таблиц сэмплов.
        /// </summary>
        public static WaveTableOscillator[] WaveTables;

        /// <summary>
        /// Функция-генератор синусоиды.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double SineGenerator(double phase, double freq, double maxFreq) => 
            Math.Sin(2 * Math.PI * phase);

        /// <summary>
        /// Функция-генератор треугольной волны.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double TriangleGenerator(double phase, double freq, double maxFreq)
        {
            int harmonicsCount = (int)(maxFreq / freq) / 2;
            double res = 0;
            for (int i = 0; i < harmonicsCount; ++i)
                res += Math.Pow(-1, i) * Math.Sin(2 * Math.PI * (2 * i + 1) * phase) / Math.Pow(2 * i + 1, 2);
            return res;
        }
        
        /// <summary>
        /// Функция-генератор пилообразной волны.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double SawGenerator(double phase, double freq, double maxFreq)
        {
            int harmonicsCount = (int)(maxFreq / freq);
            double res = 0;
            for (int i = 1; i <= harmonicsCount; ++i)
                res += Math.Pow(-1, i + 1) * Math.Sin(2 * Math.PI * i * phase) / i;
            return res;
        }

        /// <summary>
        /// Функция-генератор квадратной волны.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double SquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.5, freq, maxFreq);

        /// <summary>
        /// Функция-генератор квадратной волны с применением широтно-импульсной модуляции.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double HalfSquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.75, freq, maxFreq);

        /// <summary>
        /// Функция-генератор квадратной волны с применением широтно-импульсной модуляции.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="freq"></param>
        /// <param name="maxFreq"></param>
        /// <returns></returns>
        private static double QuarterSquareGenerator(double phase, double freq, double maxFreq) =>
            SawGenerator(phase, freq, maxFreq) - SawGenerator(phase + 0.875, freq, maxFreq);
    }
}
