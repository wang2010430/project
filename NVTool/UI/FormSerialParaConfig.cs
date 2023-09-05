/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : FormSerialParaConfig.cs
* date      : 2023/04/12
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Channel;
using Channel.CommPort;
using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace NVTool.UI
{
    public partial class FormSerialParaConfig : DevExpress.XtraEditors.XtraForm
    {
        public static string XML_PATH = "./CommParam.xml";
        /// <summary>
        /// set param
        /// </summary>
        public FormSerialParaConfig()
        {
            InitializeComponent();
            if (File.Exists(XML_PATH))
            {
                cbbCom.DataSource = new List<string>(SerialPort.GetPortNames());
                LoadComParam();
            }
            else
            {
                cbbCom.DataSource = new List<string>(SerialPort.GetPortNames());
                cbbBandRate.Text = "115200";
                cbbParity.SelectedIndex = 0;
                cbbDataBit.SelectedIndex = 3;
                cbbStopBit.SelectedIndex = 0;
            }
        }

        public ComParam GetComParam()
        {
            ComParam para = new ComParam();

            para.PortName = cbbCom.Text;

            para.Parity = (Parity)cbbParity.SelectedIndex;
            para.BaudRate = Convert.ToInt32(cbbBandRate.Text);
            para.DataBits = Convert.ToInt32(cbbDataBit.Text);
            if (cbbStopBit.Text == "1")
            {
                para.StopBits = StopBits.One;
            }
            else if (cbbStopBit.Text == "1.5")
            {
                para.StopBits = StopBits.OnePointFive;
            }
            else if (cbbStopBit.Text == "2")
            {
                para.StopBits = StopBits.Two;
            }

            XmlHelper.SaveXML(para, XML_PATH);

            return para;
        }

        public void LoadComParam()
        {
            try
            {
                if (File.Exists(XML_PATH))
                {
                    ComParam param = XmlHelper.OpenXML<ComParam>(XML_PATH);
                    cbbCom.Text = param.PortName;
                    cbbBandRate.Text = param.BaudRate.ToString();
                    cbbParity.SelectedIndex = (int)param.Parity;
                    cbbDataBit.Text = param.DataBits.ToString();
                    if (param.StopBits == StopBits.One)
                    {
                        cbbStopBit.Text = "1";
                    }
                    else if (param.StopBits == StopBits.OnePointFive)
                    {
                        cbbStopBit.Text = "1.5";

                    }
                    else if (param.StopBits == StopBits.Two)
                    {
                        cbbStopBit.Text = "2";
                    }
                }
            }
            finally
            {

            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cbbCom.Text == string.Empty)
            {
                labelWarning.Visible = true;
                labelWarning.ForeColor = System.Drawing.Color.Red;
                labelWarning.Text = "Warning: No available serial port.";
                return;
            }
            else
            {
                labelWarning.Visible = false;
            }

            ComParam para = GetComParam();
            CommPortCom commPort = new CommPortCom(para);
            bool bConnect = commPort.Open();
            if (bConnect)
            {
                DialogResult = DialogResult.OK;
                commPort.Close();
            }
            else
            {
                labelWarning.Visible = true;
                labelWarning.ForeColor = System.Drawing.Color.Red;
                labelWarning.Text = "Warning:The serial port is already in use.";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            labelWarning.Visible = false;
            DialogResult = DialogResult.Cancel;
        }
    }
}
