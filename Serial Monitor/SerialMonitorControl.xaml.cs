using System;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Serial_Monitor
{
    public partial class SerialMonitorControl : UserControl
    {
        private SerialMonitorControlSettings settings;
        private SerialPort port;

        private void ConfigurePort()
        {
            port.PortName = ComPorts.SelectedItem.ToString();
            port.BaudRate = settings.BaudRate;
            port.DataBits = settings.DataBits;
            port.Handshake = settings.Handshake;
            port.Parity = settings.Parity;
            port.StopBits = settings.StopBits;
            port.ReadTimeout = settings.ReadTimeout;
            port.WriteTimeout = settings.WriteTimeout;
        }

        private void ReceiveByte(byte data)
        {
            Output.AppendText(settings.Encoding.GetString(new byte[] { data }));
        }

        private void ReceiveNewLine()
        {
            Output.Document.ContentEnd.InsertLineBreak();
        }

        private void PrintColorMessage(string message, SolidColorBrush brush, bool withNewLine = false)
        {
            if (withNewLine)
            {
                Output.Document.ContentEnd.InsertLineBreak();
            }
            TextRange text = new TextRange(Output.Document.ContentEnd.DocumentEnd, Output.Document.ContentEnd.DocumentEnd);
            text.Text = message;
            Output.Document.ContentEnd.InsertLineBreak();
            text.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            text.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Oblique);
        }

        private void PrintErrorMessage(string message, bool withNewLine = false)
        {
            PrintColorMessage(message, Brushes.Red, withNewLine);
        }

        private void PrintWarningMessage(string message, bool withNewLine = false)
        {
            PrintColorMessage(message, Brushes.Yellow, withNewLine);
        }

        private void PrintSuccessMessage(string message, bool withNewLine = false)
        {
            PrintColorMessage(message, Brushes.Green, withNewLine);
        }

        private void PrintProcessMessage(string message, bool withNewLine = false)
        {
            PrintColorMessage(message, Brushes.Aqua, withNewLine);
        }

        private void SerialUpdate(object e, EventArgs s)
        {
            try
            {
                if (port.BytesToRead > 0)
                {
                    byte character = (byte)port.ReadByte();
                    if (character == settings.ReceiveNewLine[0])
                    {
                        for (int i = 1; i < settings.ReceiveNewLine.Length; i++)
                        {
                            int newLineCharacter = port.ReadByte();
                            if (newLineCharacter == -1)
                            {
                                ReceiveByte(character);
                                return;
                            }
                            else if ((char)newLineCharacter != settings.ReceiveNewLine[i])
                            {
                                ReceiveByte(character);
                                ReceiveByte((byte)newLineCharacter);
                                return;
                            }
                        }

                        ReceiveNewLine();
                    }
                    else
                    {
                        ReceiveByte(character);
                    }
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message, true);
            }
        }

        private DispatcherTimer portHandlerTimer;

        public SerialMonitorControl()
        {
            this.InitializeComponent();
            settings = new SerialMonitorControlSettings(this);
            settings.Fill();

            port = new SerialPort();

            portHandlerTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, Application.Current.Dispatcher);
            portHandlerTimer.Tick += SerialUpdate;
        }

        public void Dispose()
        {
            portHandlerTimer.Stop();
            port.Close();
            port.Dispose();
        }

        private void SettingsOutputControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (settings.IsOpen)
            {
                settings.Hide();
            }
            else
            {
                settings.Show();
            }
        }

        private void Connect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settings.Hide();

            if (ComPorts.SelectedIndex != -1)
            {
                try
                {
                    PrintProcessMessage("Configuring port...");
                    ConfigurePort();

                    PrintProcessMessage("Connecting...");
                    port.Open();

                    if (settings.DtrEnable == true)
                    {
                        port.DtrEnable = true;
                        port.DiscardInBuffer();
                        Thread.Sleep(1000);
                        port.DtrEnable = false;
                    }

                    ConnectButton.Visibility = Visibility.Collapsed;
                    DisconnectButton.Visibility = Visibility.Visible;
                    ReconnectButton.Visibility = Visibility.Visible;
                    ComPorts.IsEnabled = false;

                    MessageToSend.IsEnabled = true;
                    SendButton.IsEnabled = true;

                    settings.Lock();

                    PrintSuccessMessage("Connected!");
                    portHandlerTimer.Start();
                }
                catch (Exception ex)
                {
                    PrintErrorMessage(ex.Message);
                }
            }
            else
            {
                PrintErrorMessage("COM Port not selected!");
            }
        }

        private void Disconnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                PrintProcessMessage("Closing port...", true);

                portHandlerTimer.Stop();

                port.Close();

                ConnectButton.Visibility = Visibility.Visible;
                DisconnectButton.Visibility = Visibility.Collapsed;
                ReconnectButton.Visibility = Visibility.Collapsed;
                ComPorts.IsEnabled = true;

                MessageToSend.IsEnabled = false;
                SendButton.IsEnabled = false;

                settings.Unlock();

                PrintSuccessMessage("Port closed!");
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }
        }

        private void Reconnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Disconnect_Click(null, null);
            Connect_Click(null, null);
        }

        private void Clear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Output.Document.Blocks.Clear();
        }

        private void Send_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                byte[] data = Encoding.Convert(
                    Encoding.Default,
                    settings.Encoding,
                    Encoding.Default.GetBytes(MessageToSend.Text + settings.SendNewLine));

                port.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }
        }

        private void ComPorts_DropDownOpened(object sender, EventArgs e)
        {
            string selectedPort = null;
            if (ComPorts.SelectedIndex != -1)
            {
                selectedPort = ComPorts.SelectedItem.ToString();
            }
            ComPorts.Items.Clear();

            foreach (string portName in SerialPort.GetPortNames())
            {
                ComPorts.Items.Add(portName);
            }

            if (selectedPort != null && ComPorts.Items.Contains(selectedPort))
            {
                ComPorts.SelectedItem = selectedPort;
            }
        }

        private void Output_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoscrollCheck.IsChecked == true)
            {
                Output.ScrollToEnd();
            }
        }

        private void MessageToSend_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Send_Click(null, null);
                MessageToSend.Text = "";
            }
        }
    }
}
