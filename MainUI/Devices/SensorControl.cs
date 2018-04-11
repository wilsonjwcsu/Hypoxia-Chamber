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
    public class SensorDataProvider
    {
        const float ReferenceVoltage = 3.3F;
        public event DataReceivedEventHandler DataReceived;
        public MCP3008 mcp3008;
        public BME280 BME280;
        public MHZ16 MHZ16;

        // Values for which channels we will be using from the ADC chip
        const byte O2ADCChannel = 0;
        //const byte LightSensorADCChannel = 1;
        //const byte HighPotentiometerADCChannel = 2;

        float currentO2;
        float currentCO2;

        private Timer SampleTimer;
        private Timer writeToFile;
        Random rand = new Random();

        public SensorDataProvider()
        {
            mcp3008 = new MCP3008(ReferenceVoltage);
            BME280 = new BME280();
            MHZ16 = new MHZ16();
            //StartTimer();
        }

        public void StartTimer()
        {
            SampleTimer = new Timer(TimerCallback, this, 1000, 1000);    //1000ms period
            Debug.WriteLine("SampleTimer Initialize");
            //writeToFile = new Timer(WriteToFileTimerCallback, this, 25000, 1000);     //2500ms startup delay, 1000ms period
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
        private async void TimerCallback(object state)                //ASYNCH with BME I2C sensor
        {
            //ensures that the temperature sensor is initialized before it is measured from
            if (BME280 == null)
            {
                Debug.WriteLine("BME280 is null");
            }
            else
            {
                //receives the value from the temperature sensor and saves 
                //the data in the SensorDataEventArgs class, which holds
                //the sensor name, the data point, and the time the value was measured.
                //this data is then sent back to the main page and the UI is adjusted based
                //off of the measurement. 
                //float currentTemperature = (float) rand.NextDouble() * 10;
                float currentTemperature = await BME280.ReadTemperature();
                var tempArgs = new SensorDataEventArgs()
                {
                    SensorName = "Temperature",
                    SensorValue = currentTemperature*(9/5)+32,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(tempArgs);

                float currentPressure = await BME280.ReadPressure();
                var pressureArgs = new SensorDataEventArgs()
                {
                    SensorName = "Pressure",
                    SensorValue = currentPressure / 1000, //into kPa (1hPa --> 0.1kPa)  ???
                    Timestamp = DateTime.Now
                };
                OnDataReceived(pressureArgs);

                float currentHumidity = await BME280.ReadHumidity();
                var humidityArgs = new SensorDataEventArgs()
                {
                    SensorName = "Humidity",
                    SensorValue = currentHumidity,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(humidityArgs);

                //float currentAltitude = await BME280.ReadAltitude(seaLevel);
                //var altitudeArgs = new SensorDataEventArgs()
                //{
                //    SensorName = "Altitude",
                //    SensorValue = currentAltitude,
                //    Timestamp = DateTime.Now
                //};
                //OnDataReceived(altitudeArgs);
            }

            //MCP3008 is an ADC and checks to see if this is initialized. 
            //the soil moisture sensor and the photocell are on different channels of the ADC
            if (mcp3008 == null)
            {
                Debug.WriteLine("mcp3008 is null");
                return;
            }
            else
            {
                ////Debug.WriteLine("O2Call");
               
                currentO2 = mcp3008.ReadADC(O2ADCChannel);
                
                currentO2 = (currentO2 * (3300.0F / 1024.0F)) / 132F;    // Change characteristic EQ based on Sensor 
                //  3300mV/132 = 25(%) for a full high signal
                ////Debug.WriteLine(currentO2);
                var O2Args = new SensorDataEventArgs()
                {
                    SensorName = "O2",
                    SensorValue = currentO2,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(O2Args);
                
            }

            if(MHZ16 == null)
            {
                Debug.WriteLine("MHZ16 is null");
                return;
            }
            else
            {
                currentCO2 = (float)(MHZ16.ReadCO2());
                //if (currentCO2 == 999999f)
                //{
                //    //Add Some Method for restarting disconnected/reconnected i2c device
                //}
                var CO2Args = new SensorDataEventArgs()
                {
                    SensorName = "CO2",
                    SensorValue = currentCO2,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(CO2Args);
               
               

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
