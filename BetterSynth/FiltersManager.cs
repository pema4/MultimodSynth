using Jacobi.Vst.Framework;
using System.Collections.Generic;

namespace BetterSynth
{
    class FiltersManager : ManagerOfManagers
    {
        private Plugin plugin;
        private string parameterPrefix;
        private List<Filter> filters;
        private SvfFilterType filterType;
        private float cutoff;
        private float trackingCoeff;
        private float curve;

        public FiltersManager(Plugin plugin, string parameterPrefix = "F")
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            filters = new List<Filter>();
            InitializeParameters();
        }

        public Filter CreateNewFilter()
        {
            var filter = new Filter(plugin);
            filter.Cutoff = cutoff;
            filter.Curve = curve;
            filter.TrackingCoeff = trackingCoeff;
            filter.FilterType = filterType;

            filters.Add(filter);
            return filter;
        }

        public void RemoveFilter(Filter filter)
        {
            filters.Remove(filter);
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "filters");

            FilterTypeManager = factory.CreateParameterManager(
                name: parameterPrefix + "_T",
                valueChangedHandler: SetFilterType);
            CreateRedirection(FilterTypeManager, nameof(FilterTypeManager));

            CutoffManager = factory.CreateParameterManager(
                name: parameterPrefix + "_CUT",
                defaultValue: 1,
                valueChangedHandler: SetCutoff);
            CreateRedirection(CutoffManager, nameof(CutoffManager));

            TrackingCoeffManager = factory.CreateParameterManager(
                name: parameterPrefix + "_TRK",
                defaultValue: 1,
                valueChangedHandler: SetTrackingCoeff);
            CreateRedirection(TrackingCoeffManager, nameof(TrackingCoeffManager));

            CurveManager = factory.CreateParameterManager(
                name: parameterPrefix + "_CRV",
                defaultValue: 0.5f,
                valueChangedHandler: SetCurve);
            CreateRedirection(CurveManager, nameof(CurveManager));
        }

        public VstParameterManager FilterTypeManager { get; private set; }

        public VstParameterManager CutoffManager { get; private set; }

        public VstParameterManager TrackingCoeffManager { get; private set; }

        public VstParameterManager CurveManager { get; private set; }

        private void SetFilterType(float value)
        {
            SvfFilterType newType;

            if (value < 0.25f)
                newType = SvfFilterType.Low;
            else if (value < 0.5f)
                newType = SvfFilterType.Band;
            else if (value < 0.75f)
                newType = SvfFilterType.Notch;
            else
                newType = SvfFilterType.High;

            if (filterType != newType)
            {
                filterType = newType;

                foreach (var filter in filters)
                    filter.FilterType = filterType;
            }

        }

        private void SetCutoff(float value)
        {
            cutoff = value * 20000;

            foreach (var filter in filters)
                filter.Cutoff = cutoff;
        }

        private void SetTrackingCoeff(float value)
        {
            trackingCoeff = value;

            foreach (var filter in filters)
                filter.TrackingCoeff = value;
        }

        private void SetCurve(float value)
        {
            curve = value;

            foreach (var filter in filters)
                filter.Curve = curve;   
        }
    }
}
