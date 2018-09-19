using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using HypoxiaChamber;
using Adafruit.Pwm;
using Windows.Devices.Gpio;

namespace HypoxiaChamber
{

    public class HardwareDeviceController
    {

        //SERVO constants
        //want servo to travel across 0-90 degree angle from 0-100% angle (1ms-1.5ms)
        const int servo_min = 300;  // Min pulse length out of 4095
        const int servo_max = 480;  // Max pulse length out of 4095--should be 660 for 2 ms?
        const int OAservo_pin = 1;
        const int EAservo_pin = 2;

        //Fan PWM constants
        const int FanPWM_min = 300;
        const int FanPWM_max = 600;
        const int SFpwm_pin = 5;
        const int RFpwm_pin = 6;


        public HardwareDeviceController()
        {

        }



        public void ServoRotate(int PWMchannel,int percent)
        {
            float angle = percent * 0.9F;
            float pulse = angle * (servo_max / servo_min) + servo_min;
            int pulsed = Convert.ToInt32(pulse);
            
          //insert angle to pulse width linear fx

            try
            {
                //The servoMin/servoMax values are dependant on the hardware you are using.
                //The values below are for my SR-4303R continuous rotating servos.
                //If you are working with a non-continous rotatng server, it will have an explicit
                //minimum and maximum range; crossing that range can cause the servo to attempt to
                //spin beyond its capability, possibly damaging the gears.

                //using (var PWMhat = new Adafruit.Pwm.PwmController())
                using (var PWMhat = new Adafruit.Pwm.PwmController())
                {
                    DateTime timeout = DateTime.Now.AddSeconds(10);
                    PWMhat.SetDesiredFrequency(60);
                    while (timeout >= DateTime.Now)
                    {
                        PWMhat.SetPulseParameters(PWMchannel, pulsed, false);
                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        //PWMhat.SetPulseParameters(PWMchannel, servoMax, false);
                        //Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    }
                }
            }

            /* If the write fails display the error and stop running */
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to communicate with servo hat: " + ex.Message);
                return;
            }

        }


    }

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