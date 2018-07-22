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

        public void startSaldatura(SqlConnection conn, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald, ConcurrentQueue<int> _queueForno, AutoResetEvent _signalForno)
        {
            while(true)
            {
                _signalSald.WaitOne();
                object codiceBarreTemp,idLottoTemp;
                string[] codiceBarre;
                int idLotto;
                _queueSald.TryDequeue(out codiceBarreTemp);
                _queueSald.TryDequeue(out idLottoTemp);
                codiceBarre = (string[])codiceBarreTemp;
                idLotto = (int)idLottoTemp;

                string query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                SqlCommand comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "storing for welming");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                //transponting the tubes from the storage to the welder (saldatrice)
                Thread.Sleep(2000);

                //going for the welmer then
                Console.WriteLine("WELMING");
                //creating a new row into the table that contains frames that are being welmed/cooked/painted/dried
                query = "INSERT INTO dbo.saldessdp (startTimeSald,stato) VALUES (@startTimeSald, @stato)";

                comm = new SqlCommand(query, conn);
                //state is "welding"
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@startTimeSald", DateTime.Now.ToString());
                comm.Parameters.AddWithValue("@stato", "welding");
                
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
                //once we have a number for the frame, we get it
                query = "SELECT TOP 1 idTelaio FROM dbo.saldess ORDER BY idTelaio DESC";

                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                SqlDataReader reader;
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                reader = comm.ExecuteReader();
                reader.Read();
                int idTelaio = (int)reader["idTelaio"];
                reader.Close();
                //and then we come back to update the storage infos.
                for (int i=0; i<codiceBarre.Length;i++)
                {
                    query = "UPDATE dbo.accumulosaldaturadp SET idTelaio = @idTelaio WHERE codiceTubo = @codiceTubo";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@idTelaio", idTelaio);
                    comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);


                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                }

                query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "welming");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                //setting data into the queue for the Furnace
                _queueForno.Enqueue(idTelaio);
                _queueForno.Enqueue(idLotto);
                //and we signal it.
                _signalForno.Set();
                conn.Close();
            }
        }
    }
}
