using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    class Voice
    {
        private Plugin plugin;
        private float noteVolume;

        public MidiNote Note { get; private set; }

        public WaveTablePlayer Osc { get; set; }

        public AdsrEnvelope VolumeEnvelope { get; set; }

        public Voice(Plugin plugin, Action<Voice> onSoundStop)
        {
            this.plugin = plugin;
            Osc = new WaveTablePlayer(plugin);
            VolumeEnvelope = new AdsrEnvelope();
            VolumeEnvelope.SoundStop += (sender, e) => Osc.Bypass = true;
            VolumeEnvelope.SoundStop += (sender, e) => onSoundStop(this);
        }

        public void PlayNote(MidiNote note)
        {
            Note = note;
            noteVolume = note.Velocity / 128f;
            Osc.Bypass = false;
            Osc.Frequency = (float)CommonFunctions.MidiNoteToFrequency(note.NoteNo);
            VolumeEnvelope.TriggerAttack();
        }

        public void Release()
        {
            VolumeEnvelope.TriggerRelease();
        }

        public void Process(out float output)
        {
            if (Osc.Bypass)
            {
                output = 0;
            }
            else
            {
                Osc.Process(out var oscOutput);
                VolumeEnvelope.Process(out var volEnvOutput);
                output = oscOutput * volEnvOutput * noteVolume;
            }
        }
    }
}
