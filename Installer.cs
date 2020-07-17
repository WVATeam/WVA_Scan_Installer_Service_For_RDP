using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WVA_Scan_Installer_Service.Utility.Files;

namespace WVA_Scan_Installer_Service
{
    class Installer
    {

        public void Run()
        {
            while (true) 
            {
                try
                {
                    RunInstall();
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private void LogError(Exception ex)
        {
            try
            {
                if (!Directory.Exists(Paths.PublicErrorDir))
                    Directory.CreateDirectory(Paths.PublicErrorDir);

                var errorLog = new StringBuilder();
                errorLog.AppendLine("Error: " + ex.Message);
                errorLog.AppendLine("Location: " + ex.TargetSite);
                errorLog.AppendLine("StackTrace: " + ex.StackTrace);
                var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                var fileName = "WVA_Scan_Installer_Service_Error--" + time;
                var fullPath = Paths.PublicErrorDir + fileName;

                File.WriteAllText(fullPath, errorLog.ToString());
            }
            catch { }
        }

        private void RunInstall()
        {
            // Get parent dir path that will be used to copy child apps
            string path = GetParentCopyPath();

            if (path == null)
                return;

            DownloadShortcutIcon();
            CopyParentDirToChildren(path);
        }

        private string GetParentCopyPath()
        {
            if (!Directory.Exists(Paths.PublicDataDir))
                Directory.CreateDirectory(Paths.PublicDataDir);

            if (!File.Exists(Paths.ParentCopyPath))
            {
                File.Create(Paths.ParentCopyPath).Close();
                return FindNewInstallPath();
            }
            else
            {
                string parentPath = File.ReadAllText(Paths.ParentCopyPath);

                if (Directory.Exists(parentPath))
                    return parentPath;
                else return null;
            }
        }

        private string FindNewInstallPath()
        {
            List<string> directories = Directory.GetDirectories(@"C:\Users").ToList();

            foreach (string dir in directories)
            {
                string fullPath = dir + @"\AppData\Local\WVA_Scan\";
                if (Directory.Exists(fullPath))
                {
                    File.WriteAllText(Paths.ParentCopyPath, fullPath);
                    return fullPath;
                }
            }
            
            return null;
        }

        private string GetUserNameFromPath(string path)
        {
            path = path.Replace("C:\\Users\\","");

            if (!path.Contains("\\"))
                return path;

            int sliceCt = 0;
            foreach (char character in path)
            {
                if (character == '\\')
                    break;
                else
                    sliceCt++;
            }

            path = path.Remove(sliceCt, path.Length - sliceCt);

            return path;
        }

        private void DownloadShortcutIcon()
        {
            if (!Directory.Exists(Paths.PublicDataDir))
                Directory.CreateDirectory(Paths.PublicDataDir);

            // Try to download file up to 3 times if it fails
            int tryCount = 0;
            while (tryCount < 3 && !File.Exists(Paths.ShortcutIconPath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://wisvis.com/images/wvascanlogoclear.ico", Paths.ShortcutIconPath);
                }

                Thread.Sleep(500); // Give time for file to download successfully
                tryCount++;
            }
        }

        private void CopyParentDirToChildren(string parentDir)
        {
            List<string> directories = Directory.GetDirectories(@"C:\Users").ToList();

            // Remove default Windows dirs that won't need the app installed into
            directories = RemoveAllJunkPaths(directories);

            foreach (string dir in directories)
            {
                string childDirBase = dir + @"\AppData\Local\WVA_Scan\";

                // Don't touch the directory if it already has WVA Scan installed
                if (Directory.Exists(childDirBase))
                    continue;

                string childUserName = GetUserNameFromPath(dir);
                string parentUserName = GetUserNameFromPath(parentDir);

                try
                {
                    CopyAppFolder(parentDir, childDirBase, parentUserName, childUserName);
                    CopyDataFolder(parentDir, childDirBase, parentUserName, childUserName);
                    CopyPackagesFolder(parentDir, childDirBase, parentUserName, childUserName);
                    CopyMiscFiles(parentDir, childDirBase, parentUserName, childUserName);
                    CreateDesktopShortCut(parentDir, childDirBase, parentUserName, childUserName);
                    LogNewInstall(childUserName, true);
                }
                catch (Exception)
                {
                    LogNewInstall(childUserName, false);
                }
            }
        }

        private List<string> RemoveAllJunkPaths(List<string> directories)
        {
            directories.RemoveAll(x => x.EndsWith("All Users"));
            directories.RemoveAll(x => x.EndsWith("Default User"));
            directories.RemoveAll(x => x.EndsWith("Default"));
            directories.RemoveAll(x => x.EndsWith("Public"));
            directories.RemoveAll(x => x.EndsWith("wva"));
            directories.RemoveAll(x => x.EndsWith("wva.WVA"));

            return directories;
        }

        private void CopyAppFolder(string parentDir, string childDir, string parentUserName, string childUserName)
        {
            Directory.CreateDirectory(childDir);

            // Copy App dir
            string version = Directory.GetDirectories(parentDir).Where(x => x.Contains("app-")).First().Replace($"{parentDir}app-", "");
            string parentDirApp = parentDir + $"app-{version}";
            string childDirApp = childDir + $"app-{version}";
            string[] parentFiles = Directory.GetFiles(parentDirApp);

            Directory.CreateDirectory(childDirApp);

            for (int i = 0; i < parentFiles.Length; i++)
            {
                string sourceFile = parentFiles[i];
                string destinationFile = parentFiles[i].Replace(parentUserName, childUserName);

                File.Copy(sourceFile, destinationFile);
            }
        }

        private void CopyDataFolder(string parentDir, string childDir, string parentUserName, string childUserName)
        {
            string dataDir = childDir + "Data";
            Directory.CreateDirectory(dataDir);
            string[] dataDirFiles = Directory.GetFiles(parentDir + "Data");

            for (int i = 0; i < dataDirFiles.Length; i++)
            {
                string sourceFile = dataDirFiles[i];
                string destinationFile = dataDirFiles[i].Replace(parentUserName, childUserName);

                // Don't copy the Accounts file
                if (!destinationFile.EndsWith("Accounts.txt"))
                    File.Copy(sourceFile, destinationFile);
            }
        }

        private void CopyPackagesFolder(string parentDir, string childDir, string parentUserName, string childUserName)
        {
            string packagesDir = childDir + "packages";
            Directory.CreateDirectory(packagesDir);
            Directory.CreateDirectory(packagesDir + "\\SquirrelTemp");
            string[] packageDirFiles = Directory.GetFiles(parentDir + "packages");

            for (int i = 0; i < packageDirFiles.Length; i++)
            {
                string sourceFile = packageDirFiles[i];
                string destinationFile = packageDirFiles[i].Replace(parentUserName, childUserName);

                File.Copy(sourceFile, destinationFile);
            }
        }

        private void CopyMiscFiles(string parentDir, string childDir, string parentUserName, string childUserName)
        {
            //Copy Update.exe
            string sourceFile1 = parentDir + "Update.exe";
            string destinationFile1 = sourceFile1.Replace(parentUserName, childUserName);

            File.Copy(sourceFile1, destinationFile1);

            // Copy WVA Scan.exe
            string sourceFile2 = parentDir + "WVA Scan.exe";
            string destinationFile2 = sourceFile2.Replace(parentUserName, childUserName);

            File.Copy(sourceFile2, destinationFile2);
        }

        private void CreateDesktopShortCut(string parentDir, string childDir, string parentUserName, string childUserName)
        {
            string childDesktopPath = $@"C:\Users\{childUserName}\Desktop";
            string childShortcutPath = $@"{childDesktopPath}\WVA Scan.lnk";
            string childTargetRunPath = $@"{childDir}WVA Scan.exe";

            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut childShortCut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(childShortcutPath);
            childShortCut.IconLocation = Paths.ShortcutIconPath;
            childShortCut.TargetPath = childTargetRunPath;
            childShortCut.Save();
        }

        private void LogNewInstall(string userName, bool installedSuccessfully)
        {
            try
            {
                if (!Directory.Exists(Paths.PublicLogsDir))
                    Directory.CreateDirectory(Paths.PublicLogsDir);

                if (!File.Exists(Paths.LogPath))
                    File.Create(Paths.LogPath).Close();

                var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string status = installedSuccessfully ? "SUCCESS" : "FAIL";
                var message = $"[{status}] Ran WVA Scan install for user '{userName}' on '{time}'\n";

                File.AppendAllText(Paths.LogPath, message);
            }
            catch { }
        }
    }
}
