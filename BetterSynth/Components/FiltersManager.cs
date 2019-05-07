using Jacobi.Vst.Framework;
using System.Collections.Generic;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, управляющий одним фильтром многих голосов.
    /// </summary>
    class FiltersManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Текущий тип фильтра.
        /// </summary>
        private SvfFilter.FilterType filterType;
        
        /// <summary>
        /// Текущая "ширина" фильтра.
        /// </summary>
        private float curve;

        /// <summary>
        /// Текущий множитель частоты среза фильтра.
        /// </summary>
        private float cutoffMultiplier;

        /// <summary>
        /// Текущий коэффициент отслеживания частоты играющей ноты.
        /// </summary>
        private float trackingCoeff;

        /// <summary>
        /// Список фильтров, связанных с этим менеджером фильтров.
        /// </summary>
        private List<Filter> filters;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра множителя частоты среза фильтра.
        /// </summary>
        private ParameterFilter cutoffMultiplierFilter;

        /// <summary>
        /// Объект, управляющий параметром типа фильтра.
        /// </summary>
        public VstParameterManager FilterTypeManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром множителя частоты среза фильтра.
        /// </summary>
        public VstParameterManager CutoffManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром коффициента отслеживания частоты играющей ноты.
        /// </summary>
        public VstParameterManager TrackingCoeffManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром "ширины" фильтра.
        /// </summary>
        public VstParameterManager CurveManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса FiltersManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public FiltersManager(
            Plugin plugin,
            string parameterPrefix)
            : base(plugin, parameterPrefix)
        {
            filters = new List<Filter>();

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Параметр типа фильтра.
            FilterTypeManager = factory.CreateParameterManager(
                name: "TYPE",
                valueChangedHandler: SetFilterType);

            // Параметр множителя частоты среза фильтра.
            CutoffManager = factory.CreateParameterManager(
                name: "CUT",
                defaultValue: 1,
                valueChangedHandler: SetCutoffMultiplierTarget);
            cutoffMultiplierFilter = new ParameterFilter(UpdateCutoffMultiplier, 1, 10);

            // Параметр коэффициента отслеживания частоты играющей ноты.
            TrackingCoeffManager = factory.CreateParameterManager(
                name: "TRK",
                defaultValue: 1,
                valueChangedHandler: SetTrackingCoeff);

            // Параметр "ширины" фильтра.
            CurveManager = factory.CreateParameterManager(
                name: "CRV",
                defaultValue: 0.5f,
                valueChangedHandler: SetCurve);
        }

        /// <summary>
        /// Обработчик изменения длительности типа фильтра.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetFilterType(float value)
        {
            var newType = Converters.ToFilterType(value);

            if (filterType != newType)
            {
                filterType = newType;

                foreach (var filter in filters)
                    filter.SetFilterType(filterType);
            }
        }

        /// <summary>
        /// Обработчик изменения множителя частоты среза фильтра.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetCutoffMultiplierTarget(float value)
        {
            var mult = (float)Converters.ToFilterCutoffMultiplier(value);
            cutoffMultiplierFilter.SetTarget(mult);
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" множителя частоты среза фильтра.
        /// </summary>
        /// <param name="value">Новое значение множителя частоты среза фильтра.</param>
        private void UpdateCutoffMultiplier(float value)
        {
            cutoffMultiplier = value;
            foreach (var filter in filters)
                filter.SetCutoffMultiplier(cutoffMultiplier);
        }

        /// <summary>
        /// Обработчик изменения коэффициента отслеживания частоты играющей ноты.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetTrackingCoeff(float value)
        {
            trackingCoeff = value;

            foreach (var filter in filters)
                filter.SetTrackingCoeff(value);
        }

        /// <summary>
        /// Обработчик изменения "ширины" фильтра.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetCurve(float value)
        {
            curve = value;

            foreach (var filter in filters)
                filter.SetCurve(curve);
        }

        /// <summary>
        /// Возвращает новый объект фильтра, связанный с этим объектом.
        /// </summary>
        /// <returns>Новый фильтр.</returns>
        public Filter CreateNewFilter()
        {
            var filter = new Filter();
            filter.SetCutoffMultiplier(cutoffMultiplier);
            filter.SetCurve(curve);
            filter.SetTrackingCoeff(trackingCoeff);
            filter.SetFilterType(filterType);

            filters.Add(filter);
            return filter;
        }

        /// <summary>
        /// Метод, выполняющий обновление всех сглаживающих фильтров.
        /// </summary>
        public void Process()
        {
            cutoffMultiplierFilter.Process();
        }
    }
}
