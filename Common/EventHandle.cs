/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ManagedAutoResetEvent.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 事件处理类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;
using System.Threading;

namespace Common
{
    /// <summary>
    /// Interface for disposable objects that can inform they are already
    /// disposed without throwing an exception.
    /// </summary>
    public interface IAdvancedDisposable : IDisposable
    {
        /// <summary>
        /// Gets a value indicating if the object was already disposed.
        /// </summary>
        bool WasDisposed { get; }
    }

    /// <summary>
    /// Interface implemented by PooledEventWait. And, thanks to StructuralCaster, can be used to
    /// access custom EventWait objects.
    /// </summary>
    public interface IEventWait : IDisposable
    {
        /// <summary>
        /// Waits for this event to be signalled.
        /// </summary>
        void WaitOne();

        /// <summary>
        /// Waits for this event to be signalled or times-out.
        /// Returns if the object was signalled.
        /// </summary>
        bool WaitOne(int millisecondsTimeout);

        /// <summary>
        /// Waits for this event to be signalled or times-out.
        /// Returns if the object was signalled.
        /// </summary>
        bool WaitOne(TimeSpan timeout);

        /// <summary>
        /// Resets (unsignals) this wait event.
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets (signals) this wait event.
        /// </summary>
        void Set();

        void Close();
    }

    public sealed class ManagedAutoResetEvent : IAdvancedDisposable, IEventWait
    {
        private bool _value;
        private readonly object _lock = new object();

        public ManagedAutoResetEvent(bool initialState)
        {
            _value = initialState;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                WasDisposed = true;
                _value = true;
                Monitor.PulseAll(_lock);
            }
        }

        public void Close()
        {
            Dispose();
        }

        public bool WasDisposed { get; private set; }

        public void Reset()
        {
            lock (_lock)
            {
                if (WasDisposed)
                {
                    return;
                }

                _value = false;
            }
        }

        public void Set()
        {
            lock (_lock)
            {
                _value = true;
                Monitor.Pulse(_lock);
            }
        }

        public void WaitOne()
        {
            lock (_lock)
            {
                while (!_value)
                {
                    Monitor.Wait(_lock);
                }

                if (!WasDisposed)
                {
                    _value = false;
                }
            }
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            lock (_lock)
            {
                while (!_value)
                {
                    if (!Monitor.Wait(_lock, millisecondsTimeout))
                    {
                        return false;
                    }
                }

                if (!WasDisposed)
                {
                    _value = false;
                }
            }

            return true;
        }

        public bool WaitOne(TimeSpan timeout)
        {
            lock (_lock)
            {
                while (!_value)
                {
                    if (!Monitor.Wait(_lock, timeout))
                    {
                        return false;
                    }
                }

                if (!WasDisposed)
                {
                    _value = false;
                }
            }

            return true;
        }
    }

    public sealed class SystemAutoResetEvent : IAdvancedDisposable, IEventWait
    {
        private AutoResetEvent wrappedEvent;

        public SystemAutoResetEvent(bool initialState)
        {
            wrappedEvent = new AutoResetEvent(initialState);
        }

        public void Dispose()
        {
            if (wrappedEvent != null)
            {
                wrappedEvent.Close();
                wrappedEvent = null;
            }
        }

        public void Close()
        {
            Dispose();
        }

        public bool WasDisposed
        {
            get
            {
                return wrappedEvent == null;
            }
        }

        public void Reset()
        {
            if (wrappedEvent != null)
            {
                wrappedEvent.Reset();
            }
        }

        public void Set()
        {
            if (wrappedEvent != null)
            {
                wrappedEvent.Set();
            }
        }

        public void WaitOne()
        {
            if (wrappedEvent != null)
            {
                wrappedEvent.WaitOne();
            }
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            if (wrappedEvent != null)
            {
                return wrappedEvent.WaitOne(millisecondsTimeout);
            }

            return true;
        }

        public bool WaitOne(TimeSpan timeout)
        {
            if (wrappedEvent != null)
            {
                return wrappedEvent.WaitOne((int)timeout.TotalMilliseconds);
            }

            return true;
        }
    }

    public sealed class AutoResetEventFactory
    {
        static public IEventWait CreateAutoResetEvent(bool initialState)
        {
            return new SystemAutoResetEvent(initialState);
        }

        static public IEventWait CreateAutoResetEvent()
        {
            return new SystemAutoResetEvent(false);
        }
    }
}