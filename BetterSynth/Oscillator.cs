using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private float timePeriod;
        private float pitchMultiplier;
        private float phaseOffset;

        public Oscillator(Plugin plugin)
        {
            this.plugin = plugin;

            PitchMultiplier = 1;

            plugin.Opened += (sender, e) =>
            {
                sampleRate = plugin.AudioProcessor.SampleRate;
                timePeriod = 1 / sampleRate;
            };
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
            float res = CurrentWave[phaseOffset + phaseModulation];
            phaseOffset = (phaseOffset + phaseIncrement + phaseModulation) % 1;
            return res;
        }

        public void ResetPhase() => phaseOffset = 0;
    }
}
