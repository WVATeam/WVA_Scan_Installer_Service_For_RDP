using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WVA_Scan_Installer_Service.Utility.Files
{
    class Paths
    {
        public static readonly string PublicDocs        = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
        public static readonly string AppName           = "WVA_Scan";

        public static readonly string PublicDataDir     = $@"{PublicDocs}\{AppName}\Data\";
        public static readonly string PublicErrorDir    = $@"{PublicDocs}\{AppName}\ErrorLogs\";
        public static readonly string PublicLogsDir     = $@"{PublicDocs}\{AppName}\Logs\";
        public static readonly string LogPath           = $@"{PublicLogsDir}WVA_Scan_Installer_Service_Logs.txt";
        public static readonly string ParentCopyPath    = $@"{PublicDataDir}ParentCopyPath.txt";
        public static readonly string ShortcutIconPath  = $@"{PublicDataDir}wvascanlogoclear.ico";
    }
}
