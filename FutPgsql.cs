using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;

namespace twin_futs
{
    class FutPgsql
    {
        NpgsqlConnection conn;
        public FutPgsql()
        {
            conn = GetConn();
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
                conn.Open();
                
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
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(queryStr, conn);
                NpgsqlDataReader dr = cmd.ExecuteReader();
                Console.WriteLine("read successfully!");

                while (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        Console.Write("{0} \t", dr[i]);
                    }
                    Console.WriteLine();
                }
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
                conn.Open();
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

        public void DatasetFill(string cmdStr)
        {
            try
            {
                conn.Open();
                DataSet ds = new DataSet();
                NpgsqlDataAdapter dap = new NpgsqlDataAdapter(cmdStr, conn);
                dap.Fill(ds, "fut");

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
            }
        }
    }
}
