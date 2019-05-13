namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, представляющий собой один голос.
    /// </summary>
    class Voice : AudioComponent
    {
        /// <summary>
        /// Указывает тип взаимодействия двух осцилляторов.
        /// </summary>
        public enum ModulationType
        {
            None,
            PhaseModulationA,
            PhaseModulationB,
            AmplitudeModulationA,
            AmplitudeModulationB,
        }

        /// <summary>
        /// Осциллятор A.
        /// </summary>
        private Oscillator oscA;

        /// <summary>
        /// Осциллятор B.
        /// </summary>
        private Oscillator oscB;

        /// <summary>
        /// Фильтр.
        /// </summary>
        private Filter filter;

        /// <summary>
        /// Огибающая громкости осциллятора A.
        /// </summary>
        private Envelope envA;

        /// <summary>
        /// Огибающая громкости осциллятора B.
        /// </summary>
        private Envelope envB;

        /// <summary>
        /// Огибающая частоты среза фильтра.
        /// </summary>
        private Envelope envFilter;

        /// <summary>
        /// Сила нажатия текущей играемой ноты.
        /// </summary>
        private float noteVelocity;

        /// <summary>
        /// Максимальная "сила" частотной модуляции.
        /// </summary>
        private float fmAmountMultiplier;

        /// <summary>
        /// Указывает, активен ли данный голос.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Текущая играемая нота.
        /// </summary>
        public MidiNote Note { get; private set; }

        /// <summary>
        /// Текущий тип модуляции.
        /// </summary>
        public ModulationType Modulation { get; set; }

        /// <summary>
        /// Инициализирует новый объект класса Voice, имеющий переданные компоненты.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="oscA">Осциллятор А.</param>
        /// <param name="oscB">Осциллятор B.</param>
        /// <param name="filter">Фильтр.</param>
        /// <param name="envA">Огибающая уровня осциллятора A.</param>
        /// <param name="envB">Огибающая уровня оциллятора B.</param>
        /// <param name="envFilter">Огибающая частоты среза фильтра.</param>
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

        /// <summary>
        /// Играет переданную ноту.
        /// </summary>
        /// <param name="note">Нота, которую необходимо проиграть.</param>
        public void PlayNote(MidiNote note)
        {
            Note = note;
            noteVelocity = note.Velocity / 128f;
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

        /// <summary>
        /// Прекращает проигрывание ноты.
        /// </summary>
        public void TriggerRelease()
        {
            envA.TriggerRelease();
            envB.TriggerRelease();
            envFilter.TriggerRelease();
        }

        /// <summary>
        /// Генерация новых выходных данных.
        /// </summary>
        /// <returns>Выходной сигнал.</returns>
        public float Process()
        {
            if (!IsActive)
                return 0;
            
            float oscMix = 0; // Здесь хранится результат
            switch (Modulation)
            {
                case ModulationType.None:
                    if (envA.IsActive) // Если первый осциллятор активен, то
                    {
                        oscMix += envA.Process() * oscA.Process(); // Прибавляем сигнал первого осциллятора
                        if (envB.IsActive) // Если второй осциллятор активен
                            oscMix += envB.Process() * oscB.Process(); // Прибавляем сигнал второго осциллятора
                    }
                    else 
                    {
                        if (envB.IsActive) // Если второй осциллятор активен, то
                            oscMix = envB.Process() * oscB.Process(); // Прибавляем сигнал второго осциллятора
                        else
                        {
                            // Оба осциллятора неактивны, этот голос прекратил производить сигнал
                            IsActive = false;
                            return 0;
                        }
                    }
                    break;

                case ModulationType.AmplitudeModulationA:
                    if (envA.IsActive) // Если несущий осциллятор активен, то
                    {
                        float mod = 0;
                        if (envB.IsActive) // Если модулирующий осциллятор активен, то
                            mod = oscB.Process() * envB.Process();
                        oscMix = oscA.Process() * envA.Process() * (1 + mod);
                    }
                    else
                    {
                        // Несущий осциллятор не активен, этот голос прекратил производить сигнал.
                        IsActive = false;
                        return 0;
                    }
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
                    {
                        IsActive = false;
                        return 0;
                    }
                    break;

                case ModulationType.PhaseModulationA:
                    if (envA.IsActive) // Если несущий осциллятор активен
                    {
                        float mod = 0;
                        if (envB.IsActive) // Если модулирующий осциллятор активен
                            mod = 10 * oscB.Process() * envB.Process(); // Сдвиг фазы несущего сигнала.
                        oscMix = oscA.Process(phaseModulation: mod) * envA.Process();
                    }
                    else
                    {
                        IsActive = false;
                        return 0;
                    }
                    break;

                case ModulationType.PhaseModulationB:
                    if (envB.IsActive)
                    {
                        float mod = 0;
                        if (envA.IsActive)
                            mod = 10 * oscA.Process() * envA.Process();
                        oscMix = oscB.Process(phaseModulation: mod) * envB.Process();
                    }
                    else
                    {
                        IsActive = false;
                        return 0;
                    }
                    break;
            }

            var filterEnvOut = envFilter.Process();
            return noteVelocity * filter.Process(oscMix, filterEnvOut);
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
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
