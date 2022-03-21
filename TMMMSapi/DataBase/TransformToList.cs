using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TMMMSapi.Class;

namespace TMMMSapi.DataBase
{
    public class TransformToList
    {
        private static TransformToList instance = null;

        public static TransformToList Instance
        {
            get
            {
                return instance ?? new TransformToList();
            }
        }

        private TransformToList()
        {
            instance = this;
        }

        public List<TMMMSEmployee> GetTMMMSEmployees(DataSet ds)
        {
            List<TMMMSEmployee> List = new List<TMMMSEmployee>();

            DataTable dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                List.Add(new TMMMSEmployee()
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Dept = row["Dept"].ToString(),
                    EmployeeId = row["EmployeeId"].ToString(),
                    IS_DELETE = row["IS_DELETE"].ToString(),
                });
            }

            return List;
        }
    }
}