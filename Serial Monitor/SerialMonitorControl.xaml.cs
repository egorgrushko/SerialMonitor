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

        private void PrintColorMessage(string message, SolidColorBrush brush)
        {

          Dispatcher.Invoke(
            () => {
              var fontSize = Settings.OutputFontSize;
              Output.AppendText(message + Environment.NewLine, brush, fontSize);
            });

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
            PrintColorMessage(message, Brushes.LightGreen);
        }

        private void PrintProcessMessage(string message)
        {
            PrintColorMessage(message, Brushes.Aqua);
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e) {

          try
          {
            var p = sender as SerialPort;
            int bytesToRead = p.BytesToRead;

            if (bytesToRead > 0)
            {
              byte[] buffer = new byte[bytesToRead];
              p.Read(buffer, 0, bytesToRead);



              Dispatcher.Invoke(
                () => {
                  string data = Settings.Encoding.GetString(buffer);
                  var fontSize = Settings.OutputFontSize;
                  var newLine = Settings.ReceiveNewLine;
                  Output.AppendText(data.Replace(newLine, "\r"), fontSize);

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
                );


            } else {
              var data = p.ReadExisting();


              Dispatcher.Invoke(
                () => {
                  var fontSize = Settings.OutputFontSize;
                  var newLine = Settings.ReceiveNewLine;

                  Output.AppendText(data.Replace(newLine, "\r"), fontSize);

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
                );


            }
          }
          catch (Exception ex)
          {
            PrintErrorMessage(Environment.NewLine + ex.Message);
          }
        }
        private void ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
          try
          {
            var p = sender as SerialPort;
            int bytesToRead = port.BytesToRead;

            if (bytesToRead > 0)
            {
              byte[] buffer = new byte[bytesToRead];
              p.Read(buffer, 0, bytesToRead);

              Dispatcher.Invoke(
                () => {
                  string data = Settings.Encoding.GetString(buffer);

                  PrintErrorMessage(data);

                  if (autoScrollEnabled == true) {
                    Output.ScrollToEnd();
                  }

                  if (Settings.OutputToFileEnabled) {
                    string file = Settings.RecordFile;

                    if (!string.IsNullOrEmpty(file) && File.Exists(file)) {
                      File.AppendAllText(file,
                        data.Replace(Settings.ReceiveNewLine, Environment.NewLine)
                        );
                    }
                  }
                });
            } else {
              var data = p.ReadExisting();
              PrintErrorMessage(data);

              Dispatcher.Invoke(
                () => {
                  if (autoScrollEnabled == true) {
                    Output.ScrollToEnd();
                  }

                  if (Settings.OutputToFileEnabled) {
                    string file = Settings.RecordFile;

                    if (!string.IsNullOrEmpty(file) && File.Exists(file)) {
                      File.AppendAllText(file,
                        data.Replace(Settings.ReceiveNewLine, Environment.NewLine)
                        );
                    }
                  }
                });
            }

          }
          catch (Exception ex)
          {
            PrintErrorMessage(Environment.NewLine + ex.Message);
          }
        }


        private bool autoScrollEnabled = true;

        public SerialMonitorControl()
        {
            this.InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;


            //portHandlerTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, Application.Current.Dispatcher);
            //portHandlerTimer.Tick += SerialUpdate;
        }



        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
          port?.Dispose();
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
                    port = new SerialPort(
                      ComPorts.SelectedItem.ToString(),
                      Settings.BaudRate,
                      Settings.Parity,
                      Settings.DataBits,
                      Settings.StopBits);

                    port.DataReceived += DataReceived;
                    port.ErrorReceived += ErrorReceived;

          port.Handshake = Settings.Handshake;
          port.ReadTimeout = Settings.ReadTimeout;
          port.WriteTimeout = Settings.WriteTimeout;

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


                port?.Close();

                ConnectButton.Visibility = Visibility.Visible;
                DisconnectButton.Visibility = Visibility.Collapsed;
                ReconnectButton.Visibility = Visibility.Collapsed;
                ComPorts.IsEnabled = true;

                MessageToSend.IsEnabled = false;
                SendButton.IsEnabled = false;

                Settings.IsEnabled = true;

                PrintSuccessMessage("Port closed!" + Environment.NewLine + Environment.NewLine);
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
              PrintProcessMessage(MessageToSend.Text);
                //byte[] data = Encoding.Convert(
                //    Encoding.Default,
                //    Settings.Encoding,
                //    Encoding.Default.GetBytes(MessageToSend.Text + Settings.SendNewLine));

                //port.Write(data, 0, data.Length);
                port.Write(MessageToSend.Text + "\r");
                MessageToSend.Text = string.Empty;
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
