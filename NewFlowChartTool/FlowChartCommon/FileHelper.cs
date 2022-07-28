using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChartCommon
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

        public static string GetRelativePath(string filePath, string referencePath)
        {
            if (referencePath == null)
                referencePath = GetExeFolder();

            string fileName = null;
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
        }

        public static string GetExeFolder()
        {
            return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }
    }
}
