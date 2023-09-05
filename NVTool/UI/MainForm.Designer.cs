
namespace NVTool.UI
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.barManagerMenu = new DevExpress.XtraBars.BarManager(this.components);
            this.barMenu = new DevExpress.XtraBars.Bar();
            this.barLoad = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barSave = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barCom = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barConnect = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barStop = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barDownload = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barUpload = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barSaveForBin = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barLoadFromBin = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barInPhone = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barOutPhone = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barBtnTest = new DevExpress.XtraBars.BarLargeButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.barItemFile = new DevExpress.XtraBars.BarSubItem();
            this.barItemSaveAs = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem3 = new DevExpress.XtraBars.BarButtonItem();
            this.statusBar = new DevExpress.XtraBars.Bar();
            this.barCopyright = new DevExpress.XtraBars.BarStaticItem();
            this.barVersion = new DevExpress.XtraBars.BarStaticItem();
            this.barRelease = new DevExpress.XtraBars.BarStaticItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
            this.popupMenu1 = new DevExpress.XtraBars.PopupMenu(this.components);
            this.ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.panelWorkspace = new System.Windows.Forms.Panel();
            this.barLargeButtonItem1 = new DevExpress.XtraBars.BarLargeButtonItem();
            this.barToolbarsListItem1 = new DevExpress.XtraBars.BarToolbarsListItem();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barToolbarsListItem2 = new DevExpress.XtraBars.BarToolbarsListItem();
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this.barItemExcelParser = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.barManagerMenu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).BeginInit();
            this.SuspendLayout();
            // 
            // barManagerMenu
            // 
            this.barManagerMenu.AllowCustomization = false;
            this.barManagerMenu.AllowMoveBarOnToolbar = false;
            this.barManagerMenu.AllowQuickCustomization = false;
            this.barManagerMenu.AllowShowToolbarsPopup = false;
            this.barManagerMenu.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.barMenu,
            this.bar2,
            this.statusBar});
            this.barManagerMenu.DockControls.Add(this.barDockControlTop);
            this.barManagerMenu.DockControls.Add(this.barDockControlBottom);
            this.barManagerMenu.DockControls.Add(this.barDockControlLeft);
            this.barManagerMenu.DockControls.Add(this.barDockControlRight);
            this.barManagerMenu.DockWindowTabFont = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.barManagerMenu.Form = this;
            this.barManagerMenu.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barButtonItem3,
            this.barCopyright,
            this.barVersion,
            this.barRelease,
            this.barSave,
            this.barLoad,
            this.barCom,
            this.barDownload,
            this.barUpload,
            this.barConnect,
            this.barStop,
            this.barBtnTest,
            this.barItemFile,
            this.barSubItem2,
            this.barInPhone,
            this.barOutPhone,
            this.barSaveForBin,
            this.barLoadFromBin,
            this.barItemSaveAs,
            this.barToolbarsListItem2,
            this.barSubItem1,
            this.barItemExcelParser});
            this.barManagerMenu.MainMenu = this.bar2;
            this.barManagerMenu.MaxItemId = 50;
            this.barManagerMenu.StatusBar = this.statusBar;
            // 
            // barMenu
            // 
            this.barMenu.BarName = "Tools";
            this.barMenu.DockCol = 0;
            this.barMenu.DockRow = 1;
            this.barMenu.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.barMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barLoad, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSave),
            new DevExpress.XtraBars.LinkPersistInfo(this.barCom, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.barConnect),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStop),
            new DevExpress.XtraBars.LinkPersistInfo(this.barDownload),
            new DevExpress.XtraBars.LinkPersistInfo(this.barUpload),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSaveForBin, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.Standard),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barLoadFromBin, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.barInPhone),
            new DevExpress.XtraBars.LinkPersistInfo(this.barOutPhone),
            new DevExpress.XtraBars.LinkPersistInfo(this.barBtnTest, true)});
            this.barMenu.OptionsBar.DrawDragBorder = false;
            this.barMenu.OptionsBar.UseWholeRow = true;
            this.barMenu.Text = "Tools";
            // 
            // barLoad
            // 
            this.barLoad.Caption = "Load";
            this.barLoad.Id = 14;
            this.barLoad.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barLoad.ImageOptions.SvgImage")));
            this.barLoad.Name = "barLoad";
            this.barLoad.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barLoad_ItemClick);
            // 
            // barSave
            // 
            this.barSave.Caption = "Save";
            this.barSave.Id = 12;
            this.barSave.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barSave.ImageOptions.Image")));
            this.barSave.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barSave.ImageOptions.LargeImage")));
            this.barSave.Name = "barSave";
            this.barSave.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barSave_ItemClick);
            // 
            // barCom
            // 
            this.barCom.Caption = "Com";
            this.barCom.Id = 15;
            this.barCom.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barCom.ImageOptions.Image")));
            this.barCom.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barCom.ImageOptions.LargeImage")));
            this.barCom.Name = "barCom";
            this.barCom.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barCom_ItemClick);
            // 
            // barConnect
            // 
            this.barConnect.Caption = "Connect";
            this.barConnect.Id = 21;
            this.barConnect.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barConnect.ImageOptions.Image")));
            this.barConnect.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barConnect.ImageOptions.LargeImage")));
            this.barConnect.Name = "barConnect";
            this.barConnect.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barConnect_ItemClick);
            // 
            // barStop
            // 
            this.barStop.Caption = "Stop";
            this.barStop.Id = 22;
            this.barStop.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barStop.ImageOptions.Image")));
            this.barStop.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barStop.ImageOptions.LargeImage")));
            this.barStop.Name = "barStop";
            this.barStop.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barStop_ItemClick);
            // 
            // barDownload
            // 
            this.barDownload.Caption = "Downlaod";
            this.barDownload.Id = 18;
            this.barDownload.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barDownload.ImageOptions.Image")));
            this.barDownload.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barDownload.ImageOptions.LargeImage")));
            this.barDownload.Name = "barDownload";
            this.barDownload.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barDownload_ItemClick);
            // 
            // barUpload
            // 
            this.barUpload.Caption = "Upload";
            this.barUpload.Id = 19;
            this.barUpload.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barUpload.ImageOptions.Image")));
            this.barUpload.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barUpload.ImageOptions.LargeImage")));
            this.barUpload.Name = "barUpload";
            this.barUpload.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barUpload_ItemClick);
            // 
            // barSaveForBin
            // 
            this.barSaveForBin.Caption = "Save For bin";
            this.barSaveForBin.Id = 38;
            this.barSaveForBin.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barSaveForBin.ImageOptions.SvgImage")));
            this.barSaveForBin.Name = "barSaveForBin";
            this.barSaveForBin.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barSaveForBin_ItemClick);
            // 
            // barLoadFromBin
            // 
            this.barLoadFromBin.Caption = "Load From Bin";
            this.barLoadFromBin.Id = 39;
            this.barLoadFromBin.ImageOptions.Image = global::NVTool.Properties.Resources.LoadBin;
            this.barLoadFromBin.Name = "barLoadFromBin";
            this.barLoadFromBin.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barLoadFromBin_ItemClick);
            // 
            // barInPhone
            // 
            this.barInPhone.Caption = "InPhone";
            this.barInPhone.Id = 34;
            this.barInPhone.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barInPhone.ImageOptions.SvgImage")));
            this.barInPhone.Name = "barInPhone";
            this.barInPhone.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barInPhone_ItemClick);
            // 
            // barOutPhone
            // 
            this.barOutPhone.Caption = "OutPhone";
            this.barOutPhone.Id = 35;
            this.barOutPhone.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barOutPhone.ImageOptions.SvgImage")));
            this.barOutPhone.Name = "barOutPhone";
            this.barOutPhone.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barOutPhone_ItemClick);
            // 
            // barBtnTest
            // 
            this.barBtnTest.Caption = "Test";
            this.barBtnTest.Id = 23;
            this.barBtnTest.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barBtnTest.ImageOptions.Image")));
            this.barBtnTest.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barBtnTest.ImageOptions.LargeImage")));
            this.barBtnTest.Name = "barBtnTest";
            this.barBtnTest.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barTest_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.Caption, this.barItemFile, "File"),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem3)});
            this.bar2.OptionsBar.DrawDragBorder = false;
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // barItemFile
            // 
            this.barItemFile.Caption = "File";
            this.barItemFile.Id = 24;
            this.barItemFile.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barItemSaveAs)});
            this.barItemFile.Name = "barItemFile";
            // 
            // barItemSaveAs
            // 
            this.barItemSaveAs.Caption = "Save As";
            this.barItemSaveAs.Id = 40;
            this.barItemSaveAs.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barItemSaveAs.ImageOptions.LargeImage")));
            this.barItemSaveAs.Name = "barItemSaveAs";
            this.barItemSaveAs.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barItemSaveAs_ItemClick);
            // 
            // barButtonItem3
            // 
            this.barButtonItem3.Caption = "About";
            this.barButtonItem3.Id = 3;
            this.barButtonItem3.Name = "barButtonItem3";
            // 
            // statusBar
            // 
            this.statusBar.BarName = "Status bar";
            this.statusBar.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.statusBar.DockCol = 0;
            this.statusBar.DockRow = 0;
            this.statusBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.statusBar.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barCopyright),
            new DevExpress.XtraBars.LinkPersistInfo(this.barVersion),
            new DevExpress.XtraBars.LinkPersistInfo(this.barRelease)});
            this.statusBar.OptionsBar.AllowQuickCustomization = false;
            this.statusBar.OptionsBar.DrawDragBorder = false;
            this.statusBar.OptionsBar.UseWholeRow = true;
            this.statusBar.Text = "Status bar";
            // 
            // barCopyright
            // 
            this.barCopyright.Caption = "Copyright";
            this.barCopyright.Id = 6;
            this.barCopyright.Name = "barCopyright";
            // 
            // barVersion
            // 
            this.barVersion.Caption = "Version";
            this.barVersion.Id = 8;
            this.barVersion.Name = "barVersion";
            // 
            // barRelease
            // 
            this.barRelease.Caption = "ReleaseTime";
            this.barRelease.Id = 9;
            this.barRelease.Name = "barRelease";
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(232)))), ((int)(((byte)(244)))));
            this.barDockControlTop.Appearance.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(232)))), ((int)(((byte)(244)))));
            this.barDockControlTop.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.barDockControlTop.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barDockControlTop.Appearance.Options.UseBackColor = true;
            this.barDockControlTop.Appearance.Options.UseBorderColor = true;
            this.barDockControlTop.Appearance.Options.UseFont = true;
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManagerMenu;
            this.barDockControlTop.Margin = new System.Windows.Forms.Padding(6);
            this.barDockControlTop.Size = new System.Drawing.Size(1078, 79);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 665);
            this.barDockControlBottom.Manager = this.barManagerMenu;
            this.barDockControlBottom.Margin = new System.Windows.Forms.Padding(6);
            this.barDockControlBottom.Size = new System.Drawing.Size(1078, 23);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 79);
            this.barDockControlLeft.Manager = this.barManagerMenu;
            this.barDockControlLeft.Margin = new System.Windows.Forms.Padding(6);
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 586);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1078, 79);
            this.barDockControlRight.Manager = this.barManagerMenu;
            this.barDockControlRight.Margin = new System.Windows.Forms.Padding(6);
            this.barDockControlRight.Size = new System.Drawing.Size(0, 586);
            // 
            // barSubItem2
            // 
            this.barSubItem2.Caption = "Load Image";
            this.barSubItem2.Id = 25;
            this.barSubItem2.Name = "barSubItem2";
            // 
            // popupMenu1
            // 
            this.popupMenu1.Manager = this.barManagerMenu;
            this.popupMenu1.Name = "popupMenu1";
            // 
            // ribbonPage2
            // 
            this.ribbonPage2.Name = "ribbonPage2";
            this.ribbonPage2.Text = "ribbonPage2";
            // 
            // panelWorkspace
            // 
            this.panelWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWorkspace.Location = new System.Drawing.Point(0, 79);
            this.panelWorkspace.Name = "panelWorkspace";
            this.panelWorkspace.Size = new System.Drawing.Size(1078, 586);
            this.panelWorkspace.TabIndex = 4;
            // 
            // barLargeButtonItem1
            // 
            this.barLargeButtonItem1.Id = 36;
            this.barLargeButtonItem1.Name = "barLargeButtonItem1";
            // 
            // barToolbarsListItem1
            // 
            this.barToolbarsListItem1.Caption = "11";
            this.barToolbarsListItem1.Id = 43;
            this.barToolbarsListItem1.Name = "barToolbarsListItem1";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "barButtonItem1";
            this.barButtonItem1.Id = 44;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barToolbarsListItem2
            // 
            this.barToolbarsListItem2.Caption = "Tool";
            this.barToolbarsListItem2.Id = 47;
            this.barToolbarsListItem2.Name = "barToolbarsListItem2";
            // 
            // barSubItem1
            // 
            this.barSubItem1.Caption = "Tool";
            this.barSubItem1.Id = 48;
            this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barItemExcelParser)});
            this.barSubItem1.Name = "barSubItem1";
            // 
            // barItemExcelParser
            // 
            this.barItemExcelParser.Caption = "Excel Parse";
            this.barItemExcelParser.Id = 49;
            this.barItemExcelParser.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barItemExcelParser.ImageOptions.Image")));
            this.barItemExcelParser.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barItemExcelParser.ImageOptions.LargeImage")));
            this.barItemExcelParser.Name = "barItemExcelParser";
            this.barItemExcelParser.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barItemExcelParser_ItemClick);
            // 
            // MainForm
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1078, 688);
            this.Controls.Add(this.panelWorkspace);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Font = new System.Drawing.Font("Tahoma", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IconOptions.Image = global::NVTool.Properties.Resources.Logo;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MainForm";
            this.Text = "NVTool";
            ((System.ComponentModel.ISupportInitialize)(this.barManagerMenu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManagerMenu;
        private DevExpress.XtraBars.Bar barMenu;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.Bar statusBar;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem barButtonItem3;
        private DevExpress.XtraBars.BarStaticItem barCopyright;
        private DevExpress.XtraBars.BarStaticItem barVersion;
        private DevExpress.XtraBars.BarStaticItem barRelease;
        private DevExpress.XtraBars.BarLargeButtonItem barSave;
        private DevExpress.XtraBars.BarLargeButtonItem barLoad;
        private DevExpress.XtraBars.BarLargeButtonItem barCom;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2;
        private System.Windows.Forms.Panel panelWorkspace;
        private DevExpress.XtraBars.BarLargeButtonItem barDownload;
        private DevExpress.XtraBars.BarLargeButtonItem barUpload;
        private DevExpress.XtraBars.BarLargeButtonItem barConnect;
        private DevExpress.XtraBars.BarLargeButtonItem barStop;
        private DevExpress.XtraBars.BarLargeButtonItem barBtnTest;
        private DevExpress.XtraBars.PopupMenu popupMenu1;
        private DevExpress.XtraBars.BarSubItem barItemFile;
        private DevExpress.XtraBars.BarSubItem barSubItem2;
        private DevExpress.XtraBars.BarLargeButtonItem barInPhone;
        private DevExpress.XtraBars.BarLargeButtonItem barOutPhone;
        private DevExpress.XtraBars.BarLargeButtonItem barSaveForBin;
        private DevExpress.XtraBars.BarLargeButtonItem barLoadFromBin;
        private DevExpress.XtraBars.BarButtonItem barItemSaveAs;
        private DevExpress.XtraBars.BarLargeButtonItem barLargeButtonItem1;
        private DevExpress.XtraBars.BarToolbarsListItem barToolbarsListItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarButtonItem barItemExcelParser;
        private DevExpress.XtraBars.BarToolbarsListItem barToolbarsListItem2;
    }
}

