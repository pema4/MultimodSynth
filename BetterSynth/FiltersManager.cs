using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    class FiltersManager : AudioComponentWithParameters
    {
        private float cutoffMultiplier;
        private ParameterFilter cutoffMultiplierFilter;
        private float curve;
        private List<Filter> filters;
        private SvfFilterType filterType;
        private float trackingCoeff;

        public VstParameterManager FilterTypeManager { get; private set; }

        public VstParameterManager CutoffManager { get; private set; }

        public VstParameterManager TrackingCoeffManager { get; private set; }

        public VstParameterManager CurveManager { get; private set; }

        public FiltersManager(
            Plugin plugin,
            string parameterPrefix,
            string parameterCategory = "filters")
            : base(plugin, parameterPrefix, parameterCategory)
        {
            filters = new List<Filter>();
            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            FilterTypeManager = factory.CreateParameterManager(
                name: "TYPE",
                canBeAutomated: false,
                valueChangedHandler: SetFilterType);
            CreateRedirection(FilterTypeManager, nameof(FilterTypeManager));

            CutoffManager = factory.CreateParameterManager(
                name: "CUT",
                defaultValue: 1,
                valueChangedHandler: SetCutoffMultiplierTarget);
            CreateRedirection(CutoffManager, nameof(CutoffManager));
            cutoffMultiplierFilter = new ParameterFilter(UpdateCutoffMultiplier, 1);

            TrackingCoeffManager = factory.CreateParameterManager(
                name: "TRK",
                defaultValue: 1,
                valueChangedHandler: SetTrackingCoeff);
            CreateRedirection(TrackingCoeffManager, nameof(TrackingCoeffManager));

            CurveManager = factory.CreateParameterManager(
                name: "CRV",
                defaultValue: 0.5f,
                valueChangedHandler: SetCurve);
            CreateRedirection(CurveManager, nameof(CurveManager));
        }
        
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
                    filter.SetFilterType(filterType);
            }
        }

        private void SetCutoffMultiplierTarget(float value)
        {
            cutoffMultiplierFilter.SetTarget((float)Math.Pow(2, 13 * value) / 4);
        }

        private void UpdateCutoffMultiplier(float value)
        {
            cutoffMultiplier = value;
            foreach (var filter in filters)
                filter.SetCutoffMultiplier(cutoffMultiplier);
        }

        private void SetTrackingCoeff(float value)
        {
            trackingCoeff = value;

            foreach (var filter in filters)
                filter.SetTrackingCoeff(value);
        }

        private void SetCurve(float value)
        {
            curve = value;

            foreach (var filter in filters)
                filter.SetCurve(curve);
        }

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

        public void RemoveFilter(Filter filter)
        {
            filters.Remove(filter);
        }

        public void Process()
        {
            cutoffMultiplierFilter.Process();
        }
    }
}
