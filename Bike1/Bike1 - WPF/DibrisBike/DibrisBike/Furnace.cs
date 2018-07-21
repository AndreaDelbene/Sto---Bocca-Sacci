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

        public void startCooking(SqlConnection conn, ConcurrentQueue<int> _queueForno, AutoResetEvent _signalForno, ConcurrentQueue<int> _queueToPrint, AutoResetEvent _signalToPrint)
        {
            while(true)
            {
                _signalForno.WaitOne();
                int idTelaio;
                _queueForno.TryDequeue(out idTelaio);
                // simulating the Welmer
                Thread.Sleep(5000);

                //Let's cook the frame now!
                Console.WriteLine("FURNACE");
                string query = "UPDATE stodb.dbo.saldessdp SET stato = @stato WHERE idTelaio = @idTelaio";

                SqlCommand comm = new SqlCommand(query, conn);
                //state is "cooking"
                comm.Parameters.AddWithValue("@stato", "cooking");
                comm.Parameters.AddWithValue("@idTelaio", idTelaio);

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                _queueToPrint.Enqueue(idTelaio);
                conn.Close();
            }
        }
    }
}
