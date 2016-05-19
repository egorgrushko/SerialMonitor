using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Windows;

namespace Serial_Monitor
{
    public class SerialMonitorControlSettings
    {
        #region SettingsMapping
        public string[] BaudRateValues
        {
            get
            {
                return new string[]
                {
                    "110",
                    "300",
                    "600",
                    "1200",
                    "2400",
                    "4800",
                    "9600",
                    "14400",
                    "19200",
                    "28800",
                    "38400",
                    "56000",
                    "57600",
                    "115200",
                    "128000",
                    "153600",
                    "230400",
                    "256000",
                    "460800",
                    "921600"
                };
            }
        }
        public string DefaultBaudRate
        {
            get
            {
                return "9600";
            }
        }

        public Dictionary<string, string> ReceiveNewLineMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "CR + NL", "\r\n" },
                    { "CR", "\r" },
                    { "NL", "\n" }
                };
            }
        }
        public string DefaultReceiveNewLine
        {
            get
            {
                return "CR + NL";
            }
        }

        public Dictionary<string, string> SendNewLineMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "None", "" },
                    { "CR + NL", "\r\n" },
                    { "CR", "\r" },
                    { "NL", "\n" }
                };
            }
        }
        public string DefaultSendNewLine
        {
            get
            {
                return "None";
            }
        }

        public Dictionary<string, StopBits> StopBitsMap
        {
            get
            {
                return new Dictionary<string, StopBits>()
                {
                    { "1", StopBits.One },
                    { "1.5", StopBits.OnePointFive },
                    { "2", StopBits.Two }
                };
            }
        }
        public string DefaultStopBits
        {
            get
            {
                return "1";
            }
        }

        public Dictionary<string, Handshake> HandshakeMap
        {
            get
            {
                Dictionary<string, Handshake> map = new Dictionary<string, Handshake>();
                foreach (string handshakeValue in Enum.GetNames(typeof(Handshake)))
                {
                    map.Add(handshakeValue, (Handshake)Enum.Parse(typeof(Handshake), handshakeValue));
                }
                return map;
            }
        }
        public string DefaultHandshake
        {
            get
            {
                return Enum.GetNames(typeof(Handshake))[0];
            }
        }

        public Dictionary<string, Parity> ParityMap
        {
            get
            {
                Dictionary<string, Parity> map = new Dictionary<string, Parity>();
                foreach (string parityValue in Enum.GetNames(typeof(Parity)))
                {
                    map.Add(parityValue, (Parity)Enum.Parse(typeof(Parity), parityValue));
                }
                return map;
            }
        }
        public string DefaultParity
        {
            get
            {
                return Enum.GetNames(typeof(Parity))[0];
            }
        }

        public string[] DataBitsValues
        {
            get
            {
                return new string[]
                {
                    "5",
                    "6",
                    "7",
                    "8"
                };
            }
        }

        public string DefaultDataBits
        {
            get
            {
                return "8";
            }
        }

        public string DefaultReadTimeout
        {
            get
            {
                return "500";
            }
        }
        public string DefaultWriteTimeout
        {
            get
            {
                return "500";
            }
        }


        public Dictionary<string, Encoding> EncodingsMap
        {
            get
            {
                Dictionary<string, Encoding> map = new Dictionary<string, Encoding>();
                foreach (EncodingInfo encoding in Encoding.GetEncodings())
                {
                    map.Add(encoding.CodePage + " " + encoding.Name + " - " + encoding.DisplayName, encoding.GetEncoding());
                }
                return map;
            }
        }
        public string DefaultEncoding
        {
            get
            {
                Encoding defaultEncoding = Encoding.GetEncoding(0);
                return defaultEncoding.CodePage + " " + defaultEncoding.WebName + " - " + defaultEncoding.EncodingName;
            }
        }
        #endregion

        #region Settings
        public int BaudRate
        {
            get
            {
                int baudRate;
                if (!int.TryParse(control.BaudRateComboBox.Text, out baudRate))
                {
                    throw new Exception("Invalid baud rate value!");
                }
                return baudRate;
            }
        }

        public string ReceiveNewLine
        {
            get
            {
                return ReceiveNewLineMap[control.ReceiveNewLineComboBox.Text];
            }
        }

        public string SendNewLine
        {
            get
            {
                return SendNewLineMap[control.SendNewLineComboBox.Text];
            }
        }

        public StopBits StopBits
        {
            get
            {
                return StopBitsMap[control.StopBitsComboBox.Text];
            }
        }

        public Handshake Handshake
        {
            get
            {
                return HandshakeMap[control.HandshakeComboBox.Text];
            }
        }

        public Parity Parity
        {
            get
            {
                return ParityMap[control.ParityComboBox.Text];
            }
        }

        public int DataBits
        {
            get
            {
                return Convert.ToInt32(control.DataBitsComboBox.Text);
            }
        }

        public int ReadTimeout
        {
            get
            {
                int readTimeout;
                if (!int.TryParse(control.ReadTimeoutTextBox.Text, out readTimeout))
                {
                    throw new Exception("Invalid read timeout value!");
                }
                return readTimeout;
            }
        }

        public int WriteTimeout
        {
            get
            {
                int writeTimeout;
                if (!int.TryParse(control.WriteTimeoutTextBox.Text, out writeTimeout))
                {
                    throw new Exception("Invalid write timeout value!");
                }
                return writeTimeout;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return EncodingsMap[control.EncodingComboBox.Text];
            }
        }

        public bool DtrEnable
        {
            get
            {
                return control.DTRToggleButton.IsChecked.Value;
            }
        }
        #endregion

        public bool IsOpen
        {
            get
            {
                return control.SettingsOutputControl.Content.ToString() == "Show Output" ? true : false;
            }
        }

        private SerialMonitorControl control;

        public SerialMonitorControlSettings(SerialMonitorControl control)
        {
            this.control = control;
        }

        public void Fill()
        {
            foreach (string rate in BaudRateValues)
            {
                control.BaudRateComboBox.Items.Add(rate);
            }
            control.BaudRateComboBox.SelectedItem = DefaultBaudRate;

            foreach (string newLine in ReceiveNewLineMap.Keys)
            {
                control.ReceiveNewLineComboBox.Items.Add(newLine);
            }
            control.ReceiveNewLineComboBox.SelectedItem = DefaultReceiveNewLine;

            foreach (string newLine in SendNewLineMap.Keys)
            {
                control.SendNewLineComboBox.Items.Add(newLine);
            }
            control.SendNewLineComboBox.SelectedItem = DefaultSendNewLine;

            foreach (string dataBits in DataBitsValues)
            {
                control.DataBitsComboBox.Items.Add(dataBits);
            }
            control.DataBitsComboBox.SelectedItem = DefaultDataBits;

            foreach (string stopBits in StopBitsMap.Keys)
            {
                control.StopBitsComboBox.Items.Add(stopBits);
            }
            control.StopBitsComboBox.SelectedItem = DefaultStopBits;

            foreach (string encoding in EncodingsMap.Keys)
            {
                control.EncodingComboBox.Items.Add(encoding);
            }
            control.EncodingComboBox.SelectedItem = DefaultEncoding;

            foreach (string handshakeValue in HandshakeMap.Keys)
            {
                control.HandshakeComboBox.Items.Add(handshakeValue);
            }
            control.HandshakeComboBox.SelectedItem = DefaultHandshake;

            foreach (string parityValue in ParityMap.Keys)
            {
                control.ParityComboBox.Items.Add(parityValue);
            }
            control.ParityComboBox.SelectedItem = DefaultParity;

            control.ReadTimeoutTextBox.Text = DefaultReadTimeout;
            control.WriteTimeoutTextBox.Text = DefaultWriteTimeout;
        }

        public void Show()
        {
            control.Output.Visibility = Visibility.Collapsed;
            control.SettingsWindow.Visibility = Visibility.Visible;
            control.SettingsOutputControl.Content = "Show Output";
        }

        public void Hide()
        {
            control.Output.Visibility = Visibility.Visible;
            control.SettingsWindow.Visibility = Visibility.Collapsed;
            control.SettingsOutputControl.Content = "Show Settings";
        }

        public void Lock()
        {
            control.BaudRateComboBox.IsEnabled = false;
            control.DataBitsComboBox.IsEnabled = false;
            control.EncodingComboBox.IsEnabled = false;
            control.HandshakeComboBox.IsEnabled = false;
            control.ParityComboBox.IsEnabled = false;
            control.StopBitsComboBox.IsEnabled = false;
            control.ReadTimeoutTextBox.IsEnabled = false;
            control.WriteTimeoutTextBox.IsEnabled = false;
            control.DTRToggleButton.IsEnabled = false;
        }

        public void Unlock()
        {
            control.BaudRateComboBox.IsEnabled = true;
            control.DataBitsComboBox.IsEnabled = true;
            control.EncodingComboBox.IsEnabled = true;
            control.HandshakeComboBox.IsEnabled = true;
            control.ParityComboBox.IsEnabled = true;
            control.StopBitsComboBox.IsEnabled = true;
            control.ReadTimeoutTextBox.IsEnabled = true;
            control.WriteTimeoutTextBox.IsEnabled = true;
            control.DTRToggleButton.IsEnabled = true;
        }
    }
}
