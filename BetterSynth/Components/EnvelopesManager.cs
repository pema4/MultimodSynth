using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    class EnvelopesManager : AudioComponentWithParameters
    {
        private const int MaximumTime = 10;
        
        private List<Envelope> envelopes;
        private float sustainLevel;
        private float envelopeAmplitude;
        private ParameterFilter envelopeAmplitudeFilter;
        private float decayTime;
        private float attackTime;
        private float releaseTime;
        private float attackCurve;
        private float decayReleaseCurve;

        public VstParameterManager AttackTimeManager { get; private set; }

        public VstParameterManager DecayTimeManager { get; private set; }

        public VstParameterManager SustainLevelManager { get; private set; }

        public VstParameterManager ReleaseTimeManager { get; private set; }

        public VstParameterManager AttackCurveManager { get; private set; }

        public VstParameterManager DecayReleaseCurveManager { get; private set; }

        public VstParameterManager EnvelopeAmplitudeManager { get; private set; }

        public EnvelopesManager(
            Plugin plugin,
            string parameterPrefix = "E",
            string parameterCategory = "env")
            : base(plugin, parameterPrefix, parameterCategory)
        {
            envelopes = new List<Envelope>();
            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            AttackTimeManager = factory.CreateParameterManager(
                name: "AT",
                defaultValue: 0.001f,
                valueChangedHandler: SetAttackTime);

            DecayTimeManager = factory.CreateParameterManager(
                name: "DT",
                valueChangedHandler: SetDecayTime);

            SustainLevelManager = factory.CreateParameterManager(
                name: "SL",
                defaultValue: 1,
                valueChangedHandler: SetSustainLevel);

            ReleaseTimeManager = factory.CreateParameterManager(
                name: "RT",
                defaultValue: 0.001f,
                valueChangedHandler: SetReleaseTime);

            AttackCurveManager = factory.CreateParameterManager(
                name: "AC",
                defaultValue: 1,
                valueChangedHandler: SetAttackCurve);

            DecayReleaseCurveManager = factory.CreateParameterManager(
                name: "DRC",
                valueChangedHandler: SetDecayReleaseCurve);

            EnvelopeAmplitudeManager = factory.CreateParameterManager(
                name: "AMP",
                defaultValue: 1f,
                valueChangedHandler: x => envelopeAmplitudeFilter.SetTarget(x));
            envelopeAmplitudeFilter = new ParameterFilter(UpdateEnvelopeAmplitude, 1);
        }
        
        private void SetAttackTime(float value)
        {
            attackTime = (float)Converters.ToEnvelopeTime(value);
            
            foreach (var envelope in envelopes)
                envelope.SetAttackTime(attackTime);
        }

        private void SetDecayTime(float value)
        {
            decayTime = (float)Converters.ToEnvelopeTime(value);

            foreach (var envelope in envelopes)
                envelope.SetDecayTime(decayTime);
        }

        private void SetSustainLevel(float value)
        {
            sustainLevel = value;

            foreach (var envelope in envelopes)
                envelope.SetSustainLevel(value);
        }

        private void SetReleaseTime(float value)
        {
            releaseTime = (float)Converters.ToEnvelopeTime(value);

            foreach (var envelope in envelopes)
                envelope.SetReleaseTime(releaseTime);
        }

        private void SetAttackCurve(float value)
        {
            attackCurve = value;

            foreach (var envelope in envelopes)
                envelope.SetAttackCurve(value);
        }

        private void SetDecayReleaseCurve(float value)
        {
            decayReleaseCurve = value;

            foreach (var envelope in envelopes)
                envelope.SetDecayReleaseCurve(value);
        }

        private void UpdateEnvelopeAmplitude(float value)
        {
            envelopeAmplitude = value;
            foreach (var envelope in envelopes)
                envelope.SetAmplitude(envelopeAmplitude);
        }

        public Envelope CreateNewEnvelope()
        {
            var envelope = new Envelope()
            {
            };
            envelope.SetAttackTime(attackTime);
            envelope.SetDecayTime(decayTime);
            envelope.SetSustainLevel(sustainLevel);
            envelope.SetReleaseTime(releaseTime);
            envelope.SetAttackCurve(attackCurve);
            envelope.SetDecayReleaseCurve(decayReleaseCurve);
            envelope.SetAmplitude(envelopeAmplitude);
            envelopes.Add(envelope);
            return envelope;
        }

        public void RemoveEnvelope(Envelope envelope)
        {
            envelopes.Remove(envelope);
        }
        
        public void Process()
        {
            envelopeAmplitudeFilter.Process();
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            foreach (var envelope in envelopes)
                envelope.SampleRate = newSampleRate;
        }
    }
}
