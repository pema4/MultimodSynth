using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.Linq;

namespace BetterSynth
{
    class PluginPrograms : VstPluginProgramsBase
    {
        private Plugin plugin;

        public PluginPrograms(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public VstParameterInfoCollection ParameterInfos { get; protected set; } = new VstParameterInfoCollection();

        public VstParameterCategoryCollection ParameterCategories { get; protected set; } = new VstParameterCategoryCollection();

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
