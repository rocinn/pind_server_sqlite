﻿using Newtonsoft.Json.Linq;
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
            DataTable dt = SqliteHelper.GetInstance().fnGetNote(Convert.ToInt32(userid));

            return Json(new { code = 1, data = new { arrNote = dt } });
        }
    }
}
