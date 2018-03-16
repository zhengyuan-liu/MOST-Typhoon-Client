namespace TyphoonClient.TyphoonServer
{
    /// <summary>
    /// 台风路径数据请求
    /// </summary>
    class TyphoonRequest
    {
        #region Members
        /// <summary>
        /// 请求消息对应的SQL语句
        /// </summary>
        private string sql;
        #endregion

        #region Attribute
        /// <summary>
        /// 请求消息对应的SQL语句
        /// </summary>
        public string SQL { get { return sql; } }
        #endregion

        #region Constructors
        /// <summary>
        /// 根据客户端请求消息生成，并生成请求消息对应的SQL语句
        /// </summary>
        public TyphoonRequest(string requestData)
        {
            string[] reqs = requestData.Split(' ');          //分割请求
            string tablename = "`ch" + reqs[2] + "bst`";            //根据年份设置要请求的表
            if (reqs[1] == "Attribute")          //属性查询
            {
                if (reqs[3] == "ID")          //ID查询
                    sql = "SELECT* FROM " + tablename + " where ID=" + reqs[4];
                else if (reqs[3] == "NAME")         //名称查询
                    sql = "SELECT* FROM " + tablename + " where NAME=\"" + reqs[4] + "\"";
                else                       //强度查询
                    sql = "SELECT* FROM " + tablename + " where ID in (select ID from " + tablename + " where STRENGTH >=" + reqs[4] + ")";
            }
            else if (reqs[1] == "Time")              //时间查询
                sql = "SELECT* FROM " + tablename + " where ID in (select ID from " + tablename + " where RECORDTIME >= \"" + reqs[4] + "\" and RECORDTIME <= \"" + reqs[5] + "\")";
            else                                    //空间查询
                sql = "SELECT* FROM " + tablename + " where ID in (select ID from " + tablename + " where LATITUDE >=" + reqs[4] + " and LATITUDE <=" + reqs[6] + " and LONGITUDE >=" + reqs[5] + " and LONGITUDE <=" + reqs[7] + ")";
        }
        #endregion
    }
}
