// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HypoxiaChamber
{
    /// <summary>
    /// This settings page allows the user to change the settings relavent to the app and the user experience
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            //Status.Text = "Save Status: all changes unsaved";
            //NameOfPlantTextBox.Text = App.PlantSettings.NameOfPlant;
            //PlantTwitterAccountTextBox.Text = App.PlantSettings.PlantTwitterAccount;
            //IdealTempTextBox.Text = App.PlantSettings.IdealTemp.ToString();
            //IdealBrightTextBox.Text = App.PlantSettings.IdealBright.ToString();
            //IdealSoilTextBox.Text = App.PlantSettings.IdealSoilMoist.ToString();
            Unloaded += SettingsPage_Unloaded;
        }

        //private void SettingsSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    App.PlantSettings.NameOfPlant = NameOfPlantTextBox.Text;
        //    App.PlantSettings.PlantTwitterAccount = PlantTwitterAccountTextBox.Text;
        //    //check if actual numbers not strings
        //    //cant enter text in those textboxes
        //    int idealTempFromString;
        //    int idealBrightFromString;
        //    int idealSoilMoistFromString;
        //    if (Int32.TryParse(IdealTempTextBox.Text, out idealTempFromString)
        //        && Int32.TryParse(IdealBrightTextBox.Text, out idealBrightFromString)
        //        && Int32.TryParse(IdealSoilTextBox.Text, out idealSoilMoistFromString))
        //    {
        //        //App.PlantSettings.IdealTemp = idealTempFromString;
        //        //App.PlantSettings.IdealBright = idealBrightFromString;
        //        //App.PlantSettings.IdealSoilMoist = idealSoilMoistFromString;
        //        Status.Text = "Save Status: you have succesfully saved!";
        //    }
        //    else
        //    {
        //        Status.Text = "Save Status: ideal values must be integers";
        //    }
        //    App.PlantSettings.Save();
        //}

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)

        {
            //Add confirmation Popup and information about disconnecting the mains power after screen goes white and then black again, NOT BEFORE.
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)

        {
            //Add Confirmation Popup and notice that the device will take approximately 2 minutes to reboot automatically and to NOT disconnect the mains power.
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(0));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)

        {
            //Add confirmation Popup about exiting the application to the IoT homescreen.
            Application.Current.Exit();
        }
        private void SettingsPage_Unloaded(object sender, object args)
        {
            //redPin.Dispose();         //not sure where GPIO disposal should go. Here (upon exit of this page and then the whole app) or in the Device Sequencer which controls the GPIOs
            //greenPin.Dispose();
        }

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
            //Frame.Navigate(typeof(SettingsPage));
        }

        private void HelpPageButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }

    public partial class SettingsPage
    {
        public string NameOfPlant = "";
        public string PlantTwitterAccount = "";
        public int IdealTemp = 0;
        public int IdealBright = 0;
        public int IdealSoilMoist = 0;
        public string ConsumerKeySetting = "";
        public string ConsumerSecretSetting = "";
        public string AccessKeySetting = "";
        public string AccessTokenSetting = "";

        public async void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SettingsPage));
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(FileNames.SettingsfileName, CreationCollisionOption.ReplaceExisting);
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, this);
            }
        }

        public async static Task<SettingsPage> Load(string filename)
        {
            SettingsPage objectFromXml = new SettingsPage();
            var serializer = new XmlSerializer(typeof(SettingsPage));
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync(filename);
            Stream stream = await file.OpenStreamForReadAsync();
            objectFromXml = (SettingsPage)serializer.Deserialize(stream);
            stream.Dispose();
            return objectFromXml;
        }
    }
}
