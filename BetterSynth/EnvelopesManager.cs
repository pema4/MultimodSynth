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
        private float envelopeAmplitudeTarget;
        private bool isEnvelopeAmplitudeChanging;
        private OnePoleLowpassFilter envelopeAmplitudeFilter = new OnePoleLowpassFilter();
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
                maxValue: MaximumTime,
                defaultValue: 0.01f,
                valueChangedHandler: SetAttackTime);
            CreateRedirection(AttackTimeManager, nameof(AttackTimeManager));

            DecayTimeManager = factory.CreateParameterManager(
                name: "DT",
                maxValue: MaximumTime,
                valueChangedHandler: SetDecayTime);
            CreateRedirection(DecayTimeManager, nameof(DecayTimeManager));

            SustainLevelManager = factory.CreateParameterManager(
                name: "SL",
                defaultValue: 1,
                valueChangedHandler: SetSustainLevel);
            CreateRedirection(SustainLevelManager, nameof(SustainLevelManager));

            ReleaseTimeManager = factory.CreateParameterManager(
                name: "RT",
                maxValue: MaximumTime,
                defaultValue: 0.01f,
                valueChangedHandler: SetReleaseTime);
            CreateRedirection(ReleaseTimeManager, nameof(ReleaseTimeManager));

            AttackCurveManager = factory.CreateParameterManager(
                name: "AC",
                valueChangedHandler: SetAttackCurve);
            CreateRedirection(AttackCurveManager, nameof(AttackCurveManager));

            DecayReleaseCurveManager = factory.CreateParameterManager(
                name: "DRC",
                valueChangedHandler: SetDecayReleaseCurve);
            CreateRedirection(DecayReleaseCurveManager, nameof(DecayReleaseCurveManager));

            EnvelopeAmplitudeManager = factory.CreateParameterManager(
                name: "AMP",
                defaultValue: 1f,
                valueChangedHandler: SetEnvelopeAmplitude);
            CreateRedirection(EnvelopeAmplitudeManager, nameof(EnvelopeAmplitudeManager));
        }
        
        private void SetAttackTime(float value)
        {
            attackTime = value;
            
            foreach (var envelope in envelopes)
                envelope.AttackTime = attackTime;
        }

        private void SetDecayTime(float value)
        {
            decayTime = value;

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
            releaseTime = value;

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
            envelopeAmplitudeTarget = value;
            isEnvelopeAmplitudeChanging = true;
        }

        public Envelope CreateNewEnvelope()
        {
            var envelope = new Envelope()
            {
                Amplitude = envelopeAmplitudeTarget,
                AttackTime = attackTime,
                DecayTime = decayTime,
                SustainLevel = sustainLevel,
                ReleaseTime = releaseTime,
                AttackCurve = attackCurve,
                DecayReleaseCurve = decayReleaseCurve
            };
            envelopes.Add(envelope);
            return envelope;
        }

        public void RemoveEnvelope(Envelope envelope)
        {
            envelopes.Remove(envelope);
        }

        private void UpdateEnvelopeAmplitude()
        {
            var newValue = envelopeAmplitudeFilter.Process(envelopeAmplitudeTarget);
            if (newValue != envelopeAmplitude)
            {
                envelopeAmplitude = newValue;
                foreach (var envelope in envelopes)
                    envelope.Amplitude = envelopeAmplitude;
            }
            else
                isEnvelopeAmplitudeChanging = false;
        }

        public void Process()
        {
            if (isEnvelopeAmplitudeChanging)
                UpdateEnvelopeAmplitude();
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            foreach (var envelope in envelopes)
                envelope.SampleRate = newSampleRate;
        }
    }
}
