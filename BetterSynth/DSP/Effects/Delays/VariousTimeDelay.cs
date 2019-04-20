using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    class VariousTimeDelay : AudioComponent, IDelay
    {
        private MonoDelay delayL;
        private MonoDelay delayR;
        private float delay;
        private float leftDelayCoeff;
        private float rightDelayCoeff;

        public VariousTimeDelay()
        {
            delayL = new MonoDelay();
            delayR = new MonoDelay();
        }

        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            outputL = delayL.Process(inputL);
            outputR = delayR.Process(inputR);
        }

        public void Reset()
        {
            delayL.Reset();
            delayR.Reset();
        }

        public void SetDelay(float value)
        {
            delay = value;
            delayL.SetDelay(delay * leftDelayCoeff);
            delayR.SetDelay(delay * rightDelayCoeff);
        }

        public void SetFeedback(float value)
        {
            delayL.SetFeedback(value);
            delayR.SetFeedback(value);
        }

        public void SetStereo(float value)
        {
            leftDelayCoeff = 1 + Math.Min(value, 0) * 0.99f;
            rightDelayCoeff = 1 - Math.Max(value, 0) * 0.99f;
            delayL.SetDelay(delay * leftDelayCoeff);
            delayR.SetDelay(delay * rightDelayCoeff);
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            delayL.SampleRate = newSampleRate;
            delayR.SampleRate = newSampleRate;
        }
    }
}
