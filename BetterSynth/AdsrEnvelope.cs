using System;

namespace BetterSynth
{
    /// <summary>
    /// Almost translation of http://www.earlevel.com/main/2013/06/01/envelope-generators/
    /// </summary>
    class AdsrEnvelope
    {
        public enum State
        {
            Idle,
            Attack,
            Decay,
            Sustain,
            Release
        }

        private Plugin plugin;

        private float attackBase;
        private float attackCoef;
        private float attackRate;
        private float attackTargetRatio;
        private float currValue;
        private float decayBase;
        private float decayCoef;
        private float decayRate;
        private float decayReleaseTargetRatio;
        private float releaseBase;
        private float releaseCoef;
        private float releaseRate;
        private float sampleRate;

        private float attackTime;
        private float decayTime;
        private float sustainLevel;
        private float releaseTime;
        private float attackCurve;
        private float decayReleaseCurve;

        public AdsrEnvelope(Plugin plugin)
        {
            this.plugin = plugin;
            SetAttackRate(0);
            SetDecayRate(0);
            SetReleaseRate(0);
            SustainLevel = 1;
            SetAttackTargetRatio(0.3f);
            SetDecayReleaseTargetRatio(0.0001f);

            plugin.Opened += (sender, e) =>
            {
                sampleRate = plugin.AudioProcessor.SampleRate;

                AttackTime = attackTime;
                DecayTime = decayTime;
                ReleaseTime = releaseTime;
            };
        }
        
        public float AttackTime
        {
            get => attackTime;
            set
            {
                if (attackTime != value)
                {
                    attackTime = value;
                    SetAttackRate(attackTime * sampleRate);
                }
            }
        }

        public float DecayTime
        {
            get => decayTime;
            set
            {
                decayTime = value;
                SetDecayRate(decayTime * sampleRate);
            }
        }

        public float SustainLevel
        {
            get => sustainLevel;
            set
            {
                sustainLevel = value;
                decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
            }
        }

        public float ReleaseTime
        {
            get => releaseTime;
            set
            {
                releaseTime = value;
                SetReleaseRate(releaseTime * sampleRate);
            }
        }

        public float AttackCurve
        {
            get => attackCurve;
            set
            {
                attackCurve = value;
                attackTargetRatio = 0.001f * ((float)Math.Exp(12 * (0.005f + 0.995f * value)) - 1);
                SetAttackTargetRatio(attackTargetRatio);
            }
        }

        public float DecayReleaseCurve
        {
            get => decayReleaseCurve;
            set
            {
                decayReleaseCurve = value;
                decayReleaseTargetRatio = 0.001f * ((float)Math.Exp(12 * (0.0001f + 0.9999f * value)) - 1);
                SetDecayReleaseTargetRatio(decayReleaseTargetRatio);
            }
        }

        public float Amplitude { get; set; }

        public State CurrentState { get; private set; }

        private void SetAttackRate(float rate)
        {
            attackRate = rate;
            attackCoef = CalcCoef(rate, attackTargetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        private void SetDecayRate(float rate)
        {
            decayRate = rate;
            decayCoef = CalcCoef(decayRate, decayReleaseTargetRatio);
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
        }

        private void SetReleaseRate(float rate)
        {
            releaseRate = rate;
            releaseCoef = CalcCoef(rate, decayReleaseTargetRatio);
            releaseBase = -decayReleaseTargetRatio * (1 - releaseCoef);
        }

        private void SetAttackTargetRatio(float targetRatio)
        {
            if (targetRatio < 0.000000001f)
                attackTargetRatio = 0.000000001f;
            else
                attackTargetRatio = targetRatio;

            attackCoef = CalcCoef(attackRate, targetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        private void SetDecayReleaseTargetRatio(float targetRatio)
        {
            if (targetRatio < 0.000000001f)
                decayReleaseTargetRatio = 0.000000001f;
            else
                decayReleaseTargetRatio = targetRatio;

            decayCoef = CalcCoef(decayRate, targetRatio);
            releaseCoef = CalcCoef(releaseRate, targetRatio);
            decayBase = (SustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
            releaseBase = -decayReleaseTargetRatio * (1 - releaseCoef);
        }

        public float Process()
        {
            switch (CurrentState)
            {
                case State.Idle:
                    break;
                case State.Attack:
                    currValue = attackBase + currValue * attackCoef;
                    if (currValue >= 1f)
                    {
                        currValue = 1f;
                        if (decayCoef != 0)
                            CurrentState = State.Decay;
                        else
                            CurrentState = State.Sustain;
                    }
                    break;
                case State.Decay:
                    currValue = decayBase + currValue * decayCoef;
                    if (currValue <= sustainLevel)
                    {
                        currValue = sustainLevel;
                        CurrentState = State.Sustain;
                    }
                    break;
                case State.Sustain:
                    currValue = sustainLevel;
                    break;
                case State.Release:
                    currValue = releaseBase + currValue * releaseCoef;
                    if (currValue <= 0)
                    {
                        currValue = 0;
                        CurrentState = State.Idle;
                        OnSoundStop();
                    }
                    break;
            }

            return Amplitude * currValue;
        }

        private void OnSoundStop()
        {
            SoundStop?.Invoke(this, new EventArgs());
        }

        public event EventHandler SoundStop;

        private float CalcCoef(float rate, float targetRatio) => 
            (float)((rate <= 0f) ? 0f : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate));

        public void TriggerAttack()
        {
            if (attackCoef != 0)
            {
                currValue = 0;
                CurrentState = State.Attack;
            }
            else if (decayCoef != 0)
            {
                currValue = 1;
                CurrentState = State.Decay;
            }
            else
                CurrentState = State.Sustain;
        }

        public void TriggerRelease()
        {
            CurrentState = State.Release;
        }
    }
}
