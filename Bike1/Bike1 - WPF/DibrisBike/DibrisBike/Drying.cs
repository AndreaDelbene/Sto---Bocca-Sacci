using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;

namespace DibrisBike
{
    class Drying
    {
        public Drying()
        {
        }

        public void startDrying(SqlConnection conn, ConcurrentQueue<int> _queueEssic, AutoResetEvent _signalEssic, ConcurrentQueue<int> _queueAssemb, AutoResetEvent _signalAssemb, AutoResetEvent _signalError)
        {
            while(true)
            {
                //waiting for the signal
                _signalEssic.WaitOne();
                int idTelaio, idLotto;
                while(_queueEssic.TryDequeue(out idTelaio))
                {
                    _queueEssic.TryDequeue(out idLotto);
                    //simulating the drying
                    Thread.Sleep(6000);

                    Console.WriteLine("ASSEMBLING");

                    string query = "UPDATE dbo.saldessdp SET stato = @stato, endTimeEssic = @endTimeEssic WHERE idTelaio = @idTelaio";

                    SqlCommand comm = new SqlCommand(query, conn);
                    //state is "finisheddry"; from now on the data will be handled by another table
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@stato", "finisheddry");
                    comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                    comm.Parameters.AddWithValue("@endTimeEssic", DateTime.Now);
                   
                    comm.ExecuteNonQuery();

                    //and the state of orders.
                    query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@stato", "assembling");
                    comm.Parameters.AddWithValue("@idLotto", idLotto);

                    comm.ExecuteNonQuery();
                    //Getting the frame type from the order
                    query = "SELECT tipoTelaio FROM dbo.statoordini WHERE idLotto = @idLotto";
                    comm = new SqlCommand(query, conn);
                    SqlDataReader reader;
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@idLotto", idLotto);
                    
                    reader = comm.ExecuteReader();
                    reader.Read();
                    string tipoTelaio = (string)reader["tipoTelaio"];
                    reader.Close();

                    //Getting now a box with the pieces for the assembling, filtering them by the frame type
                    query = "SELECT TOP 1 id FROM dbo.scatole WHERE tipo = @tipoTelaio";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio);
                   
                    reader = comm.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        string idScatola = (string)reader["id"];
                        reader.Close();
                        //if there are still avaiable boxes in the storage

                        //I update the table
                        query = "INSERT INTO dbo.assemblaggiodp (idTelaio, idScatola, startTime) VALUES (@idTelaio, @idScatola, @startTime)";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                        comm.Parameters.AddWithValue("@idScatola", idScatola);
                        comm.Parameters.AddWithValue("@startTime", DateTime.Now);

                        comm.ExecuteNonQuery();

                        //getting the id of the row we just added
                        query = "SELECT id FROM dbo.assemblaggiodp WHERE idTelaio = @idTelaio";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                        
                        reader = comm.ExecuteReader();
                        reader.Read();
                        int idAssemblaggio = (int)reader["id"];
                        reader.Close();

                        //I fill the queue for the assembling
                        _queueAssemb.Enqueue(idAssemblaggio);
                        _queueAssemb.Enqueue(idLotto);
                        //and I signal it
                        _signalAssemb.Set();
                    }
                    else
                    {
                        //else we launch an error and we wait 'til boxes are added again in the storage
                        Console.WriteLine("NO BOXES AVAIABLE FOR ASSEMBLING");
                        _signalError.WaitOne();
                    }
                    Thread.Sleep(3000);
                    //closing the connection
                    //conn.Close();
                }
            }
        }
    }
}
