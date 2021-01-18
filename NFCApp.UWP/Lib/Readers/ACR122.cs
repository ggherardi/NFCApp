using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace CSharp.NFC.Readers
{
    public class ACR122 : NFCReader, IReaderSignalControl
    {
        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xCA (1 byte) - P1: 0x00 (1 byte) - P2: 0x00 (1 byte) - Le: 0x00 (1 byte)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 4.1. Get Data, pag. 11
        /// </summary>
        private byte[] GetUIDCommand { get => new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; }
        private ACR122Command UIDCommand = new ACR122Command()
        {
            Bytes = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0x82 (1 byte) - P1: 0x00 (Key Structure) (1 byte) - P2: 0x00 (Key Number) (1 byte) - Le: 0x06 (1 byte) - Data In: {key} (6 bytes)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 5.1. Load Authentication Keys, pag. 12
        /// </summary>
        private byte[] LoadAuthenticationKeysCommand { get => new byte[] { 0xFF, 0x82, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; }

        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xB0 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Le: {numberOfBytesToRead: max 16 bytes} (1 byte)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 5.3. Read Binary Blocks, pag. 16
        /// </summary>
        private byte[] ReadBinaryBlocksCommand { get => new byte[] { 0xFF, 0xB0, 0x00, 0x00, 0x00 }; }

        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xB1 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Le: 0x04 (1 byte)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 5.5.2. Read Value Block, pag. 19
        /// </summary>
        private byte[] ReadValueBlockCommand { get => new byte[] { 0xFF, 0xB1, 0x00, 0x00, 0x04 }; }

        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xD6 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Lc: {numberOfBytesToUpdate} (1 byte) - Data In: {dataIn} (4 bytes)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 5.4. Update Binary Blocks, pag. 17
        /// </summary>
        private byte[] UpdateBinaryBlockCommand { get => new byte[] { 0xFF, 0xD6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; }

        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0x00 (1 byte) - P1: 0x00 (1 byte) - P2: 0x00 (1 byte) - Lc: {payloadLength} (1 byte) - Data In: {payload} (payload.length bytes, max 255 bytes)
        /// Reference: ACR122U Application Programming Interface V2.04, chapter: 6.1. Direct Transmit, pag. 21
        /// </summary>
        private byte[] DirectTransmitCommand { get => new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x00 }; }


        public ACR122(SmartCardReader reader) : base(reader) { }

        public ACR122() : base() { }

        protected override byte[] Get_GetUIDCommand()
        {
            return GetUIDCommand;
        }

        protected override byte[] Get_LoadAuthenticationKeysCommand()
        {
            return LoadAuthenticationKeysCommand;
        }

        protected override byte[] Get_ReadBinaryBlocksCommand(byte blockNumber, int numberOfBytesToRead)
        {
            byte[] command = ReadBinaryBlocksCommand;
            command.SetValue(blockNumber, 3);
            command.SetValue((byte)numberOfBytesToRead, 4);
            return command;
        }

        protected override byte[] Get_ReadValueBlockCommand(byte blockNumber)
        {
            byte[] command = ReadValueBlockCommand;
            command.SetValue(blockNumber, 3);
            return command;
        }

        protected override byte[] Get_UpdateBinaryBlockCommand(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate = 4)
        {
            byte[] command = UpdateBinaryBlockCommand;
            command.SetValue(blockNumber, 3);
            command.SetValue((byte)numberOfBytesToUpdate, 4);
            for (int i = 0; i < dataIn.Length; i++)
            {
                command.SetValue(dataIn[i], i + 5);
            }
            return command;
        }

        protected override byte[] Get_DirectTransmitCommand(byte[] payload)
        {
            byte[] command = DirectTransmitCommand;
            command.SetValue((byte)payload.Length, 4);            
            return command.Concat(payload).ToArray();
        }

        #region Signaling
        public byte[] GetErrorSignalCommand()
        {
            LEDAndBuzzerControl control = new LEDAndBuzzerControl()
            {
                InitialBlinkingState = LEDAndBuzzerControl.LEDLights.Red,
                BlinkingMask = LEDAndBuzzerControl.LEDLights.Red,
                FinalState = LEDAndBuzzerControl.LEDLights.Green,
                InitialBlinkingStateDuration = 3,
                Buzzer = LEDAndBuzzerControl.BuzzerSound.AlwaysOn,
                Repetitions = 1
            };
            return control.GetCommand();
        }

        public byte[] GetSuccessSignalCommand()
        {
            LEDAndBuzzerControl control = new LEDAndBuzzerControl()
            {
                InitialBlinkingState = LEDAndBuzzerControl.LEDLights.Green,
                BlinkingMask = LEDAndBuzzerControl.LEDLights.Green,
                FinalState = LEDAndBuzzerControl.LEDLights.Green,
                InitialBlinkingStateDuration = 1,
                ToggleBlinkingStateDuration = 1,
                Buzzer = LEDAndBuzzerControl.BuzzerSound.OnDuringBlinkLEDOn                
            };
            return control.GetCommand();
        }

        public byte[] GetGreenBlinking(uint repetitions = 1)
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

        public uint GetControlCode()
        {
            return LEDAndBuzzerControl.ControlCode;
        }

        protected class LEDAndBuzzerControl
        {            
            private readonly byte[] BaseCommand = { 0xFF, 0x00, 0x40, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            public static uint ControlCode { get => Winscard.SCARD_CTL_CODE(3500); }
            public LEDLights FinalState { get; set; }
            public LEDLights StateMask { get; set; }
            public LEDLights InitialBlinkingState { get; set; }
            public LEDLights BlinkingMask { get; set; }
            public BuzzerSound Buzzer { get; set; }
            /// <summary>
            /// Unit: 1 = 100ms, max 7 = 700ms
            /// </summary>
            public uint InitialBlinkingStateDuration { get; set; }
            /// <summary>
            /// Unit: 1 = 100ms, max 7 = 700ms
            /// </summary>
            public uint ToggleBlinkingStateDuration { get; set; }
            /// <summary>
            /// Max 7 repetitions
            /// </summary>
            public uint Repetitions { get; set; }

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

            public byte[] GetLEDCommand(LEDAndBuzzerControl control)
            {
                return control.GetCommand();
            }
            #endregion
        }
    }

    public class ACR122Command : NFCCommand
    {

    }
}
