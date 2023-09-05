/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ChannelManager.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : ChannelManager
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Common;
using System;
using System.Collections.Generic;

namespace Channel
{
    /// <summary>
    /// 通道添加或删除后产生的事件委托。
    /// </summary>
    /// <param name="channel">添加或被删除的通道对象。</param>
    /// <param name="isAdd">值为true表示添加，否则表示删除。</param>
    public delegate void ChannelAddedRemovedEventHandler(object sender, Channel channel, bool isAdd);

    /// <summary>
    /// 通道对象管理器。
    /// </summary>
    public class ChannelManager
    {
        public static object lockChannel = new object();

        /// <summary>
        /// 默认通道前缀名。在未设置通道名的情况下将设置通道名为DefaultChannelPrefixName加数组（从1开始）
        /// </summary>
        const string DefaultChannelPrefixName = "通道";
        List<Channel> channels = new List<Channel>();

        /// <summary>
        /// 通道成功添加或被删除后将产生此事件。
        /// </summary>
        public event ChannelAddedRemovedEventHandler ChannelAddedOrRemoved;

        /// <summary>
        /// 获取通道个数。
        /// </summary>
        public int Count
        {
            get
            {
                return channels.Count;
            }
        }

        /// <summary>
        /// 获取通道列表（列表为只读）。
        /// </summary>
        public IList<Channel> Channels
        {
            get
            {
                return channels;
            }
        }

        /// <summary>
        /// 根据通道列表索引（从0开始）获取通道对象。
        /// 此函数不对通道计数加锁
        /// </summary>
        /// <param name="index">从0开始的索引值。</param>
        public Channel this[int index]
        {
            get
            {
                if (index >= 0 && index < channels.Count)
                {
                    return channels[index];
                }

                return null;
            }
        }

        /// <summary>
        /// 返回当前通道计数， 此函数不对通道计数加锁
        /// </summary>
        public int ChannelCount
        {
            get
            {
                return channels.Count;
            }
        }

        /// <summary>
        /// 根据通道名获取通道对象。
        /// </summary>
        /// <param name="channelName">通道名</param>
        public Channel this[string channelName]
        {
            get
            {
                lock (lockChannel)
                {
                    return FindChannel(channelName);
                }
            }
        }

        /// <summary>
        /// 添加通道。
        /// 若已经存在item，或存在与item相同通道名则不添加该对象，返回false。
        /// </summary>
        /// <param name="channel">通道对象。</param>
        public bool Add(Channel channel)
        {
            lock (lockChannel)
            {
                if (channel == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(channel.Name))
                {
                    channel.Name = AutoAssignChannelName();
                }

                if (channels.Count > 0 && channel.Protocol.NeedPort && channel.Protocol != null && channel.Protocol.Port != null && channel.Protocol.Port is CommPortTcpServer)
                {
                    channels.Insert(0, channel);
                }
                else
                {
                    channels.Add(channel);
                }

                if (ChannelAddedOrRemoved != null)
                {
                    ChannelAddedOrRemoved.Invoke(this, channel, true);
                }

                return true;
            }
        }

        /// <summary>
        /// 删除通道。
        /// </summary>
        /// <param name="channel">通道对象</param>
        public bool Remove(Channel channel)
        {
            lock (lockChannel)
            {
                bool result = false;

                try
                {
                    result = channels.Remove(channel);

                    if (result && ChannelAddedOrRemoved != null)
                    {
                        ChannelAddedOrRemoved(this, channel, false);
                    }
                }
                catch
                { }

                return result;
            }
        }

        /// <summary>
        /// 根据端口移除某通道
        /// </summary>
        /// <param name="comPort"></param>
        /// <returns></returns>
        public List<Channel> Remove(ICommPort comPort)
        {
            List<Channel> removed = new List<Channel>();

            try
            {
                if (comPort != null)
                {
                    for (int i = channels.Count - 1; i >= 0; i--)
                    {
                        if (channels[i].Protocol.Port != comPort)
                        {
                            continue;
                        }

                        removed.Add(channels[i]);
                        channels.Remove(channels[i]);
                    }

                    for (int i = channels.Count - 1; i >= 0; i--)
                    {
                        // wuqiubin 2013-1-30 删除共享通道修改
                        if (!(channels[i].Protocol.Port is CommPortShare) ||
                            !removed.Contains(channels[i].Protocol.SharedProtocol.Channel))
                        {
                            continue;
                        }

                        removed.Add(channels[i]);
                        channels.Remove(channels[i]);
                    }
                }
            }
            catch { }

            return removed;
        }

        /// <summary>
        /// 清空通道列表。
        /// </summary>
        public void Clear()
        {
            lock (lockChannel)
            {
                channels.Clear();
            }
        }

        /// <summary>
        /// 若已经存在item，或存在与item相同通道名则返回true.
        /// </summary>
        /// <param name="channel">通道对象。</param>
        public bool Contains(Channel channel)
        {
            if (channels.Contains(channel))
            {
                return true;
            }

            for (int i = channels.Count - 1; i >= 0; i--)
            {
                if (channels[i].Name == channel.Name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据通道名查找并返回通道，找不到则返回null。
        /// </summary>
        /// <param name="channelName">通道名</param>
        public Channel FindChannel(string channelName)
        {
            for (int i = channels.Count - 1; i >= 0; i--)
            {
                if (channels[i].Name == channelName)
                {
                    return channels[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 所有通道全部停止工作。
        /// </summary>
        /// <param name="waitForStop">是否等待所有通道都已经停止工作。（等待事件不会超过 通道数×1秒）</param>
        public void AllStopWork(bool waitForStop)
        {
            for (int i = channels.Count - 1; i >= 0; i--)
            {
                try
                {
                    channels[i].StopWork(waitForStop);
                    System.Threading.Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    UtMessageBase.ShowOneMessage(ex.ToString(), PopupMessageType.Exception);
                }
            }

            channels.Clear();
        }

        /// <summary>
        /// 自动分配通道名称并返回。
        /// </summary>
        string AutoAssignChannelName()
        {
            int channelNo = 1;

            for (int i = channels.Count - 1; i >= 0; i--)
            {
                if (!channels[i].Name.StartsWith(DefaultChannelPrefixName))
                {
                    continue;
                }

                string chNo = channels[i].Name.Substring(DefaultChannelPrefixName.Length);

                try
                {
                    channelNo = int.Parse(chNo) + 1;
                    break;
                }
                catch
                {// do nothing.
                }
            }

            return DefaultChannelPrefixName + channelNo;
        }

        /// <summary>
        /// 根据端口移除某通道
        /// </summary>
        /// <param name="comPort"></param>
        /// <returns></returns>
        public Channel FindChanelByPort(ICommPort comPort)
        {
            try
            {
                if (comPort != null)
                {
                    for (int i = channels.Count - 1; i >= 0; i--)
                    {
                        if (channels[i].Protocol.Port == comPort)
                        {
                            return channels[i];
                        }
                    }
                }
            }
            catch
            { }
            return null;
        }
    }
}
