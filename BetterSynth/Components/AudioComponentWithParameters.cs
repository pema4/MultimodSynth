using Jacobi.Vst.Framework;
using System;
using System.ComponentModel;

namespace BetterSynth
{
    abstract class AudioComponentWithParameters : AudioComponent, INotifyPropertyChanged
    {
        private string parameterPrefix;
        private string parameterCategory;

        public Plugin Plugin { get; }

        public AudioComponentWithParameters(
            Plugin plugin,
            string parameterPrefix,
            string parameterCategory)
        {
            Plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            this.parameterCategory = parameterCategory;
        }

        protected abstract void InitializeParameters(ParameterFactory factory);

        protected void InitializeParameters()
        {
            var factory = new ParameterFactory(Plugin, parameterCategory, parameterPrefix);
            InitializeParameters(factory);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void CreateRedirection(VstParameterManager manager, string managerName)
        {
            manager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "CurrentValue" || e.PropertyName == "ActiveParameter")
                    OnPropertyChanged(managerName);
            };
        }
    }
}
