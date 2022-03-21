using ADODB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace TMMMSapi.DataBase
{
    public class OracleDataBase
    {
        private static OracleDataBase instance = null;

        public static OracleDataBase Instance
        {
            get
            {
                return instance ?? new OracleDataBase();
            }
        }

        private OracleDataBase()
        {
            instance = this;
        }

        /// <summary>
        /// 傳入 SQL傳出 DataSet
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataSet ExecuteSQL(string strSQL)
        {
            Connection objCon = new Connection();
            Recordset objRec = new Recordset();
            object missing;
            string strCon;

            strCon =
                "PROVIDER=MSDAORA.1;PASSWORD=YYC1;USER ID=DBASRM;DATA SOURCE=yyc3;PERSIST SECUITY INFO=TURE";

            objCon.Open(strCon);

            //execute the SQL and return the recrodset of results
            objRec = objCon.Execute(strSQL, out missing, 0);

            // Create dataset and data adapter objects
            DataSet ds = new DataSet("Recordset");
            OleDbDataAdapter da = new OleDbDataAdapter();

            // Call data adapter's Fill method to fill data from ADO
            // Recordset to the dataset
            da.Fill(ds, objRec, "Recordset");

            return ds;
        }
    }
}