using DevExpress.XtraEditors;
using DevExpress.XtraLayout.Utils;
using NVParam.DAL;
using NVTool.DAL.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace NVTool.UI
{
    public partial class FormNodeInfo : XtraForm
    {
        #region Constructor
        public FormNodeInfo()
        {
            InitializeComponent();

            InitParam();
        }
        #endregion

        #region Init Function
        void InitParam()
        {
            EDataType[] enumValues = Enum.GetValues(typeof(EDataType)).Cast<EDataType>().ToArray();
            // 去除最后一项
            EDataType[] trimmedEnumValues = new EDataType[enumValues.Length - 1];
            Array.Copy(enumValues, trimmedEnumValues, trimmedEnumValues.Length);
            cBoxType.Properties.Items.AddRange(trimmedEnumValues);

            SettingArrayVisable(false);
            cBoxType.SelectedIndex = 0;
        }
        #endregion

        #region Normal Function
        /// <summary>
        /// 类型变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingArrayVisable(cBoxType.SelectedText == EDataType.Array.ToString());
        }

        /// <summary>
        /// 设置数组长度的参数
        /// </summary>
        /// <param name="isVisable"></param>
        private void SettingArrayVisable(bool isVisable)
        {
            //Hide Length setting
            LayoutVisibility layoutVisibility = isVisable ? LayoutVisibility.Always : LayoutVisibility.Never;
            itemArray1D.Visibility = layoutVisibility;
            itemArray2D.Visibility = layoutVisibility;
        }

        /// <summary>
        /// 获取Item Node Data
        /// </summary>
        /// <returns></returns>
        public ItemNodeInfo GetItemNode()
        {
            ItemNodeInfo itemNodeInfo = new ItemNodeInfo()
            {
                Name = textEditName.Text,
                ID = textEditID.Text,
                DataType = (EDataType)Enum.Parse(typeof(EDataType), cBoxType.Text),
                Content = memoEditContent.Text,
            };

            if (itemNodeInfo.DataType == EDataType.Array)
            {
                itemNodeInfo.Array1DLen = string.IsNullOrEmpty(textArray1D.Text) ? 0 : int.Parse(textArray1D.Text);
                itemNodeInfo.Array2DLen = string.IsNullOrEmpty(textArray2D.Text) ? 0 : int.Parse(textArray2D.Text);
            }

            return itemNodeInfo;
        }

        #endregion

        #region Clicked Event

        //响应取消事件
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 响应OK事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;
            //逻辑判断
            if (string.IsNullOrEmpty(textEditName.Text))
            {
                XtraMessageBox.Show("Please Input Name!", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                return;
            }

            if (cBoxType.SelectedText == EDataType.Array.ToString())
            {
                if (string.IsNullOrEmpty(textArray1D.Text))
                {
                    XtraMessageBox.Show("Please enter the length of the array!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            DialogResult = DialogResult.OK;
        }


        #endregion
    }
}