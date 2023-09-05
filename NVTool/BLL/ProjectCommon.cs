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
    internal enum EDiagFileType
    {
        txt,
        xml,
        Excel
    }

    internal static class ProjectCommon
    {
        /// <summary>
        /// 打开文件对话框，获取用户选择的文件路径
        /// </summary>
        /// <param name="eDiagFileType">指定文件类型</param>
        /// <returns>所选文件的路径</returns>
        internal static string GetFilePathByDialog(EDiagFileType eDiagFileType)
        {
            string sTitle = string.Empty;
            string sFilter = string.Empty;

            // 根据选择的文件类型设置对话框标题和筛选器
           switch (eDiagFileType)
{
    case EDiagFileType.txt:
        sTitle = "Select a txt file";
        sFilter = "TXT Files (*.txt)|*.txt";
        break;
    case EDiagFileType.xml:
        sTitle = "Select an xml File";
        sFilter = "Xml Files (*.xml)|*.xml";
        break;
    case EDiagFileType.Excel:
        sTitle = "Select an Excel File";
        sFilter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls";
        break;
}

            string filePath = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = sTitle;
                openFileDialog.Filter = sFilter;

                // 显示打开文件对话框，获取用户选择的文件
                DialogResult dialogResult = openFileDialog.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
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
