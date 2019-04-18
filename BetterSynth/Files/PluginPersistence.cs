using System.IO;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using Jacobi.Vst.Framework.Plugin.IO;

namespace BetterSynth
{
    internal class PluginPersistence : VstPluginPersistenceBase
    {
        private Plugin plugin;

        public PluginPersistence(Plugin plugin)
        {
            this.plugin = plugin;
        }

        protected override VstProgramReaderBase CreateProgramReader(Stream input)
        {
            return new ProgramReader(input);
        }
    }
}