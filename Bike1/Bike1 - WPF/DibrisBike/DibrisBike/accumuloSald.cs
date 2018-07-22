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
    class AccumuloSald
    {
        public AccumuloSald()
        {
        }

        public void setAccumuloSald1(SqlConnection conn, ConcurrentQueue<string[]> _queueLC1, AutoResetEvent _signalLC1, ConcurrentQueue<string[]> _queueSald, AutoResetEvent _signalSald)
        {
            while(true)
            {
                _signalLC1.WaitOne();
                string[] codiceBarre;
                
                _queueLC1.TryDequeue(out codiceBarre);
               
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i=0;i<codiceBarre.Length;i++)
                {
                    string query = "INSERT INTO stodb.dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                     comm = new SqlCommand(query, conn);

                    comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                    comm.Parameters.AddWithValue("@descrizione", "");
                    comm.Parameters.AddWithValue("@diametro", 5.0);
                    comm.Parameters.AddWithValue("@peso", 10.2);
                    comm.Parameters.AddWithValue("@lunghezza", 1.7);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                }
                //insereting the bar codes into the queue for the next step
                _queueSald.Enqueue(codiceBarre);
                //and signaling it to another thread
                _signalSald.Set();
                conn.Close();

                //Thread.Sleep(2000);
            }
        }

        public void setAccumuloSald2(SqlConnection conn, ConcurrentQueue<string[]> _queueLC2, AutoResetEvent _signalLC2, ConcurrentQueue<string[]> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC2.WaitOne();
                string[] codiceBarre;
               
                _queueLC2.TryDequeue(out codiceBarre);
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i = 0; i < codiceBarre.Length; i++)
                {
                    string query = "INSERT INTO stodb.dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                    comm = new SqlCommand(query, conn);

                    comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                    comm.Parameters.AddWithValue("@descrizione", "");
                    comm.Parameters.AddWithValue("@diametro", 5.0);
                    comm.Parameters.AddWithValue("@peso", 10.2);
                    comm.Parameters.AddWithValue("@lunghezza", 1.7);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                }
                //insereting the bar codes into the queue for the next step
                _queueSald.Enqueue(codiceBarre);
                //and signaling it to another thread
                _signalSald.Set();
                conn.Close();

                //Thread.Sleep(2000);
            }
        }

        public void setAccumuloSald3(SqlConnection conn, ConcurrentQueue<string[]> _queueLC3, AutoResetEvent _signalLC3, ConcurrentQueue<string[]> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC3.WaitOne();
                string[] codiceBarre;
                
                _queueLC3.TryDequeue(out codiceBarre);
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i = 0; i < codiceBarre.Length; i++)
                {
                    string query = "INSERT INTO stodb.dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                    comm = new SqlCommand(query, conn);

                    comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                    comm.Parameters.AddWithValue("@descrizione", "");
                    comm.Parameters.AddWithValue("@diametro", 5.0);
                    comm.Parameters.AddWithValue("@peso", 10.2);
                    comm.Parameters.AddWithValue("@lunghezza", 1.7);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                }
                //insereting the bar codes into the queue for the next step
                _queueSald.Enqueue(codiceBarre);
                //and signaling it to another thread
                _signalSald.Set();
                conn.Close();

                //Thread.Sleep(2000);
            }
        }
    }
}
