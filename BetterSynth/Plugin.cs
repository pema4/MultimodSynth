using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace BetterSynth
{
    class Plugin : VstPluginWithInterfaceManagerBase
    {
        public Plugin() : base(
            "BetterSynth",
            new VstProductInfo("BetterSynth", "pema4", 1000),
            VstPluginCategory.Synth,
            VstPluginCapabilities.None,
            0,
            new FourCharacterCode("BSYN").ToInt32())
        {
        }

        public AudioProcessor AudioProcessor => GetInstance<AudioProcessor>();

        public MidiProcessor MidiProcessor => GetInstance<MidiProcessor>();

        public PluginPrograms Programs => GetInstance<PluginPrograms>();

        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
            {
                return new AudioProcessor(this);
            }

            return base.CreateAudioProcessor(instance);
        }

        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (instance == null)
            {
                return new MidiProcessor(this);
            }

            return base.CreateMidiProcessor(instance);
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
            {
                return new PluginPrograms(this);
            }

            return base.CreatePrograms(instance);
        }

        /*
        protected override IVstPluginPersistence CreatePersistence(IVstPluginPersistence instance)
        {
            if (instance == null)
            {
                return new PluginPersistence(this);
            }

            return base.CreatePersistence(instance);
        }
        */
    }
}