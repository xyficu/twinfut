using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;
using System.Windows.Forms;

namespace twin_futs
{
    public class FutPgsql
    {
        NpgsqlConnection conn;
        public FutPgsql()
        {
            conn = GetConn();
            conn.Open();
        }

        public NpgsqlConnection GetConn()
        {
            string connStr = "Server=localhost; Port=5432; User Id=postgres;" +
                             "Password=fut; Database=fut;" +
                             "CommandTimeout=0; ConnectionLifeTime=0;";
            NpgsqlConnection conn = new NpgsqlConnection(connStr);
            return conn;
        }

        public int QueryOnly(string queryStr)
        {
            try
            {
                if (conn.State==ConnectionState.Closed)
                {
                    conn.Open();
                }

                NpgsqlCommand cmd = new NpgsqlCommand(queryStr, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                Console.WriteLine("count: " + count);
                conn.Close();
                return count;
                
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
                return 0;
            }
        }

        public NpgsqlDataReader QueryReader(string queryStr)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                NpgsqlCommand cmd = new NpgsqlCommand(queryStr, conn);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                Console.WriteLine("read successfully!");

                //while (dr.Read())
                //{
                //    for (int i = 0; i < dr.FieldCount; i++)
                //    {
                //        Console.Write("{0} \t", dr[i]);
                //    }
                //    Console.WriteLine();
                //}
                
                conn.Close();
                return dr;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
                return null;
            }
        }

        public void ChangeTable(string changeStr)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                NpgsqlCommand cmd = new NpgsqlCommand(changeStr, conn);
                int count = cmd.ExecuteNonQuery();
                Console.WriteLine("count: " + count);
                conn.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
            }
        }

        public DataSet DatasetFill(string cmdStr)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                DataSet ds = new DataSet();
                NpgsqlDataAdapter dap = new NpgsqlDataAdapter(cmdStr, conn);
                dap.Fill(ds, "fut");
                conn.Close();
                return ds;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
                return null;
            }
        }
        //添加OT到数据库
        public void AddOT(OT ot)
        {
            string cmdStr = "insert into ot values (" +
                   ot.id.ToString() + ",'" +
                   ot.ra + "','" +
                   ot.dec + "'," +
                   ot.mag.ToString() + "," +
                   ot.status.ToString() + "," +
                   "to_timestamp('" + ot.addTime + "','" + "yyyymmddThh24miss'" + "));";
            ChangeTable(cmdStr);
        }
        //更新数据库OT表某个OT的状态
        public void UpdateOTStat(int id, int status)
        {
            string cmdStr = "update ot set status = " + status.ToString() + " where id = " + id.ToString();
            ChangeTable(cmdStr);
        }
        //添加GRB到数据库
        public void AddGRB()
        {

        }
        //更新数据库GRB表某个GRB的状态
        public void UpdateGRBStat()
        {

        }

        
    }
}
