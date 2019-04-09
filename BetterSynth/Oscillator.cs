using System;
using WavesData;

namespace BetterSynth
{
    class Oscillator
    {
        private Plugin plugin;
        private float noteFrequency;
        private float overallFrequency;
        private float phaseIncrement;
        public float sampleRate;
        private float pitchMultiplier;
        private float phaseOffset;

        public Oscillator(Plugin plugin)
        {
            this.plugin = plugin;

            PitchMultiplier = 1;

            plugin.Opened += (sender, e) =>
            {
                SampleRate = plugin.AudioProcessor.SampleRate;
            };
        }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                phaseIncrement = overallFrequency / sampleRate;
            }
        }

        public WaveTableLookup CurrentWave { get; set; }

        public float NoteFrequency
        {
            get => noteFrequency;
            set
            {
                noteFrequency = value;
                overallFrequency = noteFrequency * pitchMultiplier;
                phaseIncrement = overallFrequency / sampleRate;
            }
        }

        public float PitchMultiplier
        {
            get => pitchMultiplier;
            set
            {
                pitchMultiplier = value;
                overallFrequency = noteFrequency * pitchMultiplier;
                phaseIncrement = overallFrequency / sampleRate;
            }
        }

        public float Process(float phaseModulation = 0)
        {
            float res = CurrentWave[phaseOffset];
            phaseOffset = (phaseOffset + phaseIncrement + phaseModulation);
            if (phaseOffset >= 2)
                phaseOffset -= 2;
            else if (phaseOffset >= 1)
                phaseOffset -= 1;
            else if (phaseOffset < -1)
                phaseOffset += 2;
            else if (phaseOffset < 0)
                phaseOffset += 1;

            if (phaseOffset == 1)
                throw new ArgumentException();

            return res;
        }

        public void ResetPhase() => phaseOffset = 0;
    }
}
