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

namespace WVA_Scan_Installer_Service
{
    // Windows Service Commands
    // install service ------- sc create AppName binPath="path"
    // uninstall service ----- sc delete AppName
    // start service --------- net start AppName
    // stop service ---------- net stop AppName

    public partial class Service1 : ServiceBase
    {

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            new Installer().Run();
        }

        protected override void OnStop()
        {
            
        }
    }
}
