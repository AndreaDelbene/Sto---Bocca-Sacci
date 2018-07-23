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
                _queueToPaint.TryDequeue(out idTelaio);
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

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                /*query = "SELECT idLotto FROM stodb.dbo.routing INNER JOIN stodb.dbo.accumulosaldaturadp ON stodb.dbo.routing.idPezzo = stodb.dbo.accumulosaldaturadp.codiceTubo " +
                    "INNER JOIN stodb.dbo.saldessdp ON stodb.dbo.saldessdp.idTelaio = @idTelaio";

                comm = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                DataTable table = new DataTable();
                adapter.Fill(table);

                int[] idLottoTemp = (from DataRow r in table.Rows select (int)r["idLotto"]).ToArray();
                int idLotto = idLottoTemp[0];*/

                //updating the order state table
                query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "painting");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //we get then the color and the mode of Printing that the customer selected
                query = "SELECT colore,linea FROM stodb.dbo.mps WHERE id = @idLotto";
                comm = new SqlCommand(query, conn);
                SqlDataReader reader;
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@idLotto", idLotto);

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                reader = comm.ExecuteReader();
                reader.Read();
                string colore = (string)reader["colore"];
                string linea = (string)reader["linea"];

                reader.Close();
                //and we fill the queue, according to values.
                if (linea.CompareTo("pastello")==0)
                {
                    _queuePast.Enqueue(linea);
                    _queuePast.Enqueue(colore);
                    _queuePast.Enqueue(idTelaio);
                    _queuePast.Enqueue(idLotto);
                    _signalPast.Set();
                }
                else if(linea.CompareTo("metallizzato")==0)
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

                conn.Close();
            }
        }
    }
}
