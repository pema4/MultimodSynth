using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    class StereoOffsetDelay : AudioComponent, IDelay
    {
        private MonoDelay delayL;
        private MonoDelay delayR;
        private MonoDelay offsetBufferL;
        private MonoDelay offsetBufferR;

        public StereoOffsetDelay()
        {
            delayL = new MonoDelay();
            delayR = new MonoDelay();
            offsetBufferL = new MonoDelay();
            offsetBufferR = new MonoDelay();
        }

        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            inputL = offsetBufferL.Process(inputL);
            inputR = offsetBufferR.Process(inputR);
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
            delayL.SetDelay(value);
            delayR.SetDelay(value);
        }

        public void SetFeedback(float value)
        {
            delayL.SetFeedback(value);
            delayR.SetFeedback(value);
        }

        public void SetStereo(float value)
        {
            value *= SampleRate;
            var leftDelay = -Math.Min(value, 0);
            var rightDelay = value + leftDelay;
            offsetBufferL.SetDelay(leftDelay);
            offsetBufferR.SetDelay(rightDelay);
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            delayL.SampleRate = newSampleRate;
            delayR.SampleRate = newSampleRate;
            offsetBufferL.SampleRate = newSampleRate;
            offsetBufferR.SampleRate = newSampleRate;
        }
    }
}
