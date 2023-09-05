using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeBin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = GetProjectFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                listBox1.Items.Add(fileName);
            }
        }

        /// <summary>
        /// Merge bin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.Items.Count == 0)
                {
                    MessageBox.Show("No files selected!");
                    return;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Bin Files (*.bin)|*.bin"; // 设置文件过滤器
                saveFileDialog.Title = "Save File"; // 设置对话框标题

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    for (int count = 0; count < listBox1.Items.Count; count++)
                    {
                        string fileName = listBox1.Items[count].ToString();

                        byte[] hexBytes = ReadHexBytesFromFile(fileName);

                        try
                        {
                            FileUtils.AppendFileBytes(hexBytes, filePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Read {fileName} Error: {ex.Message}");
                            break;
                        }
                    }
                    MessageBox.Show($"Merge {filePath} successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private byte[] ReadHexBytesFromFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string[] hexValues = content.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Calculate the size needed for alignment
            int alignedLength = (hexValues.Length + 3) & ~3; // Round up to the nearest multiple of 4

            byte[] hexBytes = new byte[alignedLength];

            for (int i = 0; i < hexValues.Length; i++)
            {
                hexBytes[i] = Convert.ToByte(hexValues[i], 16);
            }

            // Fill the remaining bytes with 0xFF
            for (int i = hexValues.Length; i < alignedLength; i++)
            {
                hexBytes[i] = 0xFF;
            }

            return hexBytes;
        }

        private string GetProjectFile()
        {
            string filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a txt File";
            openFileDialog.Filter = "TXT Files (*.txt)|*.txt"; // 可以根据需要设置文件过滤器

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }

            return filePath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 获取所有被选中的项的索引
            List<int> selectedIndices = new List<int>();
            foreach (int selectedIndex in listBox1.SelectedIndices)
            {
                selectedIndices.Add(selectedIndex);
            }

            // 按索引从后向前删除所选项
            selectedIndices.Reverse();
            foreach (int index in selectedIndices)
            {
                listBox1.Items.RemoveAt(index);
            }
        }
    }
}
