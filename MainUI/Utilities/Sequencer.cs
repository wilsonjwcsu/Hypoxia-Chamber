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
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;

namespace HypoxiaChamber
{
    public delegate void SequenceChangeEventHandler(object sender, SensorDataEventArgs e);
    public class Sequencer
    {
        public string sequencer_status;
        string sequence_filename;

        double O2_VAL;
        double O2_SP = 10;
        const double O2_DB = 0.1;
        double CO2_VAL;
        double CO2_SP;
        const double CO2_DB = 150;
        double Temp_val;
        double Hum_val;
        double Pres_val;

        public bool N2_C_OverRide = false;
        bool N2_C;
        bool LTG_C;
        int OAD_O;
        int SF_O;
        int EAD_O;
        int RF_O;

        int elapsed_time = 0;


        private Timer SequenceValidateTimer;

        public Sequencer() 
        {

            App.OutputController.ServoRotate(3, 0);
            App.OutputController.ServoRotate(4, 0);
            App.GPIOController.N2_Com(N2_C = false);
            sequencer_status = "unloaded";

        }


        //bool LoadSequence(string sequence_file)
        //{
        //    List sequence = sequence_file;        //Load list sequence in from file 

        //    if (sequence == null) {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        public void StartSequence()
        {
            //load list into new list with current time offsets for each phase of program
            sequencer_status = "loaded";
            //temporary solution below--load the single value set from UI on the sequencing page
            //HomeView.Status_Bar.IsIndeterminate = true;
            App.GPIOController.StartButtonLight(true);
            //popup confimation message

            SequenceValidateTimer = new Timer(SequenceRuntime, this, 1000, 5000);    //1000ms period for sequence timer call
        }

        public void StopSequence()
        {
            //dispose of (stop) SequenceValidateTimer 
            sequencer_status = "stopped";
        }

        public void PauseSequence()
        {
            //stop SquenceValidateTimer
            sequencer_status = "paused";
        }

        void SequenceRuntime(object state)
        {
            elapsed_time++;     //increment elapsed second timer by +1
            sequencer_status = "running";
            if (N2_C_OverRide == true)
            {
                return;
            }

            if (N2_C == true)
            {
                if (O2_VAL <= O2_SP)
                {
                    N2_C = false;
                    App.GPIOController.N2_Com(N2_C);
                    return;
                }
                
            }
            else if (N2_C == false)
            {
                if(O2_VAL > (O2_SP + O2_DB))
                {
                    N2_C = true;
                    App.GPIOController.N2_Com(N2_C);
                    return;
                }
            }
            
            //integrate CO2 sequencing
                
        }

        private void SequenceEditor_DataReceived(object sender, SequenceDataArgs e)
        {
            String format = FormatOfSensorValue((float)e.ParameterValue);
            String nextValue = e.ParameterValue + "," + DateTime.Now + Environment.NewLine;
            switch (e.ParameterName)
            {
                case "O2":
                    {
                        O2_SP = e.ParameterValue;
                    }
                    break;

                case "CO2":
                    {
                        CO2_SP = e.ParameterValue;
                    }
                    break;
            }

            //call validation w/sequence or overrides
        }

        private void SensorProvider_DataReceived(object sender, SensorDataEventArgs e)
        {
            String format = FormatOfSensorValue(e.SensorValue);
            String nextValue = e.SensorValue + "," + DateTime.Now + Environment.NewLine;
            switch (e.SensorName)
            {
                case "O2":
                    {
                        O2_VAL = e.SensorValue;
                    };
                    break;
                case "CO2":
                    {
                        CO2_VAL = e.SensorValue;

                    };
                    break;
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


    }
}