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
                _connectedCard = new MIFARE(cardInt);
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(retCode));
            }
        }

        private string GetCardUID()
        {
            string cardUID;
            byte[] receivedUID = new byte[256];
            Winscard.SCARD_IO_REQUEST request = new Winscard.SCARD_IO_REQUEST()
            {
                dwProtocol = Winscard.SCARD_PROTOCOL_T1,
                cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Winscard.SCARD_IO_REQUEST))
            };
            byte[] sendBytesBuffer = _connectedCard.Get_GetUIDCommand();
            int outBytes = receivedUID.Length;
            int status = Winscard.SCardTransmit(_connectedCard.CardNumber, ref request, ref sendBytesBuffer[0], sendBytesBuffer.Length, ref request, ref receivedUID[0], ref outBytes);

            if (status == Winscard.SCARD_S_SUCCESS)
            {
                cardUID = BitConverter.ToString(receivedUID.ToArray()).Replace("-", string.Empty).ToLower();
            }
            else
            {
                throw new Exception(Winscard.GetScardErrMsg(status));
            }
            return cardUID;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void btnReadCard_Click(object sender, RoutedEventArgs e)
        //{
        //    ReadCard();
        //}

        //private void ReadCard()
        //{
        //    byte[] responseBuffer = new byte[256];
        //    int bytesReturned = -1;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        int status = Winscard.SCardControl(_connectedCard.CardNumber, Winscard.SCARD_CTL_CODE(3500), ref ACR122.GetGreenBlinking(1)[0], ACR122.GetGreenBlinking().Length, ref responseBuffer[0], responseBuffer.Length, ref bytesReturned);
        //    }
        //}

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
