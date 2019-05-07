using System;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент голоса плагина, представляющий ADSR-огибающую.
    /// </summary>
    class Envelope : AudioComponent
    {
        /// <summary>
        /// Объект огибающей.
        /// </summary>
        private AdsrEnvelope envelope;

        /// <summary>
        /// Текущее значение длительности стадии атаки (в векундах).
        /// </summary>
        private float attackTime;

        /// <summary>
        /// Текущее значение длительности стадии спада с 100% до 0% (в секундах).
        /// </summary>
        private float decayTime;

        /// <summary>
        /// Текущее значение уровня периода поддержки.
        /// </summary>
        private float sustainLevel;

        /// <summary>
        /// Текущее значение длительности стадии затухания с 100% до 0% (в секундах).
        /// </summary>
        private float releaseTime;

        /// <summary>
        /// Текущее значение изгиба огибающей в стадии атаки (в диапазоне [0, 1]).
        /// </summary>
        private float attackCurve;

        /// <summary>
        /// Значение, показывающее форму стадии атаки.
        /// </summary>
        private double attackTargetRatio;

        /// <summary>
        /// Текущее значение изгиба огибающей в стадии спада и затухания (в диапазоне [0, 1]).
        /// </summary>
        private float decayReleaseCurve;

        /// <summary>
        /// Значение, показывающее форму стадии спада и затухания.
        /// </summary>
        private double decayReleaseTargetRatio;

        /// <summary>
        /// Максимальное значение уровня огибающей.
        /// </summary>
        private float amplitude;

        /// <summary>
        /// Инициализирует новый объект класса Envelope.
        /// </summary>
        public Envelope()
        {
            envelope = new AdsrEnvelope();
        }

        /// <summary>
        /// Показывает, активна ли огибающая в данный момент времени.
        /// </summary>
        public bool IsActive => envelope.State != AdsrEnvelope.EnvelopeState.Idle;

        /// <summary>
        /// Устанавливает новое значение длительности стадии атаки (в секундах).
        /// </summary>
        /// <param name="value">Время (в секундах).</param>
        public void SetAttackTime(float value)
        {
            attackTime = value;
            envelope.SetAttackRate(attackTime * SampleRate);
        }

        /// <summary>
        /// Устанавливает новое значение длительности стадии спада (в секундах).
        /// </summary>
        /// <param name="value">Время (в секундах).</param>
        public void SetDecayTime(float value)
        {
            decayTime = value;
            envelope.SetDecayRate(decayTime * SampleRate);
        }

        /// <summary>
        /// Устанавливает новое значение уровня поддержки.
        /// </summary>
        /// <param name="value">Уровень (в диапазоне [0, 1]).</param>
        public void SetSustainLevel(float value)
        {
            sustainLevel = value;
            envelope.SetSustainLevel(sustainLevel);
        }

        /// <summary>
        /// Устанавливает новое значение длительности стадии затухания (в секундах).
        /// </summary>
        /// <param name="value">Время (в секундах).</param>
        public void SetReleaseTime(float value)
        {
            releaseTime = value;
            envelope.SetReleaseRate(releaseTime * SampleRate);
        }

        /// <summary>
        /// Устанавливает новое значение изгиба огибающей в стадии атаки.
        /// </summary>
        /// <param name="value">Изгиб (в диапазоне [0, 1]).</param>
        public void SetAttackCurve(float value)
        {
            attackCurve = value;
            attackTargetRatio = 0.001 * (Math.Exp(12 * (0.05f + 0.95f * attackCurve)) - 1);
            envelope.SetAttackTargetRatio(attackTargetRatio);
        }

        /// <summary>
        /// Устанавливает новое значение изгиба огибающей в стадиях спада и затухания.
        /// </summary>
        /// <param name="value">Изгиб (в диапазоне [0, 1]).</param>
        public void SetDecayReleaseCurve(float value)
        {
            decayReleaseCurve = value;
            decayReleaseTargetRatio = 
                0.001 * (Math.Exp(12 * (0.0005 + 0.9995 * decayReleaseCurve)) - 1);
            envelope.SetDecayReleaseTargetRatio(decayReleaseTargetRatio);
        }

        /// <summary>
        /// Устанавливает новое значение амплитуды огибающей.
        /// </summary>
        /// <param name="value">Амплитуда (в диапазоне [0, 1]).</param>
        public void SetAmplitude(float value)
        {
            amplitude = value;
        }

        /// <summary>
        /// Начинает стадию атаки.
        /// </summary>
        public void TriggerAttack() => envelope.TriggerAttack();

        /// <summary>
        /// Начинает стадию затухания.
        /// </summary>
        public void TriggerRelease() => envelope.TriggerRelease();

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <returns>Новое значение "высоты" огибающей</returns>
        public float Process()
        {
            return (float)envelope.Process() * amplitude;
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            SetAttackTime(attackTime);
            SetDecayTime(decayTime);
            SetReleaseTime(releaseTime);
        }
    }
}
