using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой плагин.
    /// </summary>
    class Plugin : VstPluginWithInterfaceManagerBase
    {
        /// <summary>
        /// Инициализирует новый объект типа Plugin.
        /// </summary>
        public Plugin() : base(
            "multimod synth",
            new VstProductInfo("multimod synth", "pema4", 1000),
            VstPluginCategory.Synth,
            VstPluginCapabilities.None,
            0,
            new FourCharacterCode("MSYN").ToInt32())
        {
        }

        /// <summary>
        /// Ссылка на компонент AudioProcessor плагина.
        /// </summary>
        public AudioProcessor AudioProcessor => GetInstance<AudioProcessor>();

        /// <summary>
        /// Ссылка на компонент MidiProcessor плагина.
        /// </summary>
        public MidiProcessor MidiProcessor => GetInstance<MidiProcessor>();

        /// <summary>
        /// Ссылка на компонент PluginPrograms плагина.
        /// </summary>
        public PluginPrograms Programs => GetInstance<PluginPrograms>();

        /// <summary>
        /// Метод, возвращающий реализацию интерфейса IVstPluginAudioProcessor.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null)
            {
                return new AudioProcessor(this);
            }

            return base.CreateAudioProcessor(instance);
        }

        /// <summary>
        /// Метод, возвращающий реализацию интерфейса IVstNidiProcessor.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (instance == null)
            {
                return new MidiProcessor(this);
            }

            return base.CreateMidiProcessor(instance);
        }

        /// <summary>
        /// Метод, возвращающий реализацию интерфейса IVstPluginPrograms.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
            {
                return new PluginPrograms(this);
            }

            return base.CreatePrograms(instance);
        }

        /// <summary>
        /// Метод, возвращающий реализацию интерфейса IVstPluginEditor.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
            {
                return new PluginEditor(this);
            }

            return base.CreateEditor(instance);
        }
    }
}