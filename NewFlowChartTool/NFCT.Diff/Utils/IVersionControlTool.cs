using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCT.Diff.Utils
{
    public class VersionItem
    {
        public VersionItem(string version, string author, DateTime time, string message)
        {
            Version = version;
            Author = author;
            Time = time;
            Message = message;
        }

        public string Version { get; set; } // empty for local version
        public string Author { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public bool IsLocalUnCommit { get; set; }
    }

    public interface IVersionControlTool
    {
        public string ExePath { get; set; }
        public string WorkingDir { set; }
        public List<VersionItem> GetHistory(string path);
        public List<string> GetChangedFiles(string path, string version);
        public Tuple<string, string> ExportFiles(string file, string version);
    }
}
