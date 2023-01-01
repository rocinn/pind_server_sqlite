﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Web;
using Newtonsoft.Json;

namespace pind_server_sqlite.Common
{
    public class SqliteHelper
    {
        private static SqliteHelper m_instance;

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
                    SetParameters(cmd, "@name", "admin");
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
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create unique index if not exists tnote_index_guid on tnote (guid);";
                cmd = new SQLiteCommand(sql, con, tran);
                cmd.ExecuteNonQuery();

                sql = @"create table if not exists tnoteHistory(
fid integer primary key autoincrement not null,
userid integer,
note_id integer,
note_guid varchar(100),
note_content nvarchar,
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

                sql = @"create table if not exists tnoteImage(
fid integer primary key autoincrement not null,
noteid integer,
imageByte blob,
status integer default 1,
iTime datetime default (datetime('now', 'localtime')) );
create index if not exists tnoteImage_index_noteid on tnoteImage (noteid);";
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
            if (!File.Exists(dbfilepath))
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
            DataTable dt = new DataTable();
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                //dt = reader.GetSchemaTable();
                dt.Load(reader);
            }

            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : null;
        }

        public static SqliteHelper GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new SqliteHelper();
            }

            return m_instance;
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

        public int fnAddNote(int userid, string guid, string content)
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

                sql = @"insert into tnote (userid,guid,content,iTime) values (@userid,@guid,@content,@iTime);
select last_insert_rowid();";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@userid", userid);
                SetParameters(cmd, "@guid", guid);
                SetParameters(cmd, "@content", content);
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

        public void fnUpdNote(int userid, string fid, string content)
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

                sql = "update tnote set content = @content where fid=@fid";
                cmd = new SQLiteCommand(sql, con, tran);
                SetParameters(cmd, "@fid", fid);
                SetParameters(cmd, "@content", content);
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

        public DataTable fnGetNote(int userid)
        {
            string sql = "select * from tnote where userid= @userid order by fid desc";
            SQLiteCommand cmd = new SQLiteCommand(sql);
            SetParameters(cmd, "@userid", userid);
            DataTable dt = GetDT(cmd);
            dt.Columns.Add("iTimeShow");
            foreach (DataRow dr in dt.Rows)
            {
                dr["iTimeShow"] = DateTime.Parse(dr["iTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return dt;
        }
    }
}