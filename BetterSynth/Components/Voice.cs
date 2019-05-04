using System;

namespace BetterSynth
{
    class Voice : AudioComponent
    {
        public enum ModulationType
        {
            None,
            FrequencyModulationA,
            FrequencyModulationB,
            AmplitudeModulationA,
            AmplitudeModulationB,
        }

        private Oscillator oscA;
        private Oscillator oscB;
        private Filter filter;
        private Envelope envA;
        private Envelope envB;
        private Envelope envFilter;
        private float noteVolume;
        private float fmAmountMultiplier;

        public Voice(
            Plugin plugin,
            Oscillator oscA,
            Oscillator oscB,
            Filter filter,
            Envelope envA,
            Envelope envB,
            Envelope envFilter)
        {
            this.oscA = oscA;
            this.oscB = oscB;
            this.filter = filter;
            this.envA = envA;
            this.envB = envB;
            this.envFilter = envFilter;
        }

        public bool IsActive { get; private set; }

        public MidiNote Note { get; private set; }

        public ModulationType Modulation { get; set; }

        public void PlayNote(MidiNote note)
        {
            Note = note;
            noteVolume = note.Velocity / 128f;
            var noteFrequency = (float)Utilities.MidiNoteToFrequency(note.NoteNo);

            oscA.Reset();
            oscB.Reset();
            filter.Reset();

            oscA.SetNoteFrequency(noteFrequency);
            oscB.SetNoteFrequency(noteFrequency);
            filter.SetNoteFrequency(noteFrequency);

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
            switch (Modulation)
            {
                case ModulationType.None:
                    if (envA.IsActive)
                    {
                        oscMix += envA.Process() * oscA.Process();
                        if (envB.IsActive)
                            oscMix += envB.Process() * oscB.Process();
                    }
                    else
                    {
                        if (envB.IsActive)
                            oscMix = envB.Process() * oscB.Process();
                        else
                            goto default;
                    }
                    break;

                case ModulationType.AmplitudeModulationA:
                    if (envA.IsActive)
                    {
                        float mod = 0;
                        if (envB.IsActive)
                            mod = oscB.Process() * envB.Process();
                        oscMix = oscA.Process() * envA.Process() * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.AmplitudeModulationB:
                    if (envB.IsActive)
                    {
                        float mod = 0;
                        if (envA.IsActive)
                            mod = oscA.Process() * envA.Process();
                        oscMix = oscB.Process() * envB.Process() * (1 + mod);
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationA:
                    if (envA.IsActive)
                    {
                        float mod = 0;
                        if (envB.IsActive)
                            mod = 10 * oscB.Process() * envB.Process();
                        oscMix = oscA.Process(phaseModulation: mod) * envA.Process();
                    }
                    else
                        goto default;
                    break;

                case ModulationType.FrequencyModulationB:
                    if (envB.IsActive)
                    {
                        float mod = 0;
                        if (envA.IsActive)
                            mod = 10 * oscA.Process() * envA.Process();
                        oscMix = oscB.Process(phaseModulation: mod) * envB.Process();
                    }
                    else
                        goto default;
                    break;

                default:
                    IsActive = false;
                    //OnSoundStop();
                    return 0;
            }

            var filterEnvOut = envFilter.Process();
            return noteVolume * filter.Process(oscMix, filterEnvOut);
        }

        public event EventHandler SoundStop;

        private void OnSoundStop() =>
            SoundStop?.Invoke(this, new EventArgs());

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            fmAmountMultiplier = 5000 / SampleRate;
            oscA.SampleRate = newSampleRate;
            oscB.SampleRate = newSampleRate;
            filter.SampleRate = newSampleRate;
            envA.SampleRate = newSampleRate;
            envB.SampleRate = newSampleRate;
            envFilter.SampleRate = newSampleRate;
        }
    }
}
