using Jacobi.Vst.Framework;

namespace BetterSynth
{
    class Routing : AudioComponentWithParameters
    {
        private double[] samplesForOversampling = new double[8];
        private float masterVolume;
        private ParameterFilter masterVolumeFilter;

        public VoicesManager VoicesManager { get; private set; }

        public Downsampler Downsampler { get; private set; }

        public DistortionManager Distortion { get; private set; }

        public DelayManager DelayManager { get; private set; }

        public VstParameterManager OversamplingOrderManager { get; private set; }

        public VstParameterManager MasterVolumeManager { get; private set; }

        public Routing(
            Plugin plugin,
            string parameterPrefix = "M_",
            string parameterCategory = "master") 
            : base(plugin, parameterPrefix, parameterCategory)
        {
            VoicesManager = new VoicesManager(plugin, "M_");
            Downsampler = new Downsampler();
            Distortion = new DistortionManager(plugin, "DS_");
            DelayManager = new DelayManager(plugin, "DL_");

            plugin.MidiProcessor.NoteOn += MidiProcessor_NoteOn;
            plugin.MidiProcessor.NoteOff += MidiProcessor_NoteOff;
            plugin.Opened += (sender, e) => SampleRate = plugin.AudioProcessor.SampleRate;

            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            OversamplingOrderManager = factory.CreateParameterManager(
                name: "OVSMP",
                valueChangedHandler: SetOversamplingOrder);
            CreateRedirection(OversamplingOrderManager, nameof(OversamplingOrderManager));

            MasterVolumeManager = factory.CreateParameterManager(
                name: "VOL",
                defaultValue: 0.5f,
                valueChangedHandler: x => masterVolumeFilter.SetTarget(x));
            masterVolumeFilter = new ParameterFilter(UpdateMasterVolume, 1);
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
                UpdateSampleRates();
            }
        }

        private void UpdateMasterVolume(float value) => masterVolume = value;

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
            masterVolumeFilter.Process();

            for (int i = 0; i < Downsampler.Order; ++i)
            {
                var voicesOutput = VoicesManager.Process();
                var saturationOutput = Distortion.Process(voicesOutput);
                samplesForOversampling[i] = saturationOutput;
            }
            
            var output = (float)Downsampler.Process(samplesForOversampling);
            DelayManager.Process(output, output, out left, out right);
            left *= masterVolume;
            right *= masterVolume;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateSampleRates();
            DelayManager.SampleRate = newSampleRate;
        }

        private void UpdateSampleRates()
        {
            var scaledSampleRate = SampleRate * Downsampler.Order;
            VoicesManager.SampleRate = scaledSampleRate;
            Distortion.SampleRate = scaledSampleRate;
        }
    }
}