using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NASServerCP
{
    class DbWrapper
    {
        const string connString = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASClientTCP\NASServerTCP\bin\filesInfo_server.sqlite";
        String dbConnection;

        public DbWrapper()
        {
            dbConnection = @"Data Source=filesInfo_server.sqlite";
        }

        public DbWrapper(String inputFile)
        {
            dbConnection = String.Format(@"Data Source={0}", inputFile);
        }


        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = sql;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                cnn.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return dt;
        }

        public Hashtable SelectRow(string filename)
        {
            Hashtable row = new Hashtable();
            using (SQLiteConnection con = new SQLiteConnection(dbConnection))
            {
                con.Open();

                string stm = String.Format("SELECT id,name, checksum, changed, modified FROM Files where name = '{0}';", filename);

                using (SQLiteCommand cmd = new SQLiteCommand(stm, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                row["ID"] = Int32.Parse(rdr["id"].ToString());
                                row["fileName"] = rdr["name"].ToString();
                                row["checksum"] = rdr["checksum"].ToString();
                                row["changed"] = rdr["changed"];
                                row["modified"] = rdr["modified"].ToString();
                            }
                        }
                    }
                }

                con.Close();
            }
            return row;
        }
        ///
        ///     Allows the programmer to interact with the database for purposes other than a query.
        ///
        /// The SQL to be run.
        /// An Integer containing the number of rows updated.
        public int ExecuteNonQuery(string sql)
        {
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            int rowsUpdated = mycommand.ExecuteNonQuery();
            cnn.Close();
            return rowsUpdated;
        }

        public bool UpInsert(string tabName, string name, string checksum, int changed, string modifiedTime)
        {
            Boolean returnCode = true;
            try
            {
                this.ExecuteNonQuery(
                    String.Format(
                        "insert or replace into {0}(id, name, checksum, changed, modified) values ((select id from {0} where name = '{1}'), '{1}', '{2}', {3}, '{4}');",
                        tabName, name, checksum, changed, modifiedTime)
                );
            }
            catch
            {
                returnCode = false;
            }
            return returnCode;
        }

        public bool Update(String tableName, String where)
        {
            Boolean returnCode = true;
            /* String vals = "";

             if (data.Count >= 1)
             {
                 foreach (KeyValuePair val in data)
                 {
                     vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                 }
                 vals = vals.Substring(0, vals.Length - 1);
             }
             try
             {
                 this.ExecuteNonQuery(String.Format("update {0} set {1} where {2};", tableName, vals, where));
             }
             catch
             {
                 returnCode = false;
             }*/
            return returnCode;
        }

        public bool Delete(String tableName, String where)
        {
            Boolean returnCode = true;
            try
            {
                this.ExecuteNonQuery(String.Format("delete from {0} where {1};", tableName, where));
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message);
                returnCode = false;
            }
            return returnCode;
        }

        public bool Insert(String tableName)
        {
            Boolean returnCode = true;
            /* String columns = "";
             String values = "";
             
             foreach (KeyValuePair val in data)
             {
                 columns += String.Format(" {0},", val.Key.ToString());
                 values += String.Format(" '{0}',", val.Value);
             }
             columns = columns.Substring(0, columns.Length - 1);
             values = values.Substring(0, values.Length - 1);
             try
             {
                 this.ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
             }
             catch (Exception fail)
             {
                 MessageBox.Show(fail.Message);
                 returnCode = false;
             }*/
            return returnCode;

        }

        public bool ClearDB()
        {
            DataTable tables;
            try
            {
                tables = this.GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
                foreach (DataRow table in tables.Rows)
                {
                    this.ClearTable(table["NAME"].ToString());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ClearTable(String table)
        {
            try
            {
                this.ExecuteNonQuery(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
