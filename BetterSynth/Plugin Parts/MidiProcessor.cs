using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using System;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация интерефейса IVstMidiProcessor.
    /// </summary>
    class MidiProcessor : IVstMidiProcessor
    {
        /// <summary>
        /// Ссылка на плагин, которому принадлежит этот компонент.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Инициализирует новых объект типа MidiProcessor, принадлежащий заданному плагину.
        /// </summary>
        /// <param name="plugin"></param>
        public MidiProcessor(Plugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Количество каналов.
        /// </summary>
        public int ChannelCount => 16;

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="events"></param>
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

        /// <summary>
        /// Событие, возникающее при отпускании клавиши.
        /// </summary>
        public event EventHandler<MidiNoteEventArgs> NoteOff;

        /// <summary>
        /// Событие, возникающее при нажатии клавиши.
        /// </summary>
        public event EventHandler<MidiNoteEventArgs> NoteOn;

        /// <summary>
        /// Текущее количество нажатых клавиш.
        /// </summary>
        private int pressedNotesCount = 0;

        /// <summary>
        /// Метод, вызывающий событие NoteOff с заданными параметрами.
        /// </summary>
        /// <param name="noteNo"></param>
        /// <param name="velocity"></param>
        private void OnNoteOff(byte noteNo, byte velocity)
        {
            NoteOff?.Invoke(this, new MidiNoteEventArgs(noteNo, velocity, pressedNotesCount));
        }

        /// <summary>
        /// Метод, вызывающий событие NoteOn с заданными параметрами.
        /// </summary>
        /// <param name="noteNo"></param>
        /// <param name="velocity"></param>
        private void OnNoteOn(byte noteNo, byte velocity)
        {
            NoteOn?.Invoke(this, new MidiNoteEventArgs(noteNo, velocity, pressedNotesCount));
        }

        /// <summary>
        /// Метод, "нажимающий" клавишу из UI.
        /// </summary>
        /// <param name="noteNo"></param>
        /// <param name="velocity"></param>
        public void PressNoteFromUI(byte noteNo, byte velocity)
        {
            plugin.AudioProcessor.ProcessingMutex.WaitOne();
            OnNoteOn(noteNo, velocity);
            plugin.AudioProcessor.ProcessingMutex.ReleaseMutex();
        }

        /// <summary>
        /// Метод, "отпускающий" клавишу из UI.
        /// </summary>
        /// <param name="noteNo"></param>
        /// <param name="velocity"></param>
        public void ReleaseNoteFromUI(byte noteNo, byte velocity)
        {
            plugin.AudioProcessor.ProcessingMutex.WaitOne();
            OnNoteOff(noteNo, velocity);
            plugin.AudioProcessor.ProcessingMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Аргумент событий MidiProcessor.NoteOn и MidiProcessor.NoteOff.
    /// </summary>
    public class MidiNoteEventArgs : EventArgs
    {
        /// <summary>
        /// Инициализирует новый объект типа MidiNoteEventArgs с заданными параметрами.
        /// </summary>
        /// <param name="noteNo"></param>
        /// <param name="velocity"></param>
        /// <param name="pressedNotesCount"></param>
        public MidiNoteEventArgs(byte noteNo, byte velocity, int pressedNotesCount)
        {
            Note = new MidiNote { NoteNo = noteNo, Velocity = velocity };
            PressedNotesCount = pressedNotesCount;
        }

        /// <summary>
        /// Количество нажатых нот.
        /// </summary>
        public int PressedNotesCount { get; protected set; }

        /// <summary>
        /// Играемая нота.
        /// </summary>
        public MidiNote Note { get; protected set; }
    }

    /// <summary>
    /// Представляет собой одну ноту.
    /// </summary>
    public struct MidiNote
    {
        /// <summary>
        /// Номер ноты.
        /// </summary>
        public byte NoteNo { get; set; }
        
        /// <summary>
        /// Сила нажатия клавиши.
        /// </summary>
        public byte Velocity { get; set; }
    }
}
