using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace BetterSynth
{
    public class PluginCommandStub : StdPluginCommandStub
    {
        protected override IVstPlugin CreatePluginInstance()
        {
            return new Plugin();
        }
    }
}
