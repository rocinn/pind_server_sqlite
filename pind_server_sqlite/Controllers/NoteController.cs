using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using pind_server_sqlite.App_Start;
using pind_server_sqlite.Common;
using System.Security.Cryptography;

namespace pind_server_sqlite.Controllers
{
    [ApiAuthorizationFilter]
    [ApiExceptionFilter]
    public class NoteController : ApiController
    {
        [HttpPost]
        [Route("note/add")]
        public IHttpActionResult addNote(JObject obj)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            string guid = obj["guid"]?.ToString();
            string content = obj["content"]?.ToString();
            string htmlcontent = obj["htmlcontent"]?.ToString();

            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(content))
            {
                throw new CustomException("参数错误");
            }

            DataRow dr = SqliteHelper.GetInstance().fnAddNote(Convert.ToInt32(userid), guid, content, htmlcontent);
            return Json(new { code = 1, data = new { arrNote = dr.Table } });
        }

        [HttpPost]
        [Route("note/upd")]
        public IHttpActionResult updNote(JObject obj)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            string fid = obj["fid"]?.ToString();
            string content = obj["content"]?.ToString();
            string htmlcontent = obj["htmlcontent"]?.ToString();
            string uTime = obj["uTime"]?.ToString();

            if (string.IsNullOrWhiteSpace(fid) || string.IsNullOrWhiteSpace(content))
            {
                throw new CustomException("参数错误");
            }

            DataRow dr = SqliteHelper.GetInstance().fnUpdNote(Convert.ToInt32(userid), fid, content, htmlcontent, uTime);
            return Json(new { code = 1, data = new { arrNote = dr.Table } });
        }

        [HttpPost]
        [Route("note/del")]
        public IHttpActionResult delNote(JObject obj)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            string fid = obj["fid"]?.ToString();

            if (string.IsNullOrWhiteSpace(fid))
            {
                throw new CustomException("参数错误");
            }

            SqliteHelper.GetInstance().fnDelNote(Convert.ToInt32(userid), fid);
            return Json(new { code = 1, message = "ok", data = new { } });
        }

        [HttpGet]
        [Route("note/get")]
        public IHttpActionResult getNote()
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            DataSet ds = SqliteHelper.GetInstance().fnGetNote(Convert.ToInt32(userid));
            List<object> lst = new List<object>();
            foreach (DataRow dr in ds.Tables["note"].Rows)
            {
                Dictionary<string, object> dicData = new Dictionary<string, object>();
                foreach (DataColumn dc in ds.Tables["note"].Columns)
                {
                    dicData.Add(dc.ColumnName, dr[dc.ColumnName]);
                }

                List<object> lstFile = new List<object>();
                DataRow[] arrFile = ds.Tables["file"].Select($"noteid = {dr["fid"]}");
                foreach (DataRow drFile in arrFile)
                {
                    lstFile.Add(new
                    {
                        fid = drFile["fid"],
                        name = drFile["name"],
                        md5 = drFile["md5base64"]
                    });
                }
                dicData.Add("files", lstFile);

                lst.Add(dicData);
            }

            return Json(new { code = 1, data = new { arrNote = lst } });
        }

        [HttpGet]
        [Route("note/search")]
        public IHttpActionResult searchNote(string stext)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            DataSet ds = new DataSet();
            if (string.IsNullOrWhiteSpace(stext))
            {
                ds = SqliteHelper.GetInstance().fnGetNote(Convert.ToInt32(userid));
            }
            else
            {
                ds = SqliteHelper.GetInstance().fnSearchNote(Convert.ToInt32(userid), stext);
            }

            List<object> lst = new List<object>();
            foreach (DataRow dr in ds.Tables["note"].Rows)
            {
                Dictionary<string, object> dicData = new Dictionary<string, object>();
                foreach (DataColumn dc in ds.Tables["note"].Columns)
                {
                    dicData.Add(dc.ColumnName, dr[dc.ColumnName]);
                }

                List<object> lstFile = new List<object>();
                DataRow[] arrFile = ds.Tables["file"].Select($"noteid = {dr["fid"]}");
                foreach (DataRow drFile in arrFile)
                {
                    lstFile.Add(new
                    {
                        fid = drFile["fid"],
                        name = drFile["name"],
                        md5 = drFile["md5base64"]
                    });
                }
                dicData.Add("files", lstFile);

                lst.Add(dicData);
            }

            return Json(new { code = 1, data = new { arrNote = lst } });
        }

        [HttpPost]
        [Route("note/upfile")]
        public IHttpActionResult uploadNoteFile(JObject obj)
        {
            if (obj == null)
                throw new CustomException("invalid request");
            Request.Properties.TryGetValue("userid", out Object userid);
            string noteid = obj["noteid"]?.ToString();
            string name = obj["name"]?.ToString();
            string type = obj["type"]?.ToString();
            string base64 = obj["base64"]?.ToString();
            string md5base64 = Util.GetMD5_16_x2(base64);
            string strSize = obj["size"]?.ToString();
            int size = 0;
            if (!string.IsNullOrWhiteSpace(strSize))
            {
                bool isok = int.TryParse(strSize, out size);
            }

            string relativePath = FileHelper.toDisk(md5base64, base64);

            int id = SqliteHelper.GetInstance().fnAddNoteFileInfo(Convert.ToInt32(userid), Convert.ToInt32(noteid), name, type, size, relativePath, md5base64);

            return Json(new { code = 1, data = new { id = id } });
        }

        [HttpGet]
        [Route("note/dlfile")]
        public IHttpActionResult downloadFile(int noteid, int fileid)
        {
            Request.Properties.TryGetValue("userid", out Object userid);

            DataRow dr = SqliteHelper.GetInstance().fnGetNoteFileinfo(Convert.ToInt32(userid), noteid, fileid);
            if (dr == null)
            {
                throw new CustomException("file info not find");
            }

            string base64 = FileHelper.GetFileBase64(dr["relativePath"].ToString());

            return Json(new { code = 1, data = new { name = dr["name"], type = dr["type"], base64 = base64 } });
        }

        [HttpPost]
        [Route("note/delFile")]
        public IHttpActionResult deleteFile(int noteid, int fileid)
        {
            Request.Properties.TryGetValue("userid", out Object userid);

            SqliteHelper.GetInstance().fnDeleteNoteFile(Convert.ToInt32(userid), noteid, fileid);

            return Json(new { code = 1, data = new { } });
        }
    }
}
