using System;
using System.Windows.Forms;

namespace BetterSynth
{
    /// <summary>
    /// https://ccrma.stanford.edu/~jos/pasp/Variable_Delay_Lines.html
    /// </summary>
    class MonoDelay : AudioComponent
    {
        private const float MaxTime = 1f;
        private float[] buffer;
        private int bufferLength;
        private int writePoint;
        private int readPoint;
        private float readOffset;
        private float feedback;
        private float delay;

        public void SetDelay(float delay)
        {
            this.delay = delay;
            double delayedPoint = (double)writePoint - delay;
            if (delayedPoint < 0)
                delayedPoint += bufferLength;

            readPoint = (int)delayedPoint;
            if (readPoint == bufferLength)
                readPoint -= 1;
            readOffset = (float)(delayedPoint - readPoint);
        }

        public void SetFeedback(float value)
        {
            feedback = value;
        }

        public float CalculateOutput()
        {
            float res = buffer[readPoint] * (1 - readOffset);

            var nextPoint = readPoint + 1;
            if (nextPoint < bufferLength)
                res += buffer[nextPoint] * readOffset;
            else
                res += buffer[0] * readOffset;

            return res;
        }

        public float Process(float input)
        {
            if (delay == 0)
            {
                buffer[writePoint] = input;
                writePoint += 1;
                if (writePoint == bufferLength)
                    writePoint = 0;
                readPoint = writePoint;
                return input;
            }

            float output = CalculateOutput();
            readPoint += 1;
            if (readPoint == bufferLength)
                readPoint = 0;

            buffer[writePoint] = input + output * feedback;
            writePoint += 1;
            if (writePoint == bufferLength)
                writePoint = 0;
            
            return output;
        }

        public float Peek() => CalculateOutput();

        public void Reset()
        {
            buffer.Initialize();
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            bufferLength = (int)(newSampleRate * MaxTime);
            buffer = new float[bufferLength];
        }
    }
}
