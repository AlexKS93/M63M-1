using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Configuration;
using Microsoft.Extensions.Logging;
using AnemorumbometerService;

namespace nsComPort
{
    class ComPort
    {
        public static string ComPortName;
        public static int ComPortBaudRate;
        public static int ComPortDataBits;
        public static int CheckDataReceiveCompleted;
        public static bool ComPortRtsEnable;
        public static bool ErrorComFlag;
        public static List<byte> input_buffer = new List<byte>();

        static SerialPort mySerialPort;
        public static List<int>[] data = new List<int>[2];

        public static void Init()
        {
            try
            {
                ComPortName = System.Configuration.ConfigurationManager.AppSettings["ComPortName"];
                ComPortBaudRate = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ComPortBaudRate"]);
                ComPortDataBits = Int16.Parse(System.Configuration.ConfigurationManager.AppSettings["ComPortDataBits"]);
                ComPortRtsEnable = Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["ComPortRtsEnable"]);
                mySerialPort = new SerialPort(ComPortName);
                CheckDataReceiveCompleted = 0;
            }
            catch (Exception Error)
            {
                nsWinLogger.cWinLogger.Logger.LogError("Ошибка конфигурации COM порта:\n" + Error);
            }
            finally
            {
                nsWinLogger.cWinLogger.Logger.LogInformation($"COM Port: {ComPortName}\nBaud Rate: {ComPortBaudRate}\nData Bits: {ComPortDataBits}\n");
            }
            mySerialPort.BaudRate = ComPortBaudRate; //28800;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = ComPortDataBits; //8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.ReadTimeout = 50;
            mySerialPort.WriteTimeout = -1;
            mySerialPort.DtrEnable = true;
            mySerialPort.RtsEnable = false;
            List<int> row = new List<int>
            {
                0,
                0
            };
            data[0] = row;
            data[1] = row;
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public static void Write(byte[] data)
        {
            mySerialPort.Write(data, 0, data.Length);
        }

        public static void Open()
        {
            try
            {
                mySerialPort.Open();
            }
            catch (Exception ErrorCom)
            {
                nsWinLogger.cWinLogger.Logger.LogError($"Ошибка открытия {ComPortName}:\n" + ErrorCom);
                ErrorComFlag = true;
            }
            finally
            {
                if (ErrorComFlag != true)
                {
                    nsWinLogger.cWinLogger.Logger.LogInformation($"{ComPortName} открыт.\n");
                }
                ErrorComFlag = false;
            }
        }

        public static void Close()
        {
            try
            {
                mySerialPort.Close();
            }
            catch (Exception ErrorCom)
            {
                nsWinLogger.cWinLogger.Logger.LogError($"Ошибка закрытия {ComPortName}:\n" + ErrorCom);
                ErrorComFlag = true;
            }
            finally
            {
                if (ErrorComFlag != true)
                {
                    nsWinLogger.cWinLogger.Logger.LogInformation($"{ComPortName} закрыт.\n");
                }

                ErrorComFlag = false;
            }
        }

        public static void ClearBuffer()
        {
            mySerialPort.DiscardInBuffer();
            mySerialPort.DiscardOutBuffer();
        }

        public static void DataReceivedHandler(
                            object sender,
                            SerialDataReceivedEventArgs DataReceiveError)
        {
            try
            {
                CheckDataReceiveCompleted = 0;
                List<int> row = new List<int>();
                SerialPort sp = (SerialPort)sender;
                int indata = sp.BytesToRead;
                byte[] buf = new byte[indata];
                sp.Read(buf, 0, indata);
                foreach (var item in buf)
                {
                    input_buffer.Add(item);

                }

                if (input_buffer.Count == 18)
                {
                    if (input_buffer[0] == 0)
                    {
                        int char1 = (input_buffer[12] | 0xF0) ^ 0xF0;
                        int char2 = (input_buffer[15] | 0xF0) ^ 0xF0;
                        int char3 = (input_buffer[14] | 0xF0) ^ 0xF0;
                        int command_index = input_buffer[6];
                        char1 = char1 * 0x100;
                        char2 = char2 * 0x10;
                        int value = char1 + char2 + char3;
                        row.Add(value);
                        row.Add(command_index);
                        data[command_index] = row;
                        input_buffer.Clear();
                        
                    }
                    else
                    {
                        input_buffer.Clear();
                    }
                }
                
                
            }
            catch (Exception ErrorReceive)
            {
                nsWinLogger.cWinLogger.Logger.LogError($"Ошибка приема данных:\n" + ErrorReceive);
            }
        }
    }
}
