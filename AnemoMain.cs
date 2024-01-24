using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using nsComPort;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;
using System.Collections;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MainProgram
{
    class Anemorumbometer
    {
        static Uri url;
        static OpcDaGroup group;
        static OpcDaItem speed_item;
        static OpcDaItem direction_item;
        static OpcDaItem[] items;
        public static string OpcDaAddress;
        public static string SpeedTag;
        public static string DirectionTag;

        public int PreviousValue = 1;

        public void Start()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                byte[] directionComand = new byte[] { 0x00, 0x93, 0x01, 0x81, 0x00, 0x80, 0x00, 0x80, 0x01, 0x80, 0x00, 0x80, 0x08, 0xAE };
                byte[] speedComand = new byte[] { 0x00, 0x93, 0x00, 0x81, 0x00, 0x80, 0x00, 0x80, 0x01, 0x80, 0x00, 0x80, 0x0B, 0xAE };
                OpcDaAddress = System.Configuration.ConfigurationManager.AppSettings["OpcDaAddress"];
                SpeedTag = System.Configuration.ConfigurationManager.AppSettings["SpeedTag"];
                DirectionTag = System.Configuration.ConfigurationManager.AppSettings["DirectionTag"];
                ComPort.Init();
                ComPort.Open();
                OPCInit();
                using (var server = new OpcDaServer(url))
                {
                    server.Connect();
                    group = server.AddGroup("M63M");
                    group.IsActive = true;

                    var definition1 = new OpcDaItemDefinition
                    {
                        ItemId = SpeedTag,
                        IsActive = true
                    };

                    var definition2 = new OpcDaItemDefinition
                    {
                        ItemId = DirectionTag,
                        IsActive = true
                    };

                    OpcDaItemDefinition[] definitions = { definition1, definition2 };
                    OpcDaItemResult[] results = group.AddItems(definitions);
                    speed_item = group.Items.FirstOrDefault(i => i.ItemId == SpeedTag);
                    direction_item = group.Items.FirstOrDefault(i => i.ItemId == DirectionTag);
                    items = new OpcDaItem[] { speed_item, direction_item };

                    while (true)
                    {
                        ComPort.Write(directionComand);
                        Thread.Sleep(1000);
                        ComPort.Write(speedComand);
                        Thread.Sleep(1000);

                        if (ComPort.CheckDataReceiveCompleted < 11)
                        {
                            ComPort.CheckDataReceiveCompleted++;
                        }
                        
                        if (ComPort.CheckDataReceiveCompleted == 11)
                        {
                            ComPort.data[0][0] = -10;
                            ComPort.data[1][0] = -1;
                            nsWinLogger.cWinLogger.Logger.LogError("Нет ответа от устройства. Проверьте связь по COM Port.\n");
                            ComPort.CheckDataReceiveCompleted++;
                        } else if (ComPort.CheckDataReceiveCompleted == 1 & PreviousValue == 12)
                        {
                            nsWinLogger.cWinLogger.Logger.LogInformation("Связь с устройством восстановлена.\n");
                            ComPort.input_buffer.Clear();
                        }
                        PreviousValue = ComPort.CheckDataReceiveCompleted;
                        ComPort.ClearBuffer();
                        SendOPC(ComPort.data);
                    }
                }
            }
            catch (Exception ErrorStart)
            {
                nsWinLogger.cWinLogger.Logger.LogError($"Ошибка приема данных:\n" + ErrorStart);
            }
        }

        private static void OPCInit()
        {
            url = UrlBuilder.Build(OpcDaAddress);
        }

        public static void SendOPC(List<int>[] data)
        {
            object[] values = { (data[0][0] / 10.0), data[1][0] };
            group.Write(items, values);
            values = null;
        }

        public static void PrintConsole(List<int> data)
        {
            if (data == null)
            {
                return;
            }
            if (data.Count != 0)
            {
                if (data[1] == 0)
                {
                    double speed = data[0] / 10.0;
                    Console.Write("Speed: " + speed.ToString("0.0") + "\n" + "\n");
                }
                if (data[1] == 1)
                {
                    Console.Write("Direction: " + data[0] + "\n");
                }
            }
        }

    }
}
