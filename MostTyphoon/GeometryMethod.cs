using System;

namespace TyphoonClient.MostTyphoon
{
    /// <summary>
    /// 空间方法计算静态类
    /// </summary>
    static class GeometryMethod
    {
        #region StaticMethods
        /// <summary>
        /// 求两点间的欧氏距离
        /// </summary>
        public static double EuclideanDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        /// <summary>
        /// 求速率
        /// </summary>
        public static double Speed(double vx, double vy)
        {
            return Math.Sqrt(vx * vx + vy * vy);
        }
        /// <summary>
        /// /速度方向,返回0-2PI的一个值
        /// </summary>
        public static double VelocityDirection(double vx, double vy)  
        {
            double angle = 0;
            if (vx == 0 && vy > 0)
                return 0;
            else if (vx == 0 && vy < 0)
                return Math.PI;
            if (vx > 0 && vy > 0)  //速度方向在第一象限
                angle = Math.Atan(vy / vx);
            else if (vx < 0 && vy > 0)  //速度方向在第二象限
                angle = Math.Atan(vy / vx) + Math.PI;
            else if (vx < 0 && vy < 0)  //速度方向在第三象限          
                angle = Math.Atan(vy / vx) + Math.PI;
            else  //速度方向在第四象限
                angle = Math.Atan(vy / vx) + 2 * Math.PI;
            return angle;
        }
        #endregion
    }
}
