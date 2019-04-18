using System.IO;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin.IO;

namespace BetterSynth
{
    internal class ProgramReader : VstProgramReaderBase
    {
        public ProgramReader(Stream input) : base(input)
        {
        }

        protected override VstProgram CreateProgram()
        {
            throw new System.NotImplementedException();
        }
    }
}