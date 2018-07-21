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
    class Saldatura
    {
        public Saldatura()
        {
        }

        public void startSaldatura(SqlConnection conn, ConcurrentQueue<string[]> _queueSald, AutoResetEvent _signalSald, ConcurrentQueue<int> _queueForno, AutoResetEvent _signalForno)
        {
            while(true)
            {
                _signalSald.WaitOne();
                string[] codiceBarre;
                _queueSald.TryDequeue(out codiceBarre);
                //transponting the tubes from the storage to the welder (saldatrice)
                Thread.Sleep(2000);

                //going for the welmer then
                Console.WriteLine("WELMING");
                //creating a new row into the table that contains frames that are being welmed/cooked/painted/dried
                string query = "INSERT INTO stodb.dbo.saldessdp (startTime,stato) VALUES (@startTime, @stato)";

                SqlCommand comm = new SqlCommand(query, conn);
                //state is "welding"
                comm.Parameters.AddWithValue("@startTime", DateTime.Now.ToString());
                comm.Parameters.AddWithValue("@stato", "welding");
                
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                //once we have a number for the frame, we get it
                query = "SELECT TOP 1 idTelaio FROM sto.dbo.saldess ORDER BY idTelaio DESC";

                comm = new SqlCommand(query, conn);
                SqlDataReader reader;
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                reader = comm.ExecuteReader();
                reader.Read();
                int idTelaio = (int)reader["idTelaio"];
                //and then we come back to update the storage infos.
                for (int i=0; i<codiceBarre.Length;i++)
                {
                    query = "UPDATE stodb.dbo.accumulosaldaturadp SET idTelaio = @idTelaio WHERE codiceTubo = @codiceTubo";

                    comm = new SqlCommand(query, conn);

                    comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                    comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);


                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                }
                //setting data into the queue for the Furnace
                _queueForno.Enqueue(idTelaio);
                //and we signal it.
                _signalForno.Set();
                conn.Close();
            }
        }
    }
}
