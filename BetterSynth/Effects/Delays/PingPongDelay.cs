namespace BetterSynth
{
    class PingPongDelay : AudioComponent, IDelay
    {
        private const float MaxTime = 1;

        private MonoDelay delayL;
        private MonoDelay delayR;
        private float feedback;
        private float leftCoeff;
        private float rightCoeff;

        public PingPongDelay()
        {
            delayL = new MonoDelay();
            delayR = new MonoDelay();
        }

        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            outputL = delayL.Peek();
            outputR = delayR.Peek();
            delayL.Process(inputL * leftCoeff + outputR * feedback);
            delayR.Process(inputR * rightCoeff + outputL * feedback);
        }

        public void Reset()
        {
            delayL.Reset();
            delayR.Reset();
        }

        public void SetDelay(float delay)
        {
            delayL.SetDelay(delay);
            delayR.SetDelay(delay);
        }

        public void SetFeedback(float value)
        {
            feedback = value;
        }

        public void SetStereo(float value)
        {
            if (value < 0)
            {
                leftCoeff = 1;
                rightCoeff = 1 + value;
            }
            else
            {
                rightCoeff = 1;
                leftCoeff = 1 - value;
            }
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            delayL.SampleRate = newSampleRate;
            delayR.SampleRate = newSampleRate;
        }
    }
}
