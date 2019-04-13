﻿using System;
using System.Linq;
using Jacobi.Vst.Framework;

namespace BetterSynth
{
    class ParameterFactory
    {
        private VstParameterInfoCollection parameterInfosCollection;
        private VstParameterCategory category;

        public ParameterFactory(Plugin plugin, string categoryName)
        {
            var programs = plugin.Programs;
            parameterInfosCollection = programs.ParameterInfos;
            category = new VstParameterCategory() { Name = categoryName };
            if (programs.ParameterCategories.All(x => x.Name != categoryName))
            {
                programs.ParameterCategories.Add(category);
            }
        }

        public VstParameterManager CreateParameterManager(
            string name = null,
            string label = null,
            string shortLabel = null,
            int minValue = 0,
            int maxValue = 1,
            float defaultValue = 0,
            float stepFloat = 0,
            int stepInteger = 0,
            float largeStepFloat = 0,
            int largeStepInteger = 0,
            bool canBeAutomated = true,
            Action<float> valueChangedHandler = null)
        {
            var parameterInfo = new VstParameterInfo()
            {
                Category = category,
                Name = name,
                Label = label,
                ShortLabel = shortLabel,
                MinInteger = minValue,
                MaxInteger = maxValue,
                DefaultValue = defaultValue,
                CanBeAutomated = canBeAutomated,
                StepFloat = stepFloat,
                StepInteger = stepInteger,
                LargeStepFloat = largeStepFloat,
                LargeStepInteger = largeStepInteger
            };

            VstParameterNormalizationInfo.AttachTo(parameterInfo);

            parameterInfosCollection.Add(parameterInfo);

            var manager = new VstParameterManager(parameterInfo);

            if (valueChangedHandler != null)
                manager.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "CurrentValue" || e.PropertyName == "ActiveParameter")
                        valueChangedHandler(manager.CurrentValue);
                };

            return manager;
        }
    }
}