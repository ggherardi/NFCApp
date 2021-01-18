using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using System.Diagnostics;
using CSharp.NFC;
using CSharp.NFC.Cards;
using CSharp.NFC.NDEF;
using CSharp.NFC.Controllers;

namespace CSharp.NFC.Readers
{
    public abstract class NFCReader
    {
        #region Private fields
        private Winscard.SCARD_IO_REQUEST _standardRequest;
        private Winscard.SCARD_READERSTATE _readerState;
        private int _hContext;
        private int _protocol;
        private NFCController _controller;
        private SmartCardReader _builtinReader;
        private NFCCard _connectedCard;
        private IReaderSignalControl _signalingControl;
        #endregion

        #region Public properties
        public SmartCardReader BuiltinReader { get => _builtinReader; }
        public NFCCard ConnectedCard { get => _connectedCard; }
        public bool ActivateSignaling { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// This constructor initialises the NFC reader with the provided SmartCardReader and NFCController
        /// </summary>
        /// <param name="reader"></param>
        protected NFCReader(SmartCardReader reader, NFCController controller) 
        {
            try
            {
                if (reader == null)
                {
                    throw new Exception("Could not find any connected reader.");
                }
                _builtinReader = reader;
                _standardRequest = new Winscard.SCARD_IO_REQUEST() { dwProtocol = Winscard.SCARD_PROTOCOL_T1, cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Winscard.SCARD_IO_REQUEST)) };
                _readerState = new Winscard.SCARD_READERSTATE();
                _readerState.RdrName = reader.Name;
                _signalingControl = this as IReaderSignalControl;
                _controller = controller;
                int retCode = Winscard.SCardEstablishContext(Winscard.SCARD_SCOPE_SYSTEM, 0, 0, ref _hContext);
                if (retCode != Winscard.SCARD_S_SUCCESS)
                {
                    throw new Exception(Winscard.GetScardErrMsg(retCode));
                }
            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
        }

        /// <summary>
        /// This constructor initialises the NFC reader with the provided SmartCardReader.
        /// </summary>
        /// <param name="reader"></param>
        protected NFCReader(SmartCardReader reader) : this(reader, new PN533()) { }

        /// <summary>
        /// This constructor finds the first connected reader.
        /// </summary>
        protected NFCReader() { }
        #endregion

        #region Abstraction
        protected abstract byte[] Get_GetUIDCommand();
        protected abstract byte[] Get_LoadAuthenticationKeysCommand();
        protected abstract byte[] Get_ReadBinaryBlocksCommand(byte startingBlock, int length);
        protected abstract byte[] Get_ReadValueBlockCommand(byte block);
        protected abstract byte[] Get_UpdateBinaryBlockCommand(byte blockNumber, byte[] blockData, int numberOfBytesToUpdate);
        protected abstract byte[] Get_DirectTransmitCommand(byte[] payload);
        #endregion

        #region Card connection
        public NFCOperation ConnectCard()
        {
            NFCOperation operation = new NFCOperation();
            _protocol = 0;
            int cardInt = -1;
            operation.Status = Winscard.SCardConnect(_hContext, _readerState.RdrName, Winscard.SCARD_SHARE_SHARED, Winscard.SCARD_PROTOCOL_T0 | Winscard.SCARD_PROTOCOL_T1, ref cardInt, ref _protocol);

            if (operation.Status == Winscard.SCARD_S_SUCCESS)
            {
                // GG: Add code here to identify the card
                _connectedCard = NFCCard.CardHelper.GetCard<Ntag215>(cardInt);
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(operation.Status));
            }
            return operation;
        }
        #endregion

        #region Commands
        public NFCOperation Control(byte[] command, uint controlCode, int responseBufferLength = 255)
        {
            NFCOperation response = new NFCOperation();

            try
            {
                byte[] responseBuffer = new byte[responseBufferLength];
                int responseLength = responseBuffer.Length;
                response.Status = Winscard.SCardControl(ConnectedCard.CardNumber, Winscard.SCARD_CTL_CODE(3500), ref command[0], command.Length, ref responseBuffer[0], responseBuffer.Length, ref responseLength);
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return response;
        }

        public NFCOperation Transmit(byte[] command, int responseBufferLength = 255)
        {
            NFCOperation response = new NFCOperation();
            try
            {
                byte[] responseBuffer = new byte[responseBufferLength];
                int responseLength = responseBuffer.Length;
                response.Status = Winscard.SCardTransmit(_connectedCard.CardNumber, ref _standardRequest, ref command[0], command.Length, ref _standardRequest, ref responseBuffer[0], ref responseLength);
                response.ResponseBuffer = responseBuffer;
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return response;
        }

        public NFCOperation GetCardGuid()
        {
            return Transmit(Get_GetUIDCommand());
        }

        public NFCOperation ReadBlocks(byte startingBlock, int length)
        {
            return Transmit(Get_ReadBinaryBlocksCommand(startingBlock, length));
        }

        public NFCOperation ReadBlocks(byte startingBlock)
        {
            return ReadBlocks(startingBlock, ConnectedCard.MaxReadableBytes);
        }

        public NFCOperation ReadValue(byte block)
        {
            return Transmit(Get_ReadValueBlockCommand(block));
        }

        public NFCOperation WriteBlocks(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate)
        {
            return Transmit(Get_UpdateBinaryBlockCommand(blockNumber, dataIn, numberOfBytesToUpdate));
        }

        public NFCOperation WriteBlocks(byte blockNumber, byte[] dataIn)
        {
            return WriteBlocks(blockNumber, dataIn, _connectedCard.MaxWritableBlocks);
        }

        public void WriteNDEFMessage(string value, int startingBlock)
        {
            try
            {
                NDEFMessage message = new NDEFMessage(value);
                byte[] blockBytes = message.GetFormattedBlock();
                int j = 0;
                for (int i = 0; i < blockBytes.Length; i += _connectedCard.MaxWritableBlocks)
                {
                    WriteBlocks((byte)(startingBlock + j), blockBytes.Skip(j * _connectedCard.MaxWritableBlocks).Take(_connectedCard.MaxWritableBlocks).ToArray());
                    j++;
                }
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
        }

        public void WriteNDEFMessage(string value)
        {
            WriteNDEFMessage(value, 4);
        }

        public NFCOperation TransmitCardCommand(byte[] cardCommand)
        {
            return Transmit(cardCommand);
        }

        public NFCOperation PasswordAuthentication(string password)
        {
            NFCOperation response = new NFCOperation();
            try
            {
                byte[] dataExchangeCommand = _controller.GetDataExchangeCommand();
                byte[] passwordAuthenticationCommand = _connectedCard.GetPasswordAuthenticationCommand(password);
                byte[] directTransmitCommand = Get_DirectTransmitCommand(dataExchangeCommand.Concat(passwordAuthenticationCommand).ToArray());
                response = TransmitCardCommand(directTransmitCommand);
            }   
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return response;
        }

        public NFCOperation GetCardVersion()
        {
            NFCOperation response = new NFCOperation();
            try
            {
                byte[] dataExchangeCommand = _controller.GetDataExchangeCommand();
                byte[] getVersionCommand = _connectedCard.GetGetVersionCommand();
                byte[] directTransmitCommand = Get_DirectTransmitCommand(dataExchangeCommand.Concat(getVersionCommand).ToArray());
                response = TransmitCardCommand(directTransmitCommand);
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return response;
        }

        public NFCOperation TestOperation()
        {
            //return Transmit(new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x04, 0xD4, 0x4A, 0x01, 0x00 }); POLLING: pag. 26 API-ACR122USAM-2.01.pdf
            return Transmit(new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x05, 0xD4, 0x40, 0x01, 0x30, 0x04 });
        }
        #endregion

        #region Aux methods
        private void ManageException(Exception ex)
        {
            if(ActivateSignaling && _signalingControl != null)
            {
                Control(_signalingControl.GetErrorSignalCommand(), _signalingControl.GetControlCode());
            }
        }
        #endregion

        public static class Helper
        {
            public static T GetReader<T>(SmartCardReader reader) where T : NFCReader, new()
            {
                return (T)Activator.CreateInstance(typeof(T), new object[] { reader });
            }

            public async static Task<T> GetReader<T>() where T : NFCReader, new()
            {
                SmartCardReader reader = await GetFirstConnectedDevice();
                return GetReader<T>(reader);
            }

            private static async Task<SmartCardReader> GetFirstConnectedDevice()
            {
                SmartCardReader reader = null;
                try
                {
                    string selector = SmartCardReader.GetDeviceSelector();
                    DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
                    reader = await SmartCardReader.FromIdAsync(devices.First().Id).AsTask();
                }
                catch (Exception) { }
                return reader;
            }
        }        
    }
}
