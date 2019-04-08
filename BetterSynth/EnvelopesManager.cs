using Jacobi.Vst.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace BetterSynth
{
    class EnvelopesManager : ManagerOfManagers
    {
        private const float MaximumTime = 10f;

        private Plugin plugin;
        private string parameterPrefix;
        private List<AdsrEnvelope> envelopes;
        private float sustainLevel;
        private float envelopeAmplitude;
        private float decayTime;
        private float attackTime;
        private float releaseTime;
        private float attackCurve;
        private float decayReleaseCurve;

        public EnvelopesManager(Plugin plugin, string parameterPrefix = "E")
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            envelopes = new List<AdsrEnvelope>();
            InitializeParameters();
        }

        public AdsrEnvelope CreateNewEnvelope()
        {
            var envelope = new AdsrEnvelope(plugin);
            envelope.Amplitude = envelopeAmplitude;
            envelope.AttackTime = attackTime;
            envelope.DecayTime = decayTime;
            envelope.SustainLevel = sustainLevel;
            envelope.ReleaseTime = releaseTime;
            envelope.AttackCurve = attackCurve;
            envelope.DecayReleaseCurve = decayReleaseCurve;

            envelopes.Add(envelope);
            return envelope;
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "envelopes");

            AttackTimeManager = factory.CreateParameterManager(
                name: parameterPrefix + "_AT",
                defaultValue: 0.001f,
                valueChangedHandler: SetAttackTime);
            CreateRedirection(AttackTimeManager, nameof(AttackTimeManager));

            DecayTimeManager = factory.CreateParameterManager(
                name: parameterPrefix + "_DT",
                valueChangedHandler: SetDecayTime);
            CreateRedirection(DecayTimeManager, nameof(DecayTimeManager));

            SustainLevelManager = factory.CreateParameterManager(
                name: parameterPrefix + "_SL",
                defaultValue: 1,
                valueChangedHandler: SetSustainLevel);
            CreateRedirection(SustainLevelManager, nameof(SustainLevelManager));

            ReleaseTimeManager = factory.CreateParameterManager(
                name: parameterPrefix + "_RT",
                valueChangedHandler: SetReleaseTime);
            CreateRedirection(ReleaseTimeManager, nameof(ReleaseTimeManager));

            AttackCurveManager = factory.CreateParameterManager(
                name: parameterPrefix + "_AC",
                valueChangedHandler: SetAttackCurve);
            CreateRedirection(AttackCurveManager, nameof(AttackCurveManager));

            DecayReleaseCurveManager = factory.CreateParameterManager(
                name: parameterPrefix + "_DRC",
                valueChangedHandler: SetDecayReleaseCurve);
            CreateRedirection(DecayReleaseCurveManager, nameof(DecayReleaseCurveManager));

            EnvelopeAmplitudeManager = factory.CreateParameterManager(
                name: parameterPrefix + "_AMP",
                defaultValue: 1f,
                valueChangedHandler: SetEnvelopeAmplitude);
            CreateRedirection(EnvelopeAmplitudeManager, nameof(EnvelopeAmplitudeManager));
        }

        public VstParameterManager AttackTimeManager { get; private set; }

        public VstParameterManager DecayTimeManager { get; private set; }

        public VstParameterManager SustainLevelManager { get; private set; }

        public VstParameterManager ReleaseTimeManager { get; private set; }

        public VstParameterManager AttackCurveManager { get; private set; }

        public VstParameterManager DecayReleaseCurveManager { get; private set; }

        public VstParameterManager EnvelopeAmplitudeManager { get; private set; }

        
        private void SetAttackTime(float value)
        {
            attackTime = value * MaximumTime;
            
            foreach (var envelope in envelopes)
                envelope.AttackTime = attackTime;
        }


        private void SetDecayTime(float value)
        {
            decayTime = value * MaximumTime;

            foreach (var envelope in envelopes)
                envelope.DecayTime = decayTime;
        }

        private void SetSustainLevel(float value)
        {
            sustainLevel = value;

            foreach (var envelope in envelopes)
                envelope.SustainLevel = value;
        }

        private void SetReleaseTime(float value)
        {
            releaseTime = value * MaximumTime;

            foreach (var envelope in envelopes)
                envelope.ReleaseTime = releaseTime;
        }

        private void SetAttackCurve(float value)
        {
            attackCurve = value;

            foreach (var envelope in envelopes)
                envelope.AttackCurve = value;
        }

        private void SetDecayReleaseCurve(float value)
        {
            decayReleaseCurve = value;

            foreach (var envelope in envelopes)
                envelope.DecayReleaseCurve = value;
        }

        private void SetEnvelopeAmplitude(float value)
        {
            envelopeAmplitude = value;

            foreach (var envelope in envelopes)
                envelope.Amplitude = value;
        }
    }
}
