using Jacobi.Vst.Framework;
using System;

namespace BetterSynth
{
    /// <summary>
    /// Represents a delay effect component of the plugin.
    /// </summary>
    class DistortionManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Specifies a distortion mode.
        /// </summary>
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
        private float amp = 1;
        private float dcOffset;
        private float dryCoeff = 1;
        private DistortionMode mode;
        private float wetCoeff;
        private IDistortion currentDistortion;
        private SoftClipper softClipper;
        private AbsClipper absClipper;
        private CubicClipper cubicClipper;
        private BitCrusher bitCrusher;
        private SampleRateReductor sampleRateReductor;
        private DCBlocker dcBlocker;
        private SvfFilter lowPass;
        private ParameterFilter ampFilter;
        private ParameterFilter mixFilter;
        private ParameterFilter asymmetryFilter;

        /// <summary>
        /// Manager of the distortion mode parameter.
        /// </summary>
        public VstParameterManager ModeManager { get; private set; }

        /// <summary>
        /// Manager of the distortion amount parameter.
        /// </summary>
        public VstParameterManager AmountManager { get; private set; }

        /// <summary>
        /// Manager of the distortion asymmetry parameter.
        /// </summary>
        public VstParameterManager AsymmetryManager { get; private set; }

        /// <summary>
        /// Manager of the distortion preamp parameter.
        /// </summary>
        public VstParameterManager AmpManager { get; private set; }

        /// <summary>
        /// Manager of the distortion lowpass cutoff parameter.
        /// </summary>
        public VstParameterManager LowPassCutoffManager { get; private set; }

        /// <summary>
        /// Manager of the distortion mix parameter.
        /// </summary>
        public VstParameterManager MixManager { get; private set; }

        /// <summary>
        /// Initialized a new DistortionManager class instance that belongs to given plugin
        /// and has specified parameter name prefix.
        /// </summary>
        /// <param name="plugin">A plugin instance to which a new distortion belongs.</param>
        /// <param name="parameterPrefix">A prefix for parameter's names.</param>
        public DistortionManager(
            Plugin plugin,
            string parameterPrefix = "D")
            : base(plugin, parameterPrefix)
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

        /// <summary>
        /// Initializes parameter managers of the DistortionManager class instance.
        /// </summary>
        /// <param name="factory">A parameter factory used for parameter initialization.</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Distortion mode parameter.
            ModeManager = factory.CreateParameterManager(
                name: "TYPE",
                valueChangedHandler: SetMode);

            // Distotion amount parameter.
            AmountManager = factory.CreateParameterManager(
                name: "AMNT",
                defaultValue: 0.5f,
                valueChangedHandler: SetAmount);

            // Distortion asymmetry parameter.
            AsymmetryManager = factory.CreateParameterManager(
                name: "ASYM",
                defaultValue: 0.5f,
                valueChangedHandler: SetAsymmetryTarget);
            asymmetryFilter = new ParameterFilter(UpdateAsymmetry, 0);

            // Distortion amp parameter.
            AmpManager = factory.CreateParameterManager(
                name: "AMP",
                defaultValue: 0.25f,
                valueChangedHandler: SetAmpTarget);
            ampFilter = new ParameterFilter(UpdateAmp, 1);

            // Distortion lowpass cutoff parameter.
            LowPassCutoffManager = factory.CreateParameterManager(
                name: "LP",
                defaultValue: 1,
                valueChangedHandler: SetLowPassCutoff);

            // Distortion mix parameter.
            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0.5f,
                valueChangedHandler: x => mixFilter.SetTarget(x));
            mixFilter = new ParameterFilter(UpdateMix, 0);
        }

        /// <summary>
        /// Handles the distortion mode parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetMode(float value)
        {
            var newMode = Converters.ToDistortionMode(value);
            if (newMode != mode)
            {
                mode = newMode;
                switch (mode)
                {
                    case DistortionMode.AbsClipping:
                        ChangeDistortion(absClipper);
                        break;
                    case DistortionMode.SoftClipping:
                        ChangeDistortion(softClipper);
                        break;
                    case DistortionMode.CubicClipping:
                        ChangeDistortion(cubicClipper);
                        break;
                    case DistortionMode.BitCrush:
                        ChangeDistortion(bitCrusher);
                        break;
                    case DistortionMode.SampleRateReduction:
                        ChangeDistortion(sampleRateReductor);
                        break;
                }
            }
        }

        /// <summary>
        /// Changes a current distortion.
        /// </summary>
        /// <param name="newDelay">A new current distortion.</param>
        private void ChangeDistortion(IDistortion newDistortion)
        {
            currentDistortion = newDistortion;
            currentDistortion?.SetAmount(amount);
        }

        /// <summary>
        /// Handles the amount parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetAmount(float value)
        {
            amount = value;
            currentDistortion?.SetAmount(amount);
        }

        /// <summary>
        /// Handles the asymmetry parameter changes.
        /// Updates a target value of the asymmetry smoothing filter.
        /// </summary>
        /// <param name="target">A new value of the parameter.</param>
        private void SetAsymmetryTarget(float value)
        {
            asymmetryFilter.SetTarget(value);
        }

        /// <summary>
        /// Handles the asymmetry parameter filter changes (called every processing turn).
        /// </summary>
        /// <param name="value">A new value of the asymmetry.</param>
        private void UpdateAsymmetry(float value)
        {
            dcOffset = (float)Converters.ToAsymmetry(value);
        }

        /// <summary>
        /// Handles the lowpass cutoff parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetLowPassCutoff(float value)
        {
            var cutoff = (float)Converters.ToDistortionLowpassCutoff(value);
            lowPass.SetCutoff(cutoff);
        }

        /// <summary>
        /// Handles the dry-wet mix parameter filter changes (called every processing turn).
        /// </summary>
        /// <param name="value">A new value of the dry-wet mix.</param>
        private void UpdateMix(float value)
        {
            wetCoeff = value;
            dryCoeff = 1 - value;
        }

        /// <summary>
        /// Handles the preamp parameter changes.
        /// Updates a target value of the preamp smoothing filter.
        /// </summary>
        /// <param name="target">A new value of the parameter.</param>
        private void SetAmpTarget(float value)
        {
            ampFilter.SetTarget((float)Converters.ToDistortionAmp(value));
        }

        /// <summary>
        /// Handles the preamp parameter filter changes (called every processing turn).
        /// </summary>
        /// <param name="value">A new value of the preamp.</param>
        private void UpdateAmp(float value)
        {
            amp = value;
        }

        /// <summary>
        /// Performs a processing turn.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <returns>Output.</returns>
        public float Process(float input)
        {
            ampFilter.Process();
            mixFilter.Process();
            asymmetryFilter.Process();

            input *= amp;
            input = lowPass.Process(input);
            if (mode == DistortionMode.None)
                return dcBlocker.Process(input);
            else
            {
                var output = currentDistortion.Process(input + dcOffset);
                output = dcBlocker.Process(output);
                return dryCoeff * input + wetCoeff * output;
            }
        }

        /// <summary>
        /// Handles the sample rate value changes.
        /// </summary>
        /// <param name="newSampleRate">A new sample rate value.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            dcBlocker.SampleRate = newSampleRate;
            lowPass.SampleRate = newSampleRate;
            sampleRateReductor.SampleRate = newSampleRate;
        }
    }
}
