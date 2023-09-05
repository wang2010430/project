
namespace NVTool.UI
{
    partial class FormProject
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, null, true, true, typeof(System.Windows.Forms.UserControl));
            this.treeListNVParam = new DevExpress.XtraTreeList.TreeList();
            this.ColumnName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColumnValue = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColumnDataType = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColumnContent = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColumnItemID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColumnItemState = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem3 = new DevExpress.XtraBars.BarButtonItem();
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            this.dockMain = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.dockPanel1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.tabControlLog = new DevExpress.XtraTab.XtraTabControl();
            this.MenuMessage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuMessage_clear = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuMessage_capture = new System.Windows.Forms.ToolStripMenuItem();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.runMessage = new DevExpress.XtraEditors.MemoEdit();
            this.MenuRecord = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            this.commMessage = new DevExpress.XtraEditors.MemoEdit();
            this.dockPanel2 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel2_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.pMenuMaster = new DevExpress.XtraBars.PopupMenu(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.treeListNVParam)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockMain)).BeginInit();
            this.dockPanel1.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlLog)).BeginInit();
            this.tabControlLog.SuspendLayout();
            this.MenuMessage.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.runMessage.Properties)).BeginInit();
            this.MenuRecord.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            this.xtraTabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.commMessage.Properties)).BeginInit();
            this.dockPanel2.SuspendLayout();
            this.dockPanel2_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pMenuMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // splashScreenManager1
            // 
            splashScreenManager1.ClosingDelay = 500;
            // 
            // treeListNVParam
            // 
            this.treeListNVParam.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.ColumnName,
            this.ColumnValue,
            this.ColumnDataType,
            this.ColumnContent,
            this.ColumnItemID,
            this.ColumnItemState});
            this.treeListNVParam.CustomizationFormBounds = new System.Drawing.Rectangle(1134, 270, 264, 282);
            this.treeListNVParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListNVParam.Location = new System.Drawing.Point(0, 0);
            this.treeListNVParam.MenuManager = this.barManager1;
            this.treeListNVParam.Name = "treeListNVParam";
            this.treeListNVParam.OptionsDragAndDrop.DragNodesMode = DevExpress.XtraTreeList.DragNodesMode.Multiple;
            this.treeListNVParam.OptionsDragAndDrop.DropNodesMode = DevExpress.XtraTreeList.DropNodesMode.Advanced;
            this.treeListNVParam.OptionsSelection.MultiSelect = true;
            this.treeListNVParam.Size = new System.Drawing.Size(1072, 416);
            this.treeListNVParam.TabIndex = 0;
            this.treeListNVParam.CustomNodeCellEdit += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.TreeList_CustomNodeCellEdit);
            this.treeListNVParam.CustomDrawNodeCell += new DevExpress.XtraTreeList.CustomDrawNodeCellEventHandler(this.TreeList_CustomDrawNodeCell);
            this.treeListNVParam.PopupMenuShowing += new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(this.TreeList_PopupMenuShowing);
            // 
            // ColumnName
            // 
            this.ColumnName.Caption = "Item Name";
            this.ColumnName.FieldName = "ItemName";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.OptionsColumn.AllowSort = false;
            this.ColumnName.Visible = true;
            this.ColumnName.VisibleIndex = 0;
            this.ColumnName.Width = 332;
            // 
            // ColumnValue
            // 
            this.ColumnValue.AppearanceCell.Options.UseTextOptions = true;
            this.ColumnValue.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnValue.AppearanceHeader.Options.UseTextOptions = true;
            this.ColumnValue.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnValue.Caption = "Item Value";
            this.ColumnValue.FieldName = "ItemValue";
            this.ColumnValue.Name = "ColumnValue";
            this.ColumnValue.Visible = true;
            this.ColumnValue.VisibleIndex = 1;
            this.ColumnValue.Width = 99;
            // 
            // ColumnDataType
            // 
            this.ColumnDataType.AppearanceCell.Options.UseTextOptions = true;
            this.ColumnDataType.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnDataType.AppearanceHeader.Options.UseTextOptions = true;
            this.ColumnDataType.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnDataType.Caption = "Data Type";
            this.ColumnDataType.FieldName = "DataType";
            this.ColumnDataType.Name = "ColumnDataType";
            this.ColumnDataType.OptionsColumn.AllowEdit = false;
            this.ColumnDataType.OptionsColumn.AllowMove = false;
            this.ColumnDataType.OptionsColumn.AllowMoveToCustomizationForm = false;
            this.ColumnDataType.OptionsColumn.ReadOnly = true;
            this.ColumnDataType.Visible = true;
            this.ColumnDataType.VisibleIndex = 2;
            this.ColumnDataType.Width = 99;
            // 
            // ColumnContent
            // 
            this.ColumnContent.Caption = "Content";
            this.ColumnContent.FieldName = "Content";
            this.ColumnContent.Name = "ColumnContent";
            this.ColumnContent.Visible = true;
            this.ColumnContent.VisibleIndex = 3;
            this.ColumnContent.Width = 336;
            // 
            // ColumnItemID
            // 
            this.ColumnItemID.AppearanceCell.Options.UseTextOptions = true;
            this.ColumnItemID.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnItemID.AppearanceHeader.Options.UseTextOptions = true;
            this.ColumnItemID.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColumnItemID.Caption = "Item ID";
            this.ColumnItemID.FieldName = "ItemID";
            this.ColumnItemID.Name = "ColumnItemID";
            this.ColumnItemID.OptionsColumn.AllowEdit = false;
            this.ColumnItemID.OptionsColumn.AllowMove = false;
            this.ColumnItemID.OptionsColumn.AllowMoveToCustomizationForm = false;
            this.ColumnItemID.OptionsColumn.ReadOnly = true;
            this.ColumnItemID.Visible = true;
            this.ColumnItemID.VisibleIndex = 4;
            this.ColumnItemID.Width = 112;
            // 
            // ColumnItemState
            // 
            this.ColumnItemState.AppearanceCell.Options.UseTextOptions = true;
            this.ColumnItemState.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.ColumnItemState.Caption = "Item State";
            this.ColumnItemState.FieldName = "ItemState";
            this.ColumnItemState.Name = "ColumnItemState";
            this.ColumnItemState.OptionsColumn.AllowEdit = false;
            this.ColumnItemState.OptionsColumn.AllowMove = false;
            this.ColumnItemState.OptionsColumn.AllowSort = false;
            this.ColumnItemState.Visible = true;
            this.ColumnItemState.VisibleIndex = 5;
            this.ColumnItemState.Width = 92;
            // 
            // barManager1
            // 
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barButtonItem1,
            this.barButtonItem2,
            this.barButtonItem3});
            this.barManager1.MaxItemId = 5;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1078, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 586);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1078, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 586);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1078, 0);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 586);
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "barButtonItem1";
            this.barButtonItem1.Id = 2;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "barButtonItem2";
            this.barButtonItem2.Id = 3;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // barButtonItem3
            // 
            this.barButtonItem3.Caption = "barButtonItem3";
            this.barButtonItem3.Id = 4;
            this.barButtonItem3.Name = "barButtonItem3";
            // 
            // dockMain
            // 
            this.dockMain.Form = this;
            this.dockMain.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.dockPanel1,
            this.dockPanel2});
            this.dockMain.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane",
            "DevExpress.XtraBars.TabFormControl",
            "DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl",
            "DevExpress.XtraBars.ToolbarForm.ToolbarFormControl"});
            // 
            // dockPanel1
            // 
            this.dockPanel1.Controls.Add(this.dockPanel1_Container);
            this.dockPanel1.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel1.ID = new System.Guid("910ecb81-8337-4b76-bdb9-8355104d7ad5");
            this.dockPanel1.Location = new System.Drawing.Point(0, 446);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Options.ShowCloseButton = false;
            this.dockPanel1.OriginalSize = new System.Drawing.Size(200, 140);
            this.dockPanel1.Size = new System.Drawing.Size(1078, 140);
            this.dockPanel1.Text = "Log";
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.tabControlLog);
            this.dockPanel1_Container.Location = new System.Drawing.Point(3, 27);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1072, 110);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // tabControlLog
            // 
            this.tabControlLog.ContextMenuStrip = this.MenuMessage;
            this.tabControlLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlLog.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Bottom;
            this.tabControlLog.Location = new System.Drawing.Point(0, 0);
            this.tabControlLog.Name = "tabControlLog";
            this.tabControlLog.SelectedTabPage = this.xtraTabPage1;
            this.tabControlLog.Size = new System.Drawing.Size(1072, 110);
            this.tabControlLog.TabIndex = 3;
            this.tabControlLog.TabMiddleClickFiringMode = DevExpress.XtraTab.TabMiddleClickFiringMode.MouseUp;
            this.tabControlLog.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
            this.tabControlLog.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.TabControlLog_SelectedPageChanged);
            // 
            // MenuMessage
            // 
            this.MenuMessage.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.MenuMessage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuMessage_clear,
            this.MenuMessage_capture});
            this.MenuMessage.Name = "MenuMessage";
            this.MenuMessage.Size = new System.Drawing.Size(123, 48);
            // 
            // MenuMessage_clear
            // 
            this.MenuMessage_clear.Name = "MenuMessage_clear";
            this.MenuMessage_clear.Size = new System.Drawing.Size(122, 22);
            this.MenuMessage_clear.Text = "Clear";
            this.MenuMessage_clear.Click += new System.EventHandler(this.MenuMessage_clear_Click);
            // 
            // MenuMessage_capture
            // 
            this.MenuMessage_capture.Name = "MenuMessage_capture";
            this.MenuMessage_capture.Size = new System.Drawing.Size(122, 22);
            this.MenuMessage_capture.Text = "Capture";
            this.MenuMessage_capture.Click += new System.EventHandler(this.MenuMessage_capture_Click);
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.runMessage);
            this.xtraTabPage1.Controls.Add(this.gridControl1);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(1070, 84);
            this.xtraTabPage1.Text = "Message";
            // 
            // runMessage
            // 
            this.runMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runMessage.Location = new System.Drawing.Point(0, 0);
            this.runMessage.Name = "runMessage";
            this.runMessage.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.runMessage.Properties.Appearance.Options.UseBackColor = true;
            this.runMessage.Properties.ContextMenuStrip = this.MenuRecord;
            this.runMessage.Properties.ReadOnly = true;
            this.runMessage.Size = new System.Drawing.Size(1070, 84);
            this.runMessage.TabIndex = 2;
            this.runMessage.TextChanged += new System.EventHandler(this.RunMessage_TextChanged);
            // 
            // MenuRecord
            // 
            this.MenuRecord.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.MenuRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.MenuRecord.Name = "MenuRecord";
            this.MenuRecord.Size = new System.Drawing.Size(107, 26);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(653, 92);
            this.gridControl1.MainView = this.gridView2;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(18, 18);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.GridControl = this.gridControl1;
            this.gridView2.Name = "gridView2";
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Controls.Add(this.commMessage);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(1070, 84);
            this.xtraTabPage2.Text = "comMessage";
            // 
            // commMessage
            // 
            this.commMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commMessage.Location = new System.Drawing.Point(0, 0);
            this.commMessage.Name = "commMessage";
            this.commMessage.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.commMessage.Properties.Appearance.Options.UseBackColor = true;
            this.commMessage.Properties.ContextMenuStrip = this.MenuMessage;
            this.commMessage.Properties.ReadOnly = true;
            this.commMessage.Size = new System.Drawing.Size(1070, 84);
            this.commMessage.TabIndex = 0;
            this.commMessage.TextChanged += new System.EventHandler(this.CommMessage_TextChanged);
            // 
            // dockPanel2
            // 
            this.dockPanel2.Controls.Add(this.dockPanel2_Container);
            this.dockPanel2.Dock = DevExpress.XtraBars.Docking.DockingStyle.Fill;
            this.dockPanel2.ID = new System.Guid("5e0dd60a-e2f5-45bb-a419-33551f2adc76");
            this.dockPanel2.Location = new System.Drawing.Point(0, 0);
            this.dockPanel2.Name = "dockPanel2";
            this.dockPanel2.Options.ShowAutoHideButton = false;
            this.dockPanel2.Options.ShowCloseButton = false;
            this.dockPanel2.OriginalSize = new System.Drawing.Size(1078, 200);
            this.dockPanel2.Size = new System.Drawing.Size(1078, 446);
            this.dockPanel2.Text = "Project";
            // 
            // dockPanel2_Container
            // 
            this.dockPanel2_Container.Controls.Add(this.treeListNVParam);
            this.dockPanel2_Container.Location = new System.Drawing.Point(3, 26);
            this.dockPanel2_Container.Name = "dockPanel2_Container";
            this.dockPanel2_Container.Size = new System.Drawing.Size(1072, 416);
            this.dockPanel2_Container.TabIndex = 0;
            // 
            // pMenuMaster
            // 
            this.pMenuMaster.Manager = this.barManager1;
            this.pMenuMaster.Name = "pMenuMaster";
            // 
            // FormProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dockPanel2);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FormProject";
            this.Size = new System.Drawing.Size(1078, 586);
            ((System.ComponentModel.ISupportInitialize)(this.treeListNVParam)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockMain)).EndInit();
            this.dockPanel1.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlLog)).EndInit();
            this.tabControlLog.ResumeLayout(false);
            this.MenuMessage.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.runMessage.Properties)).EndInit();
            this.MenuRecord.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            this.xtraTabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.commMessage.Properties)).EndInit();
            this.dockPanel2.ResumeLayout(false);
            this.dockPanel2_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pMenuMaster)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraTreeList.TreeList treeListNVParam;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnValue;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnDataType;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnContent;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnItemID;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraBars.Docking.DockManager dockMain;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel2;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel2_Container;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel1;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraTab.XtraTabControl tabControlLog;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraEditors.MemoEdit runMessage;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private DevExpress.XtraEditors.MemoEdit commMessage;
        private System.Windows.Forms.ContextMenuStrip MenuRecord;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip MenuMessage;
        private System.Windows.Forms.ToolStripMenuItem MenuMessage_clear;
        private System.Windows.Forms.ToolStripMenuItem MenuMessage_capture;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColumnItemState;
        private DevExpress.XtraBars.PopupMenu pMenuMaster;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem3;
    }
}
