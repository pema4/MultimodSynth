namespace BetterSynth
{
    /// <summary>
    /// The base class that's provides a Plugin property and method for parameters initialization.
    /// </summary>
    abstract class AudioComponentWithParameters : AudioComponent
    {
        private string parameterPrefix;
        private string parameterCategory;

        /// <summary>
        /// Current plugin instance.
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        /// Initializes the AudioComponentWithParameters class that belongs to given plugin
        /// and has given parameterPrefix and parameterCategory.
        /// </summary>
        /// <param name="plugin">A plugin instance to which new component belongs.</param>
        /// <param name="parameterPrefix">A prefix for parameter's names.</param>
        /// <param name="parameterCategory">A category of parameters.</param>
        public AudioComponentWithParameters(
            Plugin plugin,
            string parameterPrefix,
            string parameterCategory = "plugin")
        {
            Plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            this.parameterCategory = parameterCategory;
        }

        /// <summary>
        /// Derived classes must override this method in order to initialize
        /// their VstParameterManager class instances.
        /// </summary>
        /// <param name="factory">A parameter factory which is used to create managers.</param>
        protected abstract void InitializeParameters(ParameterFactory factory);

        /// <summary>
        /// Initialzes parameters. Should be used in constructors of derived classes.
        /// </summary>
        protected void InitializeParameters()
        {
            var factory = new ParameterFactory(Plugin, parameterCategory, parameterPrefix);
            InitializeParameters(factory);
        }
    }
}
