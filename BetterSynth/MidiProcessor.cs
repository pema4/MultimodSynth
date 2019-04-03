using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using System;

namespace BetterSynth
{
    internal class MidiProcessor : IVstMidiProcessor
    {
        private Plugin plugin;

        public MidiProcessor(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public int ChannelCount => 16;

        public void Process(VstEventCollection events)
        {
            foreach (var e in events)
            {
                if (e.EventType == VstEventTypes.MidiEvent)
                {
                    var midiEvent = (VstMidiEvent)e;
                    byte firstByte = midiEvent.Data[0];

                    if (0x80 <= firstByte && firstByte < 0x90) // Note off
                    {
                        pressedNotesCount -= 1;
                        OnNoteOff(midiEvent.Data[1], midiEvent.Data[2]);
                    }

                    else if (0x90 <= firstByte && firstByte < 0xA0) // Note on
                    {
                        pressedNotesCount += 1;
                        OnNoteOn(midiEvent.Data[1], midiEvent.Data[2]);
                    }
                }
            }
        }

        public event EventHandler<MidiNoteEventArgs> NoteOff;

        public event EventHandler<MidiNoteEventArgs> NoteOn;

        private int pressedNotesCount = 0;

        protected void OnNoteOff(byte noteNo, byte velocity)
        {
            NoteOff?.Invoke(this, new MidiNoteEventArgs(noteNo, velocity, pressedNotesCount));
        }

        protected void OnNoteOn(byte noteNo, byte velocity)
        {
            NoteOn?.Invoke(this, new MidiNoteEventArgs(noteNo, velocity, pressedNotesCount));
        }
    }

    public class MidiNoteEventArgs : EventArgs
    {
        public MidiNoteEventArgs(byte noteNo, byte velocity, int pressedNotesCount)
        {
            Note = new MidiNote { NoteNo = noteNo, Velocity = velocity };
            PressedNotesCount = pressedNotesCount;
        }

        public int PressedNotesCount { get; protected set; }

        public MidiNote Note { get; protected set; }
    }

    public struct MidiNote
    {
        public byte NoteNo { get; set; }
        public byte Velocity { get; set; }

        public static MidiNote Empty => new MidiNote();
    }
}
