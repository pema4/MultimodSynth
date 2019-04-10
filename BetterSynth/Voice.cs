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

            float envAOut, envBOut;
            float oscMix = 0;
            switch (ModulationType)
            {
                case ModulationType.None:
                    envAOut = envA.Process();
                    envBOut = envB.Process();
                    if (envAOut == 0 && envBOut == 0)
                        goto default;
                    if (envAOut != 0)
                        oscMix += oscA.Process() * envAOut;
                    if (envBOut != 0)
                        oscMix += oscB.Process() * envBOut;
                    break;

                case ModulationType.AmplitudeModulationA:
                    envAOut = envA.Process();
                    if (envAOut != 0)
                    {
                        envBOut = envB.Process();
                        var mod = envBOut == 0 ? 0 : oscB.Process() * envBOut;
                        oscMix = oscA.Process() * envAOut * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.AmplitudeModulationB:
                    envBOut = envB.Process();
                    if (envBOut != 0)
                    {
                        envAOut = envA.Process();
                        var mod = envAOut == 0 ? 0 : oscA.Process() * envAOut;
                        oscMix = oscB.Process() * envBOut * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationA:
                    envAOut = envA.Process();
                    if (envAOut != 0)
                    {
                        envBOut = envB.Process();
                        var mod = envBOut == 0 ? 0 : fmAmountMultiplier * oscB.Process() * envBOut;
                        oscMix = oscA.Process(phaseModulation: mod) * envAOut;
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationB:
                    envBOut = envB.Process();
                    if (envBOut != 0)
                    {
                        envAOut = envA.Process();
                        var mod = envAOut == 0 ? 0 : fmAmountMultiplier * oscA.Process() * envAOut;
                        oscMix = oscB.Process(phaseModulation: mod) * envBOut;
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
            return noteVolume * filter.Process(oscMix, filterEnvOut);
        }

        public event EventHandler SoundStop;

        private void OnSoundStop() =>
            SoundStop?.Invoke(this, new EventArgs());
    }
}
