/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : LogWriteHelper.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 写日志帮助类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Common
{
    public enum LogMsgType
    {
        /// <summary>
        /// 一般信息
        /// </summary>
        Info,

        /// <summary>
        /// 提示信息
        /// </summary>
        Notice,

        /// <summary>
        /// 重要信息
        /// </summary>
        ImportantInfo,

        /// <summary>
        /// 系统警告
        /// </summary>
        Warning,

        /// <summary>
        /// 系统异常
        /// </summary>
        Exception,

        /// <summary>
        /// 系统错误
        /// </summary>
        Error,
    }

    /// <summary>
    /// 记录委托
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="msgType"></param>
    /// <param name="time"></param>
    public delegate void LogHandle(string msg, LogMsgType msgType, DateTime time);

    /// <summary>
    /// 写日志文件类
    /// </summary>
    public class LogWriteHelper : IDisposable
    {
        private IEventWait _autoEvent;

        /// <summary>
        /// 日志记录缓冲条目列表
        /// </summary>
        private readonly List<string> _messageList;

        /// <summary>
        /// 日志线程是否己关闭
        /// </summary>
        private bool _isClosed = true;

        /// <summary>
        /// 单个日志最大索引号
        /// </summary>
        private const int LogFileCount = 20;

        /// <summary>
        /// 日志叠加索引号
        /// </summary>
        private int _fileNameIndex = 1;

        /// <summary>
        /// 缺省日志名称
        /// </summary>
        private string _logFileNameSuffix = "LogFile";

        /// <summary>
        /// 缺省日志路径
        /// </summary>
        private string _logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Log");

        /// <summary>
        /// 单个日志大小限制
        /// </summary>
        private int _maxFileSize = 1024 * 1024 * 10;

        /// <summary>
        /// 日志单个文件最大大小（超过大小新建文件）
        /// </summary>
        public int MaxFileSize { get { return _maxFileSize; } set { _maxFileSize = value; } }

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string LogPath { get { return _logPath; } set { _logPath = value; } }

        /// <summary>
        /// 日志文件名（不包括后缀）
        /// </summary>
        public string LogName { get { return _logFileNameSuffix; } set { _logFileNameSuffix = value; } }

        /// <summary>
        /// 文件名称叠加索引号
        /// </summary>
        private int FileNameIndx
        {
            get
            {
                if (_fileNameIndex <= 0 ||
                    _fileNameIndex > LogFileCount)
                {
                    _fileNameIndex = 1;
                }

                return _fileNameIndex;
            }
        }

        /// <summary>
        /// 当前文件名称
        /// </summary>
        private string CurrentFileName
        {
            get
            {
                return FullLogFileName(FileNameIndx);
            }
        }

        #region 事件
        private event LogHandle eventLog = null;

        /// <summary>
        /// 日志记录事件
        /// </summary>
        public event LogHandle EventLog
        {
            add
            {
                if (value != null)
                {
                    bool alreadExist = false;

                    if (eventLog != null)
                    {
                        foreach (LogHandle d in eventLog.GetInvocationList())
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
                        eventLog += value;
                    }
                }
            }

            remove
            {
                if (value != null)
                {
                    eventLog -= value;
                }
            }
        }

        /// <summary>
        /// 通知日志记录
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgType"></param>
        /// <param name="time"></param>
        private void CallLog(string msg, LogMsgType msgType, DateTime time)
        {
            if (eventLog != null)
            {
                eventLog(msg, msgType, time);
            }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logName">日志文件名称</param>
        public LogWriteHelper(string logName = "")
        {
            if (!string.IsNullOrEmpty(logName))
            {
                LogName = logName;
            }

            _autoEvent = AutoResetEventFactory.CreateAutoResetEvent(false);
            _messageList = new List<string>();
            Start();
        }

        /// <summary>
        /// 初始化日志类
        /// </summary>
        /// <param name="logPath">日志储存路径</param>
        /// <param name="fileNameSuffix">日志文件名</param>
        public void InitLog(string logPath, string logName = "")
        {
            _logPath = logPath;

            if (!string.IsNullOrEmpty(logName))
            {
                _logFileNameSuffix = logName;
            }

            // 首先选用最近一次使用的文件
            SelectLastestFileName();
        }

        /// <summary>
        /// 记录一日志信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="msgType"></param>
        public void Log(string message, LogMsgType msgType = LogMsgType.Info)
        {
            DateTime time = DateTime.Now;
            LogMessage(FormatMessage(message, msgType, time));
            CallLog(message, msgType, time);
        }

        /// <summary>
        /// 启动日志书写线程
        /// </summary>
        public void Start()
        {
            if (!_isClosed)
            {
                return;
            }

            _isClosed = false;

            var st = new ThreadStart(StartWrite);
            var writeThread = new Thread(st)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            writeThread.Start();
        }

        /// <summary>
        /// 停止日志书写线程
        /// </summary>
        public void Stop()
        {
            if (_isClosed)
            {
                return;
            }
            _isClosed = true;
            _autoEvent.Set();
        }

        /// <summary>
        /// 格式化日志调试信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="msgType"></param>
        /// <param name="addTimeTag"></param>
        /// <returns></returns>
        private string FormatMessage(string message, LogMsgType msgType, DateTime time)
        {
            return string.Format("{0} [{1}] - {2}", time.ToString("yyyy/MM/dd HH:mm:ss.fff"), msgType.ToString(), message);
        }

        /// <summary>
        /// 消息类型名称
        /// </summary>
        /// <param name="msgType"></param>
        /// <returns></returns>
        public static string MessageTypeName(LogMsgType msgType)
        {

            switch (msgType)
            {
                case LogMsgType.Info:

                    return "一般信息";

                case LogMsgType.Notice:

                    return "提示信息";

                case LogMsgType.ImportantInfo:

                    return "重要信息";

                case LogMsgType.Exception:

                    return "系统异常";

                case LogMsgType.Error:

                    return "系统错误";

                case LogMsgType.Warning:

                    return "系统警告";

                default:

                    return "其它信息";
            }
        }

        /// <summary>
        /// 当前使用的日志文件全名
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private string FullLogFileName(int idx)
        {
            var logPath = _logPath;

            if (Directory.Exists(logPath))
            {
                return Path.Combine(logPath, PureLogFileName(idx));
            }

            Directory.CreateDirectory(logPath);

            return Path.Combine(logPath, PureLogFileName(idx));
        }

        /// <summary>
        /// 纯日志文件名（不含路径）
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private string PureLogFileName(int idx)
        {
            return string.Format("{0}{1}.txt", _logFileNameSuffix, idx);
        }

        /// <summary>
        /// 选择一日志文件名，选择标准为文件不存在或未用时间最长
        /// </summary>
        /// <returns></returns>
        private void SelectOldestFileName()
        {
            var lastWriteTime = DateTime.Now;

            for (var i = 1; i <= LogFileCount; i++)
            {
                var fi = new FileInfo(FullLogFileName(i));

                if (!fi.Exists)
                {
                    _fileNameIndex = i;
                    return;
                }

                if (fi.LastWriteTime >= lastWriteTime)
                {
                    continue;
                }

                lastWriteTime = fi.LastWriteTime;
                _fileNameIndex = i;
            }
        }

        /// <summary>
        /// 选择最近一次使用的日志名
        /// </summary>
        private void SelectLastestFileName()
        {
            var lastWriteTime = new DateTime(2000, 1, 1);

            _fileNameIndex = 1;

            for (var i = 1; i <= LogFileCount; i++)
            {
                var fi = new FileInfo(FullLogFileName(i));

                if (!fi.Exists || fi.LastWriteTime <= lastWriteTime)
                {
                    continue;
                }

                lastWriteTime = fi.LastWriteTime;
                _fileNameIndex = i;
            }
        }

        /// <summary>
        /// 日志书写线程中的书写函数
        /// </summary>
        private void StartWrite()
        {
            while (!_isClosed)
            {
                _autoEvent.WaitOne();
                WriteFile();
            }
        }

        /// <summary>
        /// 将信息写盘
        /// </summary>
        private void WriteFile()
        {
            lock (_messageList)
            {
                if (_messageList.Count <= 0)
                {
                    return;
                }

                StreamWriter sw = null;

                try
                {
                    var createNew = false;
                    var fi = new FileInfo(CurrentFileName);

                    if (fi.Exists && fi.Length > MaxFileSize)
                    {
                        SelectOldestFileName();
                        createNew = true;
                    }

                    fi = new FileInfo(CurrentFileName);

                    if (createNew || !fi.Exists)
                    {
                        sw = fi.CreateText();
                    }
                    else
                    {
                        sw = new StreamWriter(CurrentFileName, true);
                    }

                    foreach (var message in _messageList)
                    {
                        sw.WriteLine(message);
                    }

                    sw.Close();
                    sw.Dispose();
                    sw = null;
                    fi = null;
                    _messageList.Clear();
                }
                catch
                {
                    if (null != sw)
                    {
                        sw.Close();
                        sw.Dispose();
                        _messageList.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 记录一日志信息
        /// </summary>
        /// <param name="fullMessage"></param>
        private void LogMessage(string fullMessage)
        {
            if (string.IsNullOrEmpty(fullMessage))
            {
                return;
            }

            lock (_messageList)
            {
                if (_messageList.Count > 500)
                {
                    var s = string.Format("需记录的信息太多，自动删除来不及记录的{0}条信息", _messageList.Count);

                    _messageList.Clear();
                    _messageList.Add(s);
                }

                _messageList.Add(fullMessage);

                if (null == _autoEvent)
                {
                    return;
                }

                _autoEvent.Set();
            }
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (_autoEvent == null)
            {
                return;
            }

            _autoEvent.Close();
            _autoEvent.Dispose();
            _autoEvent = null;
        }
    }
}
