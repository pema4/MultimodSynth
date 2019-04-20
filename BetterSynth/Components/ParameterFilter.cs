using System;

namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Filters/257-1-pole-lpf-for-smooth-parameter-changes.html
    /// </summary>
    class ParameterFilter : AudioComponent
    {
        private const double BaseSampleRate = 44100;
        private const double DefaultExp = 0.0015995606308184566;

        private float a, b;
        private float value;
        private float target;
        private float responseTimeCoefficient;
        private bool isActive;
        private Action<float> valueChangedAction;

        public ParameterFilter(
            Action<float> valueChangedAction, 
            float initialValue = 0, 
            float responseTimeCoefficient = 1)
        {
            a = 0.99f;
            b = 1f - a;
            value = initialValue;
            this.valueChangedAction = valueChangedAction;
            this.responseTimeCoefficient = responseTimeCoefficient;
        }

        public void SetTarget(float value)
        {
            target = value;
            isActive = true;
        }

        public void SetResponseTimeCoefficient(float value)
        {
            responseTimeCoefficient = value;
        }

        public void Process()
        {
            if (isActive)
            {
                var newValue = (target * b) + (value * a);
                valueChangedAction(newValue);
                if (value != newValue)
                    value = newValue;
                else
                    isActive = false;
            }
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            var coeff = BaseSampleRate / newSampleRate / responseTimeCoefficient;
            a = (float)Math.Exp(-2 * Math.PI * DefaultExp * coeff);
            b = 1 - a;
        }
    }
}
