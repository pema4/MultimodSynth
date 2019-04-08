using Jacobi.Vst.Framework;
using System.ComponentModel;

namespace BetterSynth
{
    class ManagerOfManagers : INotifyPropertyChanged
    {
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
