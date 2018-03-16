using System.Collections.Generic;
using MySql.Data.MySqlClient;
using TyphoonClient.MostTyphoon;

namespace TyphoonClient.TyphoonServer
{
    /// <summary>
    /// 台风数据库类，用来实现与数据库的交互
    /// </summary>
    class CMyTyphoonSQL
    {
        #region Members
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        private static MySqlConnection mydbcon = new MySqlConnection(Properties.Resources.SQLDB);          //连接数据库
        /// <summary>
        /// 数据库命令对象
        /// </summary>
        private static MySqlCommand mydbcommand;
        #endregion

        #region Static Method
        /// <summary>
        /// 将某年的台风路径数据入库
        /// </summary>
        public static void SaveTyphoons(ref List<CMostTyphoon> typhoons, string filename)
        {
            int i = 0, typhNum = typhoons.Count, j = 0;
            mydbcon.Open();
            string mycommand = "CREATE TABLE `" + filename + "` (`ID` INT,`POINTNUM` INT,`END` INT,`INTERVAL` INT,`NAME` LONGTEXT,`LATITUDE` DOUBLE,`LONGITUDE` DOUBLE,`LATSPEED` DOUBLE,`LONGSPEED` DOUBLE,`STRENGTH` INT,`RECORDTIME` LONGTEXT,`CENTERPRESS` INT,`AVESPEED` INT,`ISBURDEN` INT)";
            mydbcommand = new MySqlCommand(mycommand, mydbcon);
            mydbcommand.ExecuteNonQuery();
            for (i = 0; i < typhNum; i++)
            {
                int pointsNum = typhoons[i].POINTSNUM;
                for (j = 0; j < pointsNum; j++)
                {
                    mycommand = "INSERT INTO `" + filename + "` VALUES(" + typhoons[i].GetPointMsg(j) + ")";
                    mydbcommand.CommandText = mycommand;
                    mydbcommand.ExecuteNonQuery();
                }
            }
            mydbcommand.Dispose();          //释放与数据库的链接
            mydbcon.Close();
            mydbcon.Dispose();
        }
        /// <summary>
        /// //返回数据查询适配器
        /// </summary>
        public static MySqlDataAdapter GetTyphoon(string SQL)
        {
            MySqlDataAdapter myadapter = new MySqlDataAdapter(SQL, mydbcon);   //根据sql语句生成数据库数据查询适配器
            return myadapter;
        }
        #endregion
    }
}
