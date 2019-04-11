using System;

namespace BetterSynth
{
    public enum ModulationType
    {
        None,
        FrequencyModulationA,
        FrequencyModulationB,
        AmplitudeModulationA,
        AmplitudeModulationB,
    }

    class Voice
    {
        private Plugin plugin;
        private Oscillator oscA;
        private Oscillator oscB;
        private Filter filter;
        private AdsrEnvelope envA;
        private AdsrEnvelope envB;
        private AdsrEnvelope envFilter;
        private float noteVolume;
        private float sampleRate;
        private float fmAmountMultiplier;

        public Voice(
            Plugin plugin,
            Oscillator oscA,
            Oscillator oscB,
            Filter filter,
            AdsrEnvelope envA,
            AdsrEnvelope envB,
            AdsrEnvelope envFilter)
        {
            this.plugin = plugin;
            this.oscA = oscA;
            this.oscB = oscB;
            this.filter = filter;
            this.envA = envA;
            this.envB = envB;
            this.envFilter = envFilter;
        }

        public bool IsActive { get; private set; }

        public MidiNote Note { get; private set; }

        public ModulationType ModulationType { get; set; }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    fmAmountMultiplier = 5000 / SampleRate;
                    oscA.SampleRate = sampleRate;
                    oscB.SampleRate = sampleRate;
                    filter.SampleRate = sampleRate;
                    envA.SampleRate = sampleRate;
                    envB.SampleRate = sampleRate;
                    envFilter.SampleRate = sampleRate;
                }
            }
        }

        public void PlayNote(MidiNote note)
        {
            Note = note;
            noteVolume = note.Velocity / 128f;
            var noteFrequency = (float)Utilities.MidiNoteToFrequency(note.NoteNo);

            oscA.ResetPhase();
            oscB.ResetPhase();
            filter.Reset();

            oscA.NoteFrequency = noteFrequency;
            oscB.NoteFrequency = noteFrequency;
            filter.NoteFrequency = noteFrequency;

            envA.TriggerAttack();
            envB.TriggerAttack();
            envFilter.TriggerAttack();

            IsActive = true;
        }

        public void TriggerRelease()
        {
            envA.TriggerRelease();
            envB.TriggerRelease();
            envFilter.TriggerRelease();
        }

        public float Process()
        {
            if (!IsActive)
                return 0;
            
            float oscMix = 0;
            switch (ModulationType)
            {
                case ModulationType.None:
                    if (envA.State != AdsrEnvelopeState.Idle)
                    {
                        oscMix += envA.Process() * oscA.Process();
                        if (envB.State != AdsrEnvelopeState.Idle)
                            oscMix += envB.Process() * oscB.Process();
                    }
                    else
                    {
                        if (envB.State != AdsrEnvelopeState.Idle)
                            oscMix = envB.Process() * oscB.Process();
                        else
                            goto default;
                    }
                    break;

                case ModulationType.AmplitudeModulationA:
                    if (envA.State != 0)
                    {
                        float mod = 0;
                        if (envB.State != AdsrEnvelopeState.Idle)
                            mod = oscB.Process() * envB.Process();
                        oscMix = oscA.Process() * envA.Process() * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.AmplitudeModulationB:
                    if (envB.State != 0)
                    {
                        float mod = 0;
                        if (envA.State != AdsrEnvelopeState.Idle)
                            mod = oscA.Process() * envA.Process();
                        oscMix = oscB.Process() * envB.Process() * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationA:
                    if (envA.State != AdsrEnvelopeState.Idle)
                    {
                        float mod = 0;
                        if (envB.State != AdsrEnvelopeState.Idle)
                            mod = fmAmountMultiplier * oscB.Process() * envB.Process();
                        oscMix = oscA.Process(phaseModulation: mod) * envA.Process();
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationB:
                    if (envB.State != AdsrEnvelopeState.Idle)
                    {
                        float mod = 0;
                        if (envA.State != AdsrEnvelopeState.Idle)
                            mod = fmAmountMultiplier * oscA.Process() * envA.Process();
                        oscMix = oscB.Process(phaseModulation: mod) * envB.Process();
                    }
                    else
                        goto default;
                    break;

                default:
                    IsActive = false;
                    OnSoundStop();
                    return 0;
            }

            var filterEnvOut = envFilter.Process();
            return filter.Process(oscMix, filterEnvOut);
        }

        public event EventHandler SoundStop;

        private void OnSoundStop() =>
            SoundStop?.Invoke(this, new EventArgs());
    }
}
