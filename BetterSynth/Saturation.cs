using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Effects/42-soft-saturation.html
    /// </summary>
    class SaturationManager : ManagerOfManagers
    {
        private Plugin plugin;
        private string parameterPrefix;
        private float amount;
        private OnePoleLowpassFilter amountFilter = new OnePoleLowpassFilter();
        private float amountTarget;
        private float denominator;
        private bool isAmountChanging;
        private bool isMixChanging;
        private float mix;
        private float wetCoeff;
        private float dryCoeff = 1;
        private OnePoleLowpassFilter mixFilter = new OnePoleLowpassFilter();
        private float mixTarget;

        public SaturationManager(Plugin plugin, string parameterPrefix = "SAT")
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            InitializeParameters();
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "effects");

            AmountManager = factory.CreateParameterManager(
                name: parameterPrefix + "_AMT",
                defaultValue: 0.5f,
                valueChangedHandler: SetAmount);
            CreateRedirection(AmountManager, nameof(AmountManager));

            MixManager = factory.CreateParameterManager(
                name: parameterPrefix + "_MIX",
                defaultValue: 0,
                valueChangedHandler: SetMix);
            CreateRedirection(MixManager, nameof(MixManager));
        }

        public VstParameterManager AmountManager { get; private set; } 

        public VstParameterManager MixManager { get; private set; }

        private void SetAmount(float value)
        {
            amountTarget = value;
            isAmountChanging = true;
        }

        private void SetMix(float value)
        {
            mixTarget = value;
            isMixChanging = true;
        }

        private void UpdateAmount()
        {
            var newValue = amountFilter.Process(amountTarget);

            if (newValue != amount)
            {
                amount = newValue;

                var temp = 1 - amount;
                denominator = temp * temp;
            }
            else
                isAmountChanging = false;
        }

        private void UpdateMix()
        {
            var newValue = mixFilter.Process(mixTarget);

            if (newValue != mix)
            {
                mix = newValue;
                wetCoeff = mix;
                dryCoeff = 1 - wetCoeff;
            }
            else
                isMixChanging = false;
        }

        public float Process(float input)
        {
            if (isAmountChanging)
                UpdateAmount();

            if (isMixChanging)
                UpdateMix();

            int sign = Math.Sign(input);
            float abs = Math.Abs(input);

            if (abs < amount)
                return input;
            else if (abs > 1)
            {
                var wet = sign * (amount + 1) / 2;
                return dryCoeff * input + wetCoeff * wet;
            }
            else
            {
                var temp = abs - amount;
                var wet = sign * (amount + temp / (1 + temp * temp / denominator));
                return dryCoeff * input + wetCoeff * wet;
            }
        }
    }
}
