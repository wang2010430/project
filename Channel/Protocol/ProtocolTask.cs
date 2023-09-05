 /***************************************************************************************************
* copyright : 芯微半导体（珠海）有限公司
* version   : 1.00
* file      : ProtocolTask.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : ProtocolTask
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Threading;

namespace Channel
{
    /// <summary>
    /// 应用发给规约的任务类
    /// </summary>
    [Serializable]
    public class ProtocolTask
    {
        bool dead = false;

        /// <summary>
        /// 标示任务是否处理完毕
        /// </summary>
        public bool Dead
        {
            get
            {
                return dead;
            }

            set
            {
                dead = value;
            }
        }

        string name;
        object param;
        TaskResult result;
        TaskState taskState;

        public Action<BusinessResult> RetCallBack = null;

        /// <summary>
        /// -1表示无限时。
        /// </summary>
        int taskTimeouts = -1;

        [NonSerialized()]
        AsyncTaskResult asyncTaskResult;

        /// <summary>获取或设置任务关联的协议。
        /// </summary>
        [NonSerialized()]
        ProtocolBase relatedProtocol = null;

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 任务开始执行时间
        /// </summary>
        private DateTime executeStartTime;

        /// <summary>
        /// 任务数据帧从端口发送出去的时间
        /// </summary>
        public int SendOutTime
        {
            get;
            set;
        }

        public void CalculateSendOutTime()
        {
            SendOutTime = (int)(DateTime.Now - CreateTime).TotalMilliseconds;
        }

        /// <summary>
        /// 实例化协议任务。
        /// </summary>
        public ProtocolTask()
        {
            CreateTime = DateTime.Now;
            SendOutTime = -1; //表示未发送数据
        }

        /// <summary>
        /// 实例化协议任务。
        /// </summary>
        /// <param name="name">任务名。</param>
        /// <param name="param">任务参数。</param>
        public ProtocolTask(string name, object param)
        {
            CreateTime = DateTime.Now;
            Name = name;
            Param = param;
            taskState = TaskState.NewTask;
        }

        /// <summary>
        /// 获取或设置任务名。
        /// 注：它是对任务的描述。对于它的意义，主要看应用开发和协议开发之间的协商和业务的需要。
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// 获取或设置任务参数。
        /// 注：它是object类型，用来传递数据给协议。对于它的意义，主要看应用开发和协议开发之间的协商和业务的需要。
        /// </summary>
        public object Param
        {
            get { return param; }
            set { param = value; }
        }

        /// <summary>
        /// 获取或设置任务的执行结果。
        /// </summary>
        public TaskResult Result
        {
            get { return result; }
            set { result = value; }
        }

        /// <summary>
        /// 获取任务状态。
        /// </summary>
        public TaskState TaskState
        {
            get { return taskState; }
        }

        /// <summary>
        /// 任务完成。
        /// </summary>
        public void Complete()
        {
            if (relatedProtocol == null)	// 若任务未经过协议的ExecuteTask来执行，则relatedProtocol为空
            {
                Complete(TaskState.Completed);
            }
            else
            {
                // 如果任务已经被删除，说明任务的执行时间已经超时并已经通知了应用程序，
                // 因此此时再次通知任务完成实际上是没意义的。
                if (relatedProtocol.RemoveTask(this))
                {
                    Complete(TaskState.Completed);
                }
            }
        }

        protected FrameBase taskFrame;

        public FrameBase TaskFrame
        {
            get
            {
                return taskFrame;
            }
        }

        virtual public void GenerateTaskFrame()
        {
            taskFrame = null;
        }

        /// <summary>
        /// 通知任务已经处于处理状态。
        /// </summary>
        public void SetAsBeProccessing()
        {
            if (taskState == TaskState.NewTask)
            {
                taskState = TaskState.Processing;
            }
        }

        public void SetAsWaitingResult()
        {
            if (taskState == TaskState.Processing)
            {
                taskState = TaskState.WaitingResult;
            }
        }

        #region internal访问域函数

        internal void SetRelatedProtocol(ProtocolBase protocol)
        {
            relatedProtocol = protocol;
        }

        /// <summary>
        /// 设置任务超时。
        /// </summary>
        internal void SetAsOvertime()
        {
            taskState = TaskState.TaskOvertime;
        }

        private int executeCompleteTime;

        public int TaskCompleteTime
        {
            get
            {
                return executeCompleteTime;
            }
        }

        /// <summary>
        /// 函数将阻塞当前线程，直至任务完成。
        /// 另一个线程将调用SetAsCompleted函数通知任务完成以解除线程阻塞。
        /// </summary>
        /// <param name="timeout">等待的毫秒数（-1表示无限期等待）。</param>
        internal void WaitToComplete(int timeout)
        {
            if (asyncTaskResult == null)
            {
                throw new Exception("在执行同步等待前需先初始化同步对象。");
            }

            asyncTaskResult.WaitToComplete(timeout);    // 等待同步完成。
        }

        /// <summary>
        /// 设置任务完成。
        /// 若当前线程因为调用了WaitToComplete函数而阻塞，那么此函数将使当前线程恢复运行。
        /// </summary>
        /// <param name="tkState">任务状态。</param>
        internal void Complete(TaskState tkState)
        {
            taskState = tkState;

            if (asyncTaskResult != null)
            {
                asyncTaskResult.SetAsCompleted(true);
            }

            if (executeStartTime == null)
            {
                executeCompleteTime = (int)(DateTime.Now - CreateTime).TotalMilliseconds;
            }
            else
            {
                executeCompleteTime = (int)(DateTime.Now - executeStartTime).TotalMilliseconds;
            }
        }

        /// <summary>
        /// 设置异步调用的回调函数和传递的对象。
        /// </summary>
        /// <param name="asyncCallback">异步回调对象</param>
        /// <param name="state">传递的对象</param>
        /// <param name="timeout">执行任务的时限毫秒（值为-1表示无限时），若超时将调用asyncCallback的回调函数。</param>
        internal void TaskExecuteSetup(AsyncCallback asyncCallback, object state, int timeout, bool sync)
        {
            // 添加后面这个判断是怕应用程序开发员将一个已经执行了的异步任务再此执行异步任务，并更改了回调函数。
            if (asyncTaskResult == null || !asyncTaskResult.AsyncCallbackObjEquals(asyncCallback))
            {
                asyncTaskResult = new AsyncTaskResult(asyncCallback, state);
            }

            dead = false;
            
            // 重复执行该任务时，以下几项重新赋值是必须的
            taskState = TaskState.NewTask;
            taskTimeouts = timeout;
            CreateTime = DateTime.Now;
            SendOutTime = -1;
            OtherTaskWaitTime = 0;
            executeStartTime = DateTime.Now;
        }

        /// <summary>
        /// 因其它任务造成的需多等待的时间
        /// </summary>
        public int OtherTaskWaitTime
        {
            get;
            set;
        }

        /// <summary>
        /// 返回任务执行是否超时。
        /// </summary>
        internal bool TaskOvertime(int extraTime)
        {
            if (SendOutTime < 0 || executeStartTime == null || taskTimeouts < 0)
            {
                return false;
            }

            return (DateTime.Now - executeStartTime).TotalMilliseconds >= taskTimeouts + SendOutTime + OtherTaskWaitTime + extraTime;
        }
        #endregion
    }

    /// <summary>
    /// 任务执行的返回结果（它只是一个数据结构）。
    /// </summary>
    [Serializable]
    public class TaskResult
    {
        /// <summary>
        /// 任务操作成功标识。
        /// </summary>
        bool success;

        /// <summary>
        /// 任务结果描述信息。
        /// </summary>
        string description;

        /// <summary>
        /// 任务操作完成后数据的返回。
        /// </summary>
        object param;

        /// <summary>
        /// 任务操作成功标识。
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        /// <summary>
        /// 获取或设置任务结果描述信息。
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// 任务操作完成后数据的返回。
        /// </summary>
        public object Param
        {
            get { return param; }
            set { param = value; }
        }

        public TaskResult()
        {
        }

        public TaskResult(bool success, string description, object param)
        {
            Success = success;
            Description = description;
            Param = param;
        }
    }

    /// <summary>
    /// 任务状态。
    /// </summary>
    [Serializable]
    public enum TaskState
    {
        /// <summary>
        /// 一个刚收到的新任务（即还未做任何处理状态）。
        /// </summary>
        NewTask,

        /// <summary>
        /// 任务正在处理中。
        /// </summary>
        Processing,

        /// <summary>
        /// 任务已完成。
        /// </summary>
        Completed,

        /// <summary>
        /// 在指定的时间内未完成任务的执行。
        /// </summary>
        TaskOvertime,

        /// <summary>
        /// 正在等待结果
        /// </summary>
        WaitingResult
    }

    /// <summary>异步任务结果。
    /// </summary>
    internal class AsyncTaskResult : IAsyncResult
    {
        /// <summary>
        /// 它表示正在异步操作中。
        /// </summary>
        private const int StatePending = 0;

        /// <summary>
        /// 它表示同步操作完成。
        /// </summary>
        private const int StateCompletedSynchronously = 1;

        /// <summary>
        /// 它表示异步操作完成。
        /// </summary>
        private const int StateCompletedAsynchronously = 2;

        /// <summary>
        /// 异步操作完成标识。
        /// </summary>
        private int completedState = StatePending;

        /// <summary>
        /// 异步操作完成时调用的回调方法。
        /// </summary>
        private readonly AsyncCallback asyncCallback;

        /// <summary>
        /// 异步等待事件。
        /// </summary>
        private AutoResetEvent asyncWaitHandle;

        public AsyncTaskResult(AsyncCallback asyncCallback, object state)
        {
            this.asyncCallback = asyncCallback;
            AsyncState = state;
        }

        /// <summary>
        /// 返回回调函数对象是否相等。
        /// </summary>
        /// <param name="asyncCallback">要判断的回调函数对象。</param>
        internal bool AsyncCallbackObjEquals(AsyncCallback asyncCallback)
        {
            return this.asyncCallback == asyncCallback;
        }

        public void SetAsCompleted(bool completedSynchronously)
        {
            // The m_CompletedState field MUST be set prior calling the callback
            Interlocked.Exchange(ref completedState, completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously);

            // If the event exists, set it
            if (asyncWaitHandle != null)
            {
                asyncWaitHandle.Set();
            }

            // If a callback method was set, call it
            if (asyncCallback != null)
            {
                asyncCallback(this);
            }
        }

        /// <summary>
        /// 等待同步完成，它将阻塞当前线程，直至等待超过timeout时限，或等待事件被设置。
        /// </summary>
        /// <param name="timeout">等待的毫秒数。-1表示无限期等待。</param>
        public void WaitToComplete(int timeout)
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!IsCompleted)
            {
                AsyncWaitHandle.WaitOne(-1, false);
                AsyncWaitHandle.Close();
                asyncWaitHandle = null;
            }
        }

        #region Implementation of IAsyncResult

        public Object AsyncState { get { return _asyncState; } set { _asyncState = value; } }
        Object _asyncState;

        public Boolean CompletedSynchronously
        {
            get { return completedState == StateCompletedSynchronously; }
        }

        /// <summary>
        /// 获取用于等待异步操作完成的 WaitHandle。
        /// 返回值允许客户端等待异步操作完成，而不是轮询 IsCompleted 直到操作结束。返回值可用于执行 WaitOne、WaitAny 或 WaitAll 操作。
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (asyncWaitHandle == null)
                {
                    bool done = IsCompleted;
                    AutoResetEvent mre = new AutoResetEvent(done);

                    if (Interlocked.CompareExchange(ref asyncWaitHandle, mre, null) != null)
                    {
                        mre.Close();    // Another thread created this object's event; dispose the event we just created
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            asyncWaitHandle.Set();  // If the operation wasn't done when we created the event but now it is done, set the event
                        }
                    }
                }

                return asyncWaitHandle;
            }
        }

        public bool IsCompleted
        {
            get { return completedState != StatePending; }
        }

        #endregion
    }
}
