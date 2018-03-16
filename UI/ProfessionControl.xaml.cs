using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using TyphoonClient.MostTyphoon;
using TyphoonClient.TyphoonServer;

namespace TyphoonClient
{
    /// <summary>
    /// //SearchFace.xaml 的交互逻辑
    /// </summary>
    public partial class ProfessionControl : UserControl
    {
        #region Members
        /// <summary>
        /// //父界面
        /// </summary>
        private Grid mainform = null;
        /// <summary>
        /// //计时器
        /// </summary>
        private DispatcherTimer timer;
        /// <summary>
        /// //查询的台风列表
        /// </summary>
        private List<CMostTyphoon> ctyphoonlist = new List<CMostTyphoon>();
        /// <summary>
        /// //目前正在处理的台风对象
        /// </summary>
        private CMostTyphoon cTyphoon = new CMostTyphoon();
        /// <summary>
        /// 记录画布左上，右下代表的经纬度
        /// </summary>
        private double maxLat = 50, minLat = 3, maxLong = 180, minLong = 90;
        /// <summary>
        /// //播放动画控制tick
        /// </summary>
        private int tm = 0;
        /// <summary>
        /// //放大缩小倍数
        /// </summary>
        private double scale = 1;
        /// <summary>
        /// //记录折算的画布上的一个单位长度等于经纬度的几度
        /// </summary>
        private double multiple;
        /// <summary>
        /// //用来标记拉框矩形
        /// </summary>
        private Rectangle boundary;
        /// <summary>
        /// //拉框查询时用来标识鼠标原来点和现在点
        /// </summary>
        private Point oldpos = new Point(0, 0), nowpos = new Point(0, 0);
        /// <summary>
        /// //底图标志
        /// </summary>
        private byte backpic = 0;
        /// <summary>
        /// //记录鼠标点的位置
        /// </summary>
        private Point mouseXY;
        #endregion

        #region Constructors
        /// <summary>
        /// 初始化
        /// </summary>
        public ProfessionControl(Grid mainform)
        {
            InitializeComponent();
            ImageBrush bru1 = new ImageBrush(BmpToImg(Properties.Resources.TyphoonMap));
            bru1.TileMode = TileMode.Tile;
            bru1.Stretch = Stretch.Fill;
            canvasShow.Background = bru1;
            imgLegend.Source = BmpToImg(Properties.Resources.Legend);
            this.mainform = mainform;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 为一条台风赋值初始化
        /// </summary>
        private void typhoonInitial()
        {
            CMostTyphoonPoint pt = cTyphoon.points[cTyphoon.POINTSNUM - 1];
            lstBoxAttrs.Items.Clear();
            for (int i = cTyphoon.POINTSNUM - 1; i > -1; i--)//显示台风点位数据
            {
                ListBoxItem lbitem = new ListBoxItem();
                lbitem.Content = cTyphoon.points[i].recordTime.ToString("MM/dd/HH") + "      " + cTyphoon.points[i].latitude + "/" + cTyphoon.points[i].longitude + "\t" + cTyphoon.points[i].centerPress + "\t" + cTyphoon.points[i].aveSpeed;
                lstBoxAttrs.Items.Add(lbitem);
            }
            drawtyphoon();
            DrawMOSTTyphoon();
            lblStateLabel.Content = cTyphoon.header + "(编号：" + cTyphoon.header.cycloneID + ")    " + "已停编";
        }
        /// <summary>
        /// 画台风
        /// </summary>
        private void drawtyphoon()
        {
            Point a = new Point();
            a.X = (cTyphoon.points[0].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            a.Y = canvasShow.Height - (cTyphoon.points[0].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            for (int i = 1; i < cTyphoon.POINTSNUM; i++)
            {
                Ellipse tp = new Ellipse();
                tp.Height = 10 / scale;
                tp.Width = 10 / scale;
                tp.Fill = typhooncolor(cTyphoon.points[i - 1].aveSpeed);
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
                l.StrokeThickness = 2 / scale;
                l.X1 = a.X; l.X2 = c.X; l.Y1 = a.Y; l.Y2 = c.Y;
                Canvas.SetZIndex(l, 1);
                canvasShow.Children.Add(l);
                a.X = c.X; a.Y = c.Y;
            }
            Ellipse tpf = new Ellipse();
            tpf.Height = 10 / scale;
            tpf.Width = 10 / scale;
            tpf.Fill = typhooncolor(cTyphoon.points[cTyphoon.POINTSNUM - 1].aveSpeed);
            Canvas.SetLeft(tpf, a.X - 5 / scale);
            Canvas.SetTop(tpf, a.Y - 5 / scale);
            Canvas.SetZIndex(tpf, 2);
            tpf.Stroke = Brushes.Black;
            tpf.StrokeThickness = 1 / scale;
            canvasShow.Children.Add(tpf);
        }
        private void DrawMOSTTyphoon()
        {
            Point point0 = new Point();
            Point point1 = new Point();
            Point point2 = new Point();
            for (int i = 1; i < cTyphoon.mostPoints.Count; i += 2)
            {
                point0.X = (cTyphoon.mostPoints[i - 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                point0.Y = canvasShow.Height - (cTyphoon.mostPoints[i - 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                Ellipse tp = new Ellipse();
                tp.Height = 10 / scale;
                tp.Width = 10 / scale;
                tp.Fill = typhooncolor(cTyphoon.mostPoints[i - 1].aveSpeed);
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

                point2.Y = canvasShow.Height - (cTyphoon.mostPoints[i + 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                point2.X = (cTyphoon.mostPoints[i + 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
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
            if (pFTDQ != null)
            {
                point0 = new Point();
                point0.X = (pFTDQ.longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                point0.Y = canvasShow.Height - (pFTDQ.latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                Ellipse tp = new Ellipse();
                tp.Height = 10 / scale;
                tp.Width = 10 / scale;
                tp.Fill = Brushes.Red;
                tp.Stroke = Brushes.Black;
                tp.StrokeThickness = 1 / scale;
                Canvas.SetLeft(tp, point0.X - 5 / scale);
                Canvas.SetTop(tp, point0.Y - 5 / scale);
                Canvas.SetZIndex(tp, 2);
                canvasShow.Children.Add(tp);
            }
        }
        /// <summary>
        /// 风级颜色函数
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        private SolidColorBrush typhooncolor(double speed)
        {
            if (speed <= 17.1) return Brushes.Aquamarine;
            else if (speed < 24.4) return Brushes.Yellow;
            else if (speed < 32.6) return Brushes.DarkKhaki;
            else if (speed < 41.4) return Brushes.Red;
            else if (speed < 51) return Brushes.MediumVioletRed;
            else return Brushes.Violet;
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
        /// <summary>
        /// 缩放具体实现
        /// </summary>
        /// <param name="group"></param>
        /// <param name="point"></param>
        /// <param name="delta"></param>
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
                drawtyphoon();
                DrawMOSTTyphoon();
            }
        }
        #endregion

        CMostTyphoonPoint pFTDQ = null;
        #region Events
        /// <summary>
        /// //响应搜索台风按钮的事件
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            canvasShow.Children.Clear();
            string reqMsg = MySearchControl.GetRequest();
            if (reqMsg == "typhoon Wrong")
            {
                MessageBox.Show("查询请求不正确");
                btnPlay.IsEnabled = false;
                return;
            }
            else if(reqMsg.Contains("search"))
            {
                string timeText = reqMsg.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries)[1];
                DateTime datetime = DateTime.Parse(timeText);
                if (cTyphoon.POINTSNUM != 0)
                {
                    typhoonInitial();
                    pFTDQ = cTyphoon.FullTimeDomainInquiry(datetime);
                    if (pFTDQ == null)
                        MessageBox.Show("查询过程出错", "全时域查询结果", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        MessageBox.Show(datetime.ToString("yyyy-MM-dd HH:mm:ss") + "，台风位于(" + pFTDQ.latitude.ToString("F4") + "," + pFTDQ.longitude.ToString("F4") + ")", "全时域查询结果", MessageBoxButton.OK, MessageBoxImage.Information);
                        Point point0 = new Point();
                        point0.X = (pFTDQ.longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
                        point0.Y = canvasShow.Height - (pFTDQ.latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
                        Ellipse tp = new Ellipse();
                        tp.Height = 10 / scale;
                        tp.Width = 10 / scale;
                        tp.Fill = Brushes.Red;
                        tp.Stroke = Brushes.Black;
                        tp.StrokeThickness = 1 / scale;
                        Canvas.SetLeft(tp, point0.X - 5 / scale);
                        Canvas.SetTop(tp, point0.Y - 5 / scale);
                        Canvas.SetZIndex(tp, 2);
                        canvasShow.Children.Add(tp);
                    }
                }
                return;
            }
            TyphoonRequest request = new TyphoonRequest(reqMsg);
            ctyphoonlist = TyphServer.GetTyphoonData(request);
            lstBoxTyphoons.Items.Clear();
            if (ctyphoonlist == null)
            {
                MessageBox.Show("没有符合查询条件的台风");
                btnPlay.IsEnabled = false;
                return;
            }
            for (int i = 0; i < ctyphoonlist.Count; i++)
            {
                Label temp = new Label();
                temp.Content = ctyphoonlist[i].header.typhName;
                lstBoxTyphoons.Items.Add(temp);
            }
            cTyphoon = ctyphoonlist[0];
            btnPlay.IsEnabled = true;
            typhoonInitial();
        }
        /// <summary>
        /// //用来进行拉框查询
        /// </summary>
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvasShow.Children.Remove(boundary);
            boundary = new Rectangle();
            oldpos = e.GetPosition(canvasShow);
        }
        /// <summary>
        /// //鼠标滑轮事件，得到坐标，放缩函数和滑轮指数，由于滑轮值变化较大所以*0.001.
        /// </summary>
        private void Back_MouseWheel(object sender, MouseWheelEventArgs e)
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
        /// 播放动画
        /// </summary>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = false;
            canvasShow.Children.Clear();
            BtnSearch.IsEnabled = false;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(400 / sliderPlay.Value);   //间隔1秒
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            Point ptemp = new Point();
            ptemp.X = (cTyphoon.points[0].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            ptemp.Y = canvasShow.Height - (cTyphoon.points[0].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            Ellipse tp = new Ellipse();
            tp.Height = 8;
            tp.Width = 8;
            tp.Fill = typhooncolor(cTyphoon.points[0].aveSpeed);
            tp.Stroke = Brushes.Black;
            Canvas.SetLeft(tp, ptemp.X - 4);
            Canvas.SetTop(tp, ptemp.Y - 4);
            Canvas.SetZIndex(tp, 2);
            canvasShow.Children.Add(tp);
        }
        /// <summary>
        /// //计时器，用来显示台风动画
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = TimeSpan.FromMilliseconds(400 / sliderPlay.Value);   //间隔1秒
            tm++;
            Point a = new Point();
            a.X = (cTyphoon.points[tm].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            a.Y = canvasShow.Height - (cTyphoon.points[tm].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            Ellipse tp = new Ellipse();
            tp.Height = 8;
            tp.Width = 8;
            tp.Fill = typhooncolor(cTyphoon.points[tm].aveSpeed);
            tp.Stroke = Brushes.Black;
            Canvas.SetLeft(tp, a.X - 4);
            Canvas.SetTop(tp, a.Y - 4);
            Canvas.SetZIndex(tp, 2);
            canvasShow.Children.Add(tp);
            Point c = new Point();
            c.Y = canvasShow.Height - (cTyphoon.points[tm - 1].latitude - minLat) / (maxLat - minLat) * canvasShow.Height;
            c.X = (cTyphoon.points[tm - 1].longitude - minLong) / (maxLong - minLong) * canvasShow.Width;
            Line l = new Line();
            l.Stroke = Brushes.Black;
            l.X1 = a.X; l.X2 = c.X; l.Y1 = a.Y; l.Y2 = c.Y;
            Canvas.SetZIndex(l, 1);
            canvasShow.Children.Add(l);
            if (tm >= cTyphoon.POINTSNUM - 1) { timer.Stop(); BtnSearch.IsEnabled = true; tm = 0; btnPlay.IsEnabled = true; }
        }
        /// <summary>
        /// 显示或关闭图例
        /// </summary>
        private void lblLegend_Click(object sender, RoutedEventArgs e)
        {
            if (imgLegend.Visibility == Visibility.Hidden)
                imgLegend.Visibility = Visibility.Visible;
            else imgLegend.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 显示或关闭台风列表
        /// </summary>
        private void lstTyphoons_Click(object sender, RoutedEventArgs e)
        {
            if (lstBoxTyphoons.Visibility == Visibility.Hidden)
                lstBoxTyphoons.Visibility = Visibility.Visible;
            else lstBoxTyphoons.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 显示或关闭台风状态栏
        /// </summary>
        private void lblShowState_Click(object sender, RoutedEventArgs e)
        {
            if (lstBoxStates.Visibility == Visibility.Hidden)
            {
                lstBoxStates.Visibility = Visibility.Visible;
                btnShowState.Content = "收起";
            }
            else
            {
                lstBoxStates.Visibility = Visibility.Hidden;
                btnShowState.Content = "展开";
            }
        }
        /// <summary>
        /// 切换底图
        /// </summary>
        private void lblMap_Click(object sender, RoutedEventArgs e)
        {
            if (backpic == 0)
            {
                backpic = 1;
                canvasShow.Background = new ImageBrush(BmpToImg(TyphoonClient.Properties.Resources.SatMap));
                btnMap.Content = "线路图";
            }
            else if (backpic == 1)
            {
                backpic = 0;
                canvasShow.Background = new ImageBrush(BmpToImg(TyphoonClient.Properties.Resources.TyphoonMap));
                btnMap.Content = "卫星图";
            }
        }
        /// <summary>
        /// 所选台风更改
        /// </summary>
        private void lstTyphoons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pFTDQ = null;
            if (timer != null)
            {
                timer.Stop();
                BtnSearch.IsEnabled = true;
                tm = 0;
                this.btnPlay.IsEnabled = true;
            }
            lstBoxTyphoons.Visibility = Visibility.Hidden;
            if (lstBoxTyphoons.SelectedIndex == -1)
                return;
            cTyphoon = ctyphoonlist[lstBoxTyphoons.SelectedIndex];
            canvasShow.Children.Clear();
            typhoonInitial();
        }
        /// <summary>
        /// //返回主界面
        /// </summary>
        private void BtnGoBack_Click(object sender, RoutedEventArgs e)
        {
            mainform.Children[1].Visibility = Visibility.Hidden;
            mainform.Children[0].Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 显示经纬度
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point P = e.GetPosition(canvasShow);
            double x = P.X / canvasShow.Width * 90 + 90;
            double y = 50 - P.Y / canvasShow.Height * 47;
            lblGeoPos.Content = string.Format("经度（{0}）    纬度({1})", x.ToString("0.0"), y.ToString("0.0"));
            double Height = e.GetPosition(canvasShow).Y - oldpos.Y;
            double Width = e.GetPosition(canvasShow).X - oldpos.X;
            if (e.RightButton == MouseButtonState.Pressed && Height > 0 && Width > 0)
            {
                nowpos = e.GetPosition(canvasShow);
                canvasShow.Children.Remove(boundary);
                boundary = new Rectangle();
                boundary.Height = Height;
                boundary.Width = Width;
                boundary.Stroke = Brushes.Red;
                boundary.StrokeThickness = 2;
                Canvas.SetLeft(boundary, oldpos.X);
                Canvas.SetTop(boundary, oldpos.Y);
                canvasShow.Children.Add(boundary);
                multiple = (maxLat - minLat) / canvasShow.Height;
                MySearchControl.txtTopLeftLong.Text = (oldpos.X / canvasShow.Width * 90 + 90).ToString("0.00");
                MySearchControl.txtTopLeftLat.Text = (50 - oldpos.Y / canvasShow.Height * 47).ToString("0.00");
                MySearchControl.txtDownRightLong.Text = (nowpos.X / canvasShow.Width * 90 + 90).ToString("0.00");
                MySearchControl.txtDownRightLat.Text = (50 - nowpos.Y / canvasShow.Height * 47).ToString("0.00");
            }
            int cur = 0;
            for (int i = 0; i < cTyphoon.POINTSNUM; i++)
                if (Math.Abs(x - cTyphoon.points[i].longitude) < 0.5 / Math.Sqrt(scale) && Math.Abs(y - cTyphoon.points[i].latitude) < 0.5 / Math.Sqrt(scale))
                {
                    canvasShow.Cursor = Cursors.Hand;
                    cur = 1;
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