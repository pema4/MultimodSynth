using Jacobi.Vst.Framework;
using System.Collections.Generic;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, управляющий одной огибающей многих голосов.
    /// </summary>
    class EnvelopesManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Текущая длительность стадии атаки огибающей (в секундах).
        /// </summary>
        private float attackTime;

        /// <summary>
        /// Текущая длительность стадии спада огибающей (в секундах).
        /// </summary>
        private float decayTime;

        /// <summary>
        /// Текущее значение амплитуды огибающей.
        /// </summary>
        private float envelopeAmplitude;

        /// <summary>
        /// Текущая длительность стадии затухания огибающей (в секундах).
        /// </summary>
        private float releaseTime;

        /// <summary>
        /// Текущее значение уровня стадии поддержки огибающей.
        /// </summary>
        private float sustainLevel;

        /// <summary>
        /// Текущее значение изгиба огибающей в стадии атаки.
        /// </summary>
        private float attackCurve;

        /// <summary>
        /// Текущее значение изгиба огибающей в стадии спада и затухания.
        /// </summary>
        private float decayReleaseCurve;

        /// <summary>
        /// Список огибающих, связанных с этим менеджером огибающих.
        /// </summary>
        private List<Envelope> envelopes;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра амплитуды огибающей.
        /// </summary>
        private ParameterFilter envelopeAmplitudeFilter;

        /// <summary>
        /// Объект, управляющий параметром длительности стадии атаки.
        /// </summary>
        public VstParameterManager AttackTimeManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром длительности стадии спада.
        /// </summary>
        public VstParameterManager DecayTimeManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром уровня стадии поддержки.
        /// </summary>
        public VstParameterManager SustainLevelManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром длительности стадии затухания.
        /// </summary>
        public VstParameterManager ReleaseTimeManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром изгиба стадии атаки.
        /// </summary>
        public VstParameterManager AttackCurveManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром изгиба стадий спада и затухания.
        /// </summary>
        public VstParameterManager DecayReleaseCurveManager { get; private set; }

        /// <summary>
        /// Объект, управляюющий параметром амплитуды огибающей.
        /// </summary>
        public VstParameterManager EnvelopeAmplitudeManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса EnvelopesManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public EnvelopesManager(Plugin plugin, string parameterPrefix = "E")
            : base(plugin, parameterPrefix)
        {
            envelopes = new List<Envelope>();

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Параметр длительности стадии атаки.
            AttackTimeManager = factory.CreateParameterManager(
                name: "AT",
                defaultValue: 0.001f,
                valueChangedHandler: SetAttackTime);

            // Параметр длительности стадии спада.
            DecayTimeManager = factory.CreateParameterManager(
                name: "DT",
                valueChangedHandler: SetDecayTime);

            // Параметр уровня стадии поддержки.
            SustainLevelManager = factory.CreateParameterManager(
                name: "SL",
                defaultValue: 1,
                valueChangedHandler: SetSustainLevel);

            // Параметр длительности стадии затухания.
            ReleaseTimeManager = factory.CreateParameterManager(
                name: "RT",
                defaultValue: 0.001f,
                valueChangedHandler: SetReleaseTime);

            // Параметр изгиба стадии атаки.
            AttackCurveManager = factory.CreateParameterManager(
                name: "AC",
                defaultValue: 1,
                valueChangedHandler: SetAttackCurve);

            // Параметр изгиба стадий спада и затухания.
            DecayReleaseCurveManager = factory.CreateParameterManager(
                name: "DRC",
                valueChangedHandler: SetDecayReleaseCurve);

            // Параметр амплитуды огибающей.
            EnvelopeAmplitudeManager = factory.CreateParameterManager(
                name: "AMP",
                defaultValue: 1f,
                valueChangedHandler: x => envelopeAmplitudeFilter.SetTarget(x));
            envelopeAmplitudeFilter = new ParameterFilter(UpdateEnvelopeAmplitude, 1);
        }

        /// <summary>
        /// Обработчик изменения длительности стадии атаки.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetAttackTime(float value)
        {
            attackTime = (float)Converters.ToEnvelopeTime(value);
            
            foreach (var envelope in envelopes)
                envelope.SetAttackTime(attackTime);
        }

        /// <summary>
        /// Обработчик изменения длительности стадии спада.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetDecayTime(float value)
        {
            decayTime = (float)Converters.ToEnvelopeTime(value);

            foreach (var envelope in envelopes)
                envelope.SetDecayTime(decayTime);
        }

        /// <summary>
        /// Обработчик изменения уровня стадии поддержки.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetSustainLevel(float value)
        {
            sustainLevel = value;

            foreach (var envelope in envelopes)
                envelope.SetSustainLevel(value);
        }

        /// <summary>
        /// Обработчик изменения длительности стадии затухания.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetReleaseTime(float value)
        {
            releaseTime = (float)Converters.ToEnvelopeTime(value);

            foreach (var envelope in envelopes)
                envelope.SetReleaseTime(releaseTime);
        }

        /// <summary>
        /// Обработчик изменения изгиба стадии атаки.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetAttackCurve(float value)
        {
            attackCurve = value;

            foreach (var envelope in envelopes)
                envelope.SetAttackCurve(value);
        }

        /// <summary>
        /// Обработчик изменения изгиба стадии спада и затухания.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetDecayReleaseCurve(float value)
        {
            decayReleaseCurve = value;

            foreach (var envelope in envelopes)
                envelope.SetDecayReleaseCurve(value);
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения амплитуды огибающей.
        /// </summary>
        /// <param name="value">Новое значение амплитуды огибающей.</param>
        private void UpdateEnvelopeAmplitude(float value)
        {
            envelopeAmplitude = value;
            foreach (var envelope in envelopes)
                envelope.SetAmplitude(envelopeAmplitude);
        }

        /// <summary>
        /// Возвращает новый объект огибающей, связанный с этим объектом.
        /// </summary>
        /// <returns>Новая огибающая.</returns>
        public Envelope CreateNewEnvelope()
        {
            var envelope = new Envelope();

            envelope.SetAttackTime(attackTime);
            envelope.SetDecayTime(decayTime);
            envelope.SetSustainLevel(sustainLevel);
            envelope.SetReleaseTime(releaseTime);
            envelope.SetAttackCurve(attackCurve);
            envelope.SetDecayReleaseCurve(decayReleaseCurve);
            envelope.SetAmplitude(envelopeAmplitude);
            envelopes.Add(envelope);

            return envelope;
        }
        
        /// <summary>
        /// Метод, выполняющий обновление всех сглаживающих фильтров.
        /// </summary>
        public void Process()
        {
            envelopeAmplitudeFilter.Process();
        }
    }
}
