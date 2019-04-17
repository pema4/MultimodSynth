using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Effects/42-soft-saturation.html
    /// </summary>
    class DistortionManager : AudioComponentWithParameters
    {
        public enum DistortionMode
        {
            None,
            AbsClipping,
            SoftClipping,
            CubicClipping,
            BitCrush,
            SampleRateReduction,
        }

        private float flavour;
        private float wetCoeff;
        private float dryCoeff = 1;
        private ParameterFilter ampFilter;
        private ParameterFilter mixFilter;
        private ParameterFilter asymmetryFilter;
        private float dcOffset;
        private DistortionMode mode;
        private float amp = 1;
        private DCBlocker dcBlocker;
        private SvfFilter lowPass;
        private SoftClipper softClipper;
        private AbsClipper absClipper;
        private CubicClipper cubicClipper;
        private BitCrusher bitCrusher;
        private SampleRateReductor sampleRateReductor;
        
        public DistortionMode Mode
        {
            get => mode;
            set
            {
                if (value != mode)
                {
                    mode = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }

        public VstParameterManager ModeManager { get; private set; }

        public VstParameterManager FlavourManager { get; private set; }

        public VstParameterManager AsymmetryManager { get; private set; }

        public VstParameterManager AmpManager { get; private set; }

        public VstParameterManager LowPassCutoffManager { get; private set; }

        public VstParameterManager MixManager { get; private set; }

        public DistortionManager(
            Plugin plugin,
            string parameterPrefix = "D",
            string parameterCategory = "effects")
            : base(plugin, parameterPrefix, parameterCategory)
        {
            dcBlocker = new DCBlocker(10);
            lowPass = new SvfFilter(type: SvfFilterType.Low);
            absClipper = new AbsClipper();
            softClipper = new SoftClipper();
            cubicClipper = new CubicClipper();
            bitCrusher = new BitCrusher();
            sampleRateReductor = new SampleRateReductor();

            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            ModeManager = factory.CreateParameterManager(
                name: "TYPE",
                minValue: 0,
                maxValue: 6,
                defaultValue: 0,
                valueChangedHandler: SetMode);
            CreateRedirection(ModeManager, nameof(ModeManager));

            FlavourManager = factory.CreateParameterManager(
                name: "FLAV",
                defaultValue: 0.5f,
                valueChangedHandler: SetFlavour);
            CreateRedirection(FlavourManager, nameof(FlavourManager));

            AsymmetryManager = factory.CreateParameterManager(
                name: "ASYM",
                minValue: -1,
                maxValue: 1,
                valueChangedHandler: SetAsymmetryTarget);
            CreateRedirection(AsymmetryManager, nameof(AsymmetryManager));
            asymmetryFilter = new ParameterFilter(UpdateAsymmetry, 0);

            AmpManager = factory.CreateParameterManager(
                name: "AMP",
                minValue: 0,
                maxValue: 4,
                defaultValue: 1,
                valueChangedHandler: x => ampFilter.SetTarget(x));
            CreateRedirection(AmpManager, nameof(AmpManager));
            ampFilter = new ParameterFilter(UpdateAmp, 1);

            LowPassCutoffManager = factory.CreateParameterManager(
                name: "LP",
                defaultValue: 1,
                valueChangedHandler: SetLowPassCutoff);
            CreateRedirection(LowPassCutoffManager, nameof(LowPassCutoffManager));

            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0,
                valueChangedHandler: x => mixFilter.SetTarget(x));
            CreateRedirection(MixManager, nameof(MixManager));
            mixFilter = new ParameterFilter(UpdateMix, 0);
        }

        private void SetMode(float value)
        {
            if (value < 1)
                Mode = DistortionMode.None;
            else if (value < 2)
                Mode = DistortionMode.AbsClipping;
            else if (value < 3)
                Mode = DistortionMode.SoftClipping;
            else if (value < 4)
                Mode = DistortionMode.CubicClipping;
            else if (value < 5)
                Mode = DistortionMode.BitCrush;
            else
                Mode = DistortionMode.SampleRateReduction;
        }

        private void SetFlavour(float value)
        {
            flavour = value;
            softClipper.Treshold = 1 - value;
            bitCrusher.Bits = (float)Math.Pow(1 << 16, 1 - value);
            sampleRateReductor.HoldTime = (float)Math.Pow(44100, 1 - value);
        }

        private void SetAsymmetryTarget(float value) => asymmetryFilter.SetTarget(value);

        private void UpdateAsymmetry(float value) => dcOffset = value;

        private void SetLowPassCutoff(float value)
        {
            // 14.287712379549449 == Math.Log(2, 20000)
            // 0.30249265803205166 == Math.Log(2, 20) / 14.287712379549449
            // 0.6975073419679482 = 1 - 0.30249265803205166
            var cutoff = (float)Math.Pow(2, 14.287712379549449 * (0.30249265803205166 + 0.69750734196794828 * value));
            lowPass.SetCutoff(cutoff);
        }

        private void UpdateMix(float value)
        {
            wetCoeff = value;
            dryCoeff = 1 - value;
        }

        private void UpdateAmp(float value) => amp = value;

        public float Process(float input)
        {
            ampFilter.Process();
            mixFilter.Process();
            asymmetryFilter.Process();

            input =  amp * lowPass.Process(input);
            float output = 0;
            switch (Mode)
            {
                case DistortionMode.AbsClipping:
                    output = absClipper.Process(input + dcOffset);
                    break;

                case DistortionMode.BitCrush:
                    output = bitCrusher.Process(input + dcOffset);
                    break;

                case DistortionMode.CubicClipping:
                    output = cubicClipper.Process(input + dcOffset);
                    break;

                case DistortionMode.SoftClipping:
                    output = softClipper.Process(input + dcOffset);
                    break;

                case DistortionMode.SampleRateReduction:
                    output = sampleRateReductor.Process(input + dcOffset);
                    break;

                default:
                    return input;
            }

            output = dcBlocker.Process(output);
            return dryCoeff * input + wetCoeff * output;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            dcBlocker.SampleRate = newSampleRate;
            lowPass.SampleRate = newSampleRate;
            sampleRateReductor.SampleRate = newSampleRate;
        }
    }
}
