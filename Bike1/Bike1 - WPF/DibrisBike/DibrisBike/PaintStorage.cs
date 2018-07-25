using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;

namespace DibrisBike
{
    class PaintStorage
    {
        public PaintStorage()
        {
        }

        public void setAccumuloPaint(SqlConnection conn, ConcurrentQueue<int> _queueToPaint, AutoResetEvent _signalToPaint, ConcurrentQueue<object> _queuePast, ConcurrentQueue<object> _queueMetal, AutoResetEvent _signalPast, AutoResetEvent _signalMetal)
        {
            while(true)
            {
                _signalToPaint.WaitOne();
                int idTelaio;
                int idLotto;
                while(_queueToPaint.TryDequeue(out idTelaio))
                {
                    _queueToPaint.TryDequeue(out idLotto);
                    // simulating the Furnace
                    Thread.Sleep(8000);

                    //Let's Paint the frame now!
                    Console.WriteLine("PAINTING");
                    string query = "UPDATE dbo.saldessdp SET stato = @stato, endTimeForno = @endTimeForno, startTimePaint = @startTimePaint WHERE idTelaio = @idTelaio";

                    SqlCommand comm = new SqlCommand(query, conn);
                    //state is "painting"
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@stato", "painting");
                    comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                    comm.Parameters.AddWithValue("@endTimeForno", DateTime.Now.ToString());
                    comm.Parameters.AddWithValue("@startTimePaint", DateTime.Now.ToString());

                    comm.ExecuteNonQuery();

                    //updating the order state table
                    query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@stato", "painting");
                    comm.Parameters.AddWithValue("@idLotto", idLotto);

                    comm.ExecuteNonQuery();

                    //we get then the color and the mode of Printing that the customer selected
                    query = "SELECT colore,linea FROM stodb.dbo.mps WHERE id = @idLotto";
                    comm = new SqlCommand(query, conn);
                    SqlDataReader reader;
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@idLotto", idLotto);
                    
                    reader = comm.ExecuteReader();

                    reader.Read();

                    string colore = (string)reader["colore"];
                    string linea = (string)reader["linea"];

                    reader.Close();
                    //and we fill the queue, according to values.
                    if (linea.CompareTo("pastello") == 0)
                    {
                        _queuePast.Enqueue(linea);
                        _queuePast.Enqueue(colore);
                        _queuePast.Enqueue(idTelaio);
                        _queuePast.Enqueue(idLotto);
                        _signalPast.Set();
                    }
                    else if (linea.CompareTo("metallizzato") == 0)
                    {

                        _queueMetal.Enqueue(linea);
                        _queueMetal.Enqueue(colore);
                        _queueMetal.Enqueue(idTelaio);
                        _queueMetal.Enqueue(idLotto);
                        _signalMetal.Set();
                    }
                    else
                    {
                        Console.WriteLine("NO MODE DEFINED?!");
                    }
                }
            }
        }
    }
}
