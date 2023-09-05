using DevExpress.XtraEditors;
using NVTool.BLL;
using NVTool.DAL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NVTool.UI
{
    
    public partial class FormLogin : XtraForm
    {
        private int flashCount = 0;
        private Timer flashTimer = new Timer();
        private Color originalBorderColor;
        #region Constructor
        public FormLogin()
        {
            InitializeComponent();

            InitParam();
        }

        #endregion

        #region Normal Function
        private void InitParam()
        {
            labelEye.Tag = PasswordState.ciphertext;
            textEditPwd.Properties.UseSystemPasswordChar = true;

            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                comboUser.Properties.Items.Add(role);
            }

            PermissionManager permissionManager = PermissionManager.Instance;
            comboUser.Text = permissionManager.UserRole.ToString();

            // 初始化Timer
            flashTimer.Interval = 100; // 闪动间隔为0.1秒
            flashTimer.Tick += FlashTimer_Tick;

            // 保存原始边框颜色
            originalBorderColor = textEditPwd.Properties.Appearance.BorderColor;
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            // 闪动文本框的边框颜色
            if (flashCount % 2 == 0)
            {
                textEditPwd.Properties.Appearance.BorderColor = Color.Red;
            }
            else
            {
                textEditPwd.Properties.Appearance.BorderColor = originalBorderColor;
            }

            flashCount++;

            // 停止闪动
            if (flashCount >= 20) // 闪动3次，每次切换两次颜色
            {
                flashTimer.Stop();
                textEditPwd.Properties.Appearance.BorderColor = originalBorderColor; // 恢复原始边框颜色
            }
        }
        #endregion

        #region Handle Event

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //判断密码
            if (textEditPwd.Text == "666666")
            {
                PermissionManager permissionManager = PermissionManager.Instance;
                if (permissionManager != null)
                    permissionManager.UserRole = (UserRole)Enum.Parse(typeof(UserRole), comboUser.Text);
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // 密码错误时执行闪动效果
                flashCount = 0;
                flashTimer.Start();
            }
        }
        private void labelEye_Click(object sender, EventArgs e)
        {
       
            if ((PasswordState)labelEye.Tag == PasswordState.ciphertext)
            {
                //切换成明文
                textEditPwd.Properties.UseSystemPasswordChar = false;
                labelEye.Tag = PasswordState.plaintext;
                labelEye.ImageOptions.Image = Properties.Resources.openEye;
            }
            else if((PasswordState)labelEye.Tag == PasswordState.plaintext)
            {
                //切换成暗文
                textEditPwd.Properties.UseSystemPasswordChar = true;
                labelEye.Tag = PasswordState.ciphertext;
                labelEye.ImageOptions.Image = Properties.Resources.closeEye;
            }
        }
        #endregion
    }

    
}