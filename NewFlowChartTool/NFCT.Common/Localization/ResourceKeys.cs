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

        #region Common
        public static readonly ComponentResourceKey Common_Id = KEY(nameof(Common_Id));
        public static readonly ComponentResourceKey Common_Name = KEY(nameof(Common_Name));
        public static readonly ComponentResourceKey Common_Type = KEY(nameof(Common_Type));
        public static readonly ComponentResourceKey Common_Description = KEY(nameof(Common_Description));
        public static readonly ComponentResourceKey Common_Value = KEY(nameof(Common_Value));
        public static readonly ComponentResourceKey Common_Default = KEY(nameof(Common_Default));
        public static readonly ComponentResourceKey Common_Add = KEY(nameof(Common_Add));
        public static readonly ComponentResourceKey Common_Delete = KEY(nameof(Common_Delete));
        public static readonly ComponentResourceKey Common_Modify = KEY(nameof(Common_Modify));
        public static readonly ComponentResourceKey Common_Confirm = KEY(nameof(Common_Confirm));
        public static readonly ComponentResourceKey Common_Cancel = KEY(nameof(Common_Cancel));

        #endregion

        #region MainMenu
        public static readonly ComponentResourceKey Menu_FileKey = KEY("MainMenu_File");
        public static readonly ComponentResourceKey Menu_OpenProjectKey = KEY("MainMenu_OpenProject");
        public static readonly ComponentResourceKey Menu_SaveProjectKey = KEY("MainMenu_SaveProject");
        public static readonly ComponentResourceKey Menu_NewProjectKey = KEY("MainMenu_NewProject");
        public static readonly ComponentResourceKey Menu_CloseProjectKey = KEY("MainMenu_CloseProject");
        public static readonly ComponentResourceKey Menu_TypeManagerKey = KEY("MainMenu_TypeManager");

        public static readonly ComponentResourceKey Menu_BuildKey = KEY("MainMenu_Build");
        public static readonly ComponentResourceKey Menu_BuildAllKey = KEY("MainMenu_BuildAll");

        #endregion

        #region ToolBar

        public static readonly ComponentResourceKey ToolBar_UndoKey = KEY("ToolBar_Undo");
        public static readonly ComponentResourceKey ToolBar_RedoKey = KEY("ToolBar_Redo");

        //debug
        public static readonly ComponentResourceKey ToolBar_QuickDebug = KEY("ToolBar_QuickDebug");
        public static readonly ComponentResourceKey ToolBar_DebugDialog = KEY("ToolBar_DebugDialog");
        public static readonly ComponentResourceKey ToolBar_ContinueDebug = KEY("ToolBar_ContinueDebug");
        public static readonly ComponentResourceKey ToolBar_StopDebug = KEY("ToolBar_StopDebug");
        public static readonly ComponentResourceKey ToolBar_HotReload = KEY("ToolBar_HotReload");
        #endregion

        #region PaneName
        public static readonly ComponentResourceKey Pane_ProjectKey = KEY("PaneName_Project");
        public static readonly ComponentResourceKey Pane_TypeKey = KEY("PaneName_Type");
        public static readonly ComponentResourceKey Pane_OutputKey = KEY("PaneName_Output");
        #endregion

        #region ProjectTreeItemMenu

        public const string Menu_RenameKey = "Menu_Rename";
        public const string Menu_NewGraphKey = "Menu_NewGraph";
        public const string Menu_NewFolderKey = "Menu_NewFolder";
        public const string Menu_RemoveGraphKey = "Menu_Remove";

        #endregion

    }
}
