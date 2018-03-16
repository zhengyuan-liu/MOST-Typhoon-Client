using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using TyphoonClient.MostTyphoon;
using TyphoonClient.TyphoonServer;

namespace TyphoonClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors
        /// <summary>
        /// 程序开始运行
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            mainWindow.Children.Add(new CommonControl(mainWindow)); 
            mainWindow.Children.Add(new ProfessionControl(mainWindow));
            mainWindow.Children[1].Visibility = Visibility.Hidden;
        }
        #endregion

        #region Events
        /// <summary>
        /// 添加数据菜单点击事件
        /// </summary>
        private void MenuItemAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();     //打开打开文件窗口
            openFileDialog.Title = "导入新的台风路径数据";       //打开文件窗口标题
            openFileDialog.Filter = "文本文件(*.txt)|*.txt|All files(*.*)|*.*";   //选择为txt文件和所有文件
            openFileDialog.Multiselect = true;         //可以多选
            openFileDialog.RestoreDirectory = true;     //记录目录
            string msg = string.Empty;
            if (openFileDialog.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                string[] fileName = openFileDialog.FileNames;         //获取选择的文件名
                for (int i = 0; i < fileName.Length; i++)
                    if (ImportToDB(fileName[i]))             //导入数据承购是否
                        msg += DateTime.Now.ToString() + " Import " + Path.GetFileName(fileName[i]) + " succeed\n";
                    else
                        msg += DateTime.Now.ToString() + " Import " + Path.GetFileName(fileName[i]) + " failed\n";
                MessageBox.Show(msg, "导入新台风数据", MessageBoxButton.OK, MessageBoxImage.Information);              //显示记录信息
            }
            openFileDialog.Dispose();
        }
        #endregion

        #region Methods
        /// <summary>
        /// 将最佳路径数据集数据导入数据库
        /// </summary>
        private bool ImportToDB(string fileName)
        {
            List<CMostTyphoon> MOSTTyphoons = GetTyphoon(fileName);            //从最佳路径数据中读取台风数据并转换为most模型
            CMyTyphoonSQL.SaveTyphoons(ref MOSTTyphoons, Path.GetFileNameWithoutExtension(fileName));  //数据库保存台风路径数据
            return true;
        }
        /// <summary>
        /// //从最佳路径数据中读取台风数据并转换为MOST模型
        /// </summary>
        private List<CMostTyphoon> GetTyphoon(string fileName)
        {
            List<CMostTyphoon> MOSTTyphoons = new List<CMostTyphoon>();
            StreamReader sr = new StreamReader(fileName);
            while (true)                        //依次从文件中读取台风路径数据
            {
                CMostTyphoon typh = new CMostTyphoon();
                if (typh.ReadTyphoon(sr))
                    MOSTTyphoons.Add(typh);
                else
                    break;
            }
            sr.Close();
            sr.Dispose();
            return MOSTTyphoons;
        }
        #endregion
    }
}