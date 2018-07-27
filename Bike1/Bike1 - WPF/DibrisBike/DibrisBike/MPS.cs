using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace DibrisBike
{

    class MPS
    {
        private bool flagModif = false;
        private DataTable dtSchema;
        private string Sheet1;

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

                        int quantitaOld= (int)reader["quantitaDesiderata"];

                        reader.Close();

                        //updating the order's state
                        query = "UPDATE dbo.statoordini SET quantitaDesiderata = @quantitaDesiderata WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@quantitaDesiderata", quantita[i]);
                        comm.Parameters.AddWithValue("@idLotto", id[i]);

                        comm.ExecuteNonQuery();

                        //and setting the quantity to produce = new - old
                        quantita[i] = quantita[i] - quantitaOld;
                        //if we decided to decrement the quantity, then we should not send any infos to steps that come next
                        if (quantita[i] < 0)
                            flagModif = true;

                        //and let's update the 'mps' table too
                        query = "UPDATE dbo.mps SET modified = 0 WHERE id = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
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
                    }
                }
                if (id.Length > 0 && !flagModif)
                {
                    //queue=FIFO, i save in it the amount of ids, tubes and other stuff.
                    _queue.Enqueue(id);
                    _queue.Enqueue(quantitaTubi);
                    _queue.Enqueue(linea);
                    _queue.Enqueue(quantita);
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

            using(OleDbConnection connection = new OleDbConnection(excelConnection))
            {
                connection.Open();
                dtSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");
                DataTable dt = new DataTable();
                //OleDbCommand command = new OleDbCommand("select * from ["+Sheet1+"]", connection);
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
                    Console.WriteLine();
                    foreach (var item in dr.ItemArray)
                    {
                        Console.Write(item + "\t");
                    }
                    comm.Parameters.AddWithValue("@dueDate", dr[0]);
                    comm.Parameters.AddWithValue("@quantita", dr[1]);
                    comm.Parameters.AddWithValue("@tipoTelaio", dr[2]);
                    comm.Parameters.AddWithValue("@colore", dr[3]);
                    comm.Parameters.AddWithValue("@linea", dr[4]);
                    comm.Parameters.AddWithValue("@priorita", dr[5]);
                    comm.Parameters.AddWithValue("@running", dr[6]);

                    comm.Parameters.AddWithValue("@startDate", DateTime.Now);
                    comm.Parameters.AddWithValue("@modified", Int32.Parse("0"));

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
                        Console.WriteLine("Riga nell'MPS vuota o non valida");
                    }
                }
            }
        }
    }
}