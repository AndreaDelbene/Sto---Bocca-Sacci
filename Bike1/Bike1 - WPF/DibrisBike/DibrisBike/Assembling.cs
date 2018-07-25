using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;

namespace DibrisBike
{
    class Assembling
    {
        public Assembling()
        {
        }

        public void startAssembling(SqlConnection conn, ConcurrentQueue<int> _queueAssemb, AutoResetEvent _signalAssemb)
        {
            while(true)
            {
                _signalAssemb.WaitOne();
                int idAssemblaggio, idLotto;
                while(_queueAssemb.TryDequeue(out idAssemblaggio))
                {
                    _queueAssemb.TryDequeue(out idLotto);
                    //Simulating the Assembling
                    Thread.Sleep(5000);

                    //updating the assembling table
                    string query = "UPDATE dbo.assemblaggiodp SET endTime = @endTime";
                    SqlCommand comm = new SqlCommand(query, conn);

                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@endTime", DateTime.Now);

                    comm.ExecuteNonQuery();

                    query = "SELECT quantitaDesiderata, quantitaProdotta FROM dbo.statoordini WHERE idLotto = @idLotto";
                    comm = new SqlCommand(query, conn);
                    SqlDataReader reader;
                    comm.Parameters.Clear();

                    comm.Parameters.AddWithValue("@idLotto", idLotto);

                    reader = comm.ExecuteReader();
                    reader.Read();
                    int quantitaDesiderata = (int)reader["quantitaDesiderata"];
                    int quantitaProdotta = (int)reader["quantitaProdotta"];

                    reader.Close();

                    if (quantitaDesiderata == quantitaProdotta + 1)
                    {
                        //and the state of orders.
                        query = "UPDATE dbo.statoordini SET stato = @stato, quantitaProdotta = @quantitaProdotta, dueDateEffettiva = @dueDateEffettiva WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@stato", "finished");
                        comm.Parameters.AddWithValue("@idLotto", idLotto);
                        comm.Parameters.AddWithValue("@quantitaProdotta", quantitaProdotta + 1);
                        comm.Parameters.AddWithValue("@dueDateEffettiva", DateTime.Now);

                        comm.ExecuteNonQuery();

                        //and inserting into the finished products
                        query = "INSERT INTO dbo.prodottifinitidp (idAssemblaggio) VALUES (@idAssemblaggio)";

                        comm = new SqlCommand(query, conn);
                        //state is "finisheddry"; from now on the data will be handled by another table
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idAssemblaggio", idAssemblaggio);

                        comm.ExecuteNonQuery();

                        Console.WriteLine("FINISHED");
                    }
                    else
                    {
                        //and the state of orders.
                        query = "UPDATE dbo.statoordini SET quantitaProdotta = @quantitaProdotta WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idLotto", idLotto);
                        comm.Parameters.AddWithValue("@quantitaProdotta", quantitaProdotta + 1);
                        
                        comm.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
