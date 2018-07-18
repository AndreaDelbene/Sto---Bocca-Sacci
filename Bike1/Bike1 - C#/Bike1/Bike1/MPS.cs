using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bike1
{

    class MPS
    {

        public MPS()
        {
        }

        public void getMPS(SqlConnection conn, ConcurrentQueue<object> _queue, AutoResetEvent _signal)
        {
            while(true)
            {
                //checking whenever a new MPS has been uploaded
                string query = "SELECT * FROM stodb.dbo.mps WHERE running = 0";
                SqlCommand comm = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                conn.Open();
                //and filling the results in a Table
                DataTable table = new DataTable();
                adapter.Fill(table);
                //getting then the data from the table
                int[] id, quantita, priorita;
                string[] tipoTelaio, colore;
                DateTime[] startDate, dueDate;
                Byte[] running;
                id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();
                startDate = (from DataRow r in table.Rows select (DateTime)r["startDate"]).ToArray();
                dueDate = (from DataRow r in table.Rows select (DateTime)r["dueDate"]).ToArray();
                quantita = (from DataRow r in table.Rows select (int)r["quantita"]).ToArray();
                tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                colore = (from DataRow r in table.Rows select (string)r["colore"]).ToArray();
                priorita = (from DataRow r in table.Rows select (int)r["priorita"]).ToArray();
                running = (from DataRow r in table.Rows select (Byte)r["running"]).ToArray();

                int[] quantitaTubi = new int[id.Length];
                
                conn.Close();
                //for each element in the table we got back from the first request
                for(int i = 0; i < id.Length; i++)
                {
                    //I update the 'statoordini' table in the DB
                    query = "INSERT INTO stodb.dbo.statoordini (idLotto, startPianificata, startEffettiva, dueDatePianificata, quantitaDesiderata, quantitaProdotta, tipoTelaio, stato, descrizione) " +
                        "VALUES(@idLotto, @startPianificata, @startEffettiva, @dueDatePianificata, @quantitaDesiderata, @quantitaProdotta, @tipoTelaio, @stato, @descrizione)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@idLotto", id[i]);
                    comm.Parameters.AddWithValue("@startPianificata", startDate[i]);
                    comm.Parameters.AddWithValue("@startEffettiva", startDate[i]);
                    comm.Parameters.AddWithValue("@dueDatePianificata", dueDate[i]);
                    comm.Parameters.AddWithValue("@quantitaDesiderata", quantita[i]);
                    comm.Parameters.AddWithValue("@quantitaProdotta", 0);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);
                    comm.Parameters.AddWithValue("@stato", "running");
                    comm.Parameters.AddWithValue("@descrizione", "");
                    conn.Open();
                    comm.ExecuteNonQuery();
                    conn.Close();

                    // i set then the flag to 1 into the 'mps' table
                    query = "UPDATE stodb.dbo.mps SET running = 1 WHERE id = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@idLotto", id[i]);

                    conn.Open();
                    comm.ExecuteNonQuery();
                    conn.Close();

                    //and i check how many stuff i need for that kind of bike
                    query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);
                    
                    conn.Open();

                    comm.ExecuteNonQuery();

                    SqlDataReader reader = comm.ExecuteReader();
                    quantitaTubi[i] = (int)reader["quantitaTubi"];
                    conn.Close();
                    Console.WriteLine(i);

                }
                //queue=FIFO, i save in it the amount of ids and tubes, and i sleep for the next 2 secs.
                _queue.Enqueue(id);
                _queue.Enqueue(quantitaTubi);
                _signal.Set();
                Thread.Sleep(2000);
            }
        }
    }
}
