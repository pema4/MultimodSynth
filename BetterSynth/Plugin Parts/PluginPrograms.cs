using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.IO;
using System.Linq;
using System.Text;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация интерфейса IVstPluginPrograms, отвечающего за работу с программами.
    /// </summary>
    class PluginPrograms : VstPluginProgramsBase
    {
        /// <summary>
        /// Ссылка на плагин, которому принадлежит этот компонент.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Инициализирует новых объект типа PluginPrograms, принадлежащий заданному плагину.
        /// </summary>
        /// <param name="plugin"></param>
        public PluginPrograms(Plugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Ссылка на коллекцию данных о параметрах.
        /// </summary>
        public VstParameterInfoCollection ParameterInfos { get; protected set; } = new VstParameterInfoCollection();

        /// <summary>
        /// Ссылка на коллекцию категорий параметров.
        /// </summary>
        public VstParameterCategoryCollection ParameterCategories { get; protected set; } = new VstParameterCategoryCollection();
        
        /// <summary>
        /// Создает коллекцию программ.
        /// </summary>
        /// <returns></returns>
        protected override VstProgramCollection CreateProgramCollection()
        {
            VstProgramCollection programs = new VstProgramCollection();

            VstProgram defaultProgram = new VstProgram(ParameterCategories);
            defaultProgram.Parameters.AddRange(ParameterInfos.Select(x => new VstParameter(x)));
            defaultProgram.Name = "Default";
            programs.Add(defaultProgram);

            return programs;
        }
    }
}
