using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class CsMySerialPort
    {
        /// <summary>
        /// 作者:WangJunLiang || Wechat:Joronwongx
        /// </summary>
        private SerialPort SPserialPort = null;
        public bool IsSerialPortOpen
        {
            get { return SPserialPort != null; }
        }

        //byte字节数组转string
        private string ConverToString(byte[] data)
        {
            StringBuilder stb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if ((int)data[i] > 15)
                {
                    stb.Append(Convert.ToString(data[i], 16).ToUpper()); //添加字符串
                }
                else  //如果是小于0F需要加个零
                {
                    stb.Append("0" + Convert.ToString(data[i], 16).ToUpper());
                }
                if (i != data.Length - 1)
                    stb.Append(" ");
            }
            return stb.ToString();
        }

        //string转byte字节数组
        private byte[] StringToConver(string str)
        {
            String[] SendArr = str.Split(' ');//以空格分开
            byte[] decBytes = new byte[SendArr.Length];
            for (int i = 0; i < SendArr.Length; i++)
                decBytes[i] = Convert.ToByte(SendArr[i], 16);
            return decBytes;
        }

        //初始化
        public struct SerialPortParm
        {
            public string strPortName;
            public string strBaudRate;
            public string strDataBits;
            public string strStopBits;
            public string strParity;
            public StopBits GetStopBits()
            {
                StopBits stopBits = StopBits.One;
                switch (strStopBits)
                {
                    case "1": stopBits = StopBits.One; break;
                    case "2": stopBits = StopBits.Two; break;
                    case "1.5": stopBits = StopBits.OnePointFive; break;
                    default: break;
                }
                return stopBits;
            }
            public Parity GetParity()
            {
                Parity parity = Parity.None;
                switch (strParity)
                {
                    case "0": parity = Parity.None; break;
                    case "1": parity = Parity.Odd; break;
                    case "2": parity = Parity.Even; break;
                    default: break;
                }
                return parity;
            }
        }

        public delegate bool ReceClientData(string strRece);
        public event ReceClientData ReceEvent;
        public event ReceClientData SendEvent;

        //内容监听
        private void ReceSerialPortDataClick(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate ()
                {
                    if (ReceEvent("#HEXRece"))
                    {
                        int nCnt = SPserialPort.BytesToRead;
                        byte[] Readbuffer = new byte[nCnt];
                        SPserialPort.Read(Readbuffer, 0, nCnt);
                        ReceEvent(ConverToString(Readbuffer));
                    }
                    else
                    {
                        string strReceData = SPserialPort.ReadExisting();
                        ReceEvent(strReceData);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ReceEvent(string.Format("[Error]-{0}", ex.Message));
            }
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames(); ;
        }

        //打开实现
        public bool OpenSerialPort(SerialPortParm parm)
        {
            try
            {
                if (SPserialPort != null)
                    return false;
                SPserialPort = new SerialPort
                {
                    PortName = parm.strPortName,
                    BaudRate = int.Parse(parm.strBaudRate),
                    DataBits = int.Parse(parm.strDataBits),
                    StopBits = parm.GetStopBits(),
                    Parity = parm.GetParity(),

                    WriteBufferSize = 1048576,
                    ReadBufferSize = 2097152,
                    Encoding = System.Text.Encoding.GetEncoding("UTF-8"),
                    Handshake = Handshake.None,
                    RtsEnable = true
                };
                SPserialPort.DataReceived += new SerialDataReceivedEventHandler(ReceSerialPortDataClick);//接收响应函数

                SPserialPort.Open();
                ReceEvent(string.Format("#Sp000-{0}", parm.strPortName));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        //关闭实现
        public bool CloseSerialPort()
        {
            try
            {
                if (SPserialPort != null && SPserialPort.IsOpen)
                {
                    SPserialPort.Close();
                    SPserialPort = null;
                    ReceEvent("#Sp404-");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        //发送实现 
        public bool ComSend(string strIn)
        {
            try
            {
                if (SPserialPort != null && SPserialPort.IsOpen)
                {
                    if (SendEvent("#HEXSend"))
                    {
                        byte[] decBytes = StringToConver(strIn);
                        SPserialPort.Write(decBytes, 0, decBytes.Length);
                    }
                    else
                        SPserialPort.Write(strIn.ToCharArray(), 0, strIn.Length); /* ASCII字符串发送 */
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
