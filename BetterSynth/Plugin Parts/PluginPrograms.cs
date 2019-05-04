using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.IO;
using System.Linq;
using System.Text;

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

        public void ReadParameters(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                var activeParameters = ActiveProgram.Parameters;
                foreach (var param in activeParameters)
                {
                    var name = reader.ReadString();
                    var normalizedValue = reader.ReadSingle();
                    activeParameters[name].NormalizedValue = normalizedValue;
                }
            }
        }

        public void WriteParameters(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.Default, true))
            {
                var activeParameters = ActiveProgram.Parameters;
                foreach (var param in activeParameters)
                {
                    writer.Write(param.Info.Name);
                    writer.Write(param.NormalizedValue);
                }
            }
        }
    }
}
