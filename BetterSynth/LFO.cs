using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    enum ModulationDestination
    {
        PitchA,
        AmpA,
        WaveTablePositionA,
        PitchB,
        AmpB,
        WaveTablePositionB,
        FilterCutoff,
        FilterCurve,
        FilterAmp,
    }

    class LFO
    {
        public enum WaveType
        {
            Sine,
            Saw,
            Square,
            Triangle,
        }

        private Plugin plugin;
        private float frequency;
        private WaveType lfoType;
        private float sineFilterCoeff;
        private float sampleRate;
        private float phaseIncrement;
        private float phaseOffset = 0;
        private float sin;
        private float cos;

        public LFO(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        public float Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                UpdateCoefficients();
            }
        }

        public WaveType LfoType
        {
            get => lfoType;
            set
            {
                lfoType = value;
                ResetPhase();
                UpdateCoefficients();
            }
        }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                phaseIncrement = 1 / sampleRate;
                UpdateCoefficients();
            }
        }

        private void UpdateCoefficients()
        {
            sineFilterCoeff = (float)(2 * Math.PI * frequency / sampleRate);
        }

        public float Process()
        {
            float value = 0;
            switch (lfoType)
            {
                case WaveType.Saw:
                    value = 2 * phaseOffset - 1;
                    break;

                case WaveType.Sine:
                    sin = sin + cos * sineFilterCoeff;
                    cos = cos - sin * sineFilterCoeff;
                    value = sin;
                    break;

                case WaveType.Square:
                    value = phaseOffset < 0.5f ? -1 : 1;
                    break;

                case WaveType.Triangle:
                    if (phaseOffset < 0.25f)
                        value = 4 * phaseOffset;
                    else if (phaseOffset < 0.75f)
                        value = 2 - 4 * phaseOffset;
                    else
                        value = -4 + 4 * phaseOffset;
                    break;
            }

            phaseOffset += phaseIncrement;
            if (phaseOffset >= 1)
                phaseOffset -= 1;

            return value;
        }

        public void ResetPhase()
        {
            phaseOffset = 0;
            sin = 0;
            cos = 1;
        }
    }
}
