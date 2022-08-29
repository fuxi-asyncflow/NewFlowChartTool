using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NFCT.Common.Localization
{
    public class ResourceKeys
    {
        static ComponentResourceKey KEY(string key)
        {
            return new ComponentResourceKey(typeof(ResourceKeys), key);
        }

        #region MainMenu
        public static readonly ComponentResourceKey Menu_FileKey = KEY("MainMenu_File");
        public static readonly ComponentResourceKey Menu_OpenProjectKey = KEY("MainMenu_OpenProject");
        public static readonly ComponentResourceKey Menu_SaveProjectKey = KEY("MainMenu_SaveProject");
        public static readonly ComponentResourceKey Menu_NewProjectKey = KEY("MainMenu_NewProject");
        public static readonly ComponentResourceKey Menu_CloseProjectKey = KEY("MainMenu_CloseProject");

        public static readonly ComponentResourceKey Menu_BuildKey = KEY("MainMenu_Build");
        public static readonly ComponentResourceKey Menu_BuildAllKey = KEY("MainMenu_BuildAll");

        #endregion
    }
}
