using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using TyphoonClient.MostTyphoon;
using TyphoonClient.TyphoonServer;

namespace TyphoonClient
{
    /// <summary>
    /// ///MainInterface.xaml 的交互逻辑
    /// </summary>
    public partial class CommonControl : UserControl
    {
        #region Members
        /// <summary>
        /// //用来标注父窗口
        /// </summary>
        private Grid formParent = null;
        /// <summary>
        /// //用来表示存储的台风集合
        /// </summary>
        List<CMostTyphoon> typhoonList = new List<CMostTyphoon>();
        /// <summary>
        /// //正在显示的台风对象
        /// </summary>
        CMostTyphoon cTyphoon = new CMostTyphoon();
        /// <summary>
        /// //计时器
        /// </summary>
        private DispatcherTimer timer;
        /// <summary>
        /// //放大倍数
        /// </summary>
        double scale = 1;
        /// <summary>
        /// //播放动画控制tick
        /// </summary>
        private int tm = 0;
        /// <summary>
        /// //用来标识要显示的背景图
        /// </summary>
        private byte backPicIndex = 0;
        /// <summary>
        /// //记录鼠标点的位置
        /// </summary>
        private Point mouseXY;
        #endregion

        #region Static Members
        /// <summary>
        /// //用来标识图幅范围
        /// </summary>
        private static double maxLat = 50, minLat = 0, maxLong = 170, minLong = 100;
        #endregion

        #region Constructors
        /// <summary>
        /// //构造函数
        /// </summary>
        public CommonControl(Grid mainform)
        {
            try
            {
                InitializeComponent();
                ImageBrush backImgBru = new ImageBrush(BmpToImg(Properties.Resources.TyphoonMap));
                backImgBru.TileMode = TileMode.Tile;
                backImgBru.Stretch = Stretch.Fill;
                canvasShow.Background = backImgBru;
                ImgLegend.Source = BmpToImg(Properties.Resources.Legend);
                formParent = mainform;
                typhoonlistInitial();
            }
            catch(Exception)
            {
                btnPlay.IsEnabled = false;
                btnToPro.IsEnabled = false;
                MessageBox.Show("与服务器连接异常，没有找到所需的数据");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// //台风列表初始化
        /// </summary>
        private void typhoonlistInitial()
        {
            string reqMsg = "typhoon Time " + (DateTime.Now.Year - 2).ToString() + " Time 2014-01-01-00 2014-12-31-24";
            TyphoonRequest request = new TyphoonRequest(reqMsg);
            typhoonList = TyphServer.GetTyphoonData(request);
            if (typhoonList == null)
            {
                MessageBox.Show("与服务器连接异常，没有找到所需的数据");
                btnPlay.IsEnabled = false;
                return;
            }
            for (int i = 0; i < typhoonList.Count; i++)
            {
                Label temp = new Label();
                temp.Content = typhoonList[i].header.typhName;
                lstboxTyphoons.Items.Add(temp);
            }
            cTyphoon = typhoonList[0];
            typhoonInitial();
        }
        /// <summary>
        /// 为一条台风赋值初始化
        /// </summary>
        private void typhoonInitial()
        {
            CMostTyphoonPoint pt = cTyphoon.points[cTyphoon.POINTSNUM - 1];
            lstboxAttri.Items.Clear();
            for (int i = cTyphoon.POINTSNUM - 1; i > -1; i--)//显示台风点位数据
            {
                ListBoxItem lbitem = new ListBoxItem();
                lbitem.Content = cTyphoon.points[i].recordTime.ToString("MM/dd/HH") + "      " + cTyphoon.points[i].latitude + "/" + cTyphoon.points[i].longitude + "\t" + cTyphoon.points[i].centerPress + "\t" + cTyphoon.points[i].aveSpeed;
                lstboxAttri.Items.Add(lbitem);
            }
            lblStateLabel.Content = cTyphoon.header.typhName + "(编号：" + cTyphoon.header.cycloneID + ")    " + "已停编";
            txtState.Text = "2015年第" + cTyphoon.header.cycloneID + "号热带低压，" + cTyphoon.header.typhName + "于" + pt.recordTime + "时位于北纬" + pt.latitude + "°、东经" + pt.longitude + "°，最大风速" + pt.aveSpeed + "m/s，中心气压" + pt.centerPress + "hPa";
            lblIdentify.Content = "名称：" + cTyphoon.header.typhName + "    编号：" + cTyphoon.header.cycloneID + "号";
            txtCurrent.Text = "实况信息\n" + pt.recordTime;
            txtPredict.Text = "预报信息\n---";
            lblLat.Content = pt.latitude.ToString();
            lblLong.Content = pt.longitude.ToString();
            lblSpeed.Content = pt.aveSpeed.ToString();
            lblCenter.Content = pt.centerPress.ToString();
            CMostTyphoonPoint p3h = cTyphoon.ShortTimePredict(3);
            lblLat3h.Content = p3h.latitude.ToString("F3");
            lblLong3h.Content = p3h.longitude.ToString("F3");
            CMostTyphoonPoint p6h = cTyphoon.ShortTimePredict(6);
            lblLat6h.Content = p6h.latitude.ToString("F3");
            lblLong6h.Content = p6h.longitude.ToString("F3");
            drawTyphoon();
            drawMOSTTyphoon();

        }
        /// <summary>
        /// 画台风
        /// </summary>
        private void drawTyphoon()
        {
            Point a = new Point();
            a.X = (cTyphoon.points[0].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            a.Y = canvasShow.Height - (cTyphoon.points[0].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            for (int i = 1; i < cTyphoon.POINTSNUM; i++)
            {
                Ellipse tp = new Ellipse();
                tp.Height = 10 / scale;
                tp.Width = 10 / scale;
                tp.Fill = typhSpeedColor(cTyphoon.points[i - 1].aveSpeed);
                tp.Stroke = Brushes.Black;
                tp.StrokeThickness = 1 / scale;
                Canvas.SetLeft(tp, a.X - 5 / scale);
                Canvas.SetTop(tp, a.Y - 5 / scale);
                Canvas.SetZIndex(tp, 2);
                canvasShow.Children.Add(tp);
                Point c = new Point();
                c.Y = canvasShow.Height - (cTyphoon.points[i].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                c.X = (cTyphoon.points[i].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                Line l = new Line();
                l.Stroke = Brushes.SteelBlue;
                l.StrokeThickness = 2 / scale;  //可以改成虚线
                l.X1 = a.X; l.X2 = c.X; l.Y1 = a.Y; l.Y2 = c.Y;
                Canvas.SetZIndex(l, 1);
                canvasShow.Children.Add(l);
                a.X = c.X; a.Y = c.Y;
            }
            Ellipse tpf = new Ellipse();
            tpf.Height = 10 / scale;
            tpf.Width = 10 / scale;
            tpf.Fill = typhSpeedColor(cTyphoon.points[cTyphoon.POINTSNUM - 1].aveSpeed);
            Canvas.SetLeft(tpf, a.X - 5 / scale);
            Canvas.SetTop(tpf, a.Y - 5 / scale);
            Canvas.SetZIndex(tpf, 2);
            tpf.Stroke = Brushes.Black;
            tpf.StrokeThickness = 1 / scale;
            canvasShow.Children.Add(tpf);
        }
        private void drawMOSTTyphoon()
        {
            Point point0 = new Point();
            Point point1 = new Point();
            Point point2 = new Point();
            for (int i = 1; i < cTyphoon.mostPoints.Count; i+=2)
            {              
                point0.X = (cTyphoon.mostPoints[i - 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                point0.Y = canvasShow.Height - (cTyphoon.mostPoints[i - 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                Ellipse tp = new Ellipse();
                tp.Height = 10 / scale;
                tp.Width = 10 / scale;
                tp.Fill = typhSpeedColor(cTyphoon.mostPoints[i - 1].aveSpeed);
                tp.Stroke = Brushes.Black;
                tp.StrokeThickness = 1 / scale;
                Canvas.SetLeft(tp, point0.X - 5 / scale);
                Canvas.SetTop(tp, point0.Y - 5 / scale);
                Canvas.SetZIndex(tp, 2);
                canvasShow.Children.Add(tp);      
                point1.Y = canvasShow.Height - (cTyphoon.mostPoints[i].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                point1.X = (cTyphoon.mostPoints[i].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                Line l = new Line();
                l.Stroke = Brushes.DarkBlue;
                l.StrokeThickness = 2 / scale;
                l.X1 = point0.X; l.X2 = point1.X; l.Y1 = point0.Y; l.Y2 = point1.Y;
                Canvas.SetZIndex(l, 1);
                canvasShow.Children.Add(l);

                point2.Y = canvasShow.Height - (cTyphoon.mostPoints[i+1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                point2.X = (cTyphoon.mostPoints[i+1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                DoubleCollection dc = new DoubleCollection();
                dc.Add(2);
                dc.Add(2);
                Line dl = new Line();
                dl.Stroke = Brushes.DarkBlue;
                dl.StrokeThickness = 2 / scale;
                dl.X1 = point1.X; dl.X2 = point2.X; dl.Y1 = point1.Y; dl.Y2 = point2.Y;
                dl.StrokeDashArray = dc;
                canvasShow.Children.Add(dl);
                Canvas.SetZIndex(dl, 1);
            }
            #region 画预测轨迹
            CMostTyphoonPoint p6h = cTyphoon.ShortTimePredict(6);
            point0.X = (cTyphoon.points[cTyphoon.POINTSNUM - 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            point0.Y = canvasShow.Height - (cTyphoon.points[cTyphoon.POINTSNUM - 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            point1.Y = canvasShow.Height - (p6h.latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            point1.X = (p6h.longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            Line lp = new Line();
            lp.Stroke = Brushes.Red;
            lp.StrokeThickness = 2 / scale;
            lp.X1 = point0.X; lp.X2 = point1.X; lp.Y1 = point0.Y; lp.Y2 = point1.Y;
            Canvas.SetZIndex(lp, 1);
            DoubleCollection dcl = new DoubleCollection();
            dcl.Add(2);
            dcl.Add(2);
            lp.StrokeDashArray = dcl;
            canvasShow.Children.Add(lp);
            #endregion
        }
        /// <summary>
        /// 风级颜色函数
        /// </summary>
        private SolidColorBrush typhSpeedColor(double speed)
        {
            if (speed <= 17.1) return Brushes.Aquamarine;
            else if (speed < 24.4) return Brushes.Yellow;
            else if (speed < 32.6) return Brushes.DarkKhaki;
            else if (speed < 41.4) return Brushes.Red;
            else if (speed < 51) return Brushes.MediumVioletRed;
            else return Brushes.Violet;
        }
        /// <summary>
        /// 缩放具体实现
        /// </summary>
        private void DowheelZoom(TransformGroup group, Point point, double delta)
        {
            var pointToContent = group.Inverse.Transform(point);
            var transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX <= 1 && delta < 0) return;
            if (transform.ScaleX + delta < 0.1) return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            scale = transform.ScaleY;
            var transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
            if (btnPlay.IsEnabled != false)
            {
                canvasShow.Children.Clear();
                drawTyphoon();
                drawMOSTTyphoon();
            }
        }
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        /// <summary>  
        /// 从bitmap转换成ImageSource  
        /// </summary>   
       public static ImageSource BmpToImg(System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
                throw new System.ComponentModel.Win32Exception();
            return imgsource;
        }
        #endregion

        #region Events
        /// <summary>
        /// 播放动画
        /// </summary>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = false;
            canvasShow.Children.Clear();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(400 / sliderPlay.Value);   //间隔1秒
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            Point ptemp = new Point();
            ptemp.X = (cTyphoon.points[0].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            ptemp.Y = canvasShow.Height - (cTyphoon.points[0].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            Ellipse tp = new Ellipse();
            tp.Height = 8;
            tp.Width = 8;
            tp.Fill = typhSpeedColor(cTyphoon.points[0].aveSpeed);
            tp.Stroke = Brushes.Black;
            Canvas.SetLeft(tp, ptemp.X - 4);
            Canvas.SetTop(tp, ptemp.Y - 4);
            Canvas.SetZIndex(tp, 2);
            canvasShow.Children.Add(tp);
        }
        /// <summary>
        /// //显示台风动画，计时器实现
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = TimeSpan.FromMilliseconds(400 / sliderPlay.Value);   //间隔1秒
            tm++;
            Point ptemp = new Point();
            ptemp.X = (cTyphoon.points[tm].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            ptemp.Y = canvasShow.Height - (cTyphoon.points[tm].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            Ellipse tp = new Ellipse();
            tp.Height = 8;
            tp.Width = 8;
            tp.Fill = typhSpeedColor(cTyphoon.points[tm].aveSpeed);
            tp.Stroke = Brushes.Black;
            Canvas.SetLeft(tp, ptemp.X - 4);
            Canvas.SetTop(tp, ptemp.Y - 4);
            Canvas.SetZIndex(tp, 2);
            canvasShow.Children.Add(tp);
            Point c = new Point();
            c.Y = canvasShow.Height - (cTyphoon.points[tm - 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            c.X = (cTyphoon.points[tm - 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            Line l = new Line();
            l.Stroke = Brushes.Black;
            l.X1 = ptemp.X; l.X2 = c.X; l.Y1 = ptemp.Y; l.Y2 = c.Y;
            Canvas.SetZIndex(l, 1);
            canvasShow.Children.Add(l);
            if (tm >= cTyphoon.POINTSNUM - 1) { timer.Stop(); tm = 0; btnPlay.IsEnabled = true; }
        }
        /// <summary>
        /// 显示或关闭图例
        /// </summary>
        private void lblLegend_Click(object sender, RoutedEventArgs e)
        {
            if (ImgLegend.Visibility == Visibility.Hidden)
                ImgLegend.Visibility = Visibility.Visible;
            else ImgLegend.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 显示或关闭台风列表
        /// </summary>
        private void lstTyhoons_Click(object sender, RoutedEventArgs e)
        {
            if (lstboxTyphoons.Visibility == Visibility.Hidden)
                lstboxTyphoons.Visibility = Visibility.Visible;
            else lstboxTyphoons.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 显示或关闭台风状态栏
        /// </summary>
        private void lblShowState_Click(object sender, RoutedEventArgs e)
        {
            if (lstStates.Visibility == Visibility.Hidden)
            {
                lstStates.Visibility = Visibility.Visible;
                btnShowState.Content = "收起";
            }
            else
            {
                lstStates.Visibility = Visibility.Hidden;
                btnShowState.Content = "展开";
            }
        }
        /// <summary>
        /// 切换底图
        /// </summary>
        private void lblMap_Click(object sender, RoutedEventArgs e)
        {
            if (backPicIndex == 0)
            {
                backPicIndex = 1;
                canvasShow.Background = new ImageBrush(BmpToImg(TyphoonClient.Properties.Resources.SatMap));
                btnBackPic.Content = "线路图";
            }
            else if (backPicIndex == 1)
            {
                backPicIndex = 0;
                canvasShow.Background = new ImageBrush(BmpToImg(TyphoonClient.Properties.Resources.TyphoonMap));
                btnBackPic.Content = "卫星图";
            }
        }
        /// <summary>
        /// //切换台风
        /// </summary>
        private void lstTyphoons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                this.btnPlay.IsEnabled = true;
            }
            lstboxTyphoons.Visibility = Visibility.Hidden;
            if (lstboxTyphoons.SelectedIndex == -1)
                return;
            cTyphoon = typhoonList[lstboxTyphoons.SelectedIndex];
            canvasShow.Children.Clear();
            typhoonInitial();
        }
        /// <summary>
        /// //前往专业查询
        /// </summary>
        private void BtnToPro_Click(object sender, RoutedEventArgs e)
        {
            formParent.Children[0].Visibility = Visibility.Hidden;
            formParent.Children[1].Visibility = Visibility.Visible;
        }
        /// <summary>
        /// //实现图幅的放大缩小
        /// </summary>
        private void ContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
                return;
            var point = e.GetPosition(img);
            var group = BackGrid.FindResource("Imageview") as TransformGroup;
            var delta = e.Delta * 0.001;
            DowheelZoom(group, point, delta);
        }
        /// <summary>
        /// //实现图幅的漫游
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point P = e.GetPosition(canvasShow);
            double x = P.X / canvasShow.Width * 70 + 100;
            double y = 50 - P.Y / canvasShow.Height * 50;
            lblGeoPos.Content = string.Format("经度（{0}）    纬度({1})", x.ToString("0.0"), y.ToString("0.0"));
            int cur = 0;
            for (int i = 0; i < cTyphoon.POINTSNUM; i++)
            {
                if (Math.Abs(x - cTyphoon.points[i].longitude) < 0.5 / Math.Sqrt(scale) && Math.Abs(y - cTyphoon.points[i].latitude) < 0.5 / Math.Sqrt(scale))
                {
                    canvasShow.Cursor = Cursors.Hand;
                    cur = 1;
                }
            }
            if (cur == 0)
                canvasShow.Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// //按下后开始漫游
        /// </summary>
        private void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point P = e.GetPosition(canvasShow);
            double x = P.X / canvasShow.Width * 70 + 100;
            double y = 50 - P.Y / canvasShow.Height * 50;
            var backMap = sender as ContentControl;
            if (backMap == null)
                return;
            backMap.CaptureMouse();
            control.MouseMove += new MouseEventHandler(ContentControl_MouseMove);
            mouseXY = e.GetPosition(backMap);
        }
        /// <summary>
        /// //鼠标按起后停止漫游
        /// </summary>
        private void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var backMap = sender as ContentControl;
            if (backMap == null)
                return;
            backMap.ReleaseMouseCapture();
            control.MouseMove -= new MouseEventHandler(ContentControl_MouseMove);
        }
        /// <summary>
        /// //实现漫游
        /// </summary>
        private void ContentControl_MouseMove(object sender, MouseEventArgs e)
        {
            var backMap = sender as ContentControl;
            if (backMap == null)
                return;
            Domousemove(backMap, e);
        }
        /// <summary>
        /// //移动,根据X.Y的值来移动。并把当前鼠标位置赋值给mouseXY.
        /// </summary>
        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            var group = BackGrid.FindResource("Imageview") as TransformGroup;
            var transform = group.Children[1] as TranslateTransform;
            var transform1 = group.Children[0] as TranslateTransform;
            var position = e.GetPosition(img);
            transform.X -= mouseXY.X - position.X;
            transform.Y -= mouseXY.Y - position.Y;
            if (scale < 1.1)
            {
                transform.X = 0; transform.Y = 0;
                return;
            }
            if ((700 - transform.X) >= 700 * scale && transform.Y > 0) { transform.X = (700 - 700 * scale); transform.Y = 0; return; }
            if ((600 - transform.Y) >= 600 * scale && transform.X > 0) { transform.X = 0; transform.Y = (600 - 600 * scale); return; }
            if (transform.X > 0 && transform.Y <= 0) { transform.X = 0; return; }
            if (transform.Y > 0 && transform.X <= 0) { transform.Y = 0; return; }
            if (transform.X > 0 && transform.Y > 0) { transform.X = 0; transform.Y = 0; return; }
            if ((700 - transform.X) >= 700 * scale && (600 - transform.Y) < 600 * scale) { transform.X = (700 - 700 * scale); return; }
            if ((600 - transform.Y) >= 600 * scale && (700 - transform.X) < 700 * scale) { transform.Y = (600 - 600 * scale); return; }
            if ((700 - transform.X) >= 700 * scale && (600 - transform.Y) >= 600 * scale) { transform.X = (700 - 700 * scale); transform.Y = (600 - 600 * scale); return; }
            mouseXY = position;
        }
        #endregion
    }
}