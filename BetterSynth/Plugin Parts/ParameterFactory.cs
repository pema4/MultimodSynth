using Jacobi.Vst.Framework;
using System;
using System.Linq;

namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой фабрику для удобного создания параметров.
    /// </summary>
    class ParameterFactory
    {
        /// <summary>
        /// Ссылка на плагин, которому принадлежит этот компонент.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Ссылка на компонент PluginPrograms данного плагина.
        /// </summary>
        private PluginPrograms programs;

        /// <summary>
        /// Категория параметров.
        /// </summary>
        private VstParameterCategory parameterCategory;

        /// <summary>
        /// Префикс названия параметра.
        /// </summary>
        private string namePrefix;

        /// <summary>
        /// Инициализирует новый объект типа ParameterFactory, принадлежащий заданному
        /// плагину и имеющий переданную категорию и префикс имени параметра.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="category"></param>
        /// <param name="namePrefix"></param>
        public ParameterFactory(Plugin plugin, string category = "", string namePrefix = "")
        {
            this.plugin = plugin;
            programs = plugin.Programs;
            this.namePrefix = namePrefix;
            parameterCategory = new VstParameterCategory() { Name = category };
            if (programs.ParameterCategories.All(x => x.Name != category))
                programs.ParameterCategories.Add(parameterCategory);
        }

        /// <summary>
        /// Создаёт новый параметр и регистрирует его.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        /// <param name="defaultValue">Начальное значение.</param>
        /// <param name="valueChangedHandler">Обработчик изменения значения параметра.</param>
        /// <returns>Объект, управляющий созданным параметром.</returns>
        public VstParameterManager CreateParameterManager(
            string name = null,
            float defaultValue = 0,
            Action<float> valueChangedHandler = null)
        {
            var parameterInfo = new VstParameterInfo()
            {
                Category = parameterCategory,
                Name = namePrefix + name,
                DefaultValue = defaultValue,
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