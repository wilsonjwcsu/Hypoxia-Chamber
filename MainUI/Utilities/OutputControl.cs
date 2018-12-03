using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using HypoxiaChamber;
//using Adafruit.Pwm;
using Windows.Devices.Gpio;

namespace HypoxiaChamber
{



    public class GPIODeviceController
    {
        //GPIO Int

        public const int START_LED_PIN = 5;
        public const int STOP_LED_PIN = 6;
        public const int START_BUTTON_PIN = 12;
        public const int STOP_BUTTON_PIN = 13;
        public const int N2_C_PIN = 23;
        public const int DOOR_S_PIN = 19;
        public const int LTG_C_PIN = 24;
        public GpioPin StartLEDPin;
        public GpioPin StopLEDPin;
        public GpioPin StartButtonPin;
        public GpioPin StopButtonPin;
        public GpioPin N2Pin;
        public GpioPin DoorPin;
        public GpioPin LTGPin;

        private GpioPinValue StartLEDPinValue = GpioPinValue.High;
        private GpioPinValue StopLEDPinValue = GpioPinValue.High;
        private GpioPinValue N2PinValue = GpioPinValue.High;
        private GpioPinValue LTGPinValue = GpioPinValue.High;
        public bool StopButton_S = false;
        public bool StartButton_S = false;
        private object dispatcher;

        public GPIODeviceController()
        {

        }

        public void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            StartLEDPin = gpio.OpenPin(START_LED_PIN);
            StopLEDPin = gpio.OpenPin(STOP_LED_PIN);
            StartButtonPin = gpio.OpenPin(START_BUTTON_PIN);
            StopButtonPin = gpio.OpenPin(STOP_BUTTON_PIN);
            N2Pin = gpio.OpenPin(N2_C_PIN);
            DoorPin = gpio.OpenPin(DOOR_S_PIN);
            LTGPin = gpio.OpenPin(LTG_C_PIN);

            // Initialize LEDs to the OFF state by first writing a HIGH value
            // We write HIGH because the LED is wired in a active LOW configuration

            StartLEDPin.Write(GpioPinValue.High);
            StartLEDPin.SetDriveMode(GpioPinDriveMode.Output);

            StopLEDPin.Write(GpioPinValue.High);
            StopLEDPin.SetDriveMode(GpioPinDriveMode.Output);

            N2Pin.Write(GpioPinValue.High);
            N2Pin.SetDriveMode(GpioPinDriveMode.Output);

            LTGPin.Write(GpioPinValue.High);
            LTGPin.SetDriveMode(GpioPinDriveMode.Output);

            // Check if input pull-up resistors are supported
            if (StartButtonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                StartButtonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                StartButtonPin.SetDriveMode(GpioPinDriveMode.Input);

            if (StopButtonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                StopButtonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                StopButtonPin.SetDriveMode(GpioPinDriveMode.Input);

            if (DoorPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                DoorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                DoorPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            StartButtonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            StopButtonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            DoorPin.DebounceTimeout = TimeSpan.FromMilliseconds(1000);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            StartButtonPin.ValueChanged += StartButtonPin_ValueChanged;
            StopButtonPin.ValueChanged += StopButtonPin_ValueChanged;
        }

        public void N2_Com(bool Command)
        {
            if (Command == true)
            {
                N2Pin.Write(GpioPinValue.High);
                return;
            }
            else if (Command == false)
            {
                N2Pin.Write(GpioPinValue.Low);
                return;
            }
        }

        private void StopButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                //StopLEDPinValue = (StopLEDPinValue == GpioPinValue.Low) ?
                //    GpioPinValue.High : GpioPinValue.Low;
                //StopLEDPin.Write(StopLEDPinValue);
                StopLEDPin.Write(GpioPinValue.High);
                StopButton_S = true;
            }
            else
            {
                StopLEDPin.Write(GpioPinValue.Low);
                StopButton_S = false;
            }

            //// need to invoke ui updates on the ui thread because this event
            //// handler gets invoked on a separate thread.
            //var task = dispatcher.runasync(coredispatcherpriority.normal, () =>
            //{
            //    if (e.edge == gpiopinedge.fallingedge)
            //    {
            //        ledellipse.fill = (ledpinvalue == gpiopinvalue.low) ?
            //            redbrush : graybrush;
            //        gpiostatus.text = "button pressed";
            //    }
            //    else
            //    {
            //        gpiostatus.text = "button released";
            //    }
            //});
        }
        private void StartButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                //StartLEDPinValue = (StartLEDPinValue == GpioPinValue.Low) ?
                //    GpioPinValue.High : GpioPinValue.Low;
                //StartLEDPin.Write(StartLEDPinValue);
                StartLEDPin.Write(GpioPinValue.High);
                StartButton_S = true;
            }
            else
            {
                StartLEDPin.Write(GpioPinValue.Low);
                StartButton_S = false;
            }

            //// need to invoke ui updates on the ui thread because this event
            //// handler gets invoked on a separate thread.
            //var task = dispatcher.runasync(coredispatcherpriority.normal, () =>
            //{
            //    if (e.edge == gpiopinedge.fallingedge)
            //    {
            //        ledellipse.fill = (ledpinvalue == gpiopinvalue.low) ?
            //            redbrush : graybrush;
            //        gpiostatus.text = "button pressed";
            //    }
            //    else
            //    {
            //        gpiostatus.text = "button released";
            //    }
            //});
        }

        public void StartButtonLight(bool value)
        {
            if (value == true)
            {
                StartLEDPin.Write(GpioPinValue.High);
            }
            else
            {
                StartLEDPin.Write(GpioPinValue.Low);
            }
        }
        public void StopButtonLight(bool value)
        {
            if (value == true)
            {
                StopLEDPin.Write(GpioPinValue.High);
            }
            else
            {
                StopLEDPin.Write(GpioPinValue.Low);
            }
        }
    }
}