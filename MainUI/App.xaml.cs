// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HypoxiaChamber
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        //this helps find the data from the sensors
        public static SensorDataProvider SensorProvider;
        public static GPIODeviceController GPIOController;

        //these are the files that the app reads from for the sensors
        public static Windows.Storage.StorageFile BrightnessFile;
        public static Windows.Storage.StorageFile TemperatureFile;
        public static Windows.Storage.StorageFile PreussureFile;
        public static Windows.Storage.StorageFile HumidityFile;
        //public static Windows.Storage.StorageFile AltitudeFile;
        public static Windows.Storage.StorageFile O2File;
        public static Windows.Storage.StorageFile CO2File;
        public static Windows.Storage.StorageFile TwitterFile;

        //this is the folder in which the files are stored
        static Windows.Storage.StorageFolder storageFolder;

        //when the files are read, then they are stored in these lists
        public static IList<string> Brightnessresult;
        public static IList<string> Temperatureresult;
        public static IList<string> Pressureresult;
        public static IList<string> Humidityresult;
        //public static IList<string> Altituderesult;
        public static IList<string> O2result;
        public static IList<string> CO2result;
        public static IList<string> Twitterresult;

        //these lists are where new data is temporarily stored so that we are not 
        //readong and writing to files that often
        public static List<String> BrightnessList;
        public static List<String> TemperatureList;
        public static List<String> PressureList;
        public static List<String> HumidityList;
        //public static List<String> AltitudeList;
        public static List<String> O2List;
        public static List<String> CO2List;

        //this variable holds the settings for the plant
        public static SettingsPage PlantSettings;
        public static SettingsPage TwitterSettings;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            PlantSettings = new SettingsPage();
            TwitterSettings = new SettingsPage();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            SensorProvider = new SensorDataProvider();
        }

        public async Task SetUpFile()
        {
            storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                BrightnessFile = await storageFolder.GetFileAsync(FileNames.BrightnessfileName);
            }
            catch (FileNotFoundException e)
            {
                BrightnessFile = await storageFolder.CreateFileAsync(FileNames.BrightnessfileName);
            }

            try
            {
                TemperatureFile = await storageFolder.GetFileAsync(FileNames.TemperaturefileName);
            }
            catch (FileNotFoundException e)
            {
                TemperatureFile = await storageFolder.CreateFileAsync(FileNames.TemperaturefileName);
            }

            try
            {
                O2File = await storageFolder.GetFileAsync(FileNames.O2fileName);
                Debug.WriteLine("Old O2 Files are used");
            }
            catch (FileNotFoundException e)
            {
                O2File = await storageFolder.CreateFileAsync(FileNames.O2fileName);
                Debug.WriteLine("New O2 Files were created");
            }

            try
            {
                TwitterFile = await storageFolder.GetFileAsync(FileNames.SettingsfileName);
                Debug.WriteLine("Old settings Files are used");
            }
            catch (FileNotFoundException e)
            {
                TwitterFile = await storageFolder.CreateFileAsync(FileNames.SettingsfileName);
                Debug.WriteLine("New settings Files are used");
            }

            Brightnessresult = await Windows.Storage.FileIO.ReadLinesAsync(BrightnessFile);
            Temperatureresult = await Windows.Storage.FileIO.ReadLinesAsync(TemperatureFile);
            O2result = await Windows.Storage.FileIO.ReadLinesAsync(file: O2File);
            Twitterresult = await Windows.Storage.FileIO.ReadLinesAsync(TwitterFile);

            BrightnessList = new List<string>();
            TemperatureList = new List<string>();
            O2List = new List<string>();

        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            SensorProvider = new SensorDataProvider();       //Not original location (see App())
            await SetUpFile();
            App.SensorProvider.MHZ16.Initialize();        //Initialize CO2 Sensor
            await App.SensorProvider.mcp3008.Initialize();  //Initialize ADC (for O2 Sensor Voltage Reading)
            await App.SensorProvider.BME280.Initialize();   //Initialize Environmental Sensor
            //SensorProvider.StartTimer();                    //Start Timer that triggers sensor readings
            try
            {
                PlantSettings = await SettingsPage.Load(FileNames.SettingsfileName);        //Weird auto-catch was generated somewhere RE load function
            }
            catch
            {

            }
            try
            {
                TwitterSettings = await SettingsPage.Load(FileNames.SettingsfileName);
            }
            catch
            {

            }
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(HomeView), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            SensorProvider.StartTimer();                    //Start Timer that triggers sensor readings
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

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
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
