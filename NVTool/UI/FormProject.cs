using Channel;
using Channel.CommPort;
using Common;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Menu;
using DevExpress.XtraTreeList.Nodes;
using log4net;
using NVParam.BLL;
using NVParam.DAL;
using NVTool.BLL;
using NVTool.DAL.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NVTool.UI
{
    public partial class FormProject : XtraUserControl
    {
        #region Attribute
        private NVRamManage nvRamManage;
        public Form parentForm { get; set; }

        private ComParam comParam = null;

        private Dictionary<TreeListNode, object> originalValues; // 用于记录每个节点的原始值
        // 声明状态转换的事件
        private NVState currentState = NVState.Disconnected;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        //log Message
        bool IsTabPageCommMessageSelect = false;   // 通信Tab页选中标记
        bool IsMessageCapture = false;             // 通信报文捕获标记
        string MesaageCaptrurePath = string.Empty; // 通信报文捕获路径
        bool IsShowMsg = true;
        private const int MaxLines = 1000; // 最大行数
        StreamWriter wr;
        FileStream fs;
        #endregion

        #region Constructor
        public FormProject()
        {
            InitializeComponent();

            InitParam();
        }
        #endregion

        #region Init Function
        private void InitParam()
        {
            LogNetHelper.Info("NV Tool is runing");
            treeListNVParam.BestFitColumns();
            nvRamManage = new NVRamManage();
            nvRamManage.CmindProtocol.BytesSendReceiveThrow += GlobalEventHandler.TriggerCommMsgEvent;

            // 初始化原始值字典
            originalValues = new Dictionary<TreeListNode, object>();

            // 触发状态转换事件，通知其他对象状态的改变
            OnStateChanged(new StateChangedEventArgs(currentState));
        }

        #endregion

        #region Event Function
        /// <summary>
        /// Button Clicked Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleBtnClicked(object sender, ButtonClickedEventArgs e)
        {
            // 处理按钮点击事件
            switch (e.btnType)
            {
                case ButtonType.LoadProject:
                    LoadProject();
                    break;
                case ButtonType.SaveProject:
                    SaveProject();
                    break;
                case ButtonType.SaveAsProject:
                    SaveAsProjectFile();
                    break;
                case ButtonType.Communication:
                    SetCommunication();
                    break;
                case ButtonType.Connect:
                    ConnectCommunication();
                    break;
                case ButtonType.Stop:
                    StopCommunication();
                    break;
                case ButtonType.Download:
                    DownloadNVData();
                    break;
                case ButtonType.Upload:
                    UploadNVData();
                    break;
                case ButtonType.LoadImage:
                    LoadImage();
                    break;
                case ButtonType.LoadFromPhone:
                    LoadFromPhone();
                    break;
                case ButtonType.SaveImage:
                    SaveImage();
                    break;
                case ButtonType.SaveToPhone:
                    SaveToPhone();
                    break;
                case ButtonType.Test:
                    TestFunction();
                    break;
                case ButtonType.ExcelParser:
                    ExcelParserHandle();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Subscribe Event
        /// </summary>
        /// <param name="mainForm"></param>
        public void SubscribeToButtonClickedEvent(MainForm mainForm)
        {
            this.parentForm = mainForm;
            mainForm.ButtonClicked += HandleBtnClicked;

            //log window
            GlobalEventHandler.commEvent += HandleShowCommMessageEvent;
            GlobalEventHandler.runMessageEvent += HandleShowMessageEvent;

            // 订阅CellValueChanging事件
            treeListNVParam.CellValueChanging += TreeList_CellValueChanging;

            // 订阅CellValueChanged事件
            treeListNVParam.CellValueChanged += TreeList_CellValueChanged;
        }
        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeList_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e)
        {
            string columnName = e.Column.FieldName;

            // 如果当前列名不是"ItemValue"，则不进行编辑器设置
            if (columnName != "ItemValue")
            {
                return;
            }

            // 获取当前节点
            TreeListNode node = e.Node;
            object ObjValue = node["DataType"];

            // 尝试将字符串转换为枚举类型
            if (Enum.TryParse(ObjValue.ToString(), out EDataType dataType))
            {
                if ((dataType == EDataType.Array || dataType == EDataType.Class) || dataType == EDataType.Unknown)
                {
                    e.RepositoryItem = null; // 禁止编辑
                    return;
                }

                RepositoryItemSpinEdit spinItem = new RepositoryItemSpinEdit();
                spinItem.IsFloatValue = false;
                spinItem.Buttons[0].Visible = false;
                spinItem.EditFormat.FormatString = "0"; // 编辑器格式不带单位
                // 设置为整数编辑器
                e.RepositoryItem = spinItem;

                switch (dataType)
                {
                    case EDataType.SBYTE:
                        spinItem.MinValue = sbyte.MinValue;
                        spinItem.MaxValue = sbyte.MaxValue;
                        break;
                    case EDataType.BYTE:
                        spinItem.MinValue = byte.MinValue;
                        spinItem.MaxValue = byte.MaxValue;
                        break;
                    case EDataType.SHORT:
                        spinItem.MinValue = short.MinValue;
                        spinItem.MaxValue = short.MaxValue;
                        break;
                    case EDataType.USHORT:
                        spinItem.MinValue = ushort.MinValue;
                        spinItem.MaxValue = ushort.MaxValue;
                        break;
                    case EDataType.INT:
                        spinItem.MinValue = int.MinValue;
                        spinItem.MaxValue = int.MaxValue;
                        break;
                    case EDataType.UINT:
                        spinItem.MinValue = uint.MinValue;
                        spinItem.MaxValue = uint.MaxValue;
                        break;
                    case EDataType.LONG:
                        spinItem.MinValue = long.MinValue;
                        spinItem.MaxValue = long.MaxValue;
                        break;
                }
            }
        }
        void TreeList_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            // 记录每个节点的原始值
            TreeListNode node = treeListNVParam.FocusedNode;
            if (node != null)
            {
                object originalValue = node.GetValue(e.Column);
                if (!originalValues.ContainsKey(node))
                {
                    originalValues[node] = originalValue;
                }
            }

            // 不允许将"ItemValue"列的值修改为空
            string strValue = e.Value as string;
            if (e.Column.FieldName == "ItemValue" && string.IsNullOrEmpty(strValue))
            {
                Console.WriteLine("Value = " + originalValues[node]);
                e.Value = originalValues[node]; // 保持原值不变
            }
        }

        private void TreeList_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            // 当值发生更改时，刷新单元格的显示
            treeListNVParam.RefreshCell(treeListNVParam.FocusedNode, treeListNVParam.FocusedColumn);
        }

        private void TreeList_CustomDrawNodeCell(object sender, CustomDrawNodeCellEventArgs e)
        {
            // 只有在"ItemValue"列的值更改时，才将行的字体颜色更改为红色
            if ((e.Column.FieldName == "ItemValue" || e.Column.FieldName == "ItemID") && e.Node != null)
            {
                object value = e.CellValue;
                if (value != null && int.TryParse(value.ToString(), out int intValue))
                {
                    string decimalText = intValue.ToString(); // 十进制表示
                    string hexText = $"0x{intValue:X}"; // 十六进制表示
                    string displayText = $"{decimalText} ({hexText})";
                    e.CellText = displayText; // 更新单元格显示的内容
                }

                TreeListNode node = e.Node;
                object originalValue;
                if (originalValues.TryGetValue(node, out originalValue) && !Equals(originalValue, node.GetValue(e.Column)))
                {
                    e.Appearance.ForeColor = Color.Red;
                }
                else
                {
                    e.Appearance.ForeColor = treeListNVParam.Appearance.FocusedCell.ForeColor; // 恢复默认颜色
                }
            }
        }

        private void TreeList_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.Menu is TreeListNodeMenu)
            {
                // 添加自定义菜单项
                DXMenuItem addItem = new DXMenuItem("Add Node", AddNode_Click);
                DXMenuItem addChildItem = new DXMenuItem("Add Child Node", AddChildNode_Click);
                DXMenuItem deleteItem = new DXMenuItem("Delete Node", this.DeleteNode_Click);

                e.Menu.Items.Add(addItem);
                e.Menu.Items.Add(addChildItem);
                e.Menu.Items.Add(deleteItem);
            }
        }

        #endregion

        #region Class Function

        bool bUploadFlag = false;
        /// <summary>
        /// 上传NVData
        /// </summary>
        private void UploadNVData()
        {
            if (!bUploadFlag)
            {
                bUploadFlag = true;
                // 启动一个任务
                GlobalEventHandler.TriggerrunMsgEvent("Start to Write Param");
                ShowSplashScreen("Please Waiting!", "Uploading in progress");

                Task.Run(() =>
                {
                    Action<int, double> progressCallBack = (itemId, progress) =>
                    {
                        //处理进度更新的逻辑
                        string result = progress.ToString("0.00");
                        ShowSplashTips("Please Waiting!", $"ItemID({itemId}) Completed {result}%");
                    };

                    BoolQResult bResult = nvRamManage.UploadProject(progressCallBack);

                    GlobalEventHandler.TriggerrunMsgEvent("End to Write Param");
                    bUploadFlag = false;
                    CloseSplashScreen();
                    if (bResult != null)
                    {
                        ProjectCommon.ShowMessage(bResult);
                    }
                });
            }
        }

        bool bDownloadFlag = false;
        /// <summary>
        /// Download NV Data
        /// </summary>
        private void DownloadNVData()
        {
            if (!bDownloadFlag)
            {
                bDownloadFlag = true;
                // 启动一个任务
                GlobalEventHandler.TriggerrunMsgEvent("Start to Read Param");
                ShowSplashScreen("Please Waiting!", "Downloading in progress");
                Task.Run(() =>
                {
                    Action<int, double> progressCallBack = (itemId, progress) =>
                    {
                        // 处理进度更新的逻辑
                        string result = progress.ToString("0.00");
                        ShowSplashTips("Please Waiting!", $"ItemID({itemId}) Completed {result}%");
                    };

                    BoolQResult bResult = nvRamManage.DownloadProject(progressCallBack);
                    if (bResult.Result)
                    {
                        UpdateTreelist();
                    }

                    // 释放互斥体的所有权
                    bDownloadFlag = false;
                    CloseSplashScreen();
                    GlobalEventHandler.TriggerrunMsgEvent("End to Read Param");

                    ProjectCommon.ShowMessage(bResult);
                });
            }
        }

        /// <summary>
        /// Update Tree List
        /// </summary>
        private void UpdateTreelist()
        {
            List<ItemDataNode> listNodes = nvRamManage.NvRamParam.Item.GetAllNodes();
            this.Invoke(new EventHandler(delegate
            {
                treeListNVParam.DataSource = listNodes;
                treeListNVParam.RefreshDataSource(); // 刷新TreeList显示更新后的数据
            }));
        }

        /// <summary>
        /// load project
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadProject()
        {
            string filePath = ProjectCommon.FileDialog(EDiagType.Select,EDiagFileType.xml);
            if (filePath != string.Empty)
            {
                try
                {
                    //get the source of xml
                    BoolQResult qResult = nvRamManage.LoadSource(filePath);

                    //show the result
                    if (qResult.Result == true)
                    {
                        UpdateTreelist();
                        XtraMessageBox.Show("Loading Sussecced", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show(qResult.Msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Tool
        private void ExcelParserHandle()
        {
            try
            {
                string filePath = ProjectCommon.FileDialog(EDiagType.Select, EDiagFileType.excel);
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                ExcelParser excelParser = new ExcelParser();
                ItemDataNode node;
                BoolQResult ret = excelParser.ExcelToItemNode(filePath, out node);

                if (ret.Result)
                {
                    nvRamManage.NvRamParam.Item = node;
                    UpdateTreelist();
                }
                ProjectCommon.ShowMessage(ret);
            }
            catch (Exception ex)
            {
                ProjectCommon.ShowMessage(new BoolQResult(false, ex.Message));
            }
        }
        #endregion

        /// <summary>
        /// Stop Communication
        /// </summary>
        private void StopCommunication()
        {
            //init param
            bDownloadFlag = false;
            bUploadFlag = false;
            CloseSplashScreen();

            BoolQResult bResult = nvRamManage.DisconnectCommunication();
            ProjectCommon.ShowMessage(bResult, false);
            if (bResult.Result)
            {
                currentState = NVState.Disconnected;
                OnStateChanged(new StateChangedEventArgs(currentState));
            }
        }


        #region NVS Handle
        /// <summary>
        /// Load NV bin
        /// </summary>
        private void LoadImage()
        {
            StorageParam ROParam = new StorageParam();
            StorageParam RWParam = new StorageParam();

            NVSStorage nvsSystem = new NVSStorage();
            BoolQResult bResult = nvsSystem.ReadData("NVSImage.bin", ROParam, RWParam);

            ProjectCommon.ShowMessage(bResult, true);

        }

        /// <summary>
        /// Load NV Data Form Phone
        /// </summary>
        private void LoadFromPhone()
        {
            // 启动一个任务
            GlobalEventHandler.TriggerrunMsgEvent("Start to Load From Phone");
            ShowSplashScreen("Please Waiting!", "Loading From Phone in progress");
            Task.Run(() =>
            {
                //
                Action<string> progressCallBack = (msg) =>
                {
                    // 处理进度更新的逻辑
                    ShowSplashTips("Please Waiting!", msg);
                };

                BoolQResult bResult = nvRamManage.LoadImageFromPhone(progressCallBack);
                ProjectCommon.ShowMessage(bResult, false);

                CloseSplashScreen();
                GlobalEventHandler.TriggerrunMsgEvent("End to Load From Phone");
            });
        }

        /// <summary>
        /// Save Image 
        /// </summary>
        private void SaveImage()
        {
            //read File 
            byte[] ROData = FileUtils.ReadFileBytes("partitionsRO.bin");
            ushort crc = Crc16Calculator.Calculate(ROData);
            StorageParam ROParam = new StorageParam()
            {
                SAttribute = SectorAttribute.RO,
                SectorCount = 2,
                SectorSize = 56 * 1024,
                CRC = crc,
                SectorData = ROData
            };

            byte[] RWData = FileUtils.ReadFileBytes("partitionsRW.bin");
            crc = Crc16Calculator.Calculate(RWData);
            StorageParam RWParam = new StorageParam()
            {
                SAttribute = SectorAttribute.RW,
                SectorCount = 3,
                SectorSize = 48 * 1024,
                CRC = crc,
                SectorData = RWData
            };

            NVSStorage nvsSystem = new NVSStorage();
            BoolQResult bResult = nvsSystem.SaveData("NVSImage.bin", ROParam, RWParam);

            ProjectCommon.ShowMessage(bResult);
        }

        /// <summary>
        /// Save NV Data To Phone
        /// </summary>
        private void SaveToPhone()
        {
            if (!IsProjectFileLoaded(nvRamManage.ProjectFilePath))
                return;

            if (!IsCommConnect())
                return;

            //backup nvs 
            string backFilePath = ProjectCommon.FileDialog(EDiagType.Save, EDiagFileType.bin);
            if (string.IsNullOrEmpty(backFilePath))
                return;

            ShowSplashScreen("Please Waiting!", "Save To Phone ");
            Task.Run(() =>
            {
                Action<string> progressCallBack = (msg) =>
                {
                    // 处理进度更新的逻辑
                    ShowSplashTips("Please Waiting!", msg);
                };
                BoolQResult bResult = nvRamManage.SaveImageToPhone(progressCallBack, backFilePath);


                CloseSplashScreen();

                ProjectCommon.ShowMessage(bResult);
            });
        }
        #endregion

        /// <summary>
        /// start up communication dialog
        /// </summary>
        private void SetCommunication()
        {
            //show comunication window
            using (FormSerialParaConfig form = new FormSerialParaConfig())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = GetCenteredLocation(parentForm, form.Size);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    comParam = form.GetComParam();
                }
            }
        }

        /// <summary>
        /// Connect
        /// </summary>
        private void ConnectCommunication()
        {
            //Connect 
            if (comParam == null)
            {
                XtraMessageBox.Show(
                    "Please configure the serial port!",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                //Get Port Info
                ICommPort commPort = new CommPortCom(comParam);
                BoolQResult bResult = nvRamManage.ConnectCommunication(commPort);
                if (bResult.Result == true)
                {
                    currentState = NVState.Connect;
                    OnStateChanged(new StateChangedEventArgs(currentState));
                }
                ProjectCommon.ShowMessage(bResult, false);
            }
            catch (Exception ex)
            {
                LogNetHelper.Warn(ex.Message);
            }
        }

        /// <summary>
        /// Save Project
        /// </summary>
        private void SaveProject()
        {
            string filePath = nvRamManage.ProjectFilePath;
            if (!string.IsNullOrEmpty(filePath))
            {
                ProjectCommon.ShowMessage(SaveProjectFile(nvRamManage.NvRamParam, filePath));
            }
            else
            {
                ProjectCommon.ShowMessage(new BoolQResult(false, "Project File is NULL"));
            }
        }

        /// <summary>
        /// save xml file
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private BoolQResult SaveProjectFile(NVRamParam dataBase, string filePath)
        {
            try
            {
                XmlHelper.SaveXML(dataBase, filePath);
                return new BoolQResult(true, "Save Successfully!");
            }
            catch (Exception ex)
            {
                return new BoolQResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Save as Project File
        /// </summary>
        private void SaveAsProjectFile()
        {
            try
            {
                string filePath = ProjectCommon.FileDialog(EDiagType.Save , EDiagFileType.xml);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ProjectCommon.ShowMessage(SaveProjectFile(nvRamManage.NvRamParam, filePath));
                }
            }
            catch (Exception ex)
            {
                ProjectCommon.ShowMessage(new BoolQResult(false, ex.Message));
            }

        }


        /// <summary>
        /// For Test
        /// </summary>
        void TestFunction()
        {
            (bool result, ItemDataNode rootNode) = ProjectManage.CreateProjectFile();
            if (result == true && rootNode != null)
            {
                NVRamParam nvRamParam = new NVRamParam();
                nvRamParam.Item = rootNode;
                nvRamParam.Version = "v1.0.0";
                nvRamParam.Project = "NanH";
                XmlHelper.SaveXML(nvRamParam, "NV_Project.xml");
            }
            else
            {
                LogNetHelper.Error("Can`t Save NV_Project.xml");
            }
        }

        /// <summary>
        /// 用于计算子窗口应该居中显示的位置
        /// </summary>
        /// <param name="childSize"></param>
        /// <returns></returns>
        private Point GetCenteredLocation(Form parentForm, Size childSize)
        {
            if (parentForm != null)
            {
                int x = (parentForm.Width - childSize.Width) / 2 + parentForm.Left;
                int y = (parentForm.Height - childSize.Height) / 2 + parentForm.Top;
                return new Point(x, y);
            }
            else
            {
                return new Point();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowSplashScreen(string strCaption, string strDescription)
        {
            if (SplashScreenManager.Default != null)
            {
                if (SplashScreenManager.Default.IsSplashFormVisible)
                    CloseSplashScreen();
            }
            SplashScreenManager.ShowForm(typeof(WaitingForm)); // 显示自定义的启动画面
            // 可以在这里进行一些其他设置，例如更改弹出窗口的显示样式
            SplashScreenManager.Default.SetWaitFormDescription(strDescription);
            SplashScreenManager.Default.SetWaitFormCaption(strCaption);
        }

        /// <summary>
        /// 更新显示信息
        /// </summary>
        /// <param name="strCaption"></param>
        /// <param name="strDescription"></param>
        private void ShowSplashTips(string strCaption, string strDescription)
        {
            if (SplashScreenManager.Default != null)
            {
                if (SplashScreenManager.Default.IsSplashFormVisible)
                {
                    SplashScreenManager.Default.SetWaitFormDescription(strDescription);
                    SplashScreenManager.Default.SetWaitFormCaption(strCaption);
                }
            }
        }

        private void CloseSplashScreen()
        {
            try
            {
                if (SplashScreenManager.Default != null)
                {
                    if (SplashScreenManager.Default.IsSplashFormVisible)
                        SplashScreenManager.CloseForm();
                }
            }
            catch (Exception ex)
            {
                LogNetHelper.Error(ex);
            }
        }
        #endregion

        #region Clicked Function
        /// <summary>
        /// add a node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNode_Click(object sender, EventArgs e)
        {
            AddItemNode("ParentID");
        }
        /// <summary>
        /// Add a child node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddChildNode_Click(object sender, EventArgs e)
        {
            AddItemNode("ID");
        }

        private void AddItemNode(string ID)
        {
            using (FormNodeInfo form = new FormNodeInfo())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = GetCenteredLocation(parentForm, form.Size);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 获取右键选中的节点
                    TreeListNode clickedNode = treeListNVParam.FocusedNode;
                    if (clickedNode != null)
                    {
                        int parentID = Convert.ToInt32(clickedNode.GetValue(ID));

                        ItemNodeInfo newInfo = form.GetItemNode();
                        ItemDataNode newNodeData = new ItemDataNode()
                        {
                            ID = nvRamManage.GetNewItemID(nvRamManage.NvRamParam.Item),
                            ParentID = parentID,
                            ItemName = newInfo.Name,
                            ItemValue = newInfo.Value,
                            ItemID = newInfo.ID,
                            DataType = newInfo.DataType,
                            Content = newInfo.Content
                        };

                        TreeListNode newNode = treeListNVParam.AppendNode(newNodeData, parentID);
                        SetNodeValue(newNode, newNodeData);
                        //增加节点
                        BoolQResult result = nvRamManage.AppendItemDataNode(parentID, newNodeData);
                        ProjectCommon.ShowMessage(result, true);
                    }
                    else
                    {
                        LogNetHelper.Warn("Select Node is null");
                        ProjectCommon.ShowMessage(new BoolQResult(false, "Select node is null"), true);
                    }
                }
            }
        }

        /// <summary>
        /// Delete the selected node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteNode_Click(object sender, EventArgs e)
        {
            // current item node
            TreeListNode clickedNode = treeListNVParam.FocusedNode;
            if (clickedNode != null)
            {
                // 使用 XtraMessageBox 来确认删除操作
                DialogResult result = XtraMessageBox.Show("Are you sure？", "Yes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int ID = Convert.ToInt32(clickedNode.GetValue("ID"));
                    nvRamManage.NvRamParam.Item.RemoveChild(ID);
                    treeListNVParam.DeleteNode(clickedNode);
                }
            }
        }

        /// <summary>
        /// Setting Node Value
        /// </summary>
        /// <param name="node"></param>
        /// <param name="data"></param>
        private void SetNodeValue(TreeListNode node, ItemDataNode dataNode)
        {
            if (node == null || dataNode == null)
                return;
            node.SetValue("ID", dataNode.ID); // 列名为 "ID"
            node.SetValue("ParentID", dataNode.ParentID);
            node.SetValue("ItemName", dataNode.ItemName); // 列名为 "Name"
            node.SetValue("ItemValue", dataNode.ItemValue);
            node.SetValue("DataType", dataNode.DataType);
            node.SetValue("ItemID", dataNode.ItemID);
            node.SetValue("Content", dataNode.Content);
            node.SetValue("ItemState", dataNode.ItemState);
        }
        #endregion

        #region Common Function
        /// <summary>
        /// Checks if a project file is loaded.
        /// </summary>
        /// <param name="filePath">The path to the project file.</param>
        /// <returns>True if the project file is loaded; otherwise, false.</returns>
        private bool IsProjectFileLoaded(string filePath)
        {
            // If the file path is empty or not specified, display an error message and return false.
            if (string.IsNullOrEmpty(filePath))
            {
                ProjectCommon.ShowMessage(new BoolQResult(false, "Please load the project!"));
                return false;
            }

            // If the file path is valid, return true.
            return true;
        }


        /// <summary>
        /// Checks if a board is connected for communication.
        /// </summary>
        /// <returns>
        /// True if a connection to the board is established; otherwise, false.
        /// </returns>
        private bool IsCommConnect()
        {
            try
            {
                if (nvRamManage.CmindProtocol.Port == null)
                {
                    ProjectCommon.ShowMessage(new BoolQResult(false, "Please connect the board!"));
                    return false;
                }
                // Check if the communication port is connected to the board.
                if (!nvRamManage.CmindProtocol.Port.IsConnected)
                {
                    // If not connected or if the connection is unstable, show an error message and return false.
                    ProjectCommon.ShowMessage(new BoolQResult(false, "Please connect the board!"));
                    return false;
                }

                // If the communication port is connected and stable, return true.
                return true;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur and log the error.
                ProjectCommon.ShowMessage(new BoolQResult(false, "An error occurred :" + ex.Message));
                return false;
            }
        }
        #endregion

        #region Log Window

        private void TabControlLog_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            IsTabPageCommMessageSelect = (tabControlLog.SelectedTabPage == xtraTabPage2) ? true : false;
        }

        private void CommMessage_TextChanged(object sender, EventArgs e)
        {
            commMessage.SelectionStart = commMessage.Text.Length;
            commMessage.ScrollToCaret();

            //set max line
            if (commMessage.Lines.Length > MaxLines)
            {
                int indexFirstLineToKeep = commMessage.GetFirstCharIndexFromLine(commMessage.Lines.Length - MaxLines);
                commMessage.Text = commMessage.Text.Substring(indexFirstLineToKeep);
            }
        }
        private void RunMessage_TextChanged(object sender, EventArgs e)
        {
            runMessage.SelectionStart = runMessage.Text.Length;
            runMessage.ScrollToCaret();

            //如果超过了最大限制，我们通过获取第一个要保留行的字符索引，并从RichTextBox的文本中截取出需要保留的部分。
            if (runMessage.Lines.Length > MaxLines)
            {
                int indexFirstLineToKeep = runMessage.GetFirstCharIndexFromLine(runMessage.Lines.Length - MaxLines);
                runMessage.Text = runMessage.Text.Substring(indexFirstLineToKeep);
            }
        }

        private void HandleShowMessageEvent(string msg)
        {
            if (IsShowMsg == false)
                return;

            runMessage.Invoke(new Action(() =>
            {
                runMessage.AppendText(string.Format("<{0}> - {1}\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), msg));
            }));
        }

        private void HandleShowCommMessageEvent(object sender, DateTime time, BytesType type, byte[] bytes, string message)
        {
            if ((!IsMessageCapture))
                return;

            if (!IsTabPageCommMessageSelect)
                return;

            string msg = string.Format(
                "<{0}> - {1}：{2}\r\n{3}\r\n",
                time.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                type.ToString(),
                message,
                CommonHelper.ByteToString(bytes));

            if (IsMessageCapture)
            {
                try
                {
                    wr.WriteLine(msg);
                }
                catch (Exception ex)
                {
                    UtMessageBase.ShowOneMessage($"捕获业务保存时失败，原因：{ex.Message}", PopupMessageType.Info);
                }
            }

            if (IsTabPageCommMessageSelect)
            {
                Invoke(new EventHandler(delegate
                {
                    commMessage.AppendText(msg);
                }));
            }
        }


        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runMessage.Text = string.Empty;
        }

        private void MenuMessage_clear_Click(object sender, EventArgs e)
        {
            commMessage.Text = string.Empty;
        }

        private void MenuMessage_capture_Click(object sender, EventArgs e)
        {
            if (!IsMessageCapture)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "txt File(*.txt)|*txt",
                    RestoreDirectory = true,
                    Title = "Capture Message",
                    DefaultExt = ".txt",
                    FileName = string.Format("AutoTesterCapture_{0}", DateTime.Now.ToString("yyyyMMdd_HHmmss")),
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MesaageCaptrurePath = saveFileDialog.FileName;
                    MenuMessage_capture.Text = "Stop Capture";
                    fs = new FileStream(MesaageCaptrurePath, FileMode.Append);
                    wr = new StreamWriter(fs, System.Text.Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                    IsMessageCapture = true;
                }
            }
            else
            {
                IsMessageCapture = false;
                MesaageCaptrurePath = string.Empty;
                MenuMessage_capture.Text = "Capture";
                if (wr != null)
                {
                    wr.Close();
                    wr.Dispose();
                }

                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
        #endregion
    }
}
