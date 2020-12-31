using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public class ACR122
    {
        public static byte[] GetGreenBlinking(uint repetitions = 1)
        {
            LEDAndBuzzerControl control = new LEDAndBuzzerControl()
            {
                InitialBlinkingState = LEDAndBuzzerControl.LEDLights.Green,
                BlinkingMask = LEDAndBuzzerControl.LEDLights.Green,
                FinalState = LEDAndBuzzerControl.LEDLights.Green,
                InitialBlinkingStateDuration = 1,
                ToggleBlinkingStateDuration = 1,
                Buzzer = LEDAndBuzzerControl.BuzzerSound.OnDuringBlinkLEDOn,
                Repetitions = repetitions
            };
            return control.GetCommand();
        }

        public byte[] GetLEDCommand(LEDAndBuzzerControl control)
        {
            return control.GetCommand();
        }
    }
}
