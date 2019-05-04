using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSynth.UI
{
    class DataClass : INotifyPropertyChanged
    {
        private double value;
        
        public double Value1
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
