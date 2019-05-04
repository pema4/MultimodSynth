using Jacobi.Vst.Core;
using Jacobi.Vst.Framework.Plugin;
using System.Threading;

namespace BetterSynth
{
    internal class AudioProcessor : VstPluginAudioProcessorBase
    {
        private Plugin plugin;

        public Routing Routing { get; private set; }

        public Mutex ProcessingMutex { get; set; } = new Mutex();

        public override float SampleRate { get => Routing.SampleRate; set => Routing.SampleRate = value; }

        public AudioProcessor(Plugin plugin) : base(0, 2, 0)
        {
            this.plugin = plugin;
            Routing = new Routing(plugin);
        }

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