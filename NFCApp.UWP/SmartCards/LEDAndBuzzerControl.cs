using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public class LEDAndBuzzerControl
    {
        public readonly byte[] BaseCommand = { 0xFF, 0x00, 0x40, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
        public LEDLights FinalState { get; set; }
        public LEDLights StateMask { get; set; }
        public LEDLights InitialBlinkingState { get; set; }
        public LEDLights BlinkingMask { get; set; }
        public BuzzerSound Buzzer { get; set; }
        /// <summary>
        /// Unit: 100ms, max 700ms
        /// </summary>
        public uint InitialBlinkingStateDuration { get; set; }
        /// <summary>
        /// Unit: 100ms, max 700ms
        /// </summary>
        public uint ToggleBlinkingStateDuration { get; set; }
        /// <summary>
        /// Max 7 repetitions
        /// </summary>
        public uint Repetitions { get; set; }       

        public LEDAndBuzzerControl() { }

        public byte[] GetCommand()
        {
            byte[] command = BaseCommand;
            byte[] ledStateControlByte = new byte[1];
            BitArray ledStateBits = new BitArray(ledStateControlByte);
            ledStateBits.Set(0, FinalState == LEDLights.Red); // Final State: Red LED. true = On, false = Off
            ledStateBits.Set(1, FinalState == LEDLights.Green); // Final State: Green LED. true = On, false = Off
            ledStateBits.Set(2, StateMask == LEDLights.Red); // State Mask: Red LED. true = Update the state, false = Off
            ledStateBits.Set(3, StateMask == LEDLights.Green); // State Mask: Green LED. true = Update the state, false = Off
            ledStateBits.Set(4, InitialBlinkingState == LEDLights.Red); // Initial Blinking State: Red LED. true = On, false = Off
            ledStateBits.Set(5, InitialBlinkingState == LEDLights.Green); // Initial Blinking State: Green LED. true = On, false = Off
            ledStateBits.Set(6, BlinkingMask == LEDLights.Red); // Blinking Mask: Red LED. true = Blink, false = Not Blink
            ledStateBits.Set(7, BlinkingMask == LEDLights.Green); // Blinking Mask: Green LED. true = Blink, false = Not Blink
            ledStateBits.CopyTo(ledStateControlByte, 0);
            command[3] = ledStateControlByte.Single(); // Byte 4 is LED State Control
            command[5] = byte.Parse(InitialBlinkingStateDuration.ToString("X")); // Byte 5 is Initial Blinking State
            command[6] = byte.Parse(ToggleBlinkingStateDuration.ToString("X")); // Byte 6 is Toggle Blinking State
            command[7] = byte.Parse(Repetitions.ToString("X")); // Byte 7 is number of repetitions
            command[8] = byte.Parse(((int)Buzzer).ToString("X")); // Byte 8 is Link to buzzer
            return command;
        }

        public enum LEDLights
        {
            Off = 0,
            Green = 1,
            Red = 2            
        }

        public enum BuzzerSound
        {
            Off = 0x00,
            OnDuringBlinkLEDOn = 0x01,
            OnDuringBlinkLEDOff = 0x02,
            AlwaysOn = 0x03
        }
    }
}
