using Jacobi.Vst.Framework;

namespace BetterSynth
{
    internal class Routing : ManagerOfManagers
    {
        private Plugin plugin;
        private float sampleRate;

        public VoicesManager VoicesManager { get; private set; }

        public Downsampler Downsampler { get; private set; }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    VoicesManager.SampleRate = sampleRate * Downsampler.Order;
                }
            }
        }

        public VstParameterManager OversamplingOrderManager { get; private set; }

        public Routing(Plugin plugin)
        {
            this.plugin = plugin;
            VoicesManager = new VoicesManager(plugin, "V");
            Downsampler = new Downsampler(plugin);

            InitializeParameters();

            plugin.MidiProcessor.NoteOn += MidiProcessor_NoteOn;
            plugin.MidiProcessor.NoteOff += MidiProcessor_NoteOff;
            plugin.Opened += (sender, e) => SampleRate = plugin.AudioProcessor.SampleRate;
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "oversampler");

            OversamplingOrderManager = factory.CreateParameterManager(
                name: "OVERSMP",
                valueChangedHandler: SetOversamplingOrder);
            CreateRedirection(OversamplingOrderManager, nameof(OversamplingOrderManager));
        }

        private void SetOversamplingOrder(float value)
        {
            int newOrder;
            if (value < 0.25f)
                newOrder = 1;
            else if (value < 0.5f)
                newOrder = 2;
            else if (value < 0.75f)
                newOrder = 4;
            else
                newOrder = 8;
            

            if (newOrder != Downsampler.Order)
            {
                Downsampler.Order = newOrder;
                VoicesManager.SampleRate = newOrder * SampleRate;
            }
        }

        private void MidiProcessor_NoteOn(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.PlayNote(e.Note);
        }

        private void MidiProcessor_NoteOff(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.ReleaseNote(e.Note);
        }

        private double[] samplesForOversampling = new double[8];

        public void Process(out float left, out float right)
        {
            for (int i = 0; i < Downsampler.Order; ++i)
            {
                samplesForOversampling[i] = VoicesManager.Process();
            }

            var output = (float)Downsampler.Process(samplesForOversampling);

            left = output;
            right = output;
        }
    }
}