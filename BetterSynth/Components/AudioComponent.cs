namespace BetterSynth
{
    /// <summary>
    /// Base class that provides sample rate property and sample rate changed handler.
    /// </summary>
    abstract class AudioComponent
    {
        private float sampleRate;

        /// <summary>
        /// Sample rate at which component should process its inputs and outputs.
        /// </summary>
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

        /// <summary>
        /// Handles changes of the SampleRate property.
        /// Used mainly for updating other properties sample rate value.
        /// </summary>
        /// <param name="newSampleRate">New sample rate</param>
        protected virtual void OnSampleRateChanged(float newSampleRate)
        {
        }
    }
}
