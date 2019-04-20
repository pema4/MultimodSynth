using System;
using System.Linq;
using Jacobi.Vst.Framework;

namespace BetterSynth
{
    class ParameterFactory
    {
        private PluginPrograms programs;
        private VstParameterCategory parameterCategory;
        private string namePrefix;

        public ParameterFactory(Plugin plugin, string category = "", string namePrefix = "")
        {
            programs = plugin.Programs;
            this.namePrefix = namePrefix;
            parameterCategory = new VstParameterCategory() { Name = category };
            if (programs.ParameterCategories.All(x => x.Name != category))
                programs.ParameterCategories.Add(parameterCategory);
        }

        public VstParameterManager CreateParameterManager(
            string name = null,
            string label = null,
            string shortLabel = null,
            int minValue = 0,
            int maxValue = 1,
            float defaultValue = 0,
            bool canBeAutomated = true,
            Action<float> valueChangedHandler = null)
        {
            var parameterInfo = new VstParameterInfo()
            {
                Category = parameterCategory,
                Name = namePrefix + name,
                Label = label,
                ShortLabel = shortLabel,
                MinInteger = minValue,
                MaxInteger = maxValue,
                DefaultValue = defaultValue,
                CanBeAutomated = canBeAutomated,
            };
            VstParameterNormalizationInfo.AttachTo(parameterInfo);
            programs.ParameterInfos.Add(parameterInfo);
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