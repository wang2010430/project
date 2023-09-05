/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : MessageHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 消息帮助类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/
using System.Diagnostics;

namespace Common
{
    public delegate void UtMessageShow(string msg, PopupMessageType msgType);
    public delegate void UtMessageShowEx(string title, string msg, PopupMessageType msgType, int showTime);

    public class UtMessageBase
    {
        #region 属性定义

        static private event UtMessageShow showMessage = null;

        /// <summary>
        /// ShowMessage属性
        /// </summary>
        static public event UtMessageShow ShowMessage
        {
            add
            {
                if (value != null)
                {
                    bool alreadExist = false;

                    if (showMessage != null)
                    {
                        foreach (UtMessageShow d in showMessage.GetInvocationList())
                        {
                            // 防治重复产生事件
                            if (d.Method.Name == value.Method.Name)
                            {
                                alreadExist = true;
                            }
                        }
                    }

                    if (!alreadExist)
                    {
                        showMessage += value;
                    }
                }
            }

            remove
            {
                if (value != null)
                {
                    showMessage -= value;
                }
            }
        }

        static private event UtMessageShowEx showMessageEx = null;

        /// <summary>
        /// ShowMessageEx属性
        /// </summary>
        static public event UtMessageShowEx ShowMessageEx
        {
            add
            {
                if (value != null)
                {
                    bool alreadExist = false;

                    if (showMessageEx != null)
                    {
                        foreach (UtMessageShowEx d in showMessageEx.GetInvocationList())
                        {
                            // 防治重复产生事件
                            if (d.Method.Name == value.Method.Name)
                            {
                                alreadExist = true;
                            }
                        }
                    }

                    if (!alreadExist)
                    {
                        showMessageEx += value;
                    }
                }
            }

            remove
            {
                if (value != null)
                {
                    showMessageEx -= value;
                }
            }
        }

        static public string PreviousTitle { get; set; }

        static public string PreviousMessage { get; set; }

        #endregion

        #region 显示一般提示信息

        static public event UtMessageShow ShowMessageNoPopup;

        /// <summary>
        /// 显示一错误或提示信息
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="msg"></param>
        /// <param name="msgType"></param>
        /// <param name="showTime"></param>
        public static void ShowOneMessage(string caption, string msg, PopupMessageType msgType, int showTime)
        {
            if (msgType == PopupMessageType.ImportantInfo || msgType == PopupMessageType.Notice || msgType == PopupMessageType.Error || showTime > 0)
            {
                PreviousMessage = msg;
                PreviousTitle = caption;
            }

            if (ShowMessageNoPopup != null && msgType != PopupMessageType.Info)
            {
                ShowMessageNoPopup(string.IsNullOrEmpty(caption) ? msg : string.Format("{0}|{1}", caption, msg), msgType);
            }

            if (showMessageEx != null)
            {
                showMessageEx(caption, msg, msgType, showTime);
            }
            else
            {
                if (showMessage != null)
                {
                    showMessage(string.IsNullOrEmpty(caption) ? msg : string.Format("{0}|{1}", caption, msg), msgType);
                }
                else
                {
                    if (msgType == PopupMessageType.Error || msgType == PopupMessageType.Exception)
                    {
                        Trace.WriteLine(msg);
                    }
                }
            }
        }

        /// <summary>
        /// 显示一条提示信息或错误信息
        /// </summary>
        /// <param name="msg">提示或错误信息</param>
        /// <param name="msgType">信息类型1-提示 2-错误 3-例外中的错误</param>
        public static void ShowOneMessage(string msg, PopupMessageType msgType = PopupMessageType.Info)
        {
            ShowOneMessage("", msg, msgType, 0);
        }
        #endregion
    }
}
