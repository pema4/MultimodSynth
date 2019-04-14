using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Effects/42-soft-saturation.html
    /// 
    /// </summary>
    class DistortionManager : AudioComponentWithParameters
    {
        public enum DistortionType
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
        private OnePoleLowpassFilter ampFilter = new OnePoleLowpassFilter();
        private float dcOffset;
        private DistortionType distType;
        private float ampTarget;
        private bool isAmpChanging;
        private float amp = 1;
        private DCBlocker dcBlocker;
        private SvfFilter lowPass;
        private SoftClipper softClipper;
        private AbsClipper absClipper;
        private CubicClipper cubicClipper;
        private BitCrusher bitCrusher;
        private SampleRateReductor sampleRateReductor;
        
        public DistortionType DistType
        {
            get => distType;
            set
            {
                if (value != distType)
                {
                    distType = value;
                    OnPropertyChanged(nameof(DistType));
                }
            }
        }

        public VstParameterManager DistTypeManager { get; private set; }

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
            dcBlocker = new DCBlocker();
            lowPass = new SvfFilter(plugin)
            {
                Cutoff = 20000,
                Gain = 0,
                Q = 1,
                Type = SvfFilterType.Low,
            };
            absClipper = new AbsClipper();
            softClipper = new SoftClipper();
            cubicClipper = new CubicClipper();
            bitCrusher = new BitCrusher();
            sampleRateReductor = new SampleRateReductor();

            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            DistTypeManager = factory.CreateParameterManager(
                name: "TYPE",
                minValue: 0,
                maxValue: 6,
                defaultValue: 0,
                valueChangedHandler: SetDistType);
            CreateRedirection(DistTypeManager, nameof(DistTypeManager));

            FlavourManager = factory.CreateParameterManager(
                name: "AMT",
                defaultValue: 0.5f,
                valueChangedHandler: SetFlavour);
            CreateRedirection(FlavourManager, nameof(FlavourManager));

            AsymmetryManager = factory.CreateParameterManager(
                name: "ASYM",
                minValue: -1,
                maxValue: 1,
                valueChangedHandler: SetAsymmetry);
            CreateRedirection(AsymmetryManager, nameof(AsymmetryManager));

            AmpManager = factory.CreateParameterManager(
                name: "AMP",
                minValue: 0,
                maxValue: 4,
                defaultValue: 1,
                valueChangedHandler: SetAmp);
            CreateRedirection(AmpManager, nameof(AmpManager));

            LowPassCutoffManager = factory.CreateParameterManager(
                name: "LP",
                minValue: 20,
                maxValue: 20000,
                defaultValue: 15000,
                valueChangedHandler: SetLowPassCutoff);
            CreateRedirection(LowPassCutoffManager, nameof(LowPassCutoffManager));

            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0,
                valueChangedHandler: SetMix);
            CreateRedirection(MixManager, nameof(MixManager));
        }

        private void SetDistType(float value)
        {
            if (value < 1)
                DistType = DistortionType.None;
            else if (value < 2)
                DistType = DistortionType.AbsClipping;
            else if (value < 3)
                DistType = DistortionType.SoftClipping;
            else if (value < 4)
                DistType = DistortionType.CubicClipping;
            else if (value < 4)
                DistType = DistortionType.BitCrush;
            else
                DistType = DistortionType.SampleRateReduction;
        }

        private void SetFlavour(float value)
        {
            flavour = value;
            softClipper.Treshold = 1 - value;
            bitCrusher.Bits = 4 * (float)Math.Pow(1 << 13, 1 - value);
            sampleRateReductor.HoldTime = (float)Math.Pow(44100, 1 - value);
        }

        private void SetAsymmetry(float value)
        {
            dcOffset = value;
        }

        private void SetAmp(float value)
        {
            ampTarget = value;
            isAmpChanging = true;
        }

        private void SetLowPassCutoff(float value)
        {
            lowPass.Cutoff = value;
        }

        private void SetMix(float value)
        {
            wetCoeff = value;
            dryCoeff = 1 - value;
        }

        private void UpdateAmp()
        {
            var newValue = ampFilter.Process(ampTarget);
            if (amp != newValue)
                amp = newValue;
            else
                isAmpChanging = false;
        }

        public float Process(float input)
        {
            if (isAmpChanging)
                UpdateAmp();

            input =  amp * lowPass.Process(input);
            float distortedSample;
            switch (DistType)
            {
                case DistortionType.AbsClipping:
                    distortedSample = absClipper.Process(input + dcOffset);
                    break;

                case DistortionType.BitCrush:
                    distortedSample = bitCrusher.Process(input + dcOffset);
                    break;

                case DistortionType.CubicClipping:
                    distortedSample = cubicClipper.Process(input + dcOffset);
                    break;

                case DistortionType.SoftClipping:
                    distortedSample = softClipper.Process(input + dcOffset);
                    break;

                case DistortionType.SampleRateReduction:
                    distortedSample = sampleRateReductor.Process(input + dcOffset);
                    break;

                default:
                    return input;
            }

            var output = dcBlocker.Process(distortedSample);
            return dryCoeff * input + wetCoeff * output;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            lowPass.SampleRate = newSampleRate;
            sampleRateReductor.SampleRate = newSampleRate;
        }
    }
}
