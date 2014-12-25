using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace UpService
{ 
    class Program
    {
        public static void Main()
        {


            UpServiceSection section =
                ConfigurationManager.GetSection("upService") as UpServiceSection;

            
            HostFactory.Run(x =>                                 //1
            {
                //x.Service<UpService>(hostSettings => new UpService(hostSettings));
                x.Service<UpService>();
                if (section.UserName == "LocalService")
                    x.RunAsLocalService();
                else if (section.UserName == "LocalSystem")
                    x.RunAsLocalSystem();
                else if (section.UserName == "NetworkService")
                    x.RunAsNetworkService();
                else
                    x.RunAs(section.UserName, section.Password);
                x.SetDescription(section.ServiceName);        //7
                x.SetDisplayName(section.ServiceName);                       //8
                x.SetServiceName(section.ServiceName    );                       //9
                x.UseNLog();
            });                                                  //10 
        }
        //static void Main2(string[] args)
        //{

        //    if (Environment.UserInteractive)
        //    {
        //        ConsoleCommand cc = new ConsoleCommand();

        //        cc.Run(args);

        //        //// 콘솔 명령 모드
        //        //Options options = new Options();
        //        //CommandLine.Parser parser = new Parser();
        //        //if (parser.ParseArguments(args, options))
        //        //{
                   
        //        //    if(options.InstallScript)


        //        //}
        //        //else
        //        //{
        //        //    Console.Error.Write(options.GetUsage());  
        //        //}

        //        //UpService UpService = new UpService();

        //        //UpService.Start(null);

        //    }
        //    else { 
        //        // 서비스 모드
        //        RunService();
        //    }

        //}


        //private static void RunService()
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[] 
        //    { 
        //        new UpService() 
        //    };
        //    ServiceBase.Run(ServicesToRun);

        //}
    }
}
