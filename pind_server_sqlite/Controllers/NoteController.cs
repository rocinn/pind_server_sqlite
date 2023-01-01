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

namespace pind_server_sqlite.Controllers
{
    [ApiAuthorizationFilter]
    public class NoteController : ApiController
    {
        [HttpPost]
        [Route("note/add")]
        public IHttpActionResult addNote(JObject obj)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            //string userid = "1";
            string guid = obj["guid"]?.ToString();
            string content = obj["content"]?.ToString();

            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(content))
            {
                throw new CustomException("参数错误");
            }

            int id = SqliteHelper.GetInstance().fnAddNote(Convert.ToInt32(userid), guid, content);

            return Json(new { code = 1, data = new { id = id } });
        }

        [HttpPost]
        [Route("note/upd")]
        public IHttpActionResult updNote(JObject obj)
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            string fid = obj["fid"]?.ToString();
            string content = obj["content"]?.ToString();

            if (string.IsNullOrWhiteSpace(fid) || string.IsNullOrWhiteSpace(content))
            {
                throw new CustomException("参数错误");
            }

            SqliteHelper.GetInstance().fnUpdNote(Convert.ToInt32(userid), fid, content);

            return Json(new { code = 1, data = new { } });
        }

        [HttpGet]
        [Route("note/get")]
        public IHttpActionResult getNote()
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            //string userid = "1";
            DataTable dt = SqliteHelper.GetInstance().fnGetNote(Convert.ToInt32(userid));

            return Json(new { code = 1, data = new { arrNote = dt } });
        }
    }
}
