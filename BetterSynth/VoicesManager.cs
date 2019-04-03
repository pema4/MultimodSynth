using System;
using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;
using System.Windows.Forms;
/*
* voice = new Voice(plugin);
voice.Osc = new WaveTablePlayer(plugin);
voice.VolumeEnvelope = new AdsrEnvelope()
{
AttackRate = 44100,
DecayRate = 44100,
ReleaseRate = 44100,
SustainLevel = 0.5f,
AttackTargetRatio = 0.3f,
DecayReleaseTargetRatio = 0.0001f,
};
*/
namespace BetterSynth
{
    class VoicesManager
    {
        #region vst parameters initialization

        public VstParameterManager AttackTime { get; private set; }

        public VstParameterManager DecayTime { get; private set; }

        public VstParameterManager SustainLevel { get; private set; }
        
        public VstParameterManager ReleaseTime { get; private set; }

        public VstParameterManager AttackCurve { get; private set; }

        public VstParameterManager DecayReleaseCurve { get; private set; }

        public VstParameterManager WaveTablePosition { get; private set; }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "voice");

            AttackTime = factory.CreateParameterManager(
                name: "VOL_ATT",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0.5f,
                valueChangedHandler: setAttackTime);

            DecayTime = factory.CreateParameterManager(
                name: "VOL_DEC",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0,
                valueChangedHandler: setDecayTime);

            SustainLevel = factory.CreateParameterManager(
                name: "VOL_SUS",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setSustainLevel);

            ReleaseTime = factory.CreateParameterManager(
                name: "VOL_REL",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0.5f,
                valueChangedHandler: setReleaseTime);

            AttackCurve = factory.CreateParameterManager(
                name: "VOL_A_C",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setAttackCurve);

            DecayReleaseCurve = factory.CreateParameterManager(
                name: "VOL_DR_C",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setDecayReleaseCurve);

            WaveTablePosition = factory.CreateParameterManager(
                name: "WT_POS",
                minValue: 0,
                maxValue: 1,
                defaultValue: 0.7f,
                valueChangedHandler: setWaveTablePosition);
        }

        private void setAttackTime(float value)
        {
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.AttackRate = rate;
        }

        private void setDecayTime(float value)
        {
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.DecayRate = rate;
        }

        private void setSustainLevel(float level)
        {
            foreach (var voice in voices)
                voice.VolumeEnvelope.SustainLevel = level;
        }

        private void setReleaseTime(float value)
        {
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.ReleaseRate = rate;
        }

        private void setAttackCurve(float value)
        {
            float targetRatio = 0.001f * ((float)Math.Exp(12 * (0.001f + 0.999f * value)) - 1);
            foreach (var voice in voices)
                voice.VolumeEnvelope.AttackTargetRatio = targetRatio;
            MessageBox.Show($"set attack curve: {targetRatio}");
        }

        private void setDecayReleaseCurve(float value)
        {
            float targetRatio = 0.001f * ((float)Math.Exp(12 * (0.001f + 0.999f * value)) - 1);
            foreach (var voice in voices)
                voice.VolumeEnvelope.DecayReleaseTargetRatio = targetRatio;
        }

        private void setWaveTablePosition(float pos)
        {
            foreach (var voice in voices)
                voice.Osc.WaveTablePos = pos;
        }

        #endregion vst parameters initialization

        private Plugin plugin;
        private Voice[] voices;
        private int voicesCount;

        public VoicesManager(Plugin plugin)
        {
            this.plugin = plugin;
            VoicesCount = 8;
            InitializeParameters();
        }

        public int VoicesCount
        {
            get => voicesCount;
            set
            {
                voicesCount = value;
                voices = new Voice[value];
                for (int i = 0; i < voices.Length; ++i)
                    voices[i] = CreateVoice();
                freeVoices = new SortedSet<int>(Enumerable.Range(0, value));
                noteToVoiceMapping = new Dictionary<MidiNote, int>();
                pressedNotes = new List<MidiNote>();
            }
        }

        private Voice CreateVoice()
        {
            var voice = new Voice(plugin, x => RemoveNote(x.Note));
            return voice;
        }

        private SortedSet<int> freeVoices = new SortedSet<int>();
        private Dictionary<MidiNote, int> noteToVoiceMapping = new Dictionary<MidiNote, int>();
        private List<MidiNote> pressedNotes = new List<MidiNote>();

        internal void PlayNote(MidiNote note)
        {
            int voiceIndex;

            if (freeVoices.Count == 0)
            {
                voiceIndex = noteToVoiceMapping[pressedNotes[0]];
                RemoveNote(pressedNotes[0]);
            }
            else
            {
                voiceIndex = freeVoices.Min;
                freeVoices.Remove(voiceIndex);
            }

            noteToVoiceMapping[note] = voiceIndex;
            pressedNotes.Add(note);
            voices[voiceIndex].PlayNote(note);
        }

        private void RemoveNote(MidiNote note)
        {
            if (noteToVoiceMapping.ContainsKey(note))
            {
                int voiceIndex = noteToVoiceMapping[note];
                freeVoices.Add(voiceIndex);
                noteToVoiceMapping.Remove(note);
                pressedNotes.Remove(note);
            }
        }

        internal void ReleaseNote(MidiNote note)
        {
            if (noteToVoiceMapping.ContainsKey(note))
            {
                int voiceIndex = noteToVoiceMapping[note];
                voices[voiceIndex].Release();
            }
        }

        internal void Process(out float output)
        {
            output = voices.Select(voice => { voice.Process(out float x); return x; })
                           .Sum();
        }
    }
}
