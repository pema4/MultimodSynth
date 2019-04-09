using Jacobi.Vst.Core;
using Jacobi.Vst.Framework.Plugin;

namespace BetterSynth
{
    internal class AudioProcessor : VstPluginAudioProcessorBase
    {
        private Plugin plugin;
        private Routing routing;

        public AudioProcessor(Plugin plugin) : base(0, 2, 0)
        {
            this.plugin = plugin;
            routing = new Routing(plugin);
        }

        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            var outputLeft = outChannels[0];
            var outputRight = outChannels[1];

            for (int i = 0; i < outputLeft.SampleCount; ++i)
            {
                routing.Process(out var left, out var right);

                outputLeft[i] = left;
                outputRight[i] = right;
            }
        }
    }
}