namespace WavesData
{
    public class WaveTableLookup
    {
        private WaveLookup aWave;
        private WaveLookup bWave;
        private float aCoefficient;
        private float bCoefficient;

        public WaveTableLookup(
            WaveLookup aWave,
            WaveLookup bWave,
            float aCoefficient,
            float bCoefficient)
        {
            this.aWave = aWave;
            this.bWave = bWave;
            this.aCoefficient = aCoefficient;
            this.bCoefficient = bCoefficient;
        }

        public float this[float idx] =>
            aWave[idx] * aCoefficient + bWave[idx] * bCoefficient;
    }
}
