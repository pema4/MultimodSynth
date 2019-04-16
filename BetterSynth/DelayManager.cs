﻿using Jacobi.Vst.Framework;
using System.Windows.Forms;

namespace BetterSynth
{
    class DelayManager : AudioComponentWithParameters
    {
        public enum StereoMode
        {
            None,
            StereoOffset,
            VariousTime,
            PingPong,
        }

        private float dryCoeff = 1;
        private float wetCoeff;
        private float time;
        private StereoMode mode;
        private PingPongDelay pingPongDelay;
        private VariousTimeDelay variousTimeDelay;
        private StereoOffsetDelay stereoOffsetDelay;
        private IDelay currentDelay;
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
        }

        private void SetMode(float value)
        {
            if (value < 1)
            {
                if (mode != StereoMode.None)
                {
                    mode = StereoMode.None;
                    currentDelay?.Reset();
                }
            }
            else if (value < 2)
            {
                if (mode != StereoMode.StereoOffset)
                {
                    mode = StereoMode.StereoOffset;
                    currentDelay?.Reset();
                    currentDelay = stereoOffsetDelay;
                }
            }
            else if (value < 3)
            {
                if (mode != StereoMode.VariousTime)
                {
                    mode = StereoMode.VariousTime;
                    currentDelay?.Reset();
                    currentDelay = variousTimeDelay;
                }
            }
            else
            {
                if (mode != StereoMode.PingPong)
                {
                    mode = StereoMode.PingPong;
                    currentDelay?.Reset();
                    currentDelay = pingPongDelay;
                }
            }
        }

        private void SetTimeTarget(float value)
        {
            time = value / 1000;
            timeFilter.SetTarget(time);
        }

        private void UpdateTime(float value)
        {
            var delay = SampleRate * value;
            pingPongDelay.SetDelay(delay);
            stereoOffsetDelay.SetDelay(delay);
            variousTimeDelay.SetDelay(delay);
        }

        private void SetFeedback(float value)
        {
            pingPongDelay.SetFeedback(value);
            stereoOffsetDelay.SetFeedback(value);
            variousTimeDelay.SetFeedback(value);
        }

        private void SetStereoAmountTarget(float value)
        {
            stereoAmountFilter.SetTarget(value);
        }

        private void UpdateStereoAmount(float value)
        {
            pingPongDelay.SetStereo(value);
            stereoOffsetDelay.SetStereo(value);
            variousTimeDelay.SetStereo(value);
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

        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            timeFilter.Process();
            mixFilter.Process();
            stereoAmountFilter.Process();

            if (mode != StereoMode.None)
            {
                currentDelay.Process(inputL, inputR, out var wetL, out var wetR);
                outputL = dryCoeff * inputL + wetCoeff * wetL;
                outputR = dryCoeff * inputL + wetCoeff * wetR;
            }
            else
            {
                outputL = inputL;
                outputR = inputR;
            }
        }
        
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            stereoOffsetDelay.SampleRate = newSampleRate;
            stereoOffsetDelay.SetDelay(time * newSampleRate);

            variousTimeDelay.SampleRate = newSampleRate;
            variousTimeDelay.SetDelay(time * newSampleRate);

            pingPongDelay.SampleRate = newSampleRate;
            pingPongDelay.SetDelay(time * newSampleRate);

            timeFilter.SampleRate = newSampleRate;
        }
    }
}
