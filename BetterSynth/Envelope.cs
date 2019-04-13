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
        private double attackTargetRatio;
        private float decayReleaseCurve;
        private double decayReleaseTargetRatio;
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
                attackTargetRatio = 0.001 * (Math.Exp(12 * (0.05f + 0.95f * attackCurve)) - 1);
                envelope.SetAttackTargetRatio(attackTargetRatio);
            }
        }

        public float DecayReleaseCurve
        {
            get => decayReleaseCurve;
            set
            {
                decayReleaseCurve = value;
                decayReleaseTargetRatio = 
                    0.001 * (Math.Exp(12 * (0.0005 + 0.9995 * decayReleaseCurve)) - 1);
                envelope.SetDecayReleaseTargetRatio(decayReleaseTargetRatio);
            }
        }

        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        public void TriggerAttack() => envelope.TriggerAttack();

        public void TriggerRelease() => envelope.TriggerRelease();

        public float Process()
        {
            return (float)envelope.Process() * amplitude;
        }
    }
}
