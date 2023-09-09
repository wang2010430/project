/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : NVParam.cs
* date      : 2023/7/3 17:09:45
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/3 17:09:45) - jinlong.wang
***************************************************************************************************/

using System;

namespace NVTool.DAL.Model
{
    public enum ButtonType
    {
        LoadProject,
        SaveProject,
        SaveAsProject,
        Communication,
        Connect,
        Stop,
        Download,
        Upload,
        Test,
        //Image
        LoadImage,
        SaveImage,
        LoadFromPhone,
        SaveToPhone,
        //Tool
        ExcelParser,
    }

    public class ButtonClickedEventArgs : EventArgs
    {
        public ButtonType btnType { get; private set; }

        public ButtonClickedEventArgs(ButtonType buttonType)
        {
            btnType = buttonType;
        }
    }
}
