using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace DibrisBike
{

    class MPS
    {
        private DataTable dtSchema;
        private string Sheet1;
        private string errorString = null;

        public MPS()
        {
        }

        public void getMPS(SqlConnection conn, ConcurrentQueue<object> _queue, AutoResetEvent _signal)
        {
            while (true)
            {
                //checking whenever a new MPS has been uploaded
                string query = "SELECT * FROM dbo.mps WHERE running = 0 OR modified = 1";
                SqlCommand comm = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                comm.ExecuteNonQuery();

                DataTable table = new DataTable();
                adapter.Fill(table);
                //getting then the data from the table
                int[] id, quantita, priorita;
                string[] tipoTelaio, colore, linea;
                DateTime[] startDate, dueDate;
                Byte[] running, modified;
                List<int> idToPass = new List<int>();
                List<int> quantitaTubiToPass = new List<int>();
                List<string> lineaToPass = new List<string>();
                List<int> quantitaToPass = new List<int>();

                //if we have more than one order, they may have different priorities
                priorita = (from DataRow r in table.Rows select (int)r["priorita"]).ToArray();

                if (priorita.Length > 1)
                {
                    //sorting the rows on the "priorita" column
                    DataView dv = table.DefaultView;
                    dv.Sort = "priorita asc";
                    DataTable sortedTable = dv.ToTable();
                    //then getting the data
                    id = (from DataRow r in sortedTable.Rows select (int)r["id"]).ToArray();
                    startDate = (from DataRow r in sortedTable.Rows select (DateTime)r["startDate"]).ToArray();
                    dueDate = (from DataRow r in sortedTable.Rows select (DateTime)r["dueDate"]).ToArray();
                    quantita = (from DataRow r in sortedTable.Rows select (int)r["quantita"]).ToArray();
                    tipoTelaio = (from DataRow r in sortedTable.Rows select (string)r["tipoTelaio"]).ToArray();
                    colore = (from DataRow r in sortedTable.Rows select (string)r["colore"]).ToArray();
                    linea = (from DataRow r in sortedTable.Rows select (string)r["linea"]).ToArray();
                    priorita = (from DataRow r in sortedTable.Rows select (int)r["priorita"]).ToArray();
                    running = (from DataRow r in sortedTable.Rows select (Byte)r["running"]).ToArray();
                    modified = (from DataRow r in sortedTable.Rows select (Byte)r["modified"]).ToArray();
                }
                else
                {
                    //else we get data normally from the table.
                    id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();
                    startDate = (from DataRow r in table.Rows select (DateTime)r["startDate"]).ToArray();
                    dueDate = (from DataRow r in table.Rows select (DateTime)r["dueDate"]).ToArray();
                    quantita = (from DataRow r in table.Rows select (int)r["quantita"]).ToArray();
                    tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                    colore = (from DataRow r in table.Rows select (string)r["colore"]).ToArray();
                    linea = (from DataRow r in table.Rows select (string)r["linea"]).ToArray();
                    running = (from DataRow r in table.Rows select (Byte)r["running"]).ToArray();
                    modified = (from DataRow r in table.Rows select (Byte)r["modified"]).ToArray();
                }

                int[] quantitaTubi = new int[id.Length];
                
                //for each element in the table we got back from the first request
                for (int i = 0; i < id.Length; i++)
                {
                    if(modified[i]==1)
                    {
                        //getting the old quantity
                        query = "SELECT quantitaDesiderata FROM dbo.statoordini WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idLotto", id[i]);

                        SqlDataReader reader = comm.ExecuteReader();

                        reader.Read();

                        int quantitaOld = (int)reader["quantitaDesiderata"];

                        reader.Close();

                        //updating the order's state
                        query = "UPDATE dbo.statoordini SET quantitaDesiderata = @quantitaDesiderata WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@quantitaDesiderata", quantita[i]);
                        comm.Parameters.AddWithValue("@idLotto", id[i]);

                        comm.ExecuteNonQuery();
                        
                        
                        //and I check how many stuff I need for that kind of bike
                        query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                        reader = comm.ExecuteReader();

                        reader.Read();

                        quantitaTubi[i] = (int)reader["quantitaTubi"];

                        reader.Close();

                        //and setting the quantity to produce = new - old
                        int quantitaTemp = quantita[i] - quantitaOld;
                        //if we decided to increment the quantity, then we should send infos to steps that come next, otherwhise we don't
                        if (quantitaTemp > 0)
                        {
                            //let's update the new quantity
                            quantita[i] = quantita[i] - quantitaOld;
                            //and add everything to the lists that will be passed
                            quantitaToPass.Add(quantita[i]);
                            idToPass.Add(id[i]);
                            quantitaTubiToPass.Add(quantitaTubi[i]);
                            lineaToPass.Add(linea[i]);
                        }

                        //and let's update the 'mps' table too
                        query = "UPDATE dbo.mps SET modified = 0, quantita = @quantita WHERE id = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idLotto", id[i]);
                        comm.Parameters.AddWithValue("@quantita", quantita[i]);

                        comm.ExecuteNonQuery();
                    }
                    else
                    {
                        //if the order is new, let's set the start date to now
                        startDate[i] = DateTime.Now;
                        //I update the 'statoordini' table in the DB then
                        query = "INSERT INTO dbo.statoordini (idLotto, startPianificata, startEffettiva, dueDatePianificata, quantitaDesiderata, quantitaProdotta, tipoTelaio, stato, descrizione) " +
                            "VALUES(@idLotto, @startPianificata, @startEffettiva, @dueDatePianificata, @quantitaDesiderata, @quantitaProdotta, @tipoTelaio, @stato, @descrizione)";

                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idLotto", id[i]);
                        comm.Parameters.AddWithValue("@startPianificata", startDate[i]);
                        comm.Parameters.AddWithValue("@startEffettiva", startDate[i]);
                        comm.Parameters.AddWithValue("@dueDatePianificata", dueDate[i]);
                        comm.Parameters.AddWithValue("@quantitaDesiderata", quantita[i]);
                        comm.Parameters.AddWithValue("@quantitaProdotta", 0);
                        comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);
                        comm.Parameters.AddWithValue("@stato", "running");
                        comm.Parameters.AddWithValue("@descrizione", "");

                        comm.ExecuteNonQuery();

                        //I set then the flag to 1 into the 'mps' table, I set the start time
                        query = "UPDATE stodb.dbo.mps SET running = 1, startDate = @startDate  WHERE id = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@startDate", startDate[i]);
                        comm.Parameters.AddWithValue("@idLotto", id[i]);

                        comm.ExecuteNonQuery();

                        //and I check how many stuff I need for that kind of bike
                        query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                        SqlDataReader reader = comm.ExecuteReader();

                        reader.Read();

                        quantitaTubi[i] = (int)reader["quantitaTubi"];

                        reader.Close();
                        //inserting the stuff into the lists to send after
                        quantitaToPass.Add(quantita[i]);
                        idToPass.Add(id[i]);
                        quantitaTubiToPass.Add(quantitaTubi[i]);
                        lineaToPass.Add(linea[i]);
                    }
                }
                if (idToPass.Count > 0)
                {
                    //queue=FIFO, i save in it the amount of ids, tubes and other stuff.
                    _queue.Enqueue(idToPass.ToArray());
                    _queue.Enqueue(quantitaTubiToPass.ToArray());
                    _queue.Enqueue(lineaToPass.ToArray());
                    _queue.Enqueue(quantitaToPass.ToArray());
                    _signal.Set();
                    //the stuff passes under the Quality Control Area
                    //Console.WriteLine("ACQ");
                }
               
                Thread.Sleep(2000);
            }
        }

        internal void getMPSFromFile(string pathToFile, SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand();
            String query = "INSERT INTO dbo.mps (startDate,dueDate,quantita,tipoTelaio,colore,linea,priorita,running,modified) VALUES (@startDate,@dueDate,@quantita,@tipoTelaio,@colore,@linea,@priorita,@running,@modified)";
            comm = new SqlCommand(query, conn);

            string excelConnection =
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source="+pathToFile+";" +
                @"Extended Properties='Excel 8.0;HDR=Yes;'";
            // The file excel is handled as a database
            using(OleDbConnection connection = new OleDbConnection(excelConnection))
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
                        comm.Parameters.AddWithValue("@dueDate", dr["dueDate"]);
                        comm.Parameters.AddWithValue("@quantita", dr["quantita"]);
                        comm.Parameters.AddWithValue("@tipoTelaio", dr["tipoTelaio"]);
                        comm.Parameters.AddWithValue("@colore", dr["colore"]);
                        comm.Parameters.AddWithValue("@linea", dr["linea"]);
                        comm.Parameters.AddWithValue("@priorita", dr["priorita"]);
                        comm.Parameters.AddWithValue("@running", dr["running"]);

                        comm.Parameters.AddWithValue("@startDate", DateTime.Now);
                        comm.Parameters.AddWithValue("@modified", Int32.Parse("0"));

                        if (conn != null && conn.State == ConnectionState.Closed)
                            conn.Open();
                        try
                        {
                            int result = comm.ExecuteNonQuery();
                            if (result < 0)
                            {
                                Console.WriteLine("\nErrore nell'inserimento dei raw material: result = " + result);
                            }
                        }
                        catch (SqlException e)
                        {
                            //Console.WriteLine("\nRiga nell'MPS vuota o non valida");
                        }
                    }
                    catch (ArgumentException e)
                    {
                        errorString = "formattazione";
                        break;
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