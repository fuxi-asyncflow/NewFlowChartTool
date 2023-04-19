using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Common
{
    public static class FileHelper
    {
        public static bool IsRelativePath(string path)
        {
            if (path.Length > 1 && path[1] == ':')
            {
                return false;
            }
            return true;
        }
        public static string GetFullPath(string workDir, string path)
        {
            if (!IsRelativePath(path))
                return path;
            if (path.StartsWith("/"))
            {
                path = "." + path;
            }
            // 将工作路径与相对路径进行拼接
            var fullpath = Path.Combine(workDir, path);
            // 去掉路径中的..
            return Path.GetFullPath(new Uri(fullpath).LocalPath);
        }

        public static string GetRelativePath(string filePath, string? referencePath)
        {
            referencePath ??= GetExeFolder();
#if NET6_0_OR_GREATER
            return Path.GetRelativePath(referencePath, filePath);
#else
            string? fileName = null;
            if (!filePath.EndsWith("\\"))
            {
                FileInfo fi = new FileInfo(filePath);
                filePath = fi.DirectoryName;
                fileName = fi.Name;
            }

            string result = "";
            var fileUri = new Uri(filePath + "\\");
            var referenceUri = new Uri(referencePath + "\\");
            if (filePath != referencePath)
                result = referenceUri.MakeRelativeUri(fileUri).ToString();

            if (string.IsNullOrEmpty(result))
            {
                result = "./";
            }

            if (fileName != null)
            {
                result = result + fileName;
            }
            return result;
#endif
        }

        public static string GetExeFolder()
        {
            return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }

        public static string GetFolder(string subDir)
        {
            var path =  Path.Combine(GetExeFolder(), subDir);
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var di = new DirectoryInfo(path);
            return di.FullName;
        }

        public static void Save(string path, string content)
        {
            Save(path, content, Encoding.UTF8);
        }

        public static void Save(string path, string content, Encoding encode)
        {
            if (System.IO.File.Exists(path))
            {
                var oldContent = File.ReadAllText(path, encode);
                if (oldContent == content)
                    return;
            }
            else
            {
                var di = new FileInfo(path).Directory;
                if (di == null)
                {
                    Logger.ERR($"invalid folder name for file {path}");
                }
                else
                    System.IO.Directory.CreateDirectory(di.FullName);

            }
            System.IO.File.WriteAllText(path, content, encode);
        }

        public static void Save(string path, List<string> lines)
        {
            Save(path, string.Join("\r\n", lines));
        }

        public static void Save(string path, List<string> lines, Encoding encode)
        {
            Save(path, string.Join("\r\n", lines), encode);
        }
    }
}
