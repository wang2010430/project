/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : PortBufferAccessor.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : PortBufferAccessor
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using System;

namespace Channel
{
	/// <summary>
	/// 端口缓冲区访问器。
    /// 它不会更改缓冲区的数据，而只是提供通过指定的位置来读取数据的方法或属性等。
	/// </summary>
	public class PortBufferAccessor
	{
        /// <summary>
        /// 缓冲区最大的空间
        /// </summary>
        static public int BufferMaxSize = 1024 * 1024 * 20;

        /// <summary>
        /// 端口缓冲区。
        /// </summary>
        protected byte[] readBuffer = new byte[4096];

        /// <summary>
        /// 在缓冲区中可读数据的开始位置。
        /// </summary>
        protected int positionToRead = 0;

        /// <summary>
        /// 在缓冲区中要写入数据的开始位置。
        /// </summary>
        protected int positionToWrite = 0;

        /// <summary>
        /// 返回缓冲区是否有数据。
        /// </summary>
        public bool HasData
        {
            get
            {
                return GetCacheSize() > 0;
            }
        }

        /// <summary>
        /// 写数据到缓冲区中。
        /// 后置条件：若缓冲区不够大则成倍变大缓冲区空间（默认不能超过10M）以具有储存要保存数据的容量。
        /// </summary>
        /// <param name="buffer">接收到的数据</param>
        /// <param name="offset">buffer的有效数据起始位置</param>
        /// <param name="size">数据的长度</param>
        public void Write(byte[] buffer, int offset, int size)
        {
            lock (readBuffer)
            {
                byte[] buf = PortBufferIncrease(readBuffer, readBuffer.Length - positionToWrite, size);
                
                if (buf == null)
                {
                    Clear();
                    throw new OutOfMemoryException(string.Format("读缓冲区已经超出UTComm的:{0}字节限制", BufferMaxSize));
                }
                
                readBuffer = buf;

                // 写数据
                Array.Copy(buffer, offset, readBuffer, positionToWrite, size);
                positionToWrite += size;
            }
        }

        /// <summary>
        /// 端口缓冲区空间不够时将成倍增加（如当前缓冲区大小为2048，若不够时则增大到4096，下次不够时则增大到8192，依次增加。但若超过10M则返回null）。
        /// 返回新的缓冲区，若oldBuffer够用则将它自己返回。
        /// </summary>
        /// <param name="buffer">端口缓冲区</param>
        /// <param name="remainingSize">oldBuffer中可以存储的空间</param>
        /// <param name="requiredSize">需求空间</param>
        byte[] PortBufferIncrease(byte[] buffer, int remainingSize, int requiredSize)
        {
            // 需要申请新的空间
            if (remainingSize >= requiredSize) 
            {
                return buffer; 
            }

            if (positionToRead != 0 && positionToWrite - positionToRead > 0 && positionToWrite - positionToRead < buffer.Length)
            {
                Array.ConstrainedCopy(buffer, positionToRead, buffer, 0, positionToWrite - positionToRead);
                positionToWrite = positionToWrite - positionToRead;
                positionToRead = 0;
                remainingSize += positionToRead;
            }

            Array.Resize(ref buffer, buffer.Length + requiredSize - remainingSize);

            return buffer;
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（不改变缓冲区数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        public byte[] Peek(int size)
        {
            lock (readBuffer)
            {
                int dataSize = GetCacheSize();

                if (dataSize == 0 || dataSize < size)
                {
                    return null;
                }

                byte[] retData = new byte[size];

                Array.Copy(readBuffer, positionToRead, retData, 0, size);

                return retData;
            }
        }

        /// <summary>
        /// 查看并返回缓冲区的数据（从缓冲区删除已返回的数据）。
        /// 若缓冲区中的数据少于size则返回null。
        /// </summary>
        /// <param name="size">要查看的数据大小</param>
        public byte[] Read(int size)
        {
            lock (readBuffer)
            {
                byte[] retData = Peek(size);

                // 更新读取的位置（即删除数据）
                if (retData != null)
                {
                    RemoveData(size);
                }

                return retData;
            }
        }

        /// <summary>
        /// 清除缓冲区数据。
        /// </summary>
        /// <param name="size">要清除数据的大小</param>
        public void RemoveData(int size)
        {
			lock (readBuffer)
			{
			    if (positionToRead + size >= positionToWrite)
			    {
			        positionToRead = positionToWrite = 0;
			    }
			    else
			    {
			        positionToRead += size;
			    }
			}
        }

        /// <summary>
        /// 获得缓冲区数据长度。
        /// </summary>
        public int GetCacheSize()
        {
			lock (readBuffer)
			{
				return positionToWrite - positionToRead;
			}
        }

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
		public void Clear()
		{
            lock (readBuffer)
            {
                positionToRead = 0;
                positionToWrite = 0;
            }
		}
	}
}
