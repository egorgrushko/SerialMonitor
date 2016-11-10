using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Serial_Monitor
{
    public partial class SerialMonitorControl : UserControl
    {
        private SerialPort port;

        private void ConfigurePort()
        {
            port.PortName = ComPorts.SelectedItem.ToString();
            port.BaudRate = Settings.BaudRate;
            port.DataBits = Settings.DataBits;
            port.Handshake = Settings.Handshake;
            port.Parity = Settings.Parity;
            port.StopBits = Settings.StopBits;
            port.ReadTimeout = Settings.ReadTimeout;
            port.WriteTimeout = Settings.WriteTimeout;
        }

        private void PrintColorMessage(string message, SolidColorBrush brush)
        {
            Output.AppendText(message + Environment.NewLine, brush, Settings.OutputFontSize);

            if (autoScrollEnabled == true)
            {
                Output.ScrollToEnd();
            }

            if (Settings.OutputToFileEnabled)
            {
                string file = Settings.RecordFile;

                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    File.AppendAllText(file, message);
                }
            }
        }

        private void PrintErrorMessage(string message)
        {
            PrintColorMessage(message, Brushes.Red);
        }

        private void PrintWarningMessage(string message)
        {
            PrintColorMessage(message, Brushes.Yellow);
        }

        private void PrintSuccessMessage(string message)
        {
            PrintColorMessage(message, Brushes.Green);
        }

        private void PrintProcessMessage(string message)
        {
            PrintColorMessage(message, Brushes.Aqua);
        }

        private void SerialUpdate(object e, EventArgs s)
        {
            try
            {
                int bytesToRead = port.BytesToRead;

                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    port.Read(buffer, 0, bytesToRead);

                    string data = Settings.Encoding.GetString(buffer);
                    Output.AppendText(data.Replace(Settings.ReceiveNewLine, "\r"), Settings.OutputFontSize);

                    if (autoScrollEnabled == true)
                    {
                        Output.ScrollToEnd();
                    }

                    if (Settings.OutputToFileEnabled)
                    {
                        string file = Settings.RecordFile;

                        if (!string.IsNullOrEmpty(file) && File.Exists(file))
                        {
                            File.AppendAllText(file, data.Replace(Settings.ReceiveNewLine, Environment.NewLine));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(Environment.NewLine + ex.Message);
            }
        }

        private DispatcherTimer portHandlerTimer;
        private bool autoScrollEnabled = true;

        public SerialMonitorControl()
        {
            this.InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

            port = new SerialPort();

            portHandlerTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, Application.Current.Dispatcher);
            portHandlerTimer.Tick += SerialUpdate;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            portHandlerTimer.Stop();
            port.Dispose();
        }

        private void SettingsOutputControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Settings.Visibility == Visibility.Visible)
            {
                Settings.Visibility = Visibility.Collapsed;
                Output.Visibility = Visibility.Visible;
                SettingsOutputControl.Content = "Show Settings";
            }
            else if (Settings.Visibility == Visibility.Collapsed)
            {
                Settings.Visibility = Visibility.Visible;
                Output.Visibility = Visibility.Collapsed;
                SettingsOutputControl.Content = "Show Output";
            }
        }

        private void Connect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Visibility = Visibility.Collapsed;
            Output.Visibility = Visibility.Visible;
            SettingsOutputControl.Content = "Show Settings";

            if (ComPorts.SelectedIndex != -1)
            {
                try
                {
                    PrintProcessMessage("Configuring port...");
                    ConfigurePort();

                    PrintProcessMessage("Connecting...");
                    port.Open();

                    if (Settings.DtrEnable == true)
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

                    Settings.IsEnabled = false;

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
                PrintProcessMessage(Environment.NewLine + "Closing port...");

                portHandlerTimer.Stop();

                port.Close();

                ConnectButton.Visibility = Visibility.Visible;
                DisconnectButton.Visibility = Visibility.Collapsed;
                ReconnectButton.Visibility = Visibility.Collapsed;
                ComPorts.IsEnabled = true;

                MessageToSend.IsEnabled = false;
                SendButton.IsEnabled = false;

                Settings.IsEnabled = true;

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
            Output.Clear();
        }

        private void Send_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                byte[] data = Encoding.Convert(
                    Encoding.Default,
                    Settings.Encoding,
                    Encoding.Default.GetBytes(MessageToSend.Text + Settings.SendNewLine));

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

        private void MessageToSend_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Send_Click(null, null);
                MessageToSend.Text = "";
            }
        }

        private void AutoScrollToggle_Click(object sender, RoutedEventArgs e)
        {
            autoScrollEnabled = !autoScrollEnabled;

            if (autoScrollEnabled)
            {
                AutoScrollToggle.Content = "Disable Auto Scroll";
            }
            else
            {
                AutoScrollToggle.Content = "Enable Auto Scroll";
            }
        }
    }
}
