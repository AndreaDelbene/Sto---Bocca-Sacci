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
    class Painting
    {
        public Painting()
        {
        }

        public void startPaintingPast(SqlConnection conn, ConcurrentQueue<object> _queuePast, AutoResetEvent _signalPast, ConcurrentQueue<int> _queueEssic, AutoResetEvent _signalEssic)
        {
            while(true)
            {
                //waiting for the signal
                _signalPast.WaitOne();
                int idLotto, idTelaio;
                string colore, linea;
                object idLottoTemp, idTelaioTemp, coloreTemp, lineaTemp;
                //getting stuff from the queue
                _queuePast.TryDequeue(out lineaTemp);
                _queuePast.TryDequeue(out coloreTemp);
                _queuePast.TryDequeue(out idTelaioTemp);
                _queuePast.TryDequeue(out idLottoTemp);
                
                idLotto = (int)idLottoTemp;
                idTelaio = (int)idTelaioTemp;
                colore = (string)coloreTemp;
                linea = (string)lineaTemp;

                // simulating the Paint
                Thread.Sleep(7000);

                //Let's Dry the frame now!
                Console.WriteLine("DRYING");

                //updating then the table
                string query = "UPDATE dbo.saldessdp SET stato = @stato, endTimePaint = @endTimePaint, startTimeEssic = @startTimeEssic WHERE idTelaio = @idTelaio";

                SqlCommand comm = new SqlCommand(query, conn);
                //state is "drying"
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "drying");
                comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                comm.Parameters.AddWithValue("@endTimePaint", DateTime.Now.ToString());
                comm.Parameters.AddWithValue("@startTimeEssic", DateTime.Now.ToString());

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //and the state of orders.
                query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "drying");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //filling the queue for the drying
                _queueEssic.Enqueue(idTelaio);
                _queueEssic.Enqueue(idLotto);
                //signaling it
                _signalEssic.Set();
                //and closing the connection
                conn.Close();
            }
        }

        public void startPaintingMetal(SqlConnection conn, ConcurrentQueue<object> _queueMetal, AutoResetEvent _signalMetal, ConcurrentQueue<int> _queueEssic, AutoResetEvent _signalEssic)
        {
            while (true)
            {
                //on this side we are dealing with Metal line of printing
                //operations are the same as the above method.
                _signalMetal.WaitOne();
                int idLotto, idTelaio;
                string colore, linea;
                object idLottoTemp, idTelaioTemp, coloreTemp, lineaTemp;

                _queueMetal.TryDequeue(out lineaTemp);
                _queueMetal.TryDequeue(out coloreTemp);
                _queueMetal.TryDequeue(out idTelaioTemp);
                _queueMetal.TryDequeue(out idLottoTemp);

                idLotto = (int)idLottoTemp;
                idTelaio = (int)idTelaioTemp;
                colore = (string)coloreTemp;
                linea = (string)lineaTemp;

                // simulating the Paint
                Thread.Sleep(5000);

                //Let's Dry the frame now!
                Console.WriteLine("DRYING");

                string query = "UPDATE dbo.saldessdp SET stato = @stato, endTimePaint = @endTimePaint, startTimeEssic = @startTimeEssic WHERE idTelaio = @idTelaio";

                SqlCommand comm = new SqlCommand(query, conn);
                //state is "drying"
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "drying");
                comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                comm.Parameters.AddWithValue("@endTimePaint", DateTime.Now.ToString());
                comm.Parameters.AddWithValue("@startTimeEssic", DateTime.Now.ToString());

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //updating 'statoordini'
                query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "drying");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                //and preparing the stuff for the next step.
                _queueEssic.Enqueue(idTelaio);
                _queueEssic.Enqueue(idLotto);
                _signalEssic.Set();

                conn.Close();
            }
        }
    }
}
