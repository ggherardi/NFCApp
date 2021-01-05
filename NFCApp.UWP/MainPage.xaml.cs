using NFCApp.UWP.SmartCards;
using Poz1.NFCForms.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;
using Windows.Storage.Streams;

namespace NFCApp.UWP
{
    public sealed partial class MainPage
    {
        private Card _connectedCard;
        private Winscard.SCARD_READERSTATE _readerState;
        private int _hContext = 0;
        private int _protocol = 0;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitSmartCardReaders();
            //LoadApplication(new NFCApp.App());
        }

        private async void InitSmartCardReaders()
        {
            string selector = SmartCardReader.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            foreach (DeviceInformation device in devices)
            {
                InitReader(device);
            }
        }

        private async void InitReader(DeviceInformation device)
        {
            try
            {
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                this._readerState = new Winscard.SCARD_READERSTATE();
                this._readerState.RdrName = reader.Name;
                int retCode = Winscard.SCardEstablishContext(Winscard.SCARD_SCOPE_SYSTEM, 0, 0, ref _hContext);
                if (retCode != Winscard.SCARD_S_SUCCESS)
                {
                    throw new Exception(Winscard.GetScardErrMsg(retCode));
                }
                reader.CardAdded += CardAdded;
            }
            catch(Exception ex)
            {
                ManageExceptionAsync(ex);
            }
        }

        private async void CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            try
            {
                var card = args.SmartCard;
                var atr = await card.GetAnswerToResetAsync();
                byte[] atrBytes = atr.ToArray();
                WriteMessageAsync(txtATR, string.Empty);
                AppendMessageAsync(txtATR, $"{atrBytes[0]:X2} - Initial Header");
                AppendMessageAsync(txtATR, $"{atrBytes[1]:X2} - T0");
                AppendMessageAsync(txtATR, $"{atrBytes[2]:X2} - TD1");
                AppendMessageAsync(txtATR, $"{atrBytes[3]:X2} - TD2");
                AppendMessageAsync(txtATR, $"{atrBytes[4]:X2} - T1");
                AppendMessageAsync(txtATR, $"{atrBytes[5]:X2} - TK (Application identifier Presence Indicator)");
                AppendMessageAsync(txtATR, $"{atrBytes[6]:X2} - TK (Length)");
                AppendMessageAsync(txtATR, $"{atrBytes[7]:X2} - TK - RID");
                AppendMessageAsync(txtATR, $"{atrBytes[8]:X2} - TK - RID");
                AppendMessageAsync(txtATR, $"{atrBytes[9]:X2} - TK - RID");
                AppendMessageAsync(txtATR, $"{atrBytes[10]:X2} - TK - RID");
                AppendMessageAsync(txtATR, $"{atrBytes[11]:X2} - TK - RID");
                AppendMessageAsync(txtATR, $"{atrBytes[12]:X2} - TK (Bytes for standard)");
                AppendMessageAsync(txtATR, $"{atrBytes[13]:X2} - TK (Bytes for card name)");
                AppendMessageAsync(txtATR, $"{atrBytes[14]:X2} - TK (Bytes for card name)");
                AppendMessageAsync(txtATR, $"{atrBytes[15]:X2} - RFU");
                AppendMessageAsync(txtATR, $"{atrBytes[16]:X2} - RFU");
                AppendMessageAsync(txtATR, $"{atrBytes[17]:X2} - RFU");
                AppendMessageAsync(txtATR, $"{atrBytes[18]:X2} - RFU");
                AppendMessageAsync(txtATR, $"{atrBytes[19]:X2} - Exclusive-oring of all the bytes T0 to Tk");

                var status = await card.GetStatusAsync();
                WriteMessageAsync(txtStatus, status.ToString());
                ConnectCard();
                WriteMessageAsync(txtMessages, GetCardUID());
            }
            catch(Exception ex)
            {
                ManageExceptionAsync(ex);
            }
        }

        private void ConnectCard()
        {
            int cardInt = -1;
            int retCode = Winscard.SCardConnect(_hContext, _readerState.RdrName, Winscard.SCARD_SHARE_SHARED, Winscard.SCARD_PROTOCOL_T0 | Winscard.SCARD_PROTOCOL_T1, ref cardInt, ref _protocol);

            if (retCode == Winscard.SCARD_S_SUCCESS)
            {
                // GG: Add code here to switch between cards
                _connectedCard = new MIFARE(new IntPtr(cardInt));
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(retCode));
            }
        }

        private string GetCardUID()
        {
            string cardUID;
            CardResponse response = _connectedCard.GetGuid();

            if (response.Status == Winscard.SCARD_S_SUCCESS)
            {
                cardUID = BitConverter.ToString(response.ResponseBuffer.ToArray()).Replace("-", string.Empty).ToLower();
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(response.Status));
            }
            return cardUID;
        }

        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            ReadCard();
        }

        private void ReadCard()
        {

            //byte[] responseBuffer = new byte[256];
            //int bytesReturned = -1;
            //for (int i = 0; i < 10; i++)
            //{
            //int status = Winscard.SCardControl(_connectedCard.CardNumber, Winscard.SCARD_CTL_CODE(3500), ref ACR122.GetGreenBlinking(1)[0], ACR122.GetGreenBlinking().Length, ref responseBuffer[0], responseBuffer.Length, ref bytesReturned);
            //}
            WriteMessageAsync(txtRead, string.Empty);
            WriteMessageAsync(txtReadBlocks, string.Empty);
            string inputBlock = txtInputBlock.Text;
            CardResponse readValueResponse = _connectedCard.ReadValue((byte)int.Parse(inputBlock));
            CardResponse readBlocksResponse = _connectedCard.ReadBlocks((byte)int.Parse(inputBlock));
            if (readValueResponse.Status == Winscard.SCARD_S_SUCCESS)
            {
                string data = string.Empty;
                foreach (byte b in readValueResponse.ResponseBuffer)
                {
                    //data = $"{data}{(char)b}";
                    data = $"{data}-{b}";
                }
                string blocks = string.Empty;
                foreach(byte b in readBlocksResponse.ResponseBuffer)
                {
                    //data = $"{data}{(char)b}";
                    blocks = $"{blocks}-{(char)b}";
                }
                WriteMessageAsync(txtRead, data);
                WriteMessageAsync(txtReadBlocks, blocks);
            }
        }

        private async void ManageExceptionAsync(Exception ex)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtMessages.Text = ex.Message;
            });
        }

        private async void WriteMessageAsync(TextBlock textBlock, string message)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Text = message;
            });
        }

        private async void AppendMessageAsync(TextBlock textBlock, string message)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Text = $"{textBlock.Text}{Environment.NewLine}{message}" ;
            });
        }
    }
}
