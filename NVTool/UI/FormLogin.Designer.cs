
namespace NVTool.UI
{
    partial class FormLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            this.labelPwdEye = new DevExpress.XtraEditors.PanelControl();
            this.labelEye = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnLogin = new DevExpress.XtraEditors.SimpleButton();
            this.textEditPwd = new DevExpress.XtraEditors.TextEdit();
            this.comboUser = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)(this.labelPwdEye)).BeginInit();
            this.labelPwdEye.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPwd.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboUser.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPwdEye
            // 
            this.labelPwdEye.Appearance.BackColor = System.Drawing.Color.White;
            this.labelPwdEye.Appearance.Options.UseBackColor = true;
            this.labelPwdEye.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.labelPwdEye.Controls.Add(this.labelEye);
            this.labelPwdEye.Controls.Add(this.labelControl2);
            this.labelPwdEye.Controls.Add(this.labelControl1);
            this.labelPwdEye.Controls.Add(this.btnLogin);
            this.labelPwdEye.Controls.Add(this.textEditPwd);
            this.labelPwdEye.Controls.Add(this.comboUser);
            this.labelPwdEye.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPwdEye.Location = new System.Drawing.Point(0, 0);
            this.labelPwdEye.Name = "labelPwdEye";
            this.labelPwdEye.Size = new System.Drawing.Size(227, 103);
            this.labelPwdEye.TabIndex = 0;
            // 
            // labelEye
            // 
            this.labelEye.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("labelEye.ImageOptions.Image")));
            this.labelEye.Location = new System.Drawing.Point(204, 44);
            this.labelEye.Name = "labelEye";
            this.labelEye.Size = new System.Drawing.Size(16, 16);
            this.labelEye.TabIndex = 5;
            this.labelEye.Click += new System.EventHandler(this.labelEye_Click);
            // 
            // labelControl2
            // 
            this.labelControl2.ImageOptions.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelControl2.Location = new System.Drawing.Point(15, 44);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(51, 14);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "Password";
            // 
            // labelControl1
            // 
            this.labelControl1.ImageOptions.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelControl1.Location = new System.Drawing.Point(15, 19);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(54, 14);
            this.labelControl1.TabIndex = 3;
            this.labelControl1.Text = "Username";
            // 
            // btnLogin
            // 
            this.btnLogin.AllowFocus = false;
            this.btnLogin.Location = new System.Drawing.Point(135, 73);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(63, 23);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Login";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // textEditPwd
            // 
            this.textEditPwd.Location = new System.Drawing.Point(86, 42);
            this.textEditPwd.Name = "textEditPwd";
            this.textEditPwd.Properties.Appearance.BorderColor = System.Drawing.Color.Black;
            this.textEditPwd.Properties.Appearance.Options.UseBorderColor = true;
            this.textEditPwd.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.textEditPwd.Size = new System.Drawing.Size(112, 20);
            this.textEditPwd.TabIndex = 1;
            // 
            // comboUser
            // 
            this.comboUser.Location = new System.Drawing.Point(86, 16);
            this.comboUser.Name = "comboUser";
            this.comboUser.Properties.AllowFocused = false;
            this.comboUser.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.comboUser.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboUser.Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.comboUser.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.comboUser.Size = new System.Drawing.Size(112, 20);
            this.comboUser.TabIndex = 0;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 103);
            this.Controls.Add(this.labelPwdEye);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("FormLogin.IconOptions.SvgImage")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLogin";
            this.Text = "NV Tool";
            ((System.ComponentModel.ISupportInitialize)(this.labelPwdEye)).EndInit();
            this.labelPwdEye.ResumeLayout(false);
            this.labelPwdEye.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPwd.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboUser.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl labelPwdEye;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnLogin;
        private DevExpress.XtraEditors.TextEdit textEditPwd;
        private DevExpress.XtraEditors.ComboBoxEdit comboUser;
        private DevExpress.XtraEditors.LabelControl labelEye;
    }
}