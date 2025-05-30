using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// UartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UartWindow : Window
    {
        private CsMySerialPort mySerialPort = new CsMySerialPort();
        private CsMySerialPort.SerialPortParm SpParm;

        public UartWindow()
        {
            InitializeComponent();
            SerialPortInit();
        }

        private void SerialPortInit()
        {
            Find();
            comboBoxBaudRate.ItemsSource = new List<string>() {
                "9600","19200","38400","115200","1000000",
            };
            comboBoxBaudRate.SelectedIndex = 0;

            comboBoxDataBit.ItemsSource = new List<string>() {
                "5","6","7","8",
            };
            comboBoxDataBit.SelectedIndex = 0;

            comboBoxStopBit.ItemsSource = new List<string>() {
                "1","1.5","2",
            };
            comboBoxStopBit.SelectedIndex = 0;
        }

        private void btnOpenCloseCom_Click(object sender, RoutedEventArgs e)
        {
            if (mySerialPort.IsSerialPortOpen)
            {
                mySerialPort.CloseSerialPort();
                btnOpenCloseCom.Content = "打开串口";
                Console.WriteLine("关闭串口成功");
                Debug.WriteLine("关闭串口成功");
                comboBoxCOM.IsEnabled = true;
                comboBoxBaudRate.IsEnabled = true;
                comboBoxDataBit.IsEnabled = true;
                comboBoxStopBit.IsEnabled = true;
                comboBoxSdd.IsEnabled = true;
            }
            else
            {
                SpParm.strPortName = comboBoxCOM.SelectedItem.ToString();
                SpParm.strBaudRate = comboBoxBaudRate.SelectedItem.ToString(); // 波特率
                SpParm.strDataBits = comboBoxDataBit.SelectedItem.ToString();   // 数据位
                SpParm.strStopBits = comboBoxStopBit.SelectedItem.ToString();    // 停止位
                SpParm.strParity = comboBoxSdd.SelectedIndex.ToString();   // 校验位

                mySerialPort.ReceEvent += ReceDataClick;
                mySerialPort.SendEvent += IsHEXClick;

                try
                {
                    mySerialPort.OpenSerialPort(SpParm);
                    btnOpenCloseCom.Content = "关闭串口";
                    Console.WriteLine("打开串口成功");
                    Debug.WriteLine("打开串口成功");

                    comboBoxBaudRate.IsEnabled = false;
                    comboBoxCOM.IsEnabled = false;
                    comboBoxDataBit.IsEnabled = false;
                    comboBoxStopBit.IsEnabled = false;
                    comboBoxSdd.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private bool IsHEXClick(string strIn)
        {
            if (strIn == "#HEXSend")
                return (bool)cbSendType.IsChecked;
            return true;
        }

        private bool ReceDataClick(string strIn)
        {
            if (strIn == "#HEXRece")
                return (bool)cbReceType.IsChecked;

            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (!ReceSerialPort(strIn))
                    return;
                textBlockRecv.Text += "收>" + strIn + "\r\n";
            });
            return true;
        }
        private bool ReceSerialPort(string strIn)
        {
            var strCompare = strIn.Split('-');
            if (strCompare.Count() > 1)
            {
                if (strCompare[0] == "#Sp000")
                {
                    Debug.Write(SpParm.strPortName + "已连接");
                    return false;
                }
                if (strCompare[0] == "#Sp404")
                {
                    Debug.Write(SpParm.strPortName + "已断开");
                    return false;
                }
            }
            return true;
        }

        private void btnClearRecv_Click(object sender, RoutedEventArgs e)
        {
            textBlockRecv.Text = string.Empty;
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            Find();
        }
        private void Find()
        {
            string[] names = mySerialPort.GetPortNames();
            comboBoxCOM.ItemsSource = names;
            comboBoxCOM.SelectedIndex = 0;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            textBlockRecv.Text += "发<" + textBlockSend.Text + "\r\n";
            mySerialPort.ComSend(textBlockSend.Text);//HEX 输入格式要求 末尾不要输入空格和回车 ASCII 随意
        }
    }
}
