using System;

namespace BetterSynth
{
    class Envelope : AudioComponent
    {
        private AdsrEnvelope envelope;
        private float attackTime;
        private float decayTime;
        private float sustainLevel;
        private float releaseTime;
        private float attackCurve;
        private double attackTargetRatio;
        private float decayReleaseCurve;
        private double decayReleaseTargetRatio;
        private float amplitude;

        public Envelope()
        {
            envelope = new AdsrEnvelope();
        }

        public bool IsActive => envelope.State != AdsrEnvelope.EnvelopeState.Idle;

        public void SetAttackTime(float value)
        {
            attackTime = value;
            envelope.SetAttackRate(attackTime * SampleRate);
        }

        public void SetDecayTime(float value)
        {
            decayTime = value;
            envelope.SetDecayRate(decayTime * SampleRate);
        }

        public void SetSustainLevel(float value)
        {
            sustainLevel = value;
            envelope.SetSustainLevel(sustainLevel);
        }

        public void SetReleaseTime(float value)
        {
            releaseTime = value;
            envelope.SetReleaseRate(releaseTime * SampleRate);
        }

        public void SetAttackCurve(float value)
        {
            attackCurve = value;
            attackTargetRatio = 0.001 * (Math.Exp(12 * (0.05f + 0.95f * attackCurve)) - 1);
            envelope.SetAttackTargetRatio(attackTargetRatio);
        }

        public void SetDecayReleaseCurve(float value)
        {
            decayReleaseCurve = value;
            decayReleaseTargetRatio = 
                0.001 * (Math.Exp(12 * (0.0005 + 0.9995 * decayReleaseCurve)) - 1);
            envelope.SetDecayReleaseTargetRatio(decayReleaseTargetRatio);
        }

        public void SetAmplitude(float value)
        {
            amplitude = value;
        }

        public void TriggerAttack() => envelope.TriggerAttack();

        public void TriggerRelease() => envelope.TriggerRelease();

        public float Process()
        {
            return (float)envelope.Process() * amplitude;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            SetAttackTime(attackTime);
            SetDecayTime(decayTime);
            SetReleaseTime(releaseTime);
        }
    }
}
