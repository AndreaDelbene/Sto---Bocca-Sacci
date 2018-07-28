using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace DibrisBike
{
    class RawMaterial
    {

        SqlConnection conn;
        private String query;
        private SqlCommand comm;
        AutoResetEvent _signalError;
        private DataTable dtSchema;
        private string Sheet1;
        private string errorString = null;

        public RawMaterial(SqlConnection conn, AutoResetEvent _signalError)
        {
            this._signalError = _signalError;
            this.conn = conn;
            query = "INSERT INTO dbo.magazzinomateriali (codiceBarre,descrizione,diametro,peso,lunghezza) VALUES (@codiceBarre,@descrizione,@diametro,@peso,@lunghezza)";
            comm = new SqlCommand(query, conn);
        }

        public void GetRawFromFile(String pathToFile)
        {
            string excelConnection =
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathToFile + ";" +
                @"Extended Properties='Excel 8.0;HDR=Yes;'";
            // The file excel is handled as a database
            using (OleDbConnection connection = new OleDbConnection(excelConnection))
            {
                connection.Open();
                dtSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");
                DataTable dt = new DataTable();
                using (OleDbCommand cmd = new OleDbCommand("select * from [" + Sheet1 + "]", connection))
                {
                    using (OleDbDataReader rdr = cmd.ExecuteReader())
                    {
                        dt.Load(rdr);
                    }
                }
                foreach (DataRow dr in dt.Rows)
                {
                    comm.Parameters.Clear();                    
                    try
                    {
                        comm.Parameters.AddWithValue("@codiceBarre", dr["codiceBarre"]);
                        comm.Parameters.AddWithValue("@descrizione", dr["descrizione"]);
                        comm.Parameters.AddWithValue("@diametro", dr["diametro"]);
                        comm.Parameters.AddWithValue("@peso", dr["peso"]);
                        comm.Parameters.AddWithValue("@lunghezza", dr["lunghezza"]);

                        if (conn != null && conn.State == ConnectionState.Closed)
                            conn.Open();
                        try
                        {
                            int result = comm.ExecuteNonQuery();
                            if (result < 0)
                            {
                                Console.WriteLine("Errore nell'inserimento dei raw material: result = " + result);
                            }
                        }
                        catch (SqlException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.WriteLine("Riga nel file dei raw material vuota o non valida");
                        }
                    }
                    catch (ArgumentException e)
                    {
                        errorString = "formattazione";
                    }
                }
            }
        }

        public String GetErrorString()
        {
            return errorString;
        }
    }
}
