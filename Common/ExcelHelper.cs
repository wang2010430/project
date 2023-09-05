/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : ExcelHelper.cs
* date      : 2023/04/19
* author    : haozhe.ni
* brief     : 本地配置帮忙类
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

using Microsoft.Office.Interop.Excel;
using System;
using System.Runtime.InteropServices;

namespace Common
{
	public class ExcelHelper
    {
		private string m_strPath = null;    // 打开的Excel路径 or 需要创建的excel路径;
		private bool m_isCreateMode = false;
		private object MISSING_VALUE = System.Reflection.Missing.Value;

		private Application m_AppMain = null;
		private Workbook m_Workbook = null;
		private Worksheet m_Worksheet = null;

		/// <summary>
		/// 创建一个解析器;
		/// </summary>
		public static ExcelHelper CreateExcelHelper()
		{
			Application appMain = new Application();
			if (appMain == null)
			{
				return null;
			}
			appMain.Visible = false;
			appMain.UserControl = true;

			ExcelHelper eh = new ExcelHelper();
			eh.m_AppMain = appMain;
			return eh;
		}

		public bool CreateExcel(string strPath)
		{
			m_strPath = strPath;
			m_isCreateMode = true;
			//新建一张表;
			m_Workbook = m_AppMain.Workbooks.Add(MISSING_VALUE);
			return true;
		}

		public bool OpenExcel(string strPath)
		{
			m_isCreateMode = true;
			try
			{
				m_Workbook = m_AppMain.Workbooks.Open(strPath);
				if (m_Workbook == null)
				{
					return false;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public int RowCount
		{
			get
			{
				return m_Worksheet.UsedRange.Cells.Rows.Count; //得到行数
			}
		}

		public int ColCount
		{
			get
			{
				return m_Worksheet.UsedRange.Cells.Columns.Count;//得到列数
			}
		}

		public string WorksheetName
		{
			get
			{
				return m_Worksheet.Name;//得到列数
			}
		}

		public int PageCount
		{
			get
			{
				return m_Workbook.Worksheets.Count;
			}
		}

		/// <summary>
		/// 选择一个页,BeginIndex == 1
		/// </summary>
		public void SelectPage(int nPageIndex = 1)
		{
			//取得第一个工作薄
			m_Worksheet = (Worksheet)m_Workbook.Worksheets.get_Item(nPageIndex);
		}

		public string ReadGrid(int nRow, int nCol)
		{
			return (string)m_Worksheet.Cells[nRow, nCol].Text;
		}

		public void WriteGrid(int nRow, int nCol, string strValue)
		{
			m_Worksheet.Cells[nRow, nCol] = strValue;
		}

		public bool Save()
		{
			if (m_isCreateMode)
			{
				m_Workbook.SaveAs(m_strPath);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Close()
		{
			m_Workbook.Close(true);
			m_AppMain.Quit();
			Kill(m_AppMain);//调用kill当前excel进程  
		}


		[DllImport("User32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int Processid);

		public static void Kill(Application theApp)
		{
			int iId = 0;
			IntPtr intptr = new IntPtr(theApp.Hwnd);
			System.Diagnostics.Process p = null;

			try
			{
				GetWindowThreadProcessId(intptr, out iId);
				p = System.Diagnostics.Process.GetProcessById(iId);

				if (p != null)
				{
					p.Kill();
					p.Dispose();
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}
    }
}
