using System.Collections.Generic;

namespace TyphoonClient.MostTyphoon
{
    /// <summary>
    /// //计算速度的微分计算器
    /// </summary>
    public class CNumDiff
    {
        #region Static Method
        /// <summary>
        /// 五点法计算数值微分
        /// </summary>
        public static List<double> Diff5Point(List<double> f, double h)    //五点法计算数值微分，f为离散的函数值，h为两点之间的间距
        {
            List<double> diff = new List<double>();               //存储函数微分值
            int i = 2, n = f.Count;
            if (n >= 5)
            {
                double temp = 0;                       //临时变量，用来计算函数微分值
                temp = (-25 * f[0] + 48 * f[1] - 36 * f[2] + 16 * f[3] - 3 * f[4]) / (12 * h);
                diff.Add(temp);
                temp = (-3 * f[0] - 10 * f[1] + 18 * f[2] - 6 * f[3] + f[4]) / (12 * h);
                diff.Add(temp);
                for (; i < n - 2; i++)
                {
                    temp = (f[i - 2] - 8 * f[i - 1] + 8 * f[i + 1] - f[i + 2]) / (12 * h);               //依次计算每一点的函数微分值
                    diff.Add(temp);
                }
                temp = (-f[i - 3] + 6 * f[i - 2] - 18 * f[i - 1] + 10 * f[i] + 3 * f[i + 1]) / (12 * h);
                diff.Add(temp);
                temp = (3 * f[i - 3] - 16 * f[i - 2] + 36 * f[i - 1] - 48 * f[i] + 25 * f[i + 1]) / (12 * h);
                diff.Add(temp);
            }
            return diff;
        }
        #endregion
    }
}