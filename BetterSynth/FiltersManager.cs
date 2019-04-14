using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    class FiltersManager : AudioComponentWithParameters
    {
        private float cutoffMultiplier;
        private float cutoffMultiplierTarget;
        private OnePoleLowpassFilter cutoffMultiplierFilter = new OnePoleLowpassFilter();
        private float curve;
        private List<Filter> filters;
        private SvfFilterType filterType;
        private bool isCutoffMultiplierChanging;
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
                valueChangedHandler: SetCutoff);
            CreateRedirection(CutoffManager, nameof(CutoffManager));

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
                    filter.FilterType = filterType;
            }
        }

        private void SetCutoff(float value)
        {
            cutoffMultiplierTarget = (float)Math.Pow(2, 13 * value) / 4;
            isCutoffMultiplierChanging = true;
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

        public Filter CreateNewFilter()
        {
            var filter = new Filter(Plugin);
            filter.CutoffMultiplier = cutoffMultiplier;
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

        private void UpdateCutoffMultiplier()
        {
            var newValue = cutoffMultiplierFilter.Process(cutoffMultiplierTarget);
            if (cutoffMultiplier != newValue)
            {
                cutoffMultiplier = newValue;
                foreach (var filter in filters)
                    filter.CutoffMultiplier = cutoffMultiplier;
            }
            else
                isCutoffMultiplierChanging = false;
        }

        public void Process()
        {
            if (isCutoffMultiplierChanging)
                UpdateCutoffMultiplier();
        }
    }
}
