using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace toioBleWebSocketBridge
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Task CommunicationTask = null;

        private MessageWebSocket messageWebSocket;

        private static string WebSocketUri = "ws://127.0.0.1:12345";


        public MainPage()
        {
            this.InitializeComponent();

            this.TextBlock_Status.Text = "Not connected";

            Connect();
        }

        private void NewToioFound()
        {
            Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.TextBlock_Status.Text = $"Toio count: {ToioBridge.ToioDeviceManager.Instance.GetToioCount()}";
                });
            });
            
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //            ToioDeviceManager m
            ToioBridge.ToioDeviceManager.Instance.Search(3000, NewToioFound);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //CommunicationTask.Close();
        }

        private void Connect()
        {
            Debug.WriteLine("OnConnect()");

            messageWebSocket = new MessageWebSocket();

            //In this case we will be sending/receiving a string so we need to set the MessageType to Utf8.
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;

            //Add the MessageReceived event handler.
            messageWebSocket.MessageReceived += MessageWebSocket_MessageReceived;

            //Add the Closed event handler.
            messageWebSocket.Closed += MessageWebSocket_Closed;

            Uri serverUri = new Uri(WebSocketUri);

            try
            {
                Task.Run(async () => {
                    //Connect to the server.
                    Debug.WriteLine("Connect to the server...." + serverUri.ToString());
                    await Task.Run(async () =>
                    {
                        await messageWebSocket.ConnectAsync(serverUri);
                        Debug.WriteLine("ConnectAsync OK");

                        //await WebSock_SendMessage(messageWebSocket, "Connect Start");

                        //= JsonSerializer.Serialize("");
                        await WebSock_SendMessage(messageWebSocket, "C#");
                    });

                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error : " + ex.ToString());

                //Add code here to handle any exceptions
            }

        }

        private void MessageWebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            Debug.WriteLine("MessageWebSocket_Closed()");
        }

        private void MessageWebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            DataReader messageReader = args.GetDataReader();
            messageReader.UnicodeEncoding = UnicodeEncoding.Utf8;
            string messageString = messageReader.ReadString(messageReader.UnconsumedBufferLength);
            Debug.WriteLine("messageString : " + messageString);

            if (messageString == "G")
            {
                for (int i = 0; i < ToioBridge.ToioDeviceManager.Instance.GetToioCount(); i++)
                {
                    ToioBridge.Toio toio = ToioBridge.ToioDeviceManager.Instance.GetToio(i);
                    toio.MotorControl(true, 100, true, 100);
                }
            }
            else if (messageString == "B")
            {
                for (int i = 0; i < ToioBridge.ToioDeviceManager.Instance.GetToioCount(); i++)
                {
                    ToioBridge.Toio toio = ToioBridge.ToioDeviceManager.Instance.GetToio(i);
                    toio.MotorControl(false, 100, false, 100);
                }
            }
            else if (messageString == "R")
            {
                for (int i = 0; i < ToioBridge.ToioDeviceManager.Instance.GetToioCount(); i++)
                {
                    ToioBridge.Toio toio = ToioBridge.ToioDeviceManager.Instance.GetToio(i);
                    toio.MotorControl(true, 100, false, 100);
                }
            }
            else if (messageString == "L")
            {
                for (int i = 0; i < ToioBridge.ToioDeviceManager.Instance.GetToioCount(); i++)
                {
                    ToioBridge.Toio toio = ToioBridge.ToioDeviceManager.Instance.GetToio(i);
                    toio.MotorControl(false, 100, true, 100);
                }
            }
            else if (messageString == " ")
            {
                for (int i = 0; i < ToioBridge.ToioDeviceManager.Instance.GetToioCount(); i++)
                {
                    ToioBridge.Toio toio = ToioBridge.ToioDeviceManager.Instance.GetToio(i);
                    toio.MotorControl(true, 0, true, 0);
                }
            }
            //Add code here to do something with the string that is received.

            Task.Run(async () =>
            {

                //UnityEngine.WSA.Application.InvokeOnAppThread(() => {

                //    Debug.WriteLine("InvokeOnAppThread : iTween");

                //    // WebSocket受信時に回転が変わる
                //    iTween.RotateTo(this.gameObject, iTween.Hash(
                //        "x", UnityEngine.Random.Range(1, 5) * 20,
                //        "y", UnityEngine.Random.Range(1, 5) * 20,
                //        "z", UnityEngine.Random.Range(1, 5) * 20,
                //        "time", 2f
                //    ));

                //    // WebSocket受信時に大きさが変わる
                //    var scale = UnityEngine.Random.Range(1, 5) * 0.02;
                //    iTween.ScaleTo(this.gameObject, iTween.Hash(
                //        "x", originalScale.x + scale,
                //        "y", originalScale.y + scale,
                //        "z", originalScale.z + scale,
                //        "time", 2f
                //    ));

                //}, true);

                await Task.Delay(100);
            });
        }

        private async Task WebSock_SendMessage(MessageWebSocket webSock, string message)
        {
            Debug.WriteLine("WebSock_SendMessage : " + message);

            DataWriter messageWriter = new DataWriter(webSock.OutputStream);
            messageWriter.WriteString(message);
            await messageWriter.StoreAsync();
        }

    }
}
