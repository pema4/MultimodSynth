using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    internal class Routing
    {
        private Plugin plugin;

        public VoicesManager VoicesManager { get; private set; }

        public Routing(Plugin plugin)
        {
            this.plugin = plugin;
            plugin.MidiProcessor.NoteOn += MidiProcessor_NoteOn;
            plugin.MidiProcessor.NoteOff += MidiProcessor_NoteOff;
            
            VoicesManager = new VoicesManager(plugin);
        }
        

        private void MidiProcessor_NoteOn(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.PlayNote(e.Note);
        }

        private void MidiProcessor_NoteOff(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.ReleaseNote(e.Note);
        }

        public void Process(out float left, out float right)
        {
            VoicesManager.Process(out var output);
            left = output;
            right = output;
        }
    }
}