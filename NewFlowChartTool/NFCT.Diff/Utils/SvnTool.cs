using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FlowChart.Common;

namespace NFCT.Diff.Utils
{
    internal class SvnTool: IVersionControlTool
    {
        public SvnTool(string exePath)
        {
            ExePath = System.IO.File.Exists(exePath) ? exePath : "svn.exe";
            StartInfo = new ProcessStartInfo(ExePath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
        }
        public string WorkingDir
        {
            get => StartInfo.WorkingDirectory;
            set => StartInfo.WorkingDirectory = value;
        }
        public string ExePath { get; set; }
        public string GraphFileFolder { get; set; }
        public string SvnRepoUrl { get; set; }
        ProcessStartInfo StartInfo { get; set; }

        string GetSvnRepoUrl(string path)
        {
            var args = "info --show-item url";
            var output = RunCommand(args);
            if(output == null)
                return string.Empty;
            return output.Trim();
        }

        public List<VersionItem> GetHistory(string path)
        {
            SvnRepoUrl = GetSvnRepoUrl(path);
            var args = $"log --limit 100 --xml {SvnRepoUrl}";
            var output = RunCommand(args);
            var versionItems = new List<VersionItem>();
            if (output == null)
                return versionItems;

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(output);
            }
            catch (Exception e)
            {
                Logger.ERR("load svn history failed, output is not valid xml file" + e.Message);
                return versionItems;
            }

            var rootNode = xmlDoc.DocumentElement;
            if (rootNode == null)
            {
                Logger.ERR("load svn history failed, xml info has no root");
                return versionItems;
            }

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                var time = DateTime.Parse(node["date"].InnerText);
                var item = new VersionItem(node.Attributes["revision"].Value, node["author"].InnerText, time, node["msg"] == null ? string.Empty : node["msg"].InnerText);
                versionItems.Add(item);
            }
            return versionItems;
        }

        public List<string> GetChangedFiles(string path, string version)
        {
            var args = $"diff --summarize -c {version} {path}";
            if (string.IsNullOrEmpty(version))
                args = $"status {path}";

            var output = RunCommand(args);
            var fileList = new List<string>();
            if (output == null)
                return fileList;
            output = output.TrimEnd();
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if(line.Length == 0)
                    continue;
                if (line[0] == 'M')
                {
                    if(line.Length < 8)
                        continue;
                    var name = line.Substring(8).Trim();
                    fileList.Add(name);
                }
            }

            return fileList;
        }

        public Tuple<string, string> ExportFiles(string file, string version)
        {
            string? oldFile;
            string? newFile;
            if (string.IsNullOrEmpty(version))
            {
                oldFile = ExportFile(file, "BASE");
                newFile = CopyLocalFile(file);
            }
            else
            {
                int v = Int32.Parse(version);
                oldFile = ExportFile(file, (v - 1).ToString());
                newFile = ExportFile(file, v.ToString());
            }

            if (oldFile == null || newFile == null)
                return new Tuple<string, string>(string.Empty, string.Empty);
            return new Tuple<string, string>(oldFile, newFile);
        }

        public string? ExportFile(string file, string version)
        {
            file = file.Trim('\\').Trim('/');
            var tmpFolder = FileHelper.GetFolder("tmp");
            var exportFileName = $"{file}.r{version}".Replace('\\', '_').Replace('/', '_');
            exportFileName = Path.Combine(tmpFolder, exportFileName);
            var arg = $"export -r {version} {file} {exportFileName}";
            RunCommand(arg);
            if (System.IO.File.Exists(exportFileName))
                return exportFileName;
            return null;
        }

        public string? CopyLocalFile(string file)
        {
            file = file.Trim('\\').Trim('/');
            var tmpFolder = FileHelper.GetFolder("tmp");
            var exportFileName = $"{file}.rmine".Replace('\\', '_').Replace('/', '_');
            exportFileName = Path.Combine(tmpFolder, exportFileName);

            File.Copy(Path.Combine(WorkingDir, file), exportFileName);
            
            if (System.IO.File.Exists(exportFileName))
                return exportFileName;
            return null;
        }

        public string? RunCommand(string args)
        {
            Logger.DBG($"run svn: {args}");
            StartInfo.Arguments = args;
            var process = new Process()
            {
                StartInfo = StartInfo
            };
            try
            {
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                Logger.DBG(output);
                process.WaitForExit();
                process.Close();
                return output;
            }
            catch (Exception e)
            {
                Logger.WARN($"Run svn command {args} error: {e.Message}");
                return null;
            }
            
        }
    }
}
