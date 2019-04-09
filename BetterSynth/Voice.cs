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
        private AdsrEnvelope oscAEnvelope;
        private AdsrEnvelope oscBEnvelope;
        private AdsrEnvelope filterEnvelope;
        private float noteVolume;
        private bool isActive;

        public Voice(
            Plugin plugin,
            Oscillator oscA,
            Oscillator oscB,
            Filter filter,
            AdsrEnvelope oscAEnvelope,
            AdsrEnvelope oscBEnvelope,
            AdsrEnvelope filterEnvelope)
        {
            this.plugin = plugin;
            this.oscA = oscA;
            this.oscB = oscB;
            this.filter = filter;
            this.oscAEnvelope = oscAEnvelope;
            this.oscBEnvelope = oscBEnvelope;
            this.filterEnvelope = filterEnvelope;
        }

        public MidiNote Note { get; private set; }

        public ModulationType ModulationType { get; set; }

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

            oscAEnvelope.TriggerAttack();
            oscBEnvelope.TriggerAttack();
            filterEnvelope.TriggerAttack();

            isActive = true;
        }

        public void TriggerRelease()
        {
            oscAEnvelope.TriggerRelease();
            oscBEnvelope.TriggerRelease();
            filterEnvelope.TriggerRelease();
        }

        public float Process()
        {
            if (!isActive)
                return 0;

            var envAOut = oscAEnvelope.Process();
            var envBOut = oscBEnvelope.Process();

            if (envAOut == 0 && envBOut == 0)
            {
                isActive = false;
                OnSoundStop();
                return 0;
            }

            float oscMix = 0;
            switch (ModulationType)
            {
                case ModulationType.None:
                    if (envAOut != 0)
                        oscMix += oscA.Process() * envAOut;
                    if (envBOut != 0)
                        oscMix += oscB.Process() * envBOut;
                    break;

                case ModulationType.AmplitudeModulationA:
                    if (envAOut != 0)
                    {
                        var mod = envBOut == 0 ? 0 : oscB.Process() * envBOut;
                        oscMix = oscA.Process() * envAOut * (1 + mod);
                    }
                    break;

                case ModulationType.AmplitudeModulationB:
                    if (envBOut != 0)
                    {
                        var mod = envAOut == 0 ? 0 : oscA.Process() * envAOut;
                        oscMix = oscB.Process() * envBOut * (1 + mod);
                    }
                    break;

                case ModulationType.FrequencyModulationA:
                    if (envAOut != 0)
                    {
                        var mod = envBOut == 0 ? 0 : 0.01f * oscB.Process() * envBOut;
                        oscMix = oscA.Process(phaseModulation: mod) * envAOut;
                    }
                    break;

                case ModulationType.FrequencyModulationB:
                    if (envBOut != 0)
                    {
                        var mod = envAOut == 0 ? 0 : 0.01f * oscA.Process() * envAOut;
                        oscMix = oscB.Process(phaseModulation: mod) * envBOut;
                    }
                    break;
            }

            var filterEnvOut = filterEnvelope.Process();

            return filter.Process(oscMix, filterEnvOut);
        }

        public event EventHandler SoundStop;

        private void OnSoundStop() =>
            SoundStop?.Invoke(this, new EventArgs());
    }
}
