﻿using CSharp.NFC.Cards;
using CSharp.NFC.Controllers;
using CSharp.NFC.NDEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;

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
        private NFCReader _reader;
        private NFCCard _connectedCard;
        private SmartCardReader _builtinReader;
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
                _reader = this;
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
        protected NFCReader(SmartCardReader reader) : this(reader, new PN532()) { }

        /// <summary>
        /// This constructor finds the first connected reader.
        /// </summary>
        protected NFCReader() { }
        #endregion

        #region Abstraction
        protected abstract NFCCommand Get_GetUIDCommand();
        protected abstract NFCCommand Get_LoadAuthenticationKeysCommand();
        protected abstract NFCCommand Get_ReadBinaryBlocksCommand(byte startingBlock, int length);
        protected abstract NFCCommand Get_ReadValueBlockCommand(byte block);
        protected abstract NFCCommand Get_UpdateBinaryBlockCommand(byte blockNumber, byte[] blockData, int numberOfBytesToUpdate);
        protected abstract NFCCommand Get_DirectTransmitCommand(byte[] payload);
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
                // Add code here to identify the card
                _connectedCard = NFCCard.CardHelper.GetCard<Ntag215>(cardInt);
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(operation.Status));
            }
            return operation;
        }
        #endregion

        #region Low level commands
        public NFCOperation Control(byte[] command, uint controlCode, int responseBufferLength = 255)
        {
            NFCOperation operation = new NFCOperation();

            try
            {
                byte[] responseBuffer = new byte[responseBufferLength];
                int responseLength = responseBuffer.Length;
                operation.Status = Winscard.SCardControl(ConnectedCard.CardNumber, controlCode, ref command[0], command.Length, ref responseBuffer[0], responseBuffer.Length, ref responseLength);
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return operation;
        }        

        public NFCOperation Transmit(NFCOperation operation, int responseBufferLength = 255)
        {
            try
            {
                byte[] responseBuffer = new byte[responseBufferLength];
                int responseLength = responseBuffer.Length;
                operation.Status = Winscard.SCardTransmit(_connectedCard.CardNumber, ref _standardRequest, ref operation.WrappedCommand[0], operation.WrappedCommand.Length, ref _standardRequest, ref responseBuffer[0], ref responseLength);
                operation.ResponseBuffer = responseBuffer;
                operation.ElaborateResponse();
            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
            return operation;
        }

        public NFCOperation TransmitDirectCommand(byte[] directCommand)
        {
            return Transmit(new NFCOperation(Get_DirectTransmitCommand(directCommand)));
        }
        #endregion

        #region Reading commands
        public NFCOperation GetCardGuid()
        {
            return Transmit(new NFCOperation(Get_GetUIDCommand()));
        }

        public NFCOperation ReadBlocks(byte startingBlock, int length)
        {
            return Transmit(new NFCOperation(Get_ReadBinaryBlocksCommand(startingBlock, length)));
        }

        public NFCOperation ReadBlocks(byte startingBlock)
        {
            return ReadBlocks(startingBlock, ConnectedCard.MaxReadableBytes);
        }

        public NFCOperation ReadValue(byte block)
        {
            return Transmit(new NFCOperation(Get_ReadValueBlockCommand(block)));
        }

        public NDEFOperation GetNDEFMessagesOperation()
        {
            NDEFOperation ndefOperation = new NDEFOperation();
            try
            {
                NFCOperation nfcOperation = ReadBlocks(4);
                ndefOperation.Operations.Add(nfcOperation);
                byte[] bytesToRead = nfcOperation.ReaderCommand.Payload.PayloadBytes;
                NDEFMessage message = NDEFMessage.GetNDEFMessageFromBytes(bytesToRead);
                bytesToRead = bytesToRead.Skip(message.TotalHeaderLength).ToArray();
                if (message.Length > 0)
                {
                    int i = 2;
                    while (message.ReadByesIntoMessage(bytesToRead))
                    {
                        nfcOperation = ReadBlocks((byte)(4 * i));
                        bytesToRead = nfcOperation.ReaderCommand.Payload.PayloadBytes;
                        ndefOperation.Operations.Add(nfcOperation);
                        i++;
                    }
                }
                ndefOperation.NDEFMessage = message;
            }
            catch(Exception ex)
            {

            }
            return ndefOperation;
        }

        public NDEFPayload GetNDEFPayload()
        {
            NDEFPayload payload = new NDEFPayload();
            NDEFOperation operation = GetNDEFMessagesOperation();
            payload = operation.NDEFMessage.Record?.RecordType.GetPayload();
            return payload;
        }
        #endregion

        #region Writing commands
        private NFCOperation WriteBlocks(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate)
        {
            return Transmit(new NFCOperation(Get_UpdateBinaryBlockCommand(blockNumber, dataIn, numberOfBytesToUpdate)));
        }

        private NFCOperation WriteBlocks(byte blockNumber, byte[] dataIn)
        {
            return WriteBlocks(blockNumber, dataIn, _connectedCard.MaxWritableBlocks);
        }

        public List<NFCOperation> WriteBlocks(byte[] bytes, int startingPage, string password = "")
        {
            List<NFCOperation> operations = new List<NFCOperation>();
            int j = 0;
            for (int i = 0; i < bytes.Length; i += _connectedCard.MaxWritableBlocks)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    Authenticate(password);
                }                
                operations.Add(WriteBlocks((byte)(startingPage + j), bytes.Skip(j * _connectedCard.MaxWritableBlocks).Take(_connectedCard.MaxWritableBlocks).ToArray()));
                j++;
            }
            return operations;
        }

        private List<NFCOperation> WriteTextNDEFMessage(byte[] textBytes, int startingPage, string password = "")
        {
            List<NFCOperation> operations = null;
            try
            {
                NDEFMessage message = new NDEFMessage(textBytes, NDEFRecordType.Types.Text);
                byte[] blockBytes = message.GetFormattedBlock();
                if (startingPage < _connectedCard.FirstUserDataMemoryPage)
                {
                    throw new Exception($"Invalid starting write page. The first available user data memory page for the connected card is {_connectedCard.FirstUserDataMemoryPage}");
                }
                if (blockBytes.Length > ((_connectedCard.LastUserDataMemoryPage + 1 - startingPage) * 4))
                {
                    throw new Exception($"Byte buffer to write length is higher than the connect card memory capacity.");
                }
                operations = WriteBlocks(blockBytes, startingPage, password);
            }
            catch(Exception ex)
            {
                ManageException(ex);
            }
            return operations;
        }

        public List<NFCOperation> WriteTextNDEFMessage(string value, int startingPage)
        {
            return WriteTextNDEFMessage(Encoding.ASCII.GetBytes(value), startingPage);
        }

        public List<NFCOperation> WriteTextNDEFMessage(string value)
        {
            return WriteTextNDEFMessage(Encoding.ASCII.GetBytes(value), _connectedCard.FirstUserDataMemoryPage);
        }

        public List<NFCOperation> WriteTextNDEFMessage(byte[] textByte, string password)
        {
            return WriteTextNDEFMessage(textByte, 4, password);
        }

        public List<NFCOperation> WriteTextNDEFMessage(byte[] textByte)
        {
            return WriteTextNDEFMessage(textByte, string.Empty);
        }
        #endregion

        #region Card specific commands
        private NFCOperation TransmitCardCommand(NFCCommand communicationCommand, NFCCommand cardCommand)
        {
            NFCOperation operation = null;
            try
            {
                NFCCommand dataExchangeCommand = _controller.GetDataExchangeCommand();
                NFCCommand directTransmitCommand = _reader.Get_DirectTransmitCommand(communicationCommand.CommandBytes.Concat(cardCommand.CommandBytes).ToArray());
                operation = new NFCOperation(directTransmitCommand, dataExchangeCommand, cardCommand, directTransmitCommand.CommandBytes);
                operation = Transmit(operation);
            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
            return operation;
            //return Transmit(operation);
        }

        private NFCOperation TransmitCardCommandWithDataExchange(NFCCommand cardCommand)
        {
            NFCCommand dataExchangeCommand = _controller.GetDataExchangeCommand();
            return TransmitCardCommand(dataExchangeCommand, cardCommand);
        }

        private NFCOperation TransmitCardCommandWithInCommunicateThru(NFCCommand cardCommand)
        {
            NFCCommand inCommunicateThru = _controller.GetInCommunicateThruCommand();
            return TransmitCardCommand(inCommunicateThru, cardCommand);
        }

        public NFCOperation Authenticate(string password)
        {
            return TransmitCardCommandWithInCommunicateThru(_connectedCard.GetPasswordAuthenticationCommand(password));
        }

        public NFCOperation GetCardVersion()
        {
            return TransmitCardCommandWithDataExchange(_connectedCard.GetGetVersionCommand());
        }

        public List<NFCOperation> SetupCardSecurityConfiguration(byte[] securityConfigurationBytes)
        {
            return WriteBlocks(securityConfigurationBytes, _connectedCard.UserConfigurationStartingPage);
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
