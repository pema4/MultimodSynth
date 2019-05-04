using Jacobi.Vst.Framework;
using System;
using System.Linq;

namespace BetterSynth
{
    class ParameterFactory
    {
        private Plugin plugin;
        private PluginPrograms programs;
        private VstParameterCategory parameterCategory;
        private string namePrefix;

        public ParameterFactory(Plugin plugin, string category = "", string namePrefix = "")
        {
            this.plugin = plugin;
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

            plugin.Opened += (sender, e) =>
            {
                manager.HostAutomation = plugin.Host.GetInstance<IVstHostAutomation>();
            };

            if (valueChangedHandler != null)
                manager.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "CurrentValue" || e.PropertyName == "ActiveParameter")
                    {
                        plugin.AudioProcessor.ProcessingMutex.WaitOne();
                        valueChangedHandler(manager.CurrentValue);
                        plugin.AudioProcessor.ProcessingMutex.ReleaseMutex();
                    }
                };
            return manager;
        }
    }
}