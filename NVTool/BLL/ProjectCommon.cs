/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : ProjectCommon.cs
* date      : 2023/7/17 16:44:33
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

using Common;
using DevExpress.XtraEditors;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace NVTool.BLL
{
    enum EDiagFileType
    {
        txt,
        xml,
        excel,
        bin
    }
    enum EDiagType
    {
        Select,
        Save
    }

    enum ColumnName
    {
        ItemName,
        ItemID,
        ItemValue,
        Content
    }

    internal static class ProjectCommon
    {
        /// <summary>
        /// Displays a file dialog for selecting a file or specifying a location to save a file
        /// based on the specified file type.
        /// </summary>
        /// <param name="eDiagFileType">The type of file operation to perform (Select or Save).</param>
        /// <param name="fileType">The type of file to select or save (e.g., txt, xml, bin).</param>
        /// <returns>The selected file's path or the specified save location, or an empty string if canceled.</returns>
        internal static string FileDialog(EDiagType eDiagFileType, EDiagFileType fileType)
        {
            string sTitle = string.Empty;
            string sFilter = string.Empty;
            string filePath = string.Empty;

            // Set the dialog title and filter based on the file type and operation.
            switch (fileType)
            {
                case EDiagFileType.txt:
                    sFilter = "TXT Files (*.txt)|*.txt";
                    break;
                case EDiagFileType.xml:
                    sFilter = "Xml Files (*.xml)|*.xml";
                    break;
                case EDiagFileType.excel:
                    sFilter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls";
                    break;
                case EDiagFileType.bin:
                    sFilter = "Binary Files (*.bin)|*.bin";
                    break;
            }

            // Determine the title based on the file operation.
            if (eDiagFileType == EDiagType.Select)
            {
                sTitle = $"Select a {fileType} file";
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = sTitle;
                    openFileDialog.Filter = sFilter;

                    // Show the open file dialog and get the user's file selection.
                    DialogResult dialogResult = openFileDialog.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        filePath = openFileDialog.FileName;
                    }
                }
            }
            else if (eDiagFileType == EDiagType.Save)
            {
                sTitle = $"Save as {fileType} file";
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = sTitle;
                    saveFileDialog.Filter = sFilter;

                    // Show the save file dialog and get the user's specified save location.
                    DialogResult dialogResult = saveFileDialog.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        filePath = saveFileDialog.FileName;
                    }
                }
            }

            return filePath;
        }
        /// <summary>
        /// Get Copyright
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        internal static string GetAssemblyCopyright(Assembly assembly)
        {
            Type att = typeof(AssemblyCopyrightAttribute);
            object[] r = assembly.GetCustomAttributes(att, false);
            AssemblyCopyrightAttribute copyattr = (AssemblyCopyrightAttribute)r[0];
            return copyattr.Copyright;
        }

        /// <summary>
        /// Shows a message box with the given BoolQResult information.
        /// </summary>
        /// <param name="boolQResult">The BoolQResult containing the message and result status.</param>
        /// <param name="isShowRight">Optional parameter to determine if the message should be shown for a successful result. Default is true.</param>
        internal static void ShowMessage(BoolQResult boolQResult, bool isShowRight = true)
        {
            if (boolQResult.Result)
            {
                if (isShowRight)
                {
                    // Show an information message box for successful results.
                    XtraMessageBox.Show(boolQResult.Msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Show a warning message box for unsuccessful results.
                XtraMessageBox.Show(boolQResult.Msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
