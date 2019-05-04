using Jacobi.Vst.Framework;
using System;

namespace BetterSynth
{
    /// <summary>
    /// Represents a delay effect component of the plugin.
    /// </summary>
    class DelayManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Specifies a delay stereo mode.
        /// </summary>
        public enum StereoMode
        {
            None,
            StereoOffset,
            VariousTime,
            PingPong,
        }

        private const float MaxLfoDepth = 0.05f;
        private const float MaxTime = 1f;

        private float delay;
        private float dryCoeff = 1;
        private float feedback;
        private float lfoDepth;
        private float maxDelay;
        private StereoMode mode;
        private float stereoAmount;
        private float wetCoeff;
        private int wetSign = 1;
        private IDelay currentDelay;
        private PingPongDelay pingPongDelay;
        private VariousTimeDelay variousTimeDelay;
        private StereoOffsetDelay stereoOffsetDelay;
        private SineLFO lfo;
        private ParameterFilter timeFilter;
        private ParameterFilter stereoAmountFilter;
        private ParameterFilter mixFilter;

        /// <summary>
        /// Manager of the delay stereo mode parameter.
        /// </summary>
        public VstParameterManager ModeManager { get; set; }

        /// <summary>
        /// Manager of the delay time parameter.
        /// </summary>
        public VstParameterManager TimeManager { get; set; }
        
        /// <summary>
        /// Manager of the delay feedback parameter.
        /// </summary>
        public VstParameterManager FeedbackManager { get; set; }

        /// <summary>
        /// Manager of the delay stereo amount parameter.
        /// </summary>
        public VstParameterManager StereoAmountManager { get; set; }

        /// <summary>
        /// Manager of the delay mix parameter.
        /// </summary>
        public VstParameterManager MixManager { get; set; }

        /// <summary>
        /// Manager of the delay invert parameter.
        /// </summary>
        public VstParameterManager InvertManager { get; set; }

        /// <summary>
        /// Manager of the delay lfo rate parameter.
        /// </summary>
        public VstParameterManager LfoRateManager { get; set; }

        /// <summary>
        /// Manager of the delay lfo depth parameter.
        /// </summary>
        public VstParameterManager LfoDepthManager { get; set; }

        /// <summary>
        /// Initialized a new DelayManager class instance that belongs to given plugin
        /// and has specified parameter name prefix.
        /// </summary>
        /// <param name="plugin">A plugin instance to which a new delay belongs.</param>
        /// <param name="parameterPrefix">A prefix for parameter's names.</param>
        public DelayManager(Plugin plugin, string parameterPrefix = "DL")
            : base(plugin, parameterPrefix)
        {
            stereoOffsetDelay = new StereoOffsetDelay();
            variousTimeDelay = new VariousTimeDelay();
            pingPongDelay = new PingPongDelay();
            lfo = new SineLFO();

            InitializeParameters();
        }
        
        /// <summary>
        /// Initializes parameter managers of the DelayManager class instance.
        /// </summary>
        /// <param name="factory">A parameter factory used for parameters initialization.</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Delay mode parameter.
            ModeManager = factory.CreateParameterManager(
                name: "MODE",
                valueChangedHandler: SetMode);

            // Delay time parameter.
            TimeManager = factory.CreateParameterManager(
                name: "TIME",
                defaultValue: 0.8f,
                valueChangedHandler: SetTimeTarget);
            timeFilter = new ParameterFilter(UpdateTime, 0, 100);
            TimeManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ActiveParameter")
                    currentDelay?.Reset();
            };

            // Delay feedback parameter.
            FeedbackManager = factory.CreateParameterManager(
                name: "FB",
                defaultValue: 0.5f,
                valueChangedHandler: SetFeedback);

            // Delay stereo amount parameter.
            StereoAmountManager = factory.CreateParameterManager(
                name: "STER",
                defaultValue: 0.5f,
                valueChangedHandler: SetStereoAmountTarget);
            stereoAmountFilter = new ParameterFilter(UpdateStereoAmount, 0, 100);

            // Delay mix parameter.
            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0.5f,
                valueChangedHandler: SetMixTarget);
            mixFilter = new ParameterFilter(UpdateMix, 0);

            // Delay invert parameter.
            InvertManager = factory.CreateParameterManager(
                name: "INV",
                valueChangedHandler: SetInvert);

            // Delay lfo rate parameter.
            LfoRateManager = factory.CreateParameterManager(
                name: "RATE",
                valueChangedHandler: SetLfoRate);

            // Delay lfo depth parameter.
            LfoDepthManager = factory.CreateParameterManager(
                name: "DEPTH",
                valueChangedHandler: SetLfoDepth);
        }

        /// <summary>
        /// Handles the delay mode parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetMode(float value)
        {
            var newMode = Converters.ToDelayMode(value);
            if (newMode != mode)
            {
                mode = newMode;
                switch (mode)
                {
                    case StereoMode.StereoOffset:
                        ChangeDelay(stereoOffsetDelay);
                        break;
                    case StereoMode.VariousTime:
                        ChangeDelay(variousTimeDelay);
                        break;
                    case StereoMode.PingPong:
                        ChangeDelay(pingPongDelay);
                        break;
                }
            }
        }

        /// <summary>
        /// Changes a current delay.
        /// </summary>
        /// <param name="newDelay">A new current delay.</param>
        private void ChangeDelay(IDelay newDelay)
        {
            currentDelay = newDelay;
            currentDelay?.Reset();
            currentDelay?.SetFeedback(feedback);
            currentDelay?.SetStereo(stereoAmount);
        }

        /// <summary>
        /// Handles the delay time parameter changes.
        /// Updates a target value of the delay time smoothing filter.
        /// </summary>
        /// <param name="target">A new target value of the parameter.</param>
        private void SetTimeTarget(float target)
        {
            timeFilter.SetTarget((float)Converters.ToDelayTime(target));
        }

        /// <summary>
        /// Handles the delay time parameter filter changes (called every processing turn).
        /// </summary>
        /// <param name="value">A new value of the delay time.</param>
        private void UpdateTime(float value)
        {
            delay = value * SampleRate;
        }

        /// <summary>
        /// Handles the delay feedback parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetFeedback(float value)
        {
            feedback = value;
            currentDelay?.SetFeedback(feedback);
        }

        /// <summary>
        /// Handles the stereo amount parameter changes.
        /// Updates a target value of the stereo amount smoothing filter.
        /// </summary>
        /// <param name="target">A new target value of the parameter.</param>
        private void SetStereoAmountTarget(float target)
        {
            stereoAmountFilter.SetTarget((float)Converters.ToStereoAmount(target));
        }

        /// <summary>
        /// Handles the delay time parameter filter changes (called every processing turn).
        /// </summary>
        /// <param name="value">A new value of the stereo amount.</param>
        private void UpdateStereoAmount(float value)
        {
            stereoAmount = value;
            currentDelay?.SetStereo(stereoAmount);
        }

        /// <summary>
        /// Handles the dry-wet mix parameter changes.
        /// Updates a target value of the dry-wet mix smoothing filter.
        /// </summary>
        /// <param name="target">A new target value of the parameter.</param>
        private void SetMixTarget(float target)
        {
            mixFilter.SetTarget(target);
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
        /// Handles the wet inversion parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetInvert(float value)
        {
            if (value < 0.5)
                wetSign = 1;
            else
                wetSign = -1;
        }

        /// <summary>
        /// Handles the lfo rate parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetLfoRate(float value)
        {
            lfo.SetFrequency((float)Converters.ToDelayLfoRate(value));
        }

        /// <summary>
        /// Handles the lfo depth parameter changes.
        /// </summary>
        /// <param name="value">A new value of the parameter.</param>
        private void SetLfoDepth(float value)
        {
            lfoDepth = value;
        }

        /// <summary>
        /// Perform a processing turn.
        /// </summary>
        /// <param name="inputL">Left channel input.</param>
        /// <param name="inputR">Right channel input.</param>
        /// <param name="outputL">Left channel output.</param>
        /// <param name="outputR">Right channel output.</param>
        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            // Parameter filter's updates.
            timeFilter.Process();
            mixFilter.Process();
            stereoAmountFilter.Process();

            if (mode == StereoMode.None)
            {
                outputL = inputL;
                outputR = inputR;
            }
            else
            {
                var lfoCoeff = 1 + MaxLfoDepth * lfoDepth * lfo.Process();
                currentDelay.SetDelay(Math.Min(maxDelay, delay * lfoCoeff));
                currentDelay.Process(inputL, inputR, out var wetL, out var wetR);
                outputL = dryCoeff * inputL + wetSign * wetCoeff * wetL;
                outputR = dryCoeff * inputL + wetSign * wetCoeff * wetR;
            }
        }
        
        /// <summary>
        /// Handles the sample rate value changes.
        /// </summary>
        /// <param name="newSampleRate">A new sample rate value.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            // Pass a new sample rate to inner components.
            timeFilter.SampleRate = newSampleRate;
            stereoAmountFilter.SampleRate = newSampleRate;
            mixFilter.SampleRate = newSampleRate;
            stereoOffsetDelay.SampleRate = newSampleRate;
            variousTimeDelay.SampleRate = newSampleRate;
            pingPongDelay.SampleRate = newSampleRate;
            lfo.SampleRate = newSampleRate;

            maxDelay = MaxTime * newSampleRate;
        }
    }
}
