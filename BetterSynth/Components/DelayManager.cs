using Jacobi.Vst.Framework;
using System;
using System.Windows.Forms;

namespace BetterSynth
{
    class DelayManager : AudioComponentWithParameters
    {
        private const float MaxLfoDepth = 0.05f;
        private const float MaxTime = 1f;

        public enum StereoMode
        {
            None,
            StereoOffset,
            VariousTime,
            PingPong,
        }

        private float dryCoeff = 1;
        private float wetCoeff;
        private int wetSign = 1;
        private float delay;
        private float maxDelay;
        private float feedback;
        private float lfoDepth;
        private float stereoAmount;
        private StereoMode mode;
        private PingPongDelay pingPongDelay;
        private VariousTimeDelay variousTimeDelay;
        private StereoOffsetDelay stereoOffsetDelay;
        private IDelay currentDelay;
        private SineLFO lfo;
        private ParameterFilter timeFilter;
        private ParameterFilter stereoAmountFilter;
        private ParameterFilter mixFilter;

        public VstParameterManager ModeManager { get; set; }

        public VstParameterManager TimeManager { get; set; }
        
        public VstParameterManager FeedbackManager { get; set; }

        public VstParameterManager StereoAmountManager { get; set; }

        public VstParameterManager MixManager { get; set; }

        public VstParameterManager InvertManager { get; set; }

        public VstParameterManager LfoRateManager { get; set; }

        public VstParameterManager LfoDepthManager { get; set; }

        public DelayManager(
            Plugin plugin,
            string parameterPrefix = "DL",
            string parameterCategory = "effects") 
            : base(plugin, parameterPrefix, parameterCategory)
        {
            stereoOffsetDelay = new StereoOffsetDelay();
            variousTimeDelay = new VariousTimeDelay();
            pingPongDelay = new PingPongDelay();
            lfo = new SineLFO();
            InitializeParameters();
        }
        
        protected override void InitializeParameters(ParameterFactory factory)
        {
            ModeManager = factory.CreateParameterManager(
                name: "MODE",
                minValue: 0,
                maxValue: 3,
                valueChangedHandler: SetMode);
            CreateRedirection(ModeManager, nameof(ModeManager));

            TimeManager = factory.CreateParameterManager(
                name: "TIME",
                minValue: 0,
                maxValue: 1000,
                defaultValue: 125,
                valueChangedHandler: SetTimeTarget);
            CreateRedirection(TimeManager, nameof(TimeManager));
            timeFilter = new ParameterFilter(UpdateTime, 0, 100);

            FeedbackManager = factory.CreateParameterManager(
                name: "FB",
                defaultValue: 0.5f,
                valueChangedHandler: SetFeedback);
            CreateRedirection(FeedbackManager, nameof(FeedbackManager));

            StereoAmountManager = factory.CreateParameterManager(
                name: "STER",
                minValue: -1,
                maxValue: 1,
                defaultValue: 0,
                valueChangedHandler: SetStereoAmountTarget);
            CreateRedirection(StereoAmountManager, nameof(StereoAmountManager));
            stereoAmountFilter = new ParameterFilter(UpdateStereoAmount, 0, 100);

            MixManager = factory.CreateParameterManager(
                name: "MIX",
                valueChangedHandler: SetMixTarget);
            CreateRedirection(MixManager, nameof(MixManager));
            mixFilter = new ParameterFilter(UpdateMix, 0);

            InvertManager = factory.CreateParameterManager(
                name: "INV",
                valueChangedHandler: SetInvert);
            CreateRedirection(InvertManager, nameof(InvertManager));

            LfoRateManager = factory.CreateParameterManager(
                name: "RATE",
                minValue: 0,
                maxValue: 5,
                defaultValue: 0,
                valueChangedHandler: SetLfoRate);
            CreateRedirection(LfoRateManager, nameof(LfoRateManager));

            LfoDepthManager = factory.CreateParameterManager(
                name: "DEPTH",
                valueChangedHandler: SetLfoDepth);
            CreateRedirection(LfoDepthManager, nameof(LfoDepthManager));
        }

        private void SetMode(float value)
        {
            if (value < 1)
            {
                if (mode != StereoMode.None)
                    mode = StereoMode.None;
            }
            else if (value < 2)
            {
                if (mode != StereoMode.StereoOffset)
                {
                    mode = StereoMode.StereoOffset;
                    ChangeDelay(stereoOffsetDelay);
                }
            }
            else if (value < 3)
            {
                if (mode != StereoMode.VariousTime)
                {
                    mode = StereoMode.VariousTime;
                    ChangeDelay(variousTimeDelay);
                }
            }
            else
            {
                if (mode != StereoMode.PingPong)
                {
                    mode = StereoMode.PingPong;
                    ChangeDelay(pingPongDelay);
                }
            }
        }

        private void ChangeDelay(IDelay newDelay)
        {
            currentDelay = newDelay;
            currentDelay.Reset();
            currentDelay.SetFeedback(feedback);
            currentDelay.SetStereo(stereoAmount);
        }

        private void SetTimeTarget(float value)
        {
            var target =  value / 1000;
            timeFilter.SetTarget(target);
        }

        private void UpdateTime(float value)
        {
            delay = value * SampleRate;
            // delay time is updated in a process loop.
        }

        private void SetFeedback(float value)
        {
            feedback = value;
            currentDelay?.SetFeedback(feedback);
        }

        private void SetStereoAmountTarget(float value)
        {
            stereoAmountFilter.SetTarget(value);
        }

        private void UpdateStereoAmount(float value)
        {
            stereoAmount = value;
            currentDelay?.SetStereo(stereoAmount);
        }

        private void SetMixTarget(float value)
        {
            mixFilter.SetTarget(value);
        }
            
        private void UpdateMix(float value)
        {
            wetCoeff = value;
            dryCoeff = 1 - value;
        }

        private void SetInvert(float value)
        {
            if (value < 1)
                wetSign = 1;
            else
                wetSign = -1;
        }

        private void SetLfoRate(float value)
        {
            lfo.SetFrequency(value);
        }

        private void SetLfoDepth(float value)
        {
            lfoDepth = value;
        }

        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
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
                currentDelay.SetDelay(Math.Min(maxDelay, delay * (1 + MaxLfoDepth * lfoDepth * lfo.Process())));
                currentDelay.Process(inputL, inputR, out var wetL, out var wetR);
                outputL = dryCoeff * inputL + wetSign * wetCoeff * wetL;
                outputR = dryCoeff * inputL + wetSign * wetCoeff * wetR;
            }
        }
        
        protected override void OnSampleRateChanged(float newSampleRate)
        {
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
