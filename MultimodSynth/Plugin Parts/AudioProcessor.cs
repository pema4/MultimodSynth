using Jacobi.Vst.Core;
using Jacobi.Vst.Framework.Plugin;
using System.Threading;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация интерфейса IVstPluginAudioProcessor.
    /// </summary>
    class AudioProcessor : VstPluginAudioProcessorBase
    {
        /// <summary>
        /// Ссылка на плагин, которому принадлежит этот компонент.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Ссылка на объект, предстающий собой всю цепочку создания и обработки звука.
        /// </summary>
        public Routing Routing { get; private set; }

        /// <summary>
        /// Мьютекс, используемый для синхронизации UI-потока и основного потока.
        /// </summary>
        public Mutex ProcessingMutex { get; set; } = new Mutex();

        /// <summary>
        /// Текущая частота дискретизации.
        /// </summary>
        public override float SampleRate
        {
            get => Routing.SampleRate;
            set => Routing.SampleRate = value;
        }

        /// <summary>
        /// Инициализирует новых объект типа AudioProcessor, принадлежащий заданному плагину.
        /// </summary>
        /// <param name="plugin"></param>
        public AudioProcessor(Plugin plugin) : base(0, 2, 0)
        {
            this.plugin = plugin;
            Routing = new Routing(plugin);
        }

        /// <summary>
        /// Метод, обрабатывающий входные данные, поступающие от плагина, и генерирующий новые выходные данные.
        /// </summary>
        /// <param name="inChannels">Входные каналы.</param>
        /// <param name="outChannels">Выходные каналы.</param>
        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            ProcessingMutex.WaitOne();
            var outputLeft = outChannels[0];
            var outputRight = outChannels[1];

            for (int i = 0; i < outputLeft.SampleCount; ++i)
            {
                Routing.Process(out var left, out var right);

                outputLeft[i] = left;
                outputRight[i] = right;
            }
            ProcessingMutex.ReleaseMutex();
        }
    }
}