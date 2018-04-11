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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HypoxiaChamber
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public delegate void SetpointChangeEventHandler(object sender, SequenceDataArgs e);
    public partial class SequenceEditor : Page          //was sealed class
    {
        public event SetpointChangeEventHandler SetpointChange;

        public SequenceEditor()
        {
            this.InitializeComponent();
        }

        private void Append_Button_Click(object sender, RoutedEventArgs e)
        {
            //Add values to list
        }

        private void O2_SP_slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (O2_SP_Enable.IsOn == false)
            {
                var O2_SP_Args = new SequenceDataArgs()
                {
                    ParameterName = "O2_SP",
                    ParameterValue = 999F
                };
                OnSetpointChange(O2_SP_Args);
            }
            else
            {
                var O2_SP_Args = new SequenceDataArgs()
                {
                    ParameterName = "O2_SP",
                    ParameterValue = O2_SP_slider.Value
                };
                OnSetpointChange(O2_SP_Args);
            }
        }

        private void CO2_SP_slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (CO2_SP_Enable.IsOn == false)
            {
                var CO2_SP_Args = new SequenceDataArgs()
                {
                    ParameterName = "CO2_SP",
                    ParameterValue = 99999F
                };
                OnSetpointChange(CO2_SP_Args);
            }
            else
            {
                var CO2_SP_Args = new SequenceDataArgs()
                {
                    ParameterName = "CO2_SP",
                    ParameterValue = O2_SP_slider.Value
                };
                OnSetpointChange(CO2_SP_Args);
            }
        }

        protected virtual void OnSetpointChange(SequenceDataArgs e)
        {
            if (SetpointChange!= null)
            {
                SetpointChange(this, e);
            }
        }

        //Navigation Controls
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
            //Frame.Navigate(typeof(SequenceEditor));
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

    public class SequenceDataArgs : EventArgs
    {
        public string ParameterName;
        public double ParameterValue;      //Previously float

    }
}
