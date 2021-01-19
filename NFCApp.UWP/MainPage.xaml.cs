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
using CSharp.NFC.Readers;
using CSharp.NFC;

namespace NFCApp.UWP
{
    public sealed partial class MainPage
    {
        private NFCReader TicketValidator;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeValidator();
            //LoadApplication(new NFCApp.App());
        }

        private async void InitializeValidator()
        {
            TicketValidator = await NFCReader.Helper.GetReader<ACR122>();
            TicketValidator.BuiltinReader.CardAdded += CardAdded;
            IReadOnlyList<SmartCard> cards = await TicketValidator.BuiltinReader.FindAllCardsAsync();
            if (cards.Count > 0)
            {
                TicketValidator.ConnectCard();
                WriteCardInfo(cards.First());
            }
        }


        private void CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            SmartCard card = args.SmartCard;
            TicketValidator.ConnectCard();
            WriteCardInfo(card);
        }

        private async void WriteCardInfo(SmartCard card)
        {
            try
            {                               
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
                WriteMessageAsync(txtMessages, GetCardUID());
            }
            catch (Exception ex)
            {
                ManageExceptionAsync(ex);
            }           
        }

        private string GetCardUID()
        {
            string cardUID;
            NFCOperation response = TicketValidator.GetCardGuid();

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
            WriteMessageAsync(txtRead, string.Empty);
            WriteMessageAsync(txtReadBlocks, string.Empty);
            string inputBlock = txtInputBlock.Text;
            NFCOperation readValueResponse = TicketValidator.ReadValue((byte)int.Parse(inputBlock));
            NFCOperation readBlocksResponse = TicketValidator.ReadBlocks((byte)int.Parse(inputBlock));
            if (readBlocksResponse.Status == Winscard.SCARD_S_SUCCESS)
            {
                int i = 0;
                int j = 0;
                string decimals = string.Empty;
                string characters = string.Empty;
                string hexadecimals = string.Empty;
                foreach(byte b in readBlocksResponse.ResponseBuffer)
                {
                    //data = $"{data}{(char)b}";                                       
                    hexadecimals = $"{hexadecimals}{(!string.IsNullOrEmpty(characters) ? " - " : string.Empty)}{b.ToString("X2")}";
                    characters = $"{characters}{(!string.IsNullOrEmpty(characters) ? " - " : string.Empty)}{i++}[{(b == 0x90 ? "END" : ((char)b).ToString())}]";
                    decimals = $"{decimals}{(!string.IsNullOrEmpty(characters) ? " - " : string.Empty)}{b}{(i % 4 == 0 ? Environment.NewLine : string.Empty)}";
                }
                j++;
                WriteMessageAsync(txtRead, decimals);
                WriteMessageAsync(txtReadBlocks, characters);
                WriteMessageAsync(txtReadBlocks, hexadecimals);
                AppendMessageAsync(txtReadBlocks, System.Text.Encoding.ASCII.GetString(readBlocksResponse.ResponseBuffer));
                AppendMessageAsync(txtReadBlocks, System.Text.Encoding.UTF8.GetString(readBlocksResponse.ResponseBuffer));
                AppendMessageAsync(txtReadBlocks, System.Text.Encoding.Unicode.GetString(readBlocksResponse.ResponseBuffer));
            }
        }

        private void btnTestOperation_Click(object sender, RoutedEventArgs e)
        {
            //NFCOperation response = TicketValidator.TestOperation();
            NFCOperation operation = TicketValidator.GetCardVersion();
            //WriteMessageAsync(txtTestOperation, !string.IsNullOrEmpty(operation.ResponsePayloadText));
            //TicketValidator.WriteNDEFMessage(txtInput.Text);            
        }

        #region AuxMethods
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
        #endregion

        #region ByteBufferUtilities
        private string GetBytesHexAsString(byte[] buffer)
        {
            return BitConverter.ToString(buffer.ToArray());
        }

        private string GetBytesAsString(byte[] buffer)
        {
            return string.Join('-', buffer);
        }
        #endregion
    }
}
