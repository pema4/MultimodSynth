using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    interface IDistortion
    {
        void SetAmount(float value);

        float Process(float input);
    }
}
