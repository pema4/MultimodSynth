using System;

namespace BetterSynth
{
    /// <summary>
    /// Basically translation of http://www.earlevel.com/main/2013/06/01/envelope-generators/
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

        public State CurrentState { get; private set; }

        public float AttackRate
        {
            get => attackRate;
            set
            {
                attackRate = value;
                attackCoef = CalcCoef(value, AttackTargetRatio);
                attackBase = (1 + AttackTargetRatio) * (1 - attackCoef);
            }
        }

        public float DecayRate
        {
            get => decayRate;
            set
            {
                decayRate = value;
                decayCoef = CalcCoef(value, DecayReleaseTargetRatio);
                decayBase = (sustainLevel - DecayReleaseTargetRatio) * (1 - decayCoef);
            }
        }

        public float ReleaseRate
        {
            get => releaseRate;
            set
            {
                releaseRate = value;
                releaseCoef = CalcCoef(value, DecayReleaseTargetRatio);
                releaseBase = -DecayReleaseTargetRatio * (1 - releaseCoef);
            }
        }

        public float SustainLevel
        {
            get => sustainLevel;
            set
            {
                sustainLevel = value;
                decayBase = (sustainLevel - DecayReleaseTargetRatio) * (1 - decayCoef);
            }
        }

        public float AttackTargetRatio
        {
            get => attackTargetRatio;
            set
            {
                if (value < 0.000000001f)
                    attackTargetRatio = 0.000000001f;
                else
                    attackTargetRatio = value;

                attackCoef = CalcCoef(attackRate, value);
                attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
            }
        }

        public float DecayReleaseTargetRatio
        {
            get => decayReleaseTargetRatio;
            set
            {
                if (value < 0.000000001f)
                    decayReleaseTargetRatio = 0.000000001f;
                else
                    decayReleaseTargetRatio = value;

                decayCoef = CalcCoef(DecayRate, value);
                releaseCoef = CalcCoef(ReleaseRate, value);
                decayBase = (SustainLevel - DecayReleaseTargetRatio) * (1 - decayCoef);
                releaseBase = -DecayReleaseTargetRatio * (1 - releaseCoef);
            }
        }

        private float currValue;
        private float attackRate;
        private float decayRate;
        private float releaseRate;
        private float attackCoef;
        private float decayCoef;
        private float releaseCoef;
        private float sustainLevel;
        private float attackTargetRatio;
        private float decayReleaseTargetRatio;
        private float attackBase;
        private float decayBase;
        private float releaseBase;

        public void Process(out float output)
        {
            switch (CurrentState)
            {
                case State.Idle:
                    currValue = 0;
                    break;
                case State.Attack:
                    currValue = attackBase + currValue * attackCoef;
                    if (currValue >= 1f)
                    {
                        currValue = 1f;
                        CurrentState = State.Decay;
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

            output = currValue;
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
            CurrentState = State.Attack;
        }

        public void TriggerRelease()
        {
            CurrentState = State.Release;
        }
    }
}
