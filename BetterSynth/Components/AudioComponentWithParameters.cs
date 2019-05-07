namespace MultimodSynth
{
    /// <summary>
    /// Базовый класс, предоставляющий своим наследникам доступ к созданию параметров.
    /// </summary>
    abstract class AudioComponentWithParameters : AudioComponent
    {
        /// Префикс названия создаваемых параметров.
        private string parameterPrefix;

        /// <summary>
        /// Категория, которой принадлежат создаваемые параметры.
        /// </summary>
        private string parameterCategory;

        /// <summary>
        /// Объект класса Plugin, который содержит данный компонент.
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        /// Инициализирует объект класса AudioComponentWithParameters, связанный с переданным
        /// плагином.
        /// </summary>
        /// <param name="plugin">Объяет класса Plugin, который содержит создаваемый компонент.</param>
        /// <param name="parameterPrefix">Префикс названия создаваемых параметров.</param>
        /// <param name="parameterCategory">Категория, которой принадлежат параменты этого компонента./param>
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
        /// Производные классы переопределяют этот метод, чтобы в нём создать свои параметры
        /// с помощью посланного им объекта класса ParameterFactory.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected abstract void InitializeParameters(ParameterFactory factory);

        /// <summary>
        /// Инициализирует параменты. Производные классы вызывают этот метод где-то в конструкторе (обычно в конце).
        /// </summary>
        protected void InitializeParameters()
        {
            var factory = new ParameterFactory(Plugin, parameterCategory, parameterPrefix);
            InitializeParameters(factory);
        }
    }
}
