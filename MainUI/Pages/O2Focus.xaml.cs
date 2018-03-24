using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HypoxiaChamber
{
    /// <summary>.
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    ///

    public sealed partial class O2Focus : Page
    {
        public O2Focus()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        double OX;
        private SpiDevice _mcp3008;
        DispatcherTimer SampleTimer;
        int SampleRate = 1;

                       
        ////public void SampleTimerSetup()
        ////{
        ////    SampleTimer = new DispatcherTimer();
        ////    SampleTimer.Tick += SampleTimer_Tick;
        ////    SampleTimer.Interval = new TimeSpan(0, 0, SampleRate);  //Set initial refresh rate
        ////    //IsEnabled defaults to false
        ////    //SampleTimer.Start();
        ////    //IsEnabled should now be true after calling start
        ////}

        void SampleTimer_Tick(object sender, object e)
        {
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
            OX = mv / 132; //  3300mV/132 = 25(%) for a full high signal


            // var output = "Oxygen Concentration Is " + OX;
            // TxtHeader.Text = output;
            TxtHeader.Text = "Oxygen Concentration Is " + OX;
            O2Gauge.Value = OX;

            SampleTimer.Interval = new TimeSpan(0, 0, SampleRate);  //Update refresh rate from settings page
        }
        
        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            SampleTimer.Start();
        }

        

        private void GoTo_HomeView(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeView), null);
        }

        private void O2_SampleFreq_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sampleint = Convert.ToInt32(O2_SampleFreq.Value);
            SampleRate = sampleint;
        }

        //public async void BtnConnect_Click(object sender, RoutedEventArgs e)
        //{
        //    //using SPI0 on the Pi
        //    var spiSettings = new SpiConnectionSettings(0);//for spi bus index 0
        //    spiSettings.ClockFrequency = 3600000; //3.6 MHz
        //    spiSettings.Mode = SpiMode.Mode0;

        //    string spiQuery = SpiDevice.GetDeviceSelector("SPI0");
        //    //using Windows.Devices.Enumeration;
        //    var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);
        //    if (deviceInfo != null && deviceInfo.Count > 0)
        //    {
        //        _mcp3008 = await SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings);
        //        BtnConnect.IsEnabled = false;
        //        BtnRead.IsEnabled = true;
        //        O2Gauge.Value = 0;
        //        TxtHeader.Text = "SPI Armed";
        //        SampleTimerSetup();
        //    }
        //    else
        //    {
        //        TxtHeader.Text = "SPI Device Not Found";
        //        O2Gauge.Value = 25;

        //    }
        //}

        /**
         * If the user presses the app-bar buttons, they will go to the appropriate pages
         * */
        private void HomeViewButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomeView));
        }
        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Trends));
        }

        private void AlarmsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Alarms));
        }

        private void SequenceEditorButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SequenceEditor));
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Notifications));
        }

        private void SettingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void HelpPageButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
