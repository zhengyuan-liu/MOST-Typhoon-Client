using System;
using System.Windows.Controls;

namespace TyphoonClient
{
    /// <summary>
    /// SearchControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchPanel : UserControl
    {
        #region Members
        /// <summary>
        /// 记录查询的通讯语句
        /// </summary>
        public string search_string;
        /// <summary>
        /// //用来标识图幅范围
        /// </summary>
        private double maxlatitude = 50, minlatitude = 3, maxlongitude = 180, minlongitude = 90;
        #endregion

        #region Constructor
        /// <summary>
        /// //构造函数
        /// </summary>
        public SearchPanel()
        {
            InitializeComponent();
            Set_year_item();          //设置年份
        }
        #endregion

        #region Methods
        /// <summary>
        /// //获取请求并相应
        /// </summary>
        /// <returns></returns>
        public string GetRequest()
        {
            if (Tab.SelectedIndex == 0)
                ATTR_search();
            else if (Tab.SelectedIndex == 1)
                TIME_search();
            else if (Tab.SelectedIndex == 2)
                SPACE_search();
            else if (Tab.SelectedIndex == 3)
            {
                //string datetime = calendar.SelectedDate?.ToString("yyyy-MM-dd ");
                string datetime = calendar.SelectedDate.ToString();
                datetime = datetime.Substring(0, datetime.Length - 7);
                datetime = datetime.Replace('/', '-');
                datetime += cmbBoxHour.SelectedIndex.ToString("d2") + ":";
                datetime += cmbBoxMin.SelectedIndex.ToString("d2") + ":";
                datetime += cmbBoxSec.SelectedIndex.ToString("d2");
                return "search\t" + datetime;
            }
            else
                search_string = "Wrong";
            return "typhoon " + search_string;
        }
        /// <summary>
        /// 生成属性查询语句
        /// </summary>
        private void ATTR_search()
        {
            search_string = "Attribute" + " " + comboAttrYear.Text + " ";
            Convert.ToInt32(comboAttrYear.Text);
            if (radiobtnId.IsChecked == true && txtId.Text.Replace(" ", "") != "")
            {
                try
                {        
                    int id = Convert.ToInt32(txtId.Text);
                    if (id < 0)
                    {
                        search_string = "Wrong";
                        txtId.Text = "";
                    }
                    else
                        search_string += "ID" + " " + txtId.Text;
                    return;
                }
                catch { search_string = "Wrong"; txtId.Text = ""; }
            }
            if (radiobtnName.IsChecked == true && txtName.Text.Replace(" ", "") != "")
            { search_string += "NAME" + " " + txtName.Text.Replace(" ", ""); return; }
            if (radiobtnStrength.IsChecked == true && txtStrength.Text.Replace(" ", "") != "")
            {
                try
                {
                    double strength = Convert.ToDouble(txtStrength.Text);
                    if (strength >= 10 || strength < 0)
                    {
                        search_string = "Wrong";
                        txtStrength.Text = "";
                    }
                    else
                        search_string += "STRENGTH" + " " + txtStrength.Text;
                    return;
                }
                catch { search_string = "Wrong"; txtStrength.Text = ""; }
            }
            search_string = "Wrong";
        }
        /// <summary>
        /// 生成时间查询语句
        /// </summary>
        private void TIME_search()
        {
            search_string = "Time" + " " + comboTimeYear.Text + " " + "Time" + " ";
            string from, to;
            if (DPFromDate.SelectedDate != null)
                from = ((DateTime)DPFromDate.SelectedDate).ToString("yyyy-MM-dd-HH");
            else
                from = comboTimeYear.Text + "-01-01-00";
            if (DPToDate.SelectedDate != null)
                to = ((DateTime)DPToDate.SelectedDate).ToString("yyyy-MM-dd-HH");
            else
                to = comboTimeYear.Text + "-12-31-24";
            search_string += from + " ";
            search_string += to;
        }
        /// <summary>
        /// 生成空间查询语句
        /// </summary>
        private void SPACE_search()
        {
            search_string = "Space" + " " + comboSpaceYear.Text + " " + "Bound" + " ";
            Convert.ToInt32(comboSpaceYear.Text);
            try
            {
                double ltlong = Convert.ToDouble(txtTopLeftLong.Text);
                double ltlat = Convert.ToDouble(txtTopLeftLat.Text);
                double dtlong = Convert.ToDouble(txtDownRightLong.Text);
                double dtlat = Convert.ToDouble(txtDownRightLat.Text);
                if (ltlong <= maxlongitude && ltlong >= minlongitude && ltlat <= maxlatitude && ltlat >= minlatitude)
                    search_string += txtDownRightLat.Text + " " + txtTopLeftLong.Text + " ";
                else
                    search_string = "Wrong";
                if (dtlong <= maxlongitude && dtlong >= minlongitude && dtlat <= maxlatitude && dtlat >= minlatitude && search_string != "Wrong")
                    search_string += txtTopLeftLat.Text + " " + txtDownRightLong.Text;
                else
                    search_string = "Wrong";
            }
            catch
            {
                search_string = "Wrong";
                txtDownRightLat.Text = "";
                txtTopLeftLat.Text = "";
                txtDownRightLong.Text = "";
                txtTopLeftLong.Text = "";
            }
        }
        /// <summary>
        /// 初始化年份下拉框，动态生成（1970-至今）
        /// </summary>
        private void Set_year_item()
        {
            int now_year = DateTime.Now.Year - 2;
            int year_count = now_year - 1969;
            int[] year_value = new int[year_count];
            for (int i = 0; i < year_count; i++)
                year_value[i] = now_year - i;
            comboTimeYear.ItemsSource = year_value;
            comboSpaceYear.ItemsSource = year_value;
            comboAttrYear.ItemsSource = year_value;
        }
        #endregion

        #region Events
        /// <summary>
        /// //ID输入框输入响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void id_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            radiobtnId.IsChecked = true;
            txtName.Text = "";
            txtStrength.Text = "";
        }
        /// <summary>
        /// //名称输入框输入响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void name_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            radiobtnName.IsChecked = true;
            txtId.Text = "";
            txtStrength.Text = "";
        }
        /// <summary>
        /// //强度输入框输入响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void str_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            radiobtnStrength.IsChecked = true;
            txtName.Text = "";
            txtId.Text = "";
        }
        /// <summary>
        /// //选择查询的台风年份变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string year_value = comboTimeYear.SelectedValue.ToString();
            int year_select = Convert.ToInt32(year_value);
            DPFromDate.SelectedDate = new DateTime(year_select, 1, 1);
            DPToDate.SelectedDate = new DateTime(year_select, 12, 31);
            DPFromDate.DisplayDateStart = new DateTime(year_select, 1, 1);
            DPFromDate.DisplayDateEnd = new DateTime(year_select, 12, 31);
            DPToDate.DisplayDateStart = new DateTime(year_select, 1, 1);
            DPToDate.DisplayDateEnd = new DateTime(year_select, 12, 31);
        }
        /// <summary>
        /// 起始时间选择后控制终止时间的可选范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void from_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DPToDate.DisplayDateStart = DPFromDate.SelectedDate;
        }
        #endregion
    }
}