using System;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация ADSR огибающей.
    /// </summary>
    /// <seealso cref="http://www.earlevel.com/main/2013/06/01/envelope-generators/"/>
    class AdsrEnvelope
    {
        /// <summary>
        /// Указывает стадию огибающей.
        /// </summary>
        public enum EnvelopeState
        {
            Idle,
            Attack,
            Decay,
            Sustain,
            Release
        }

        /// <summary>
        /// Значение, которое прибавляется на стадии атаки.
        /// </summary>
        private double attackBase;

        /// <summary>
        /// Коэффициент для стадии атаки.
        /// </summary>
        private double attackCoef;

        /// <summary>
        /// Длительность стадии атаки (в сэмплах).
        /// </summary>
        private double attackRate;

        /// <summary>
        /// Разность целевого значения и максимального значения стадии атаки.
        /// </summary>
        private double attackTargetRatio;

        /// <summary>
        /// Текущее значение огибающей.
        /// </summary>
        private double currValue;

        /// <summary>
        /// Значение, которое прибавляется на стадии спада.
        /// </summary>
        private double decayBase;

        /// <summary>
        /// Коэффициент для стадии спада.
        /// </summary>
        private double decayCoef;

        /// <summary>
        /// Длительность стадии спада (в сэмплах).
        /// </summary>
        private double decayRate;

        /// <summary>
        /// Разность целевого значения и максимального значения стадий спада и затухания.
        /// </summary>
        private double decayReleaseTargetRatio;

        /// <summary>
        /// Значение, которое прибавляется на стадии затухания.
        /// </summary>
        private double releaseBase;

        /// <summary>
        /// Коэффициент для стадии затухания.
        /// </summary>
        private double releaseCoef;

        /// <summary>
        /// Длительность стадии затухания (в сэмплах).
        /// </summary>
        private double releaseRate;

        /// <summary>
        /// Текущее состояние огибающей.
        /// </summary>
        private EnvelopeState state;

        /// <summary>
        /// Уровень стадии поддержки.
        /// </summary>
        private double sustainLevel;

        /// <summary>
        /// Инициализирует новый объект типа AdsrEnvelope.
        /// </summary>
        public AdsrEnvelope()
        {
            SetAttackRate(0);
            SetDecayRate(0);
            SetReleaseRate(0);
            SetSustainLevel(1);
            SetAttackTargetRatio(0.3f);
            SetDecayReleaseTargetRatio(0.0001f);
        }

        /// <summary>
        /// Текущее состояние огибающей.
        /// </summary>
        public EnvelopeState State => state;

        /// <summary>
        /// Устанавливает новое значение "скорости" стадии атаки.
        /// </summary>
        /// <param name="rate"></param>
        public void SetAttackRate(double rate)
        {
            attackRate = rate;
            attackCoef = CalcCoef(rate, attackTargetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        /// <summary>
        /// Устанавливает новое значение "скорости" стадии спада.
        /// </summary>
        /// <param name="rate"></param>
        public void SetDecayRate(double rate)
        {
            decayRate = rate;
            decayCoef = CalcCoef(decayRate, decayReleaseTargetRatio);
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
        }

        /// <summary>
        /// Устанавливает новое значение уровня стадии поддержки.
        /// </summary>
        /// <param name="level"></param>
        public void SetSustainLevel(double level)
        {
            sustainLevel = level;
            decayBase = (sustainLevel - decayReleaseTargetRatio) * (1 - decayCoef);
        }

        /// <summary>
        /// Устанавливает новое значение длительности стадии затухания (в сэмплах).
        /// </summary>
        /// <param name="rate"></param>
        public void SetReleaseRate(double rate)
        {
            releaseRate = rate;
            releaseCoef = CalcCoef(rate, decayReleaseTargetRatio);
            releaseBase = -decayReleaseTargetRatio * (1 - releaseCoef);
        }

        /// <summary>
        /// Устанавливает разность целевого и максимального значения стадии атаки.
        /// Используется для управления изгибом огибающей.
        /// </summary>
        /// <param name="targetRatio"></param>
        public void SetAttackTargetRatio(double targetRatio)
        {
            if (targetRatio < 0.000000001f)
                attackTargetRatio = 0.000000001f;
            else
                attackTargetRatio = targetRatio;

            attackCoef = CalcCoef(attackRate, targetRatio);
            attackBase = (1 + attackTargetRatio) * (1 - attackCoef);
        }

        /// <summary>
        /// Устанавливает разность целевого и максимального значения стадий спада и затухания.
        /// Используется для управления изгибом огибающей.
        /// </summary>
        /// <param name="targetRatio"></param>
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

        /// <summary>
        /// Генерация нового значения огибающей.
        /// </summary>
        /// <returns></returns>
        public double Process()
        {
            switch (state)
            {
                case EnvelopeState.Idle:
                    break;

                case EnvelopeState.Attack:
                    currValue = attackBase + currValue * attackCoef;
                    if (currValue >= 1f)
                    {
                        currValue = 1f;
                        if (decayCoef != 0)
                            state = EnvelopeState.Decay;
                        else
                            state = EnvelopeState.Sustain;
                    }
                    break;

                case EnvelopeState.Decay:
                    currValue = decayBase + currValue * decayCoef;
                    if (currValue <= sustainLevel)
                    {
                        currValue = sustainLevel;
                        state = EnvelopeState.Sustain;
                    }
                    break;

                case EnvelopeState.Sustain:
                    currValue = sustainLevel;
                    break;

                case EnvelopeState.Release:
                    currValue = releaseBase + currValue * releaseCoef;
                    if (currValue <= 0)
                    {
                        currValue = 0;
                        state = EnvelopeState.Idle;
                        OnSoundStop();
                    }
                    break;
            }

            return currValue;
        }

        /// <summary>
        /// Метод, вызываемый при остановке генератора огибающей.
        /// </summary>
        private void OnSoundStop()
        {
            SoundStop?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Событие, возникающее при остановке генератора огибающей.
        /// </summary>
        public event EventHandler SoundStop;

        /// <summary>
        /// Возвращает коэффициент для вычисления значений огибающей.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="targetRatio"></param>
        /// <returns></returns>
        private double CalcCoef(double rate, double targetRatio) =>
            (rate <= 0f) ? 0f : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate);

        /// <summary>
        /// Метод, начинающий стадию атаки.
        /// </summary>
        public void TriggerAttack()
        {
            if (attackCoef != 0)
            {
                currValue = 0;
                state = EnvelopeState.Attack;
            }
            else if (decayCoef != 0)
            {
                currValue = 1;
                state = EnvelopeState.Decay;
            }
            else
                state = EnvelopeState.Sustain;
        }

        /// <summary>
        /// Метод, начинающий стадию затухания.
        /// </summary>
        public void TriggerRelease()
        {
            state = EnvelopeState.Release;
        }
    }
}
