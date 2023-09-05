/***************************************************************************************************
* copyright : CMIND-SEMI
* version   : 1.00
* file      : PermissionContainer.cs
* date      : 2023/7/7 10:13:13
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version (2023/7/7 10:13:13) - jinlong.wang
***************************************************************************************************/

using System;

namespace NVTool.DAL.Model
{
    public enum NVState
    {
        Disconnected,
        Connect,
        Upload,
        Download,
    }

    /// <summary>
    /// NV State
    /// </summary>
    public class StateChangedEventArgs : EventArgs
    {
        //public NVState OldState { get; }
        public NVState NewState { get; }

        public StateChangedEventArgs(NVState newState)
        {
            //OldState = oldState;
            NewState = newState;
        }
    }
}
