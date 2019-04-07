namespace WavesData
{
    public class WaveTableLookup : IWaveLookup
    {
        private WaveTable waveTable;
        private int aIndex;
        private int bIndex;
        private float aCoefficient;
        private float bCoefficient;

        public WaveTableLookup(
            WaveTable waveTable,
            int aIndex,
            int bIndex,
            float aCoefficient,
            float bCoefficient)
        {
            this.waveTable = waveTable;
            this.aIndex = aIndex;
            this.bIndex = bIndex;
            this.aCoefficient = aCoefficient;
            this.bCoefficient = bCoefficient;
        }

        public float this[float idx] =>
            waveTable.Waves[aIndex][idx] * aCoefficient + waveTable.Waves[bIndex][idx] * bCoefficient;
    }
}
