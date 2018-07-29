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

namespace DibrisBike
{
    class RawMaterial
    {

        SqlConnection conn;
        private String query;
        private SqlCommand comm;
        private AutoResetEvent _signalError;
        AutoResetEvent _signalErrorRM2;
        private DataTable dtSchema;
        private string Sheet1;
        private string errorString = null;

        public RawMaterial(SqlConnection conn, AutoResetEvent _signalError, AutoResetEvent _signalErrorRM2)
        {
            this._signalError = _signalError;
            this._signalErrorRM2 = _signalErrorRM2;
            this.conn = conn;
            query = "INSERT INTO dbo.magazzinomateriali (codiceBarre,descrizione,diametro,peso,lunghezza) VALUES (@codiceBarre,@descrizione,@diametro,@peso,@lunghezza)";
            comm = new SqlCommand(query, conn);
            query1 = "INSERT INTO dbo.scatole (id, tipo, idCambio, idRuote, idFiniture, idSellino, idCatena) VALUES (@id, @tipo, @idCambio, @idRuote, @idFiniture, @idSellino, @idCatena)";
            comm1 = new SqlCommand(query1, conn);
            query2 = "INSERT INTO dbo.posizioni (x, y, z, idScatola) VALUES (@x, @y, @z, @idScatola)";
            comm2 = new SqlCommand(query2, conn);
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
                        
                        try
                        {
                            int result = comm.ExecuteNonQuery();
                            if (result < 0)
                            {
                                Console.WriteLine("Errore nell'inserimento dei raw material: result = " + result);
                            }
                            else
                            {
                                // notify new material
                                // to the routing thread
                                _signalError.Set();
                                // and to the alert UI
                                _signalErrorRM2.Set();
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

        public void GetBoxesFromFile(String pathToFile)
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
                Random r = new Random();
                int val;
                foreach (DataRow dr in dt.Rows)
                {
                    comm1.Parameters.Clear();
                    try
                    {
                        comm1.Parameters.AddWithValue("@id", dr["id"]);
                        comm1.Parameters.AddWithValue("@tipo", dr["tipo"]);
                        comm1.Parameters.AddWithValue("@idCambio", dr["idCambio"]);
                        comm1.Parameters.AddWithValue("@idRuote", dr["idRuote"]);
                        comm1.Parameters.AddWithValue("@idFiniture", dr["idFiniture"]);
                        comm1.Parameters.AddWithValue("@idSellino", dr["idSellino"]);
                        comm1.Parameters.AddWithValue("@idCatena", dr["idCatena"]);
                        
                        try
                        {
                            int result = comm1.ExecuteNonQuery();
                            if (result < 0)
                            {
                                Console.WriteLine("Errore nell'inserimento dei raw material: result = " + result);
                            }
                            else
                            {
                                val = r.Next();
                                comm2.Parameters.Clear();
                                comm2.Parameters.AddWithValue("@x", val);
                                comm2.Parameters.AddWithValue("@y", val);
                                comm2.Parameters.AddWithValue("@z", "A");
                                comm2.Parameters.AddWithValue("@idScatola", dr["id"]);

                                comm2.ExecuteNonQuery();
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
    }
}
