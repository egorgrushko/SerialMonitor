using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Windows.Controls;

namespace Serial_Monitor
{
    public partial class SerialMonitorSettingsControl : UserControl
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
                    { "CR + LF", "\r\n" },
                    { "LF", "\n" }
                };
            }
        }
        public string DefaultReceiveNewLine
        {
            get
            {
                return "CR + LF";
            }
        }

        public Dictionary<string, string> SendNewLineMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "None", "" },
                    { "CR + LF", "\r\n" },
                    { "LF", "\n" }
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
                if (!int.TryParse(BaudRateComboBox.Text, out baudRate))
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
                return ReceiveNewLineMap[ReceiveNewLineComboBox.Text];
            }
        }

        public string SendNewLine
        {
            get
            {
                return SendNewLineMap[SendNewLineComboBox.Text];
            }
        }

        public StopBits StopBits
        {
            get
            {
                return StopBitsMap[StopBitsComboBox.Text];
            }
        }

        public Handshake Handshake
        {
            get
            {
                return HandshakeMap[HandshakeComboBox.Text];
            }
        }

        public Parity Parity
        {
            get
            {
                return ParityMap[ParityComboBox.Text];
            }
        }

        public int DataBits
        {
            get
            {
                return Convert.ToInt32(DataBitsComboBox.Text);
            }
        }

        public int ReadTimeout
        {
            get
            {
                int readTimeout;
                if (!int.TryParse(ReadTimeoutTextBox.Text, out readTimeout))
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
                if (!int.TryParse(WriteTimeoutTextBox.Text, out writeTimeout))
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
                return EncodingsMap[EncodingComboBox.Text];
            }
        }

        public bool DtrEnable
        {
            get;
            private set;
        }

        public bool OutputToFileEnabled
        {
            get;
            private set;
        }

        public string RecordFile
        {
            get
            {
                if (!OutputToFileEnabled)
                {
                    return null;
                }
                return RecordFilePathTextBox.Text;
            }
        }
        #endregion

        public SerialMonitorSettingsControl()
        {
            InitializeComponent();
            Reset();
        }

        public void Reset()
        {
            foreach (string rate in BaudRateValues)
            {
                BaudRateComboBox.Items.Add(rate);
            }
            BaudRateComboBox.SelectedItem = DefaultBaudRate;

            foreach (string newLine in ReceiveNewLineMap.Keys)
            {
                ReceiveNewLineComboBox.Items.Add(newLine);
            }
            ReceiveNewLineComboBox.SelectedItem = DefaultReceiveNewLine;

            foreach (string newLine in SendNewLineMap.Keys)
            {
                SendNewLineComboBox.Items.Add(newLine);
            }
            SendNewLineComboBox.SelectedItem = DefaultSendNewLine;

            foreach (string dataBits in DataBitsValues)
            {
                DataBitsComboBox.Items.Add(dataBits);
            }
            DataBitsComboBox.SelectedItem = DefaultDataBits;

            foreach (string stopBits in StopBitsMap.Keys)
            {
                StopBitsComboBox.Items.Add(stopBits);
            }
            StopBitsComboBox.SelectedItem = DefaultStopBits;

            foreach (string encoding in EncodingsMap.Keys)
            {
                EncodingComboBox.Items.Add(encoding);
            }
            EncodingComboBox.SelectedItem = DefaultEncoding;

            foreach (string handshakeValue in HandshakeMap.Keys)
            {
                HandshakeComboBox.Items.Add(handshakeValue);
            }
            HandshakeComboBox.SelectedItem = DefaultHandshake;

            foreach (string parityValue in ParityMap.Keys)
            {
                ParityComboBox.Items.Add(parityValue);
            }
            ParityComboBox.SelectedItem = DefaultParity;

            ReadTimeoutTextBox.Text = DefaultReadTimeout;
            WriteTimeoutTextBox.Text = DefaultWriteTimeout;

            DtrEnable = true;
            OutputToFileEnabled = false;
        }

        private void RecordFilePathTextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.ShowDialog();
            RecordFilePathTextBox.Text = dialog.FileName;
        }

        private void DtrToggle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DtrEnable = !DtrEnable;

            if (DtrEnable)
            {
                DtrToggle.Content = "Disable Data Terminal Ready (DTR)";
            }
            else
            {
                DtrToggle.Content = "Enable Data Terminal Ready (DTR)";
            }
        }

        private void OutputToFileToggle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OutputToFileEnabled = !OutputToFileEnabled;

            if (OutputToFileEnabled)
            {
                OutputToFileToggle.Content = "Disable output to file";
                RecordFilePathTextBox.IsEnabled = true;
            }
            else
            {
                OutputToFileToggle.Content = "Enable output to file";
                RecordFilePathTextBox.IsEnabled = false;
            }
        }
    }
}
