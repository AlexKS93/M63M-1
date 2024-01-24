using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using nsComPort;
using MainProgram;
using System.Threading;



namespace AnemorumbometerService
{

    public partial class Service1 : ServiceBase
    {
        Anemorumbometer process;
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            process = new Anemorumbometer();
            Thread loggerThread = new Thread(new ThreadStart(process.Start));
            loggerThread.Start();
        }

        protected override void OnStop()
        {
            Thread.Sleep(1000);
            ComPort.Close();
        }
    }
}
