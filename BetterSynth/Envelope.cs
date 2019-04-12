using System;

namespace BetterSynth
{
    class Envelope : ManagerOfManagers
    {
        private Plugin plugin;
        private AdsrEnvelope envelope;
        private float sampleRate;
        private float attackTime;
        private float decayTime;
        private float sustainLevel;
        private float releaseTime;
        private float attackCurve;
        private float attackTargetRatio;
        private float decayReleaseCurve;
        private float decayReleaseTargetRatio;
        private float amplitude;

        public Envelope(Plugin plugin)
        {
            this.plugin = plugin;
            envelope = new AdsrEnvelope();
        }

        public bool IsActive => envelope.State != AdsrEnvelopeState.Idle;

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    AttackTime = attackTime;
                    DecayTime = decayTime;
                    ReleaseTime = releaseTime;
                }
            }
        }

        public float AttackTime
        {
            get => attackTime;
            set
            {
                attackTime = value;
                envelope.SetAttackRate(attackTime * sampleRate);
            }
        }

        public float DecayTime
        {
            get => decayTime;
            set
            {
                decayTime = value;
                envelope.SetDecayRate(decayTime * sampleRate);
            }
        }

        public float SustainLevel
        {
            get => sustainLevel;
            set
            {
                sustainLevel = value;
                envelope.SetSustainLevel(sustainLevel);
            }
        }

        public float ReleaseTime
        {
            get => releaseTime;
            set
            {
                releaseTime = value;
                envelope.SetReleaseRate(releaseTime * sampleRate);
            }
        }

        public float AttackCurve
        {
            get => attackCurve;
            set
            {
                attackCurve = value;
                attackTargetRatio = 0.001f * ((float)Math.Exp(12 * (0.05f + 0.95f * attackCurve)) - 1);
                envelope.SetAttackTargetRatio(attackTargetRatio);
            }
        }

        public float DecayReleaseCurve
        {
            get => decayReleaseCurve;
            set
            {
                decayReleaseCurve = value;
                decayReleaseTargetRatio = CalculateTargetRatio(attackCurve);
                envelope.SetDecayReleaseTargetRatio(decayReleaseTargetRatio);
            }
        }

        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        private float CalculateTargetRatio(float curve) =>
            0.001f * ((float)Math.Exp(12 * (0.0005f + 0.9995f * curve)) - 1);

        public void TriggerAttack() => envelope.TriggerAttack();

        public void TriggerRelease() => envelope.TriggerRelease();

        public float Process() => (float)envelope.Process() * amplitude;
    }
}
