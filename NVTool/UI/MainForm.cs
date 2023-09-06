using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using NVTool.BLL;
using NVTool.DAL;
using NVTool.DAL.Model;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Common;
using NVParam.DAL;

namespace NVTool.UI
{
    public partial class MainForm : XtraForm
    {
        public MainForm()
        {
            InitializeComponent();

            InitSkinStyle();

            InitForm();

            InitProjectParam();

            PerformUpload();
        }

        #region Event Function
        private void HandleStateChanged(object sender, StateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case NVState.Disconnected:
                    barLoad.Enabled = true;
                    barSave.Enabled = true;
                    barCom.Enabled = true;
                    barConnect.Enabled = true;
                    barStop.Enabled = false;
                    barUpload.Enabled = false;
                    barDownload.Enabled = false;
                    break;
                case NVState.Connect:
                    barLoad.Enabled = false;
                    barSave.Enabled = false;
                    barCom.Enabled = false;
                    barConnect.Enabled = false;
                    barStop.Enabled = true;
                    barUpload.Enabled = true;
                    barDownload.Enabled = true;
                    break;
            }
        }
        #endregion

        #region Init Function
        private void InitProjectParam()
        {
            // 获取当前程序集的程序集信息
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 获取程序集的版本号
            Version assemblyVersion = assembly.GetName().Version;
            // 获取程序集的版权信息
            string assemblyCopyright = ProjectCommon.GetAssemblyCopyright(assembly);
            barCopyright.Caption = assemblyCopyright;
            barVersion.Caption = "Version:" + assemblyVersion.ToString();
            barRelease.Caption = string.Format(
                "CompileTime:{0}",
                File.GetLastWriteTime(this.GetType().Assembly.Location).ToString());
        }

        private void InitSkinStyle()
        {
            // 设置全局主题
            UserLookAndFeel.Default.SetSkinStyle(SkinStyle.DevExpress);

        }

        private void InitForm()
        {
            FormProject formNVTable = new FormProject();
            formNVTable.SubscribeToButtonClickedEvent(this);
            // 设置 formNVTable 窗体的 Dock 属性为 Fill
            formNVTable.Dock = DockStyle.Fill;
            panelWorkspace.Controls.Add(formNVTable);

            formNVTable.StateChanged += HandleStateChanged;

            HandleStateChanged(this, new StateChangedEventArgs(NVState.Disconnected));

#if DEBUG
            barSaveForBin.Enabled = true;
            barLoadFromBin.Enabled = true;
            barInPhone.Enabled = true;
            barOutPhone.Enabled = true;
#else
            barSaveForBin.Enabled = false;
            barLoadFromBin.Enabled = false;
            barInPhone.Enabled = false;
            barOutPhone.Enabled = false;
#endif
        }
        #endregion

        #region Button Clicked

        private void barStop_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.Stop);
        }

        private void barConnect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.Connect);
        }

        private void barUpload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.Upload);
        }

        private void barDownload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.Download);
        }

        private void barCom_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.Communication);
        }
        private void barSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.SaveProject);
        }

        /// <summary>
        /// Load Project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barLoad_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.LoadProject);
        }

        /// <summary>
        /// Load From Bin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barLoadFromBin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.LoadImage);
        }

        /// <summary>
        /// Save For Bin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barSaveForBin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //选择bin文件
            OnButtonClicked(ButtonType.SaveImage);
        }

        /// <summary>
        /// Load From Phone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barOutPhone_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.LoadFromPhone);
        }

        private void barInPhone_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.SaveToPhone);
        }

        private void barSaveImage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.SaveImage);
        }
        private void barTest_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
       
            OnButtonClicked(ButtonType.Test);
        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barItemSaveAs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnButtonClicked(ButtonType.SaveAsProject);
        }

        private void barItemExcelParser_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //// 创建 DataTable
            //DataTable dataTable = new DataTable("MyTable");

            //// 定义数据表的列，例如添加两列：ID 和 Name
            //dataTable.Columns.Add("ID", typeof(int));
            //dataTable.Columns.Add("Name", typeof(string));

            //// 向数据表中添加数据行
            //dataTable.Rows.Add(1, "John");
            //dataTable.Rows.Add(2, "Alice");
            //dataTable.Rows.Add(3, "Bob");

            //string filePath = "NVTable.xlsx";

            //try
            //{
            //    bool success = NVExcelHelper.OutputToExcel(dataTable, filePath);

            //    if (success)
            //    {
            //        Console.WriteLine("Successfully Excel File。");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Export Excel Failed。");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ProjectCommon.ShowMessage(new BoolQResult(false, ex.Message));
            //}
            try
            {
                string filePath = ProjectCommon.GetFilePathByDialog(EDiagFileType.Excel);
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                ExcelParser excelParser = new ExcelParser();
                ItemDataNode node;
                BoolQResult ret = excelParser.ExcelToItemNode(filePath, out node);
                ProjectCommon.ShowMessage(ret);
            }
            catch (Exception ex)
            {
                ProjectCommon.ShowMessage(new BoolQResult(false, ex.Message));
            }
        }

        public event EventHandler<ButtonClickedEventArgs> ButtonClicked;
        protected virtual void OnButtonClicked(ButtonType btnType)
        {
            ButtonClicked?.Invoke(this, new ButtonClickedEventArgs(btnType));
        }
        #endregion

        #region Normal Fucntion
        /// <summary>
        /// Get Center Point
        /// </summary>
        /// <param name="childSize"></param>
        /// <returns></returns>
        private Point GetCenteredLocation(Size childSize)
        {
            int x = (this.Width - childSize.Width) / 2 + this.Left;
            int y = (this.Height - childSize.Height) / 2 + this.Top;
            return new Point(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        private void PerformUpload()
        {
            PermissionManager permissionManager = PermissionManager.Instance;

            switch (permissionManager.UserRole)
            {
                case UserRole.Administrator:
                case UserRole.Develeoper:
                    barSaveForBin.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                    barLoadFromBin.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                    barOutPhone.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                    barInPhone.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                    break;
                case UserRole.User:
                case UserRole.Tester:
                    barSaveForBin.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                    barLoadFromBin.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                    barOutPhone.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                    barInPhone.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                    break;
            }
        }
        #endregion

   
    }
}
