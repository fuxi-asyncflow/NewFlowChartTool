using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NFCT.Common
{
    public static class Dialogs
    {
        public static string? ChooseFolder()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return null;
            var selectPath = dialog.SelectedPath;
            return selectPath;
        }
    }
}
