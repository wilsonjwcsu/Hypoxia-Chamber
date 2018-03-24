using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.System.Threading;
using System.Timers;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UwpMessageRelay.Producer.Services;


namespace ADCBackground
{
    sealed partial class StartupTask : IBackgroundTask
    {

        private readonly MessageRelayService _connection = MessageRelayService.Instance;
        private SpiDevice _mcp3008;
        public StartupTask()
        {
            InitializeComponent();
            Initialize();            
        }


        public async void Initialize(IBackgroundTaskInstance ADCint)
        {
            //BackgroundTaskDeferral _deferral = ADCbackground.GetDeferral();
            //using SPI0 on the Pi
            var spiSettings = new SpiConnectionSettings(0);//for spi bus index 0
            spiSettings.ClockFrequency = 3600000; //3.6 MHz
            spiSettings.Mode = SpiMode.Mode0;

            string spiQuery = SpiDevice.GetDeviceSelector("SPI0");
            //using Windows.Devices.Enumeration;
            var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);
            if (deviceInfo != null && deviceInfo.Count > 0)
            {
                _mcp3008 = await SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings);

            }
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            //var samplerate = MainUI.O2Focus.O2_SampleFreq;
            //From data sheet -- 1 byte selector for channel 0 on the ADC
            //First Byte sends the Start bit for SPI
            //Second Byte is the Configuration Bit
            //1 - single ended 
            //0 - d2
            //0 - d1
            //0 - d0
            //             S321XXXX <-- single-ended channel selection configure bits
            // Channel 0 = 10000000 = 0x80 OR (8+channel) << 4
            var transmitBuffer = new byte[3] { 1, 0x80, 0x00 };
            var receiveBuffer = new byte[3];

            _mcp3008.TransferFullDuplex(transmitBuffer, receiveBuffer);
            //first byte returned is 0 (00000000), 
            //second byte returned we are only interested in the last 2 bits 00000011 (mask of &3) 
            //then shift result 8 bits to make room for the data from the 3rd byte (makes 10 bits total)
            //third byte, need all bits, simply add it to the above result 
            var result = ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
            //LM35 == 10mV/1degC ... 3.3V = 3300.0 mV, 10 bit chip # steps is 2 exp 10 == 1024
            var mv = result * (3300.0 / 1024.0);    // Change characteristic EQ based on Sensor 
            var OX = mv / 132; //  3300mV/132 = 25(%) for a full high signal

        }

        private async void OnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            try
            {
                await _connection.Open();
            }
            catch (Exception ex)
            {
                // failing quietly is probably ok for now since the connection will
                //  attempt to re-open itself again on next send.  It just means
                //  we won't be able to receive messages
                Debug.WriteLine("Error opening connection on startup" + ex);
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        
        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            _connection.CloseConnection();

            deferral.Complete();
        }
    }
}
    

