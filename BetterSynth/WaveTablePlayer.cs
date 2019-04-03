using System;
using WavesData;

namespace BetterSynth
{
    internal class WaveTablePlayer
    {
        private Plugin plugin;
        private IWaveLookup waveTableLookup;
        private float phaseOffset;
        private float waveTablePos;
        private float frequency;
        private float phaseIncrement;
        private bool bypass;
        private WaveTable waveTable;

        public WaveTablePlayer(Plugin plugin)
        {
            this.plugin = plugin;
            WaveTable = new WaveTable((exp, x) => Math.Sign(Math.Sin(x)) * Math.Pow(Math.Abs(Math.Sin(x)), 1 / exp), 0.001, 1,
                mode: InterpolationMode.Normal);
        }
        
        public WaveTable WaveTable
        {
            get => waveTable;
            set
            {
                if (waveTable != value)
                {
                    waveTable = value;
                    waveTableLookup = WaveTable[WaveTablePos];
                }
            }
        }

        public bool Bypass
        {
            get => bypass;
            set
            {
                if (value != bypass)
                {
                    bypass = value;
                    if (value)
                        phaseOffset = 0;
                }
            }
        }

        public float Frequency
        {
            get => frequency;
            set
            {
                if (value != frequency)
                {
                    frequency = value;
                    phaseIncrement = value / SampleRate;
                }
            }
        }

        public float WaveTablePos
        {
            get => waveTablePos;
            set
            {
                if (value != waveTablePos)
                {
                    waveTablePos = value;
                    waveTableLookup = WaveTable[value];
                }
            }
        }

        protected float SampleRate => plugin.AudioProcessor.SampleRate;

        internal void Process(out float output)
        {
            if (!Bypass)
            {
                output = waveTableLookup[phaseOffset];
                phaseOffset = (phaseOffset + phaseIncrement) % 1;
            }
            else
                output = 0;
        }
    }
}