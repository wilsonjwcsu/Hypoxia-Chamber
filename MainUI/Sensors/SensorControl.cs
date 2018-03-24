// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using HypoxiaChamber;

namespace HypoxiaChamber
{
    public delegate void DataReceivedEventHandler(object sender, SensorDataEventArgs e);
    public class SensorControl
    {
        const float ReferenceVoltage = 3.3F;                //5 or 3.3?
        public event DataReceivedEventHandler DataReceived;
        public MCP3008 mcp3008;

        //Barometric Sensor
        ////public BMP280 BMP280;

        // Values for which channels we will be using from the ADC chip
        const byte O2ADCChannel = 0;
        const byte LightSensorADCChannel = 1;
        const byte HighPotentiometerADCChannel = 2;

        float currentO2;

        private Timer SampleTimer;
        private Timer writeToFile;
        Random rand = new Random();

        public SensorControl()
        {
            mcp3008 = new MCP3008(ReferenceVoltage);
            ////BMP280 = new BMP280();
            //StartTimer();
        }

        public void StartTimer()
        {
            SampleTimer = new Timer(TimerCallback, this, 1000, 1000);    //1000ms period
            Debug.WriteLine("SampleTimer Initialize");
            writeToFile = new Timer(WriteToFileTimerCallback, this, 25000, 1000);     //2500ms startup delay, 1000ms period
            Debug.WriteLine("WriteTimer Initialize");
        }
        private async void WriteToFileTimerCallback(object state)
        {
            //for (int ii = 0; ii < App.BrightnessList.Count; ii++)
            //{
            //    App.Brightnessresult.Add(App.BrightnessList[ii]);
            //    await Windows.Storage.FileIO.AppendTextAsync(App.BrightnessFile, App.BrightnessList[ii]);
            //    Debug.WriteLine("Brightness File" + App.BrightnessList[ii]);
            //}

            //for (int ii = 0; ii < App.TemperatureList.Count; ii++)
            //{
            //    App.Temperatureresult.Add(App.TemperatureList[ii]);
            //    await Windows.Storage.FileIO.AppendTextAsync(App.TemperatureFile, App.TemperatureList[ii]);
            //    Debug.WriteLine("Temperature File" + App.TemperatureList[ii]);
            //}
            //App.O2List.Clear(); //edit
            for (int ii = 0; ii < App.O2List.Count; ii++)
            {
                App.O2result.Add(App.O2List[ii]);
                await Windows.Storage.FileIO.AppendTextAsync(App.O2File, App.O2List[ii]);
                Debug.WriteLine("O2 File" + App.O2List[ii]);
            }
           // App.BrightnessList.Clear();
            //App.TemperatureList.Clear();
            App.O2List.Clear();
            
        }

        /**
         * This method records on a SampleTimer the data measured by the temperature, brightness, and soil moisture sensor,
         * then organizes all of the information collected.  
         * */
        private void TimerCallback(object state)                //ASYNCH with BMP I2C sensor
        {
            //ensures that the temperature sensor is initialized before it is measured from
            ////if (BMP280 == null)
            ////{
            ////    Debug.WriteLine("BMP280 is null");
            ////}
            ////else
            ////{
            ////    //receives the value from the temperature sensor and saves 
            ////    //the data in the SensorDataEventArgs class, which holds
            ////    //the sensor name, the data point, and the time the value was measured.
            ////    //this data is then sent back to the main page and the UI is adjusted based
            ////    //off of the measurement. 
            ////    //float currentTemperature = (float) rand.NextDouble() * 10;
            ////    float currentTemperature = await BMP280.ReadTemperature();
            ////    var tempArgs = new SensorDataEventArgs()
            ////    {
            ////        SensorName = "Temperature",
            ////        SensorValue = currentTemperature,
            ////        Timestamp = DateTime.Now
            ////    };
            ////    OnDataReceived(tempArgs);
            ////}

            //MCP3008 is an ADC and checks to see if this is initialized. 
            //the soil moisture sensor and the photocell are on different channels of the ADC
            if (mcp3008 == null)
            {
                Debug.WriteLine("mcp3008 is null");
                return;
            }
            else
            {
                Debug.WriteLine("SensorTimerCall");
                //The first line reads a value from the ADC from the photo cell sensor usually between 0 and 1023. 
                //then the second line maps this number to a voltage that represents this number 
                ////int LSReadVal = mcp3008.ReadADC(LightSensorADCChannel);
                ////float LSVoltage = mcp3008.ADCToVoltage(LSReadVal);

                //float currentBrightness = (float)rand.NextDouble() * 10; 
                ////float currentBrightness = LSVoltage;
                ////var brightnessArgs = new SensorDataEventArgs()
                ////{
                ////    SensorName = "Brightness",
                ////    SensorValue = currentBrightness,
                ////    Timestamp = DateTime.Now
                ////};
                ////OnDataReceived(brightnessArgs);

                //float currentO2 = (float)rand.NextDouble() * 10;
                currentO2 = mcp3008.ReadADC(O2ADCChannel);
                
                currentO2 = (currentO2 * (3300.0F / 1024.0F)) / 132F;    // Change characteristic EQ based on Sensor 
                //  3300mV/132 = 25(%) for a full high signal
                Debug.WriteLine(currentO2);
                var O2Args = new SensorDataEventArgs()
                {
                    SensorName = "O2",
                    SensorValue = currentO2,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(O2Args);
                
            }
        }


        protected virtual void OnDataReceived(SensorDataEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, e);
            }
        }
    }
    public class SensorDataEventArgs : EventArgs
    {
        public string SensorName;
        public float SensorValue;      //Previously float
        public DateTime Timestamp;
    }
}
