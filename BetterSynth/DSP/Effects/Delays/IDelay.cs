using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    interface IDelay
    {
        void Process(float inputL, float inputR, out float outputL, out float outputR);

        void SetDelay(float value);

        void SetFeedback(float value);

        void SetStereo(float value);

        void Reset();
    }
}
