using Jacobi.Vst.Framework;
using System;

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

        private float amount;
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
        private IDistortion currentDistortion;

        public VstParameterManager ModeManager { get; private set; }

        public VstParameterManager AmountManager { get; private set; }

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
            lowPass = new SvfFilter(type: SvfFilter.FilterType.Low);
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

            AmountManager = factory.CreateParameterManager(
                name: "AMNT",
                defaultValue: 0.5f,
                valueChangedHandler: SetAmount);
            CreateRedirection(AmountManager, nameof(AmountManager));

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
            {
                if (mode != DistortionMode.None)
                    mode = DistortionMode.None;
            }
            else if (value < 2)
            {
                if (mode != DistortionMode.AbsClipping)
                {
                    mode = DistortionMode.AbsClipping;
                    ChangeDistortion(absClipper);
                }
            }
            else if (value < 3)
            {
                if (mode != DistortionMode.SoftClipping)
                {
                    mode = DistortionMode.SoftClipping;
                    ChangeDistortion(softClipper);
                }
            }
            else if (value < 4)
            {
                if (mode != DistortionMode.CubicClipping)
                {
                    mode = DistortionMode.CubicClipping;
                    ChangeDistortion(cubicClipper);
                }
            }
            else if (value < 5)
            {
                if (mode != DistortionMode.BitCrush)
                {
                    mode = DistortionMode.BitCrush;
                    ChangeDistortion(bitCrusher);
                }
            }
            else
            {
                if (mode != DistortionMode.SampleRateReduction)
                {
                    mode = DistortionMode.SampleRateReduction;
                    ChangeDistortion(sampleRateReductor);
                }
            }
        }

        private void ChangeDistortion(IDistortion newDistortion)
        {
            currentDistortion = newDistortion;
            currentDistortion.SetAmount(amount);
        }

        private void SetAmount(float value)
        {
            amount = value;
            currentDistortion?.SetAmount(amount);
        }

        private void SetAsymmetryTarget(float value) => asymmetryFilter.SetTarget(value);

        private void UpdateAsymmetry(float value) => dcOffset = value;

        private void SetLowPassCutoff(float value)
        {
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

            input *= amp;
            if (mode == DistortionMode.None)
                return dcBlocker.Process(input);
            else
            {
                input = lowPass.Process(input);
                var output = currentDistortion.Process(input + dcOffset);
                output = dcBlocker.Process(output);
                return dryCoeff * input + wetCoeff * output;
            }
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            dcBlocker.SampleRate = newSampleRate;
            lowPass.SampleRate = newSampleRate;
            sampleRateReductor.SampleRate = newSampleRate;
        }
    }
}
