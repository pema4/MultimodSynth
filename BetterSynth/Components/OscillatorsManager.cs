using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    /// <summary>
    /// Компонент плагина, управляющий одним осциллятором многих голосов.
    /// </summary>
    class OscillatorsManager : AudioComponentWithParameters
    {        
        /// <summary>
        /// Коэффициент высоты играемой ноты (изменяется параметром подстройки в полутонах).
        /// </summary>
        private float pitchFine;

        /// <summary>
        /// Другой коэффициент высоты играемой ноты (изменяется параметром подстройки в центах).
        /// </summary>
        private float pitchSemi;

        /// <summary>
        /// Общий коэффициент высоты играемой ноты.
        /// </summary>
        private float pitchMultiplier;

        /// <summary>
        /// Ссылка на текущую используемую таблицу сэмплов.
        /// </summary>
        private WaveTableOscillator waveTable = Utilities.WaveTables[0];

        /// <summary>
        /// Список осцилляторов, связанных с этим менеджером осцилляторов.
        /// </summary>
        private List<Oscillator> oscillators;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания изменений частоты играемой ноты.
        /// </summary>
        private ParameterFilter pitchMultiplierFilter;

        /// <summary>
        /// Объект, управляющий параметром подстройки частоты (в полутонах).
        /// </summary>
        public VstParameterManager PitchSemiManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром подстройки частоты (в центах).
        /// </summary>
        public VstParameterManager PitchFineManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром текущей используемой таблицы семплов.
        /// </summary>
        public VstParameterManager WaveTableManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса OscillatorsManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public OscillatorsManager(
            Plugin plugin,
            string parameterPrefix) :
            base(plugin, parameterPrefix)
        {
            oscillators = new List<Oscillator>();

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            PitchSemiManager = factory.CreateParameterManager(
                name: "SEMI",
                defaultValue: 0.5f,
                valueChangedHandler: SetPitchSemi);

            PitchFineManager = factory.CreateParameterManager(
                name: "FINE",
                defaultValue: 0.5f,
                valueChangedHandler: SetPitchFine);
            pitchMultiplierFilter = new ParameterFilter(UpdatePitchMultiplier, 0);

            WaveTableManager = factory.CreateParameterManager(
                name: "TYPE",
                defaultValue: 0,
                valueChangedHandler: SetWaveTable);
        }

        /// <summary>
        /// Обработчик изменения подстройки частоты в полутонах.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetPitchSemi(float value)
        {
            pitchSemi = (float)Math.Pow(2, (int)Converters.ToSemitones(value) / 12.0);
            var target = pitchSemi * pitchFine;
            pitchMultiplierFilter.SetTarget(target);
        }

        /// <summary>
        /// Обработчик изменения подстройки частоты (в центах).
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetPitchFine(float value)
        {
            pitchFine = (float)Math.Pow(2, (int)Converters.ToCents(value) / 1200.0);
            var target = pitchSemi * pitchFine;
            pitchMultiplierFilter.SetTarget(target);
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения общей подстройки частоты.
        /// </summary>
        /// <param name="value">Новое значение подстройки частоты.</param>
        private void UpdatePitchMultiplier(float value)
        {
            pitchMultiplier = value;
            foreach (var oscillator in oscillators)
                oscillator.SetPitchMultiplier(pitchMultiplier);
        }

        /// <summary>
        /// Обработчик изменения текущей таблицы сэмплов.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetWaveTable(float value)
        {
            var newWaveTable = Converters.ToWaveTable(value);

            if (waveTable != newWaveTable)
            {
                waveTable = newWaveTable;
                foreach (var oscillator in oscillators)
                    oscillator.SetWaveTable(waveTable.Clone());
            }
        }

        /// <summary>
        /// Возвращает новый объект осциллятора, связанный с этим объектом.
        /// </summary>
        /// <returns>Новая огибающая.</returns>
        public Oscillator CreateNewOscillator()
        {
            var res = new Oscillator();
            res.SetPitchMultiplier(pitchMultiplier);
            res.SetWaveTable(waveTable.Clone());
            oscillators.Add(res);
            return res;
        }

        /// <summary>
        /// Метод, выполняющий обновление всех сглаживающих фильтров.
        /// </summary>
        public void Process()
        {
            pitchMultiplierFilter.Process();
        }
    }
}