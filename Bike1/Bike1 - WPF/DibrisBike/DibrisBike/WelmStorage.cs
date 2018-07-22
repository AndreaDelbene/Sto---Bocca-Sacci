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
    class WelmStorage
    {
        public WelmStorage()
        {
        }

        public void setAccumuloSald1(SqlConnection conn, ConcurrentQueue<object> _queueLC1, AutoResetEvent _signalLC1, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald)
        {
            while(true)
            {
                _signalLC1.WaitOne();
                object codiceBarreTemp, idLottoTemp;
                string[] codiceBarre;
                int idLotto;
                _queueLC1.TryDequeue(out codiceBarreTemp);
                _queueLC1.TryDequeue(out idLottoTemp);

                codiceBarre = (string[])codiceBarreTemp;
                idLotto = (int)idLottoTemp;
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i=0;i<codiceBarre.Length;i++)
                {
                    string query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
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
                _queueSald.Enqueue(idLotto);
                //and signaling it to another thread
                _signalSald.Set();
                //conn.Close();

                //Thread.Sleep(2000);
            }
        }

        public void setAccumuloSald2(SqlConnection conn, ConcurrentQueue<object> _queueLC2, AutoResetEvent _signalLC2, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC2.WaitOne();
                object codiceBarreTemp, idLottoTemp;
                string[] codiceBarre;
                int idLotto;

                _queueLC2.TryDequeue(out codiceBarreTemp);
                _queueLC2.TryDequeue(out idLottoTemp);

                codiceBarre = (string[])codiceBarreTemp;
                idLotto = (int)idLottoTemp;
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i = 0; i < codiceBarre.Length; i++)
                {
                    string query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
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
                _queueSald.Enqueue(idLotto);
                //and signaling it to another thread
                _signalSald.Set();
                conn.Close();

                //Thread.Sleep(2000);
            }
        }

        public void setAccumuloSald3(SqlConnection conn, ConcurrentQueue<object> _queueLC3, AutoResetEvent _signalLC3, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC3.WaitOne();
                object codiceBarreTemp, idLottoTemp;
                string[] codiceBarre;
                int idLotto;

                _queueLC3.TryDequeue(out codiceBarreTemp);
                _queueLC3.TryDequeue(out idLottoTemp);

                codiceBarre = (string[])codiceBarreTemp;
                idLotto = (int)idLottoTemp;
                //sleep the Thread (simulating laser cut)
                Thread.Sleep(5000);

                //transponting the tubes from the storage to the welder (saldatrice)
                Console.WriteLine("STORING");
                //updating the storage that contains the tubes to be welmed.
                SqlCommand comm;
                for (int i = 0; i < codiceBarre.Length; i++)
                {
                    string query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
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
                _queueSald.Enqueue(idLotto);
                //and signaling it to another thread
                _signalSald.Set();
                conn.Close();

                //Thread.Sleep(2000);
            }
        }
    }
}
