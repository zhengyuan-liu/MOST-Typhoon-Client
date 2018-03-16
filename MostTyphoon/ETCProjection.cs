using System;

namespace TyphoonClient.MostTyphoon
{
    /// <summary>
    /// 等距切圆柱投影
    /// Equidistant Tangent Cylinder Projection
    /// </summary>
    class ETCProject
    {
        #region StaticMembers
        /// <summary>
        /// WGS84椭球体参数
        /// </summary>
        static double a = 6378137, b = 6356752;
        static double alpha = a;
        static double e2 = (a * a - b * b) / (a * a);
        static double e = Math.Sqrt(e2);
        /// <summary>
        /// 中央经线
        /// </summary>
        static double center = 140;
        #endregion

        #region StaticMethods
        public static double Latitude2X(double latitude)
        {
            double laRad = Deg2Rad(latitude);
            double x = 0.5 * a * (1 - e2) / e
                   * (e * Math.Sin(laRad) / (1 - e2 * Math.Sin(laRad) * Math.Sin(laRad))
                   + Math.Log((1 + e * Math.Sin(laRad)) / Math.Sqrt(1 - e2 * Math.Sin(laRad) * Math.Sin(laRad)), Math.E));
            return x;
        }
        public static double Longitude2Y(double longitude)
        {
            double lamda = Deg2Rad(longitude - center);  //弧度制经差
            double y = alpha * lamda;
            return y;
        }
        public static double X2Latitude(double x)
        {
            double laRad = dichotomy(x, 10e-6);
            double latitude = Rad2Deg(laRad);
            return latitude;
        }
        public static double Y2Longitude(double y)
        {
            double longitude = Rad2Deg(y / alpha) + center;
            return longitude;
        }
        public static double LatSpeed2Vx(double latSpeed, double latitude)
        {
            double latRadSpeed = latSpeed / 180 * Math.PI;
            double laRad = latitude / 180 * Math.PI;
            double vx = latRadSpeed * a * (1 - e2) / ((1 - e2 * Math.Sin(laRad) * Math.Sin(laRad)) * Math.Sqrt(1 - e2 * Math.Sin(laRad) * Math.Sin(laRad)));
            return vx;
        }
        public static double LngSpeed2Vy(double lngSpeed, double latitude)
        {
            double lngRadSpeed = lngSpeed / 180 * Math.PI;
            double laRad = latitude / 180 * Math.PI;
            double vy = lngRadSpeed * a * Math.Cos(laRad) / Math.Sqrt(1 - e2 * Math.Sin(laRad) * Math.Sin(laRad));
            return vy;
        }
        static double Deg2Rad(double degree)
        {
            return degree / 180 * Math.PI;
        }
        static double Rad2Deg(double radian)
        {
            return radian * 180 / Math.PI;
        }
        /// <summary>
        /// 二分法解方程
        /// </summary>
        static double dichotomy(double y0, double error)
        {
            double lower = 0;            //最小纬度0N
            double upper = Math.PI / 2;  //最大纬度90N
            double mid = -1;

            while ((upper - lower) > error)
            {
                mid = (lower + upper) / 2;
                if (F(mid, y0) * F(lower, y0) < 0)
                    upper = mid;
                else if (F(mid, y0) * F(upper, y0) < 0)
                    lower = mid;
                else
                    return mid;
            }
            return mid;
        }
        /// <summary>
        /// x为弧度制
        /// </summary>
        static double F(double x,double y0)
        {
            double phi = 0.5 * a * (1 - e2) / e
                   * (e * Math.Sin(x) / (1 - e2 * Math.Sin(x) * Math.Sin(x))
                   + Math.Log((1 + e * Math.Sin(x)) / Math.Sqrt(1 - e2 * Math.Sin(x) * Math.Sin(x)), Math.E)) - y0;
            return phi;
        }
        #endregion
    }
}
