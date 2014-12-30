using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using System.Configuration;
using System.Xml;
using Topshelf.Logging;

namespace UpService
{
    class UpService : ServiceControl
    {
        private LogWriter logger = HostLogger.Get<UpService>();

        protected Thread MainThread;
        //protected ManualResetEvent ShutdownEvent;
        //private Topshelf.Runtime.HostSettings s1;
        HostControl HostControl;

        public UpService()
        { 
        }
        PowerShell PowerShellInstance;
        private void ServiceMain()
        {
            UpServiceSection section =
                ConfigurationManager.GetSection("upService") as UpServiceSection; 
            string script = section.StartScript;
            logger.Debug(string.Format("script = {0}", script));
            RunScript(script);
            logger.Debug("StartScript Done!!");
        }

        private void RunScript(string script)
        {

            using (PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(script);

                PowerShellInstance.Streams.Progress.DataAdded += Progress_DataAdded;
                PowerShellInstance.Streams.Error.DataAdded += Error_DataAdded;
                PowerShellInstance.Streams.Verbose.DataAdded += Verbose_DataAdded;
                PowerShellInstance.Streams.Debug.DataAdded += Debug_DataAdded;
                PowerShellInstance.Streams.Warning.DataAdded += Warning_DataAdded;

                PowerShellInstance.Invoke();
            }
        }


        void Warning_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = PowerShellInstance.Streams.Warning[e.Index];
            logger.Debug(record.Message);
        }

        void Debug_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = PowerShellInstance.Streams.Debug[e.Index];
            logger.Debug(record.Message);
        }

        void Progress_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = PowerShellInstance.Streams.Progress[e.Index];
            logger.Debug(record.SecondsRemaining);
        }

        void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = PowerShellInstance.Streams.Verbose[e.Index];
            logger.Debug(record.Message);
        }

        void Error_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = PowerShellInstance.Streams.Error[e.Index];
            logger.Debug(record.ErrorDetails);
            logger.Debug(record.Exception);
        }
        public bool Start(HostControl hostControl)
        {
            HostControl = hostControl;
            ThreadStart ts = new ThreadStart(this.ServiceMain);

            // create the manual reset event and
            // set it to an initial state of unsignaled
            //ShutdownEvent = new ManualResetEvent(false);

            // create the worker thread
            MainThread = new Thread(ts);

            // go ahead and start the worker thread
            MainThread.Start();
            return true;

        }

        public bool Stop(HostControl hostControl)
        {

            UpServiceSection section =
                ConfigurationManager.GetSection("upService") as UpServiceSection;
            string script = section.StopScript;
            logger.Debug(string.Format("script = {0}", script));
            RunScript(script);
            //logger.Debug("StopScript Done!!");

            //var asyncResult = PowerShellInstance.BeginStop(null, null);

            //PowerShellInstance.EndStop(asyncResult); 

            //PowerShellInstance.Stop();
            //MainThread.Join(10000);
            return true;
        }
    }
}
