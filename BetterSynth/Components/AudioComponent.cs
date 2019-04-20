namespace BetterSynth
{
    abstract class AudioComponent
    {
        private float sampleRate;

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    OnSampleRateChanged(sampleRate);
                }
            }
        }

        protected virtual void OnSampleRateChanged(float newSampleRate)
        {
        }
    }
}
