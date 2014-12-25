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
        protected ManualResetEvent ShutdownEvent;
        //private Topshelf.Runtime.HostSettings s1;
        HostControl HostControl;

        public UpService()
        { 
        }
        PowerShell _instance;
        private void ServiceMain()
        {
            UpServiceSection section =
                ConfigurationManager.GetSection("upService") as UpServiceSection; 
            string script = section.StartScript; 
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                logger.Debug(string.Format("script = {0}", script));
                // this script has a sleep in it to simulate a long running script
                PowerShellInstance.AddScript(script);
                //Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                _instance = PowerShellInstance;
                _instance.Streams.Progress.DataAdded += Progress_DataAdded;
                _instance.Streams.Error.DataAdded += Error_DataAdded;
                _instance.Streams.Verbose.DataAdded += Verbose_DataAdded;
                _instance.Streams.Debug.DataAdded += Debug_DataAdded;
                _instance.Streams.Warning.DataAdded += Warning_DataAdded;


                // begin invoke execution on the pipeline
                IAsyncResult result = PowerShellInstance.BeginInvoke();

                // do something else until execution has completed.
                // this could be sleep/wait, or perhaps some other work
                while (result.IsCompleted == false)
                {
                    logger.Debug("Waiting for pipeline to finish...");
                    bool bSignaled = ShutdownEvent.WaitOne(0);
                    if (bSignaled)
                    {
                        PowerShellInstance.Stop();
                        logger.Debug("Stop by command");
                        HostControl.Stop();
                        return;

                    }
                    Thread.Sleep(1000);

                    // might want to place a timeout here...
                }
                logger.Debug(result.ToString());

                foreach (PSObject pso in PowerShellInstance.EndInvoke(result))
                {
                    //logger.Debug("{0,-20}{1}",
                    //        result.Members["ProcessName"].Value,
                    //        result.Members["Id"].Value);
                    logger.Debug(String.Format("PSO {0}", pso.ToString()));
                } // End foreach.

                logger.Debug("Finished!"); 
            }
        }


        void Warning_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = _instance.Streams.Warning[e.Index];
            logger.Debug(record.Message);
        }

        void Debug_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = _instance.Streams.Debug[e.Index];
            logger.Debug(record.Message);
        }

        void Progress_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = _instance.Streams.Progress[e.Index];
            logger.Debug(record.SecondsRemaining);
        }

        void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = _instance.Streams.Verbose[e.Index];
            logger.Debug(record.Message);
        }

        void Error_DataAdded(object sender, DataAddedEventArgs e)
        { 
            var record = _instance.Streams.Error[e.Index];
            logger.Debug(record.ErrorDetails);
            logger.Debug(record.Exception);
        }
        public bool Start(HostControl hostControl)
        {
            HostControl = hostControl;
            ThreadStart ts = new ThreadStart(this.ServiceMain);

            // create the manual reset event and
            // set it to an initial state of unsignaled
            ShutdownEvent = new ManualResetEvent(false);

            // create the worker thread
            MainThread = new Thread(ts);

            // go ahead and start the worker thread
            MainThread.Start();
            return true;

        }

        public bool Stop(HostControl hostControl)
        {
            ShutdownEvent.Set();
            // wait for the thread to stop giving it 10 seconds
            MainThread.Join(10000);
            return true;
        }
    }
}
