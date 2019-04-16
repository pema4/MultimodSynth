using System;

namespace BetterSynth
{
    enum AdsrEnvelopeState
    {
        Idle,
        Attack,
        Decay,
        Sustain,
        Release
    }

    /// <summary>
    /// Almost translation of http://www.earlevel.com/main/2013/06/01/envelope-generators/
    /// </summary>
    class AdsrEnvelope
    {
        private double attackBase;
        private double attackCoef;
        private double attackRate;
        private double attackTargetRatio;
        private AdsrEnvelopeState state;
        private double currValue;
        private double decayBase;
        private double decayCoef;
        private double decayRate;
        private double decayReleaseTargetRatio;
        private double releaseBase;
        private double releaseCoef;
        private double releaseRate;
        private double sustainLevel;

        public AdsrEnvelope()
        {
            SetAttackRate(0);
            SetDecayRate(0);
            SetReleaseRate(0);
            SetSustainLevel(1);
            SetAttackTargetRatio(0.3f);
            SetDecayReleaseTargetRatio(0.0001f);
        }

        public AdsrEnvelopeState State => state;

        public void SetAttackRate(double rate)
        {
            attackRate = rate;
            attackCoef = CalcCoef(rate, attackTargetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        public void SetDecayRate(double rate)
        {
            decayRate = rate;
            decayCoef = CalcCoef(decayRate, decayReleaseTargetRatio);
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
        }

        public void SetSustainLevel(double level)
        {
            sustainLevel = level;
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
        }

        public void SetReleaseRate(double rate)
        {
            releaseRate = rate;
            releaseCoef = CalcCoef(rate, decayReleaseTargetRatio);
            releaseBase = -decayReleaseTargetRatio * (1 - releaseCoef);
        }

        public void SetAttackTargetRatio(double targetRatio)
        {
            if (targetRatio < 0.000000001f)
                attackTargetRatio = 0.000000001f;
            else
                attackTargetRatio = targetRatio;

            attackCoef = CalcCoef(attackRate, targetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        public void SetDecayReleaseTargetRatio(double targetRatio)
        {
            if (targetRatio < 0.000000001f)
                decayReleaseTargetRatio = 0.000000001f;
            else
                decayReleaseTargetRatio = targetRatio;

            decayCoef = CalcCoef(decayRate, targetRatio);
            releaseCoef = CalcCoef(releaseRate, targetRatio);
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
            releaseBase = -decayReleaseTargetRatio * (1 - releaseCoef);
        }

        public double Process()
        {
            switch (state)
            {
                case AdsrEnvelopeState.Idle:
                    break;

                case AdsrEnvelopeState.Attack:
                    currValue = attackBase + currValue * attackCoef;
                    if (currValue >= 1f)
                    {
                        currValue = 1f;
                        if (decayCoef != 0)
                            state = AdsrEnvelopeState.Decay;
                        else
                            state = AdsrEnvelopeState.Sustain;
                    }
                    break;

                case AdsrEnvelopeState.Decay:
                    currValue = decayBase + currValue * decayCoef;
                    if (currValue <= sustainLevel)
                    {
                        currValue = sustainLevel;
                        state = AdsrEnvelopeState.Sustain;
                    }
                    break;

                case AdsrEnvelopeState.Sustain:
                    currValue = sustainLevel;
                    break;

                case AdsrEnvelopeState.Release:
                    currValue = releaseBase + currValue * releaseCoef;
                    if (currValue <= 0)
                    {
                        currValue = 0;
                        state = AdsrEnvelopeState.Idle;
                        OnSoundStop();
                    }
                    break;
            }

            return currValue;
        }

        private void OnSoundStop()
        {
            SoundStop?.Invoke(this, new EventArgs());
        }

        public event EventHandler SoundStop;

        private double CalcCoef(double rate, double targetRatio) =>
            (rate <= 0f) ? 0f : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate);

        public void TriggerAttack()
        {
            if (attackCoef != 0)
            {
                currValue = 0;
                state = AdsrEnvelopeState.Attack;
            }
            else if (decayCoef != 0)
            {
                currValue = 1;
                state = AdsrEnvelopeState.Decay;
            }
            else
                state = AdsrEnvelopeState.Sustain;
        }

        public void TriggerRelease()
        {
            state = AdsrEnvelopeState.Release;
        }
    }
}
