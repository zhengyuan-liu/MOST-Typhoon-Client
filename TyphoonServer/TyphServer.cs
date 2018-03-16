using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using TyphoonClient.MostTyphoon;

namespace TyphoonClient.TyphoonServer
{
    /// <summary>
    /// //台风服务器类，实现数据和管理和检索
    /// </summary>
    class TyphServer
    {
        #region Static Methods
        /// <summary>
        /// //根据台风路径请求获取台风路径数据
        /// </summary>
        public static List<CMostTyphoon> GetTyphoonData(TyphoonRequest req)
        {
            DataSet dtset = new DataSet();               //从数据库中得到检索数据
            MySqlDataAdapter mydbadapter = CMyTyphoonSQL.GetTyphoon(req.SQL);
            mydbadapter.Fill(dtset, "result");
            DataTable dt = dtset.Tables["result"];
            List<CMostTyphoon> typhoons = new List<CMostTyphoon>();     //生成台风数据集合
            int i = 0, n = dt.Rows.Count;
            if (n == 0)
                return null;
            CMostTyphoon typh = new CMostTyphoon();
            int id = (int)dt.Rows[0]["ID"];
            SetTyphoonHeader(typh, dt.Rows[0]);        //设置第一条台风数据头
            for (i = 0; i < n; i++)
            {
                if (id != (int)dt.Rows[i]["ID"])         //如果id号不一样，则说明为另一条台风数据
                {
                    id = (int)dt.Rows[i]["ID"];
                    typh.InitMostModel();
                    typhoons.Add(typh);
                    typh = new CMostTyphoon();
                    SetTyphoonHeader(typh, dt.Rows[i]);
                    typh.points.Add(GetTyphoonPoint(dt.Rows[i]));
                }
                else                          //否则读取台风路径点对象
                    typh.points.Add(GetTyphoonPoint(dt.Rows[i]));
            }
            typhoons.Add(typh);
            typh.InitMostModel();
            dtset.Dispose();
            return typhoons;
        }
        /// <summary>
        /// //设置台风信息头
        /// </summary>
        public static void SetTyphoonHeader(CMostTyphoon typh, DataRow row)
        {
            typh.SetHeaderMsg((int)row["ID"], (int)row["END"], (int)row["INTERVAL"], (string)row["NAME"], (int)row["POINTNUM"]);         
        }
        /// <summary>
        /// //根据数据库返回的数据行来生成台风数据点对象
        /// </summary>
        public static CMostTyphoonPoint GetTyphoonPoint(DataRow row)
        {
            CMostTyphoonPoint tempPoint = new CMostTyphoonPoint();
            tempPoint.SetPointMsg(Convert.ToDateTime(row["RECORDTIME"]), (int)row["STRENGTH"], (int)row["CENTERPRESS"], (int)row["AVESPEED"], (double)row["LATITUDE"], (double)row["LONGITUDE"], (double)row["LATSPEED"], (double)row["LONGSPEED"],(int)row["ISBURDEN"]);
            return tempPoint;
        } 
        #endregion
    }
}
