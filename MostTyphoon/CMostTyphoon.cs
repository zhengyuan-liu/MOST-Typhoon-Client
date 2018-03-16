using System;
using System.IO;
using System.Collections.Generic;

namespace TyphoonClient.MostTyphoon
{
    /// <summary>
    /// 台风路径信息头
    /// </summary>
    public class CMostTyphoonHeader
    {
        #region Members
        /// <summary>
        /// 编号
        /// </summary>
        public int cycloneID;
        /// <summary>
        /// 终结记录
        /// </summary>
        public byte terminatorID;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public byte interval = 6;
        /// <summary>
        /// 台风名称
        /// </summary>
        public string typhName;
        /// <summary>
        /// 台风路径点数目
        /// </summary>
        public int pointsNum = 0;
        #endregion
        
        #region Method
        /// <summary>
        /// 读取台风路径信息头
        /// </summary>
        public bool ReadTyphoonHeader(StreamReader sr)
        {
            string msg = sr.ReadLine();
            if (msg == null)
                return false;
            string[] headers = msg.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);   //分割记录
            pointsNum = Convert.ToInt32(headers[2]);    //依次得到台风点数量、编号、终结记录、时间间隔、名称
            cycloneID = Convert.ToInt32(headers[3]);
            terminatorID = Convert.ToByte(headers[5]);
            interval = Convert.ToByte(headers[6]);
            typhName = headers[7];
            return true;
        }
        /// <summary>
        /// 台风路径数据信息头信息，用来往数据库中存储
        /// </summary>
        public string GetHeaderMsg()
        {
            string msg = string.Empty;
            msg += cycloneID.ToString() + ",";      //记录所有成员信息
            msg += pointsNum.ToString() + ",";
            msg += terminatorID.ToString() + ",";
            msg += interval.ToString() + ",";
            msg += "\"" + typhName.ToString() + "\",";
            return msg;
        }
        /// <summary>
        /// 设置台风路径数据信息头信息
        /// </summary>
        public void SetHeaderMsg(int cycloneID,byte terminatorID,byte interval,string typhName, int pointsNum)
        {
            this.cycloneID = cycloneID;
            this.terminatorID = terminatorID;
            this.interval = interval;
            this.typhName = typhName;
            this.pointsNum = pointsNum;
        }
        #endregion
    }

    /// <summary>
    /// 台风路径点
    /// </summary>
    public class CMostTyphoonPoint
    {
        #region Members
        /// <summary>
        /// 该点位发生时间
        /// </summary>
        public DateTime recordTime;
        /// <summary>
        /// 强度
        /// </summary>
        public byte strength;
        /// <summary>
        /// 中心气压值
        /// </summary>
        public int centerPress = -1;
        /// <summary>
        /// 平均速度
        /// </summary>
        public int aveSpeed;
        /// <summary>
        /// 纬度
        /// </summary>
        public double latitude;
        /// <summary>
        /// 经度
        /// </summary>
        public double longitude;
        /// <summary>
        /// 纬度速度
        /// </summary>
        public double latSpeed;
        /// <summary>
        /// 经度速度
        /// </summary>
        public double longSpeed;
        /// <summary>
        /// 是否是多余的
        /// </summary>
        public bool isBurden;
        #endregion

        #region Methods
        /// <summary>
        /// 读取台风路径点信息
        /// </summary>
        public bool ReadTyphoonPoint(StreamReader sr)
        {
            string[] msgs = sr.ReadLine().Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);  //分割字符串
            msgs[0] = msgs[0].Insert(8, "-");    //依次得到台风点记录时间、强度、经纬度、中心气压、平均速度
            msgs[0] = msgs[0].Insert(6, "-");
            msgs[0] = msgs[0].Insert(4, "-");
            recordTime = Convert.ToDateTime(msgs[0]).AddHours(-8);
            strength = Convert.ToByte(msgs[1]);
            latitude = Convert.ToInt32(msgs[2]) / 10.0;
            longitude = Convert.ToInt32(msgs[3]) / 10.0;
            centerPress = Convert.ToInt32(msgs[4]);
            aveSpeed = Convert.ToInt32(msgs[5]);
            return true;
        }
        /// <summary>
        /// 台风路径点信息，用来往数据库中存储
        /// </summary>
        public string GetPointMsg()
        {
            string msg = string.Empty;
            msg += latitude.ToString() + ",";      //记录所有成员信息
            msg += longitude.ToString() + ",";
            msg += latSpeed.ToString() + ",";
            msg += longSpeed.ToString() + ",";
            msg += strength.ToString() + ",";
            msg += "\"" + recordTime.ToString("yyyy-MM-dd-HH") + "\",";
            msg += centerPress.ToString() + ",";
            msg += aveSpeed.ToString() + ",";
            msg += isBurden ? "1" : "0";
            return msg;
        }
        /// <summary>
        /// 设置台风路径点信息
        /// </summary>
        public void SetPointMsg(DateTime recordTime, int strength, int centerPress, int aveSpeed, double latitude, double longitude, double latSpeed, double longSpeed, int isBurden)
        {
            this.recordTime = recordTime;
            this.strength = (byte)strength;
            this.centerPress = centerPress;
            this.aveSpeed = aveSpeed;
            this.latitude = latitude;
            this.longitude = longitude;
            this.latSpeed = latSpeed;
            this.longSpeed = longSpeed;
            this.isBurden = isBurden == 1 ? true : false;
        }
        #endregion

        #region MOST
        /// <summary>
        /// 开始时间
        /// </summary>
        public int StartHour { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public int EndHour { get; set; }
        #endregion
    }

    /// <summary>
    /// 基于MOST模型的台风路径
    /// </summary>
    public class CMostTyphoon
    {
        #region Members
        /// <summary>
        /// 台风路径信息头
        /// </summary>
        public CMostTyphoonHeader header = new CMostTyphoonHeader();
        /// <summary>
        /// 台风路径点集合
        /// </summary>
        public List<CMostTyphoonPoint> points = new List<CMostTyphoonPoint>();
        #endregion

        #region Attributes
        /// <summary>
        /// //台风路径点数量
        /// </summary>
        public int POINTSNUM { get { return header.pointsNum; } }
        #endregion

        #region Methods
        /// <summary>
        /// //读取台风路径信息
        /// </summary>
        public bool ReadTyphoon(StreamReader sr)
        {
            if (!header.ReadTyphoonHeader(sr))       //读取台风路径数据信息头
                return false;
            int i = 0, n = header.pointsNum;
            for (i = 0; i < n; i++)         //依次读取台风路径点数据
            {
                CMostTyphoonPoint point = new CMostTyphoonPoint();
                point.isBurden = false;
                if (!point.ReadTyphoonPoint(sr))
                    return false;
                else
                    points.Add(point);
            }
            CountPointSpeed();
            InitMostModel();
            return true;
        }
        /// <summary>
        /// //计算台风路径点速度
        /// </summary>
        public bool CountPointSpeed()
        {
            int i = 0, n = points.Count;
            List<double> lats = new List<double>(), longs = new List<double>(), latspeeds, longspeeds;
            for (i = 0; i < n; i++)                 //将台风路径数据点经纬度信息提取出来
            {
                lats.Add(points[i].latitude);
                longs.Add(points[i].longitude);
            }
            latspeeds = CNumDiff.Diff5Point(lats, header.interval);              //利用5点法计算速度并保存
            longspeeds = CNumDiff.Diff5Point(longs, header.interval);
            for (i = 0; i < n; i++)                //设置台风路径点速度
            {
                points[i].latSpeed = latspeeds[i];
                points[i].longSpeed = longspeeds[i];
            }
            return true;
        }
        /// <summary>
        /// 返回MOST台风路径点数据，用于往数据库中存储
        /// </summary>
        public string GetPointMsg(int i)
        {
            return header.GetHeaderMsg() + points[i].GetPointMsg();
        }
        /// <summary>
        /// 设置台风路径数据信息头信息
        /// </summary>
        public void SetHeaderMsg(int cycloneID, int terminatorID, int interval, string typhName, int pointsNum)
        {
            header.SetHeaderMsg(cycloneID, (byte)terminatorID, (byte)interval, typhName, pointsNum);
        }
        #endregion

        #region MOSTMembers
        /// <summary>
        /// 台风MOST模型轨迹点集合，为点对的形式，便于画
        /// </summary>
        public List<CMostTyphoonPoint> mostPoints = new List<CMostTyphoonPoint>();
        /// <summary>
        /// 距离阈值,50km
        /// </summary>
        const double DistanceThreshold = 50000;
        /// <summary>
        /// 速度阈值,10km/h
        /// </summary>
        const double SpeedThreshold = 10000;
        /// <summary>
        /// 角度阈值,5度,弧度制
        /// </summary>
        const double AngleThreshold = 5 * Math.PI / 180;
        #endregion

        #region MOSTConstructor
        /// <summary>
        /// MOST模型台风构造函数
        /// </summary>
        internal void InitMostModel()
        {
            mostPoints.Clear();
            int count = POINTSNUM;  //轨迹总点数
            int hour = 0;
            CMostTyphoonPoint startpoint = points[0];
            startpoint.StartHour = hour;
            mostPoints.Add(startpoint);
            for (int j = 1; j < count; j++)
            {
                hour = j * 6;
                double x0 = ETCProject.Latitude2X(mostPoints[mostPoints.Count - 1].latitude);
                double y0 = ETCProject.Longitude2Y(mostPoints[mostPoints.Count - 1].longitude);
                double vx0 = ETCProject.LatSpeed2Vx(mostPoints[mostPoints.Count - 1].latSpeed, points[j].latitude);
                double vy0 = ETCProject.LngSpeed2Vy(mostPoints[mostPoints.Count - 1].longSpeed, points[j].latitude);
                double x_predict = x0 + vx0 * (hour - mostPoints[mostPoints.Count - 1].StartHour);  //预测x
                double y_predict = y0 + vy0 * (hour - mostPoints[mostPoints.Count - 1].StartHour);  //预测y
                double x1 = ETCProject.Latitude2X(points[j].latitude);    //真实x
                double y1 = ETCProject.Longitude2Y(points[j].longitude);  //真实y
                double vx1 = ETCProject.LatSpeed2Vx(points[j].latSpeed, points[j].latitude);   //真实vx
                double vy1 = ETCProject.LngSpeed2Vy(points[j].longSpeed, points[j].latitude);  //真实vy
                if (GeometryMethod.EuclideanDistance(x_predict, y_predict, x1, y1) > DistanceThreshold)  //大于距离阈值
                {
                    mostPoints[mostPoints.Count - 1].EndHour = hour;
                    CMostTyphoonPoint endpoint = new CMostTyphoonPoint()
                    {
                        aveSpeed = mostPoints[mostPoints.Count - 1].aveSpeed,
                        latitude = ETCProject.X2Latitude(x_predict),
                        longitude = ETCProject.Y2Longitude(y_predict)
                    };
                    mostPoints.Add(endpoint);
                    CMostTyphoonPoint newpoint = points[j];
                    newpoint.StartHour = hour;
                    mostPoints.Add(newpoint);
                }
                else if (GeometryMethod.Speed(vx0, vy0) - GeometryMethod.Speed(vx1, vy1) > SpeedThreshold)  //大于速度阈值
                {
                    mostPoints[mostPoints.Count - 1].EndHour = hour;
                    CMostTyphoonPoint endpoint = new CMostTyphoonPoint()
                    {
                        aveSpeed = mostPoints[mostPoints.Count - 1].aveSpeed,
                        latitude = ETCProject.X2Latitude(x_predict),
                        longitude = ETCProject.Y2Longitude(y_predict)
                    };
                    mostPoints.Add(endpoint);
                    CMostTyphoonPoint newpoint = points[j];
                    newpoint.StartHour = hour;
                    mostPoints.Add(newpoint);
                }
                else if (GeometryMethod.VelocityDirection(vx0, vy0) - GeometryMethod.VelocityDirection(vx1, vy1) > AngleThreshold)  //大于角度阈值
                {
                    mostPoints[mostPoints.Count - 1].EndHour = hour;
                    CMostTyphoonPoint endpoint = new CMostTyphoonPoint()
                    {
                        aveSpeed = mostPoints[mostPoints.Count - 1].aveSpeed,
                        latitude = ETCProject.X2Latitude(x_predict),
                        longitude = ETCProject.Y2Longitude(y_predict)
                    };
                    mostPoints.Add(endpoint);
                    CMostTyphoonPoint newpoint = points[j];
                    newpoint.StartHour = hour;
                    mostPoints.Add(newpoint);
                }
                else if (j == count - 1)  //是最后一个点
                {
                    mostPoints[mostPoints.Count - 1].EndHour = hour;
                    CMostTyphoonPoint endpoint = new CMostTyphoonPoint()
                    {
                        aveSpeed = mostPoints[mostPoints.Count - 1].aveSpeed,
                        latitude = ETCProject.X2Latitude(x_predict),
                        longitude = ETCProject.Y2Longitude(y_predict)
                    };
                    mostPoints.Add(endpoint);
                    CMostTyphoonPoint newpoint = points[j];
                    newpoint.StartHour = hour;
                    mostPoints.Add(newpoint);
                }
            }
        }
        #endregion

        #region MOSTMethods
        /// <summary>
        /// 全时域查询
        /// </summary>
        public CMostTyphoonPoint FullTimeDomainInquiry(DateTime time)
        {
            DateTime startTime = points[0].recordTime;
            TimeSpan span = time - startTime;
            double totalHour = (points[points.Count - 1].recordTime - startTime).TotalHours;
            double hour = span.TotalHours;
            if (hour < 0 || hour >= totalHour)
                return null;
            CMostTyphoonPoint requiredPoint = new CMostTyphoonPoint();
            for (int i = 0; i < mostPoints.Count; i += 2)
            {
                if (hour >= mostPoints[i].StartHour && hour < mostPoints[i].EndHour)
                {
                    double x0 = ETCProject.Latitude2X(mostPoints[i].latitude);
                    double y0 = ETCProject.Longitude2Y(mostPoints[i].longitude);
                    double vx0 = ETCProject.LatSpeed2Vx(mostPoints[i].latSpeed, mostPoints[i].latitude);
                    double vy0 = ETCProject.LngSpeed2Vy(mostPoints[i].longSpeed, mostPoints[i].latitude);
                    double x_predict = x0 + vx0 * (hour - mostPoints[i].StartHour);  //预测x
                    double y_predict = y0 + vy0 * (hour - mostPoints[i].StartHour);  //预测y
                    requiredPoint.aveSpeed = mostPoints[i].aveSpeed;
                    requiredPoint.latitude = ETCProject.X2Latitude(x_predict);
                    requiredPoint.longitude = ETCProject.Y2Longitude(y_predict);
                    break;
                }
            }
            return requiredPoint;
        }
        /// <summary>
        /// 短期预测
        /// </summary>
        public CMostTyphoonPoint ShortTimePredict(int hour)
        {
            CMostTyphoonPoint endPoint = points[POINTSNUM - 1];
            double x0 = ETCProject.Latitude2X(endPoint.latitude);
            double y0 = ETCProject.Longitude2Y(endPoint.longitude);
            double vx0 = ETCProject.LatSpeed2Vx(endPoint.latSpeed, endPoint.latitude);
            double vy0 = ETCProject.LngSpeed2Vy(endPoint.longSpeed, endPoint.latitude);
            double x_predict = x0 + vx0 * hour;  //预测x
            double y_predict = y0 + vy0 * hour;  //预测y
            CMostTyphoonPoint requiredPoint = new CMostTyphoonPoint()
            {
                aveSpeed = endPoint.aveSpeed,
                latitude = ETCProject.X2Latitude(x_predict),
                longitude = ETCProject.Y2Longitude(y_predict)
            };
            return requiredPoint;
        }
        #endregion
    }
}