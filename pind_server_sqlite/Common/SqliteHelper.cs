using System;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace pind_server_sqlite.Common
{
    public class SqliteHelper
    {
        private static SqliteHelper m_instance;

        public static SqliteHelper GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new SqliteHelper();
            }

            return m_instance;
        }


        private SqliteHelper()
        {
            SQLiteConnection con = null;
            SQLiteCommand cmd = null;
            SQLiteTransaction tran = null;
            string sql = "";
            try
            {
                con = GetConnection();
                con.Open();
                tran = con.BeginTransaction();

                sql = @"create table if not exists tuser(
fid integer primary key autoincrement not null,
name nvarchar(200),
pwd varchar(100),
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create unique index if not exists tuser_index_name on tuser (name);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = "select * from tuser where name = @name and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@name", "admin");
                DataRow dr = GetRow(cmd);
                if (dr == null)
                {
                    sql = "insert into tuser (name, pwd) values (@name, @pwd)";
                    cmd = new SQLiteCommand(sql, con, tran);
                    SetParameters(cmd, "@name", "pind");
                    SetParameters(cmd, "@pwd", Util.GetMD5_16_x2("pind.!@#.com"));
                    cmd.ExecuteNonQuery();
                }

                sql = @"create table if not exists tlog(
fid integer primary key autoincrement not null,
logType varchar(50),
methdname varchar(500),
parameter nvarchar,
description nvarchar
timeCost decimal(18,2),
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tloginHistory(
fid integer primary key autoincrement not null,
userid integer,
name nvarchar(200), 
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tloginToken(
fid integer primary key autoincrement not null,
userid integer,
token varchar(300), 
expireTime datetime,
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create index if not exists tloginToken_index_token on tloginToken (token);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tnote(
fid integer primary key autoincrement not null,
userid integer not null,
guid varchar(100) not null,
content nvarchar,
htmlcontent nvarchar,
status integer default 1,
iTime datetime default (datetime('now', 'localtime')),
uTime datetime default (datetime('now', 'localtime')) );
create unique index if not exists tnote_index_guid on tnote (guid);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tnoteHistory(
fid integer primary key autoincrement not null,
userid integer,
note_id integer,
note_guid varchar(100),
note_content nvarchar,
note_htmlcontent nvarchar,
note_status integer,
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create index if not exists tnoteHistory_index_note_id on tnoteHistory (note_id);
create index if not exists tnoteHistory_index_guid on tnoteHistory (note_guid);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tnoteTag(
fid integer primary key autoincrement not null,
noteid integer,
tag nvarchar(100),
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create index if not exists tnoteTag_index_noteid on tnoteTag (noteid);
create index if not exists tnoteTag_index_tag on tnoteTag (tag);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tnoteFile(
fid integer primary key autoincrement not null,
noteid integer,
name nvarchar(300),
type varchar(100),
size integer,
md5base64 varchar(100),
relativePath varchar(500),
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create index if not exists tnoteFile_index_noteid on tnoteFile (noteid);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception ex)
            {
                Rollback(tran);

                // todo log to file
                m_instance = null;
                throw ex;
            }
            finally
            {
                Close(con);
            }
        }

        private SQLiteConnection GetConnection()
        {
            string dbfilepath = HttpRuntime.AppDomainAppPath + "pind.db";
            string dbPath = "Data Source =" + dbfilepath;
            if (!System.IO.File.Exists(dbfilepath))
            {
                SQLiteConnection.CreateFile(dbfilepath);
            }

            SQLiteConnection con = new SQLiteConnection(dbPath);
            return con;
        }

        private void Close(SQLiteConnection con)
        {
            if (con != null)
            {
                con.Close();
            }
        }

        private void Rollback(SQLiteTransaction tran)
        {
            if (tran != null && tran.Connection != null)
            {
                tran.Rollback();
            }
        }

        private void SetParameters(SQLiteCommand cmd, string key, object v)
        {
            cmd.Parameters.AddWithValue(key, v);
        }

        private DataTable GetDT(SQLiteCommand cmd)
        {
            SQLiteConnection con = null;
            if (cmd.Connection == null)
            {
                con = GetConnection();
                con.Open();
                cmd.Connection = con;
            }

            DataTable dt = new DataTable();
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                //dt = reader.GetSchemaTable();
                dt.Load(reader);
            }

            Close(con);

            return dt;
        }

        private DataRow GetRow(SQLiteCommand cmd)
        {
            SQLiteConnection con = null;
            if (cmd.Connection == null)
            {
                con = GetConnection();
                con.Open();
                cmd.Connection = con;
            }

            DataTable dt = new DataTable();
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                //dt = reader.GetSchemaTable();
                dt.Load(reader);
            }

            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private string GetString(SQLiteCommand cmd)
        {
            SQLiteConnection con = null;
            if (cmd.Connection == null)
            {
                con = GetConnection();
                con.Open();
                cmd.Connection = con;
            }

            DataTable dt = new DataTable();
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                //dt = reader.GetSchemaTable();
                dt.Load(reader);
            }

            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : null;
        }

        private int ExecuteNonQuery(SQLiteCommand cmd)
        {
            SQLiteConnection con = null;
            if (cmd.Connection == null)
            {
                con = GetConnection();
                con.Open();
                cmd.Connection = con;
            }

            return cmd.ExecuteNonQuery();
        }
        
        public DataTable fnGetUsers()
        {
            string sql = "select name from tuser where status = 1";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            return GetDT(cmd);
        }

        public string fnLogin(string name, string pwd)
        {
            SQLiteConnection con = GetConnection();
            SQLiteTransaction tran = null;
            SQLiteCommand cmd = null;
            string sql = "";
            DateTime now = DateTime.Now;
            string acctoken = "";
            try
            {
                con.Open();
                tran = con.BeginTransaction();

                sql = "select * from tuser where status = 1 and name = @name ";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@name", name);
                DataRow dr = GetRow(cmd);
                if (dr == null)
                    throw new CustomException("账号不存在");
                if (dr["pwd"].ToString().ToLower() == pwd.ToLower())
                {
                    sql = "insert into tloginHistory (userid,name,iTime) values (@userid,@name,@iTime)";
                    cmd = new SQLiteCommand(sql, con, tran);
                    SetParameters(cmd, "@userid", dr["fid"]);
                    SetParameters(cmd, "@name", name);
                    SetParameters(cmd, "@iTime", now);
                    cmd.ExecuteNonQuery();

                    DateTime deExpire = now.AddDays(30);
                    var data = new
                    {
                        userid = dr["fid"].ToString(),
                        name = name,
                        expire = deExpire.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    acctoken = AccessToken.getAccessToken(JsonConvert.SerializeObject(data));
                    sql = "insert into tloginToken (userid,token,expireTime,iTime) values (@userid,@token,@expireTime,@iTime)";
                    cmd = new SQLiteCommand(sql, con, tran);
                    SetParameters(cmd, "@userid", dr["fid"]);
                    SetParameters(cmd, "@token", acctoken);
                    SetParameters(cmd, "@expireTime", deExpire);
                    SetParameters(cmd, "@iTime", now);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    throw new CustomException("密码不正确");
                }

                tran.Commit();

                return acctoken;
            }
            catch (Exception)
            {
                Rollback(tran);
                throw;
            }
            finally
            {
                Close(con);
            }
        }

        public DataRow fnAddNote(int userid, string guid, string content, string htmlcontent)
        {
            SQLiteConnection con = GetConnection();
            SQLiteTransaction tran = null;
            SQLiteCommand cmd = null;
            string sql = "";
            DateTime now = DateTime.Now;
            try
            {
                con.Open();
                tran = con.BeginTransaction();

                sql = "select * from tuser where status = 1 and fid = @fid ";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", userid);
                DataRow drUser = GetRow(cmd);
                if (drUser == null)
                    throw new CustomException("账号不存在");

                sql = @"insert into tnote (userid,guid,content,htmlcontent,iTime) values (@userid,@guid,@content,@htmlcontent,@iTime);
select last_insert_rowid();";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@userid", userid);
                SetParameters(cmd, "@guid", guid);
                SetParameters(cmd, "@content", content);
                SetParameters(cmd, "@htmlcontent", htmlcontent);
                SetParameters(cmd, "@iTime", now);
                string id = GetString(cmd);

                sql = "select * from tnote where fid = @fid and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", id);
                DataRow dr = GetRow(cmd);

                tran.Commit();

                DataTable _dt = fnDealNoteTime(dr.Table);

                return _dt.Rows[0];
            }
            catch (Exception)
            {
                Rollback(tran);
                throw;
            }
            finally
            {
                Close(con);
            }
        }

        public DataRow fnUpdNote(int userid, string fid, string content, string htmlcontent, string uTime)
        {
            SQLiteConnection con = GetConnection();
            SQLiteTransaction tran = null;
            SQLiteCommand cmd = null;
            string sql = "";
            DateTime now = DateTime.Now;
            try
            {
                con.Open();
                tran = con.BeginTransaction();

                sql = "select * from tuser where status = 1 and fid = @userid ";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@userid", userid);
                DataRow drUser = GetRow(cmd);
                if (drUser == null)
                    throw new CustomException("账号不存在");

                sql = "select * from tnote where fid = @fid and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                DataRow drNote = GetRow(cmd);
                if (drNote == null)
                    throw new CustomException("Note不存在");

                if (!string.IsNullOrWhiteSpace(drNote["uTime"].ToString()))
                {
                    DateTime.TryParse(uTime, out DateTime ftime);
                    DateTime.TryParse(drNote["uTime"].ToString(), out DateTime dtime);
                    if (ftime.ToString("yyyy-MM-dd HH:mm:ss") != dtime.ToString("yyyy-MM-dd HH:mm:ss"))
                    {
                        throw new CustomException("Note内容已被其他终端修改，请刷新后重新修改");
                    }
                }

                sql = "update tnote set content = @content, htmlcontent=@htmlcontent, uTime=@uTime where fid=@fid";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                SetParameters(cmd, "@content", content);
                SetParameters(cmd, "@htmlcontent", htmlcontent);
                SetParameters(cmd, "@uTime", now);
                cmd.ExecuteNonQuery();

                sql = "select * from tnote where fid = @fid and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                DataRow dr = GetRow(cmd);

                tran.Commit();

                DataTable _dt = fnDealNoteTime(dr.Table);

                return _dt.Rows[0];
            }
            catch (Exception)
            {
                Rollback(tran);
                throw;
            }
            finally
            {
                Close(con);
            }
        }

        public void fnDelNote(int userid, string fid)
        {
            SQLiteConnection con = GetConnection();
            SQLiteTransaction tran = null;
            SQLiteCommand cmd = null;
            string sql = "";
            DateTime now = DateTime.Now;
            try
            {
                con.Open();
                tran = con.BeginTransaction();

                sql = "select * from tuser where status = 1 and fid = @userid ";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@userid", userid);
                DataRow drUser = GetRow(cmd);
                if (drUser == null)
                    throw new CustomException("账号不存在");

                sql = "select * from tnote where fid = @fid and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                DataRow drNote = GetRow(cmd);
                if (drNote == null)
                    throw new CustomException("Note不存在");

                sql = "update tnote set status = 0 where fid=@fid";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception)
            {
                Rollback(tran);
                throw;
            }
            finally
            {
                Close(con);
            }
        }

        public DataSet fnGetNote(int userid)
        {
            string sql = "select * from tnote where userid= @userid and status = 1 order by fid desc";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@userid", userid);
            DataTable dtNote = GetDT(cmd);

            dtNote = fnDealNoteTime(dtNote);
            dtNote.TableName = "note";

            sql = "select * from tnoteFile where status = 1 ";
            cmd = new SQLiteCommand(sql);
            DataTable dtFile = GetDT(cmd);
            dtFile.TableName = "file";

            DataSet ds = new DataSet();
            ds.Tables.Add(dtNote);
            ds.Tables.Add(dtFile);

            return ds;
        }

        private DataTable fnDealNoteTime(DataTable dt)
        {
            dt.Columns.Add("iTimeShow");
            dt.Columns.Add("uTimeShow");
            foreach (DataRow dr in dt.Rows)
            {
                dr["htmlcontent"] = dr["htmlcontent"].ToString();
                dr["iTimeShow"] = DateTime.Parse(dr["iTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                dr["uTimeShow"] = DateTime.Parse(dr["uTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return dt;
        }

        public DataSet fnSearchNote(int userid, string stext)
        {
            string sql = "select * from tnote where userid= @userid and status = 1 and content like @content order by fid desc";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@userid", userid);
            SetParameters(cmd, "@content", "%" + stext + "%");
            DataTable dtNote = GetDT(cmd);

            dtNote = fnDealNoteTime(dtNote);
            dtNote.TableName = "note";

            sql = "select a.* from tnoteFile a left join tnote b on b.fid = a.noteid where a.status = 1 and b.userid = @userid and b.status = 1";
            cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@userid", userid);
            DataTable dtFile = GetDT(cmd);
            dtFile.TableName = "file";

            DataSet ds = new DataSet();
            ds.Tables.Add(dtNote);
            ds.Tables.Add(dtFile);

            return ds;
        }

        private string fnGetLikeString(string oldStr)
        {
            if (string.IsNullOrEmpty(oldStr))
            {
                return "";
            }
            string str2 = Regex.Replace(oldStr, @"[\[\+\\\|()^*%~#-&_]", delegate (Match match)
            {
                return "[" + match.Value + "]";
            });
            return str2;
        }

        public int fnAddNoteFileInfo(int userid, int noteid, string name, string type, int size, string relativePath, string md5base64)
        {
            SQLiteConnection con = GetConnection();
            SQLiteTransaction tran = null;
            SQLiteCommand cmd = null;
            string sql = "";
            DateTime now = DateTime.Now;
            try
            {
                con.Open();
                tran = con.BeginTransaction();

                sql = "select * from tuser where status = 1 and fid = @userid ";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@userid", userid);
                DataRow drUser = GetRow(cmd);
                if (drUser == null)
                    throw new CustomException("账号不存在");

                sql = "select * from tnote where fid = @fid and status = 1";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", noteid);
                DataRow drNote = GetRow(cmd);
                if (drNote == null)
                    throw new CustomException("Note不存在");

                if (userid != Convert.ToInt32(drNote["userid"]))
                {
                    throw new CustomException("数据校验失败");
                }

                sql = @"insert into tnoteFile (noteid,name,type,size,md5base64,relativePath,status,iTime) 
values (@noteid,@name,@type,@size,@md5base64,@relativePath,1,@iTime); 
select last_insert_rowid();";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@noteid", noteid);
                SetParameters(cmd, "@name", name);
                SetParameters(cmd, "@type", type);
                SetParameters(cmd, "@size", size);
                SetParameters(cmd, "@md5base64", md5base64);
                SetParameters(cmd, "@relativePath", relativePath);
                SetParameters(cmd, "@iTime", now);
                string id = GetString(cmd);

                tran.Commit();

                return Convert.ToInt32(id);
            }
            catch (Exception)
            {
                Rollback(tran);
                throw;
            }
            finally
            {
                Close(con);
            }
        }

        public DataRow fnGetNoteFileinfo(int userid, int noteid, int fileid)
        {
            string sql = @"select * from tnoteFile a 
left join tnote b on b.fid = a.noteid 
where a.status = 1 and b.status = 1 and a.fid = @fileid and a.noteid = @noteid and b.userid = @userid";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@userid", userid);
            SetParameters(cmd, "@noteid", noteid);
            SetParameters(cmd, "@fileid", fileid);

            return GetRow(cmd);
        }

        public void fnDeleteNoteFile(int userid, int noteid, int fileid)
        {
            DataRow dr = fnGetNoteFileinfo(userid, noteid, fileid);
            if (dr == null)
                throw new CustomException("文件不存在");

            string sql = "update tnoteFile set status = 0 where fid = @fileid";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@fileid", fileid);
            ExecuteNonQuery(cmd);
        }
    }
}