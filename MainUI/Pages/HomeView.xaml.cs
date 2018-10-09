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
using Windows.UI.Core;
using System.Threading;
using Windows.UI;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HypoxiaChamber
{
    /// <summary>
    /// Main User Interface 'Home Screen' that shows quick metrics and allows for navigation to other pages.
    /// This is the top level page for frame navigation
    /// </summary>
    public sealed partial class HomeView : Page
    {
        ////public HomeView()
        ////{
        ////    this.InitializeComponent();
        ////    this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        ////    DispatcherTimerSetup();
        ////}

        ////DispatcherTimer RefreshTimer;
        ////int RefreshRate = 1;

        ////public void DispatcherTimerSetup()
        ////{

        ////    RefreshTimer = new DispatcherTimer();
        ////    RefreshTimer.Tick += RefreshTimer_Tick;
        ////    RefreshTimer.Interval = new TimeSpan(0, 0, RefreshRate);  //Set initial refresh rate
        ////    //IsEnabled defaults to false
        ////    RefreshTimer.Start();
        ////    //IsEnabled should now be true after calling start
        ////}


        //This controls the timer on the panel
        public Timer controlPanelTimer;

        //This is the timer for the altitude and pressure
        public Timer altPressTimer;
        //This will be able to be set by the user in the next sprint
        ////int idealTemperature;
        ////int idealO2;
        ////int idealBrightness;

        public static float currentBrightness;
        public static float currentTemperature;
        public static float currentPressure;
        public static float currentHumidity;
        //public static float currentAltitude;
        public static float currentO2;
        public static float currentCO2;

        /**
         * sets all of the variables that were described above
         **/
        public HomeView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            controlPanelTimer = new Timer(TimerControlPanel, this, 0, 1000);

            DateTime Now = DateTime.Now;
            Random rand = new Random();     //Not sure if the random component of saving these is necessary, but if it is, the structure of the sensor data needs to incorporate this component.
            TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
            TimeSpan oneHour = new TimeSpan(1, 0, 0);
            DateTime LowerBound = Now - oneDay;
            while (LowerBound < Now)
            {
                float randomValue = (float)rand.NextDouble() * 10;
                String nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.TemperatureList.Add(nextValue);

                randomValue = (float)rand.NextDouble() * 10;
                nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.BrightnessList.Add(nextValue);

                randomValue = (float)rand.NextDouble() * 10;
                nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.O2List.Add(nextValue);

                LowerBound += oneHour;
            }
        }



        /**
     * updates the UI when the sensors make a new reading
     * */

        //class DataHandlerClass
        //{
        //    public DataHandlerClass(SensorControl SensorUpdate)
        //    {
        //        SensorUpdate.DataReceived += new HypoxiaChamber.DataReceivedEventHandler(UpdateUI);
        //    }
        //}
        private async void SensorProvider_DataReceived(object sender, SensorDataEventArgs e)
        {
            String format = FormatOfSensorValue(e.SensorValue);
            String nextValue = e.SensorValue + "," + DateTime.Now + Environment.NewLine;
            switch (e.SensorName)
            {
                case "O2":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentO2 = e.SensorValue;
                        O2Gauge.Value = currentO2;
                        App.O2List.Add(nextValue);
                    });
                    break;
                case "CO2":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentCO2 = e.SensorValue;
                        CO2Gauge.Value = currentCO2;
                        //App.CO2List.Add(nextValue);
                    });
                    break;
                //case "Brightness":
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //    {
                //        currentBrightness = e.SensorValue;
                //        App.BrightnessList.Add(nextValue);
                //    });
                //    break;
                case "Temperature":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentTemperature = e.SensorValue;
                        Temp_Gauge.Value = currentTemperature;
                        //Temp_Txt.Text = string.Format("{0:0,0.00}", currentTemperature);
                        App.TemperatureList.Add(nextValue);
                    });
                    break;
                case "Pressure":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentPressure = e.SensorValue;
                        Pres_Gauge.Value = currentPressure;
                        //App.PressureList.Add(nextValue);
                    });
                    break;
                case "Humidity":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentHumidity = e.SensorValue;
                        Humid_Gauge.Value = currentHumidity;
                        //App.HumidityList.Add(nextValue);
                    });
                    break;
                //case "Altitude":
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //    {
                //        currentAltitude = e.SensorValue;
                //        App.AltitudeList.Add(nextValue);
                //    });
                //    break;
                   
            }
        }
        private String FormatOfSensorValue(float value)
        {
            if (value == Math.Floor(value))
            {
                return "000";
            }
            return "####0.0";
        }

        protected override void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            //App.SensorProvider.StartTimer();

            App.SensorProvider.DataReceived += SensorProvider_DataReceived;


        }

        /**
         * updates the time on the control panel
         **/
        private async void TimerControlPanel(object state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string formatTime = "HH:mm:ss";
                string formatDate = "MM/dd/yy";
                ControlPanelTime.Text = DateTime.Now.ToString(formatTime);
                ControlPanelDate.Text = DateTime.Now.ToString(formatDate);
                O2Gauge.Value = currentO2;
            });
        }



        private void GoTo_O2Focus(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(O2Focus));
        }

        private void GoTo_CO2Focus(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CO2Focus));
        }


        /**
         * If the user presses the app-bar buttons, they will go to the appropriate pages
         * */
        private void HomeViewButton_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(HomeView));
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

        private void Atm_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AtmosphereFocus));
        }

        private void AirFlow_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AirflowFocus));
        }
    }
}