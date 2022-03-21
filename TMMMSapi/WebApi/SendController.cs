using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using TMMMSapi.Class;
using TMMMSapi.DataBase;

namespace TMMMSapi.WebApi
{
    public class SendController : ApiController
    {
        //GET api/<controller>
        public API_Result Get(string DestAddress, string SmsBody, string Dept, string User)
        {
            API_Result _API_Result = new API_Result();
            try
            {
                //檢查 手機號碼或訊息為空就不執行
                if (string.IsNullOrEmpty(SmsBody) || string.IsNullOrEmpty(DestAddress))
                {
                    _API_Result.ResultCode = "00020";
                    _API_Result.ResultText = "手機號碼或訊息為空";
                    return _API_Result;
                }

                //轉成 bytes
                byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(SmsBody);

                //編成 Base64 字串
                string b64SmsBody = Convert.ToBase64String(bytes);

                string Sysid = System.Web.Configuration.WebConfigurationManager.AppSettings["Sysid"];

                string SrcAddress = System.Web.Configuration.WebConfigurationManager.AppSettings["SrcAddress"];

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["url"];


                string parame = @"xml=<?xml version=""1.0"" encoding=""UTF-8""?> " +
                                "<SmsSubmitReq>" +
                                "    <SysId>" + Sysid + "</SysId >" +
                                "    <SrcAddress>" + SrcAddress + "</SrcAddress>" +
                                "    <DestAddress>" + DestAddress + "</DestAddress>" +
                                "    <SmsBody>" + b64SmsBody + "</SmsBody>" +
                                 "   <DrFlag>true</DrFlag>" +
                                "</SmsSubmitReq>";


                byte[] postData = Encoding.UTF8.GetBytes(parame);

                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 30000;
                request.ContentLength = postData.Length;
                // 寫入 Post Body Message 資料流
                using (Stream st = request.GetRequestStream())
                {
                    st.Write(postData, 0, postData.Length);
                }

                string result = "";
                // 取得回應資料
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                #region 解析XML
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(result);

                XmlNodeList nodeList = xmlDoc.SelectSingleNode("SubmitRes").ChildNodes;

                foreach (XmlNode childNode in nodeList)
                {
                    XmlElement childElement = (XmlElement)childNode;


                    switch (childElement.Name)
                    {
                        case "MessageId":
                            _API_Result.MessageId = childElement.InnerText;
                            break;
                        case "ResultCode":
                            _API_Result.ResultCode = childElement.InnerText;
                            break;
                        case "ResultText":
                            _API_Result.ResultText = childElement.InnerText;
                            break;
                    }
                }
                #endregion

                #region 寫入資料庫
                //檢查是否有寫入過 TMMMSEmployee
                DataSet TMMMSEmployee_ds = new DataSet("TMMMSEmployeeSet");

                string TMMMSEmployeeSelectSQL = string.Format("select * from TMMMSEmployee where Dept = '{0}' and EmployeeId = '{1}' and is_delete = 'N'",
                                                Dept, User);

                TMMMSEmployee_ds = OracleDataBase.Instance.ExecuteSQL(TMMMSEmployeeSelectSQL);

                //如沒寫入則寫入一筆
                if (TMMMSEmployee_ds.Tables[0].Rows.Count == 0)
                {
                    string strSQL = String.Format("INSERT INTO TMMMSEmployee (Dept, EmployeeId, IS_DELETE) VALUES ('{0}', '{1}', 'N')",
                                Dept, User);

                    OracleDataBase.Instance.ExecuteSQL(strSQL);

                    TMMMSEmployee_ds = OracleDataBase.Instance.ExecuteSQL(TMMMSEmployeeSelectSQL);
                }

                List<TMMMSEmployee> TMMMSEmployeeList = new List<TMMMSEmployee>();

                TMMMSEmployeeList = TransformToList.Instance.GetTMMMSEmployees(TMMMSEmployee_ds);

                //寫入 TMMMSLog
                string TMMMSEmployee_ID = TMMMSEmployeeList.First().ID.ToString();

                string NowStr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                string strSQL_TMMMSLog = string.Format("INSERT INTO TMMMSLog (TMMMSEMPLOYEE_ID, SYSID, SRCADDRESS, DESTADDRESS, SMSBODY, RESULTCODE, " +
                    "RESULTTEXT, MESSAGEID, CREATE_DT) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', TO_DATE('{8}', 'yyyy/mm/dd hh24:mi:ss'))",
                    TMMMSEmployee_ID, Sysid, SrcAddress, DestAddress, SmsBody, _API_Result.ResultCode, _API_Result.ResultText, _API_Result.MessageId, NowStr);


                OracleDataBase.Instance.ExecuteSQL(strSQL_TMMMSLog);

                #endregion

                return _API_Result;
            }
            catch(Exception ex)
            {
                _API_Result.ResultCode = "99999";
                _API_Result.ResultText = ex.ToString();

                return _API_Result;
            }
        }

    }
}