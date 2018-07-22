using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DibrisBike
{
    class Furnace
    {
        public Furnace()
        {
        }

        public void startCooking(SqlConnection conn, ConcurrentQueue<int> _queueForno, AutoResetEvent _signalForno, ConcurrentQueue<int> _queueToPaint, AutoResetEvent _signalToPaint)
        {
            while(true)
            {
                _signalForno.WaitOne();
                int idTelaio;
                int idLotto;
                _queueForno.TryDequeue(out idTelaio);
                _queueForno.TryDequeue(out idLotto);
                // simulating the Welmer
                Thread.Sleep(5000);

                //Let's cook the frame now!
                Console.WriteLine("FURNACE");
                string query = "UPDATE dbo.saldessdp SET stato = @stato, endTimeSald = @endTimeSald, startTimeForno = @startTimeForno WHERE idTelaio = @idTelaio";

                SqlCommand comm = new SqlCommand(query, conn);
                //state is "cooking"
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "cooking");
                comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                comm.Parameters.AddWithValue("@endTimeSald", DateTime.Now.ToString());
                comm.Parameters.AddWithValue("@startTimeForno", DateTime.Now.ToString());

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //updating the order state table
                query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "cooking");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                //filling the queue for the Painting and signaling it
                _queueToPaint.Enqueue(idTelaio);
                _queueToPaint.Enqueue(idLotto);
                _signalToPaint.Set();
                conn.Close();
            }
        }
    }
}
