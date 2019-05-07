using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace MultimodSynth
{
    /// <summary>
    /// Публичный класс, являющийся точкой связывания managed и unmanaged кода.
    /// </summary>
    public class PluginCommandStub : StdPluginCommandStub
    {
        /// <summary>
        /// Возвращает новый объект типа IVstPlugin.
        /// </summary>
        /// <returns></returns>
        protected override IVstPlugin CreatePluginInstance()
        {
            return new Plugin();
        }
    }
}
