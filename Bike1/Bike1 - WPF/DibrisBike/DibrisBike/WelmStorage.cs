using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;

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
                object codiceBarreTemp, idLottoTemp, idAssegnTemp;
                string[] codiceBarre;
                int idLotto, idAssegnazione;
                while(_queueLC1.TryDequeue(out codiceBarreTemp))
                {
                    _queueLC1.TryDequeue(out idLottoTemp);
                    _queueLC1.TryDequeue(out idAssegnTemp);

                    codiceBarre = (string[])codiceBarreTemp;
                    idLotto = (int)idLottoTemp;
                    idAssegnazione = (int)idAssegnTemp;

                    SqlCommand comm;
                    string query = "UPDATE dbo.percorsiveicoli SET tempoArrivo = @tempoArrivo WHERE id = @id";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@tempoArrivo", DateTime.Now);
                    comm.Parameters.AddWithValue("@id", idAssegnazione);

                    comm.ExecuteNonQuery();
                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(10000);

                    //transponting the tubes from the storage to the welder (saldatrice)
                    Console.WriteLine("STORING FOR WELMING");
                    //updating the storage that contains the tubes to be welmed.
                    for (int i = 0; i < codiceBarre.Length; i++)
                    {
                        query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        comm.Parameters.AddWithValue("@descrizione", "");
                        comm.Parameters.AddWithValue("@diametro", 5.0);
                        comm.Parameters.AddWithValue("@peso", 10.2);
                        comm.Parameters.AddWithValue("@lunghezza", 1.7);

                        comm.ExecuteNonQuery();

                        //updating the lasercut table for each tube
                        query = "UPDATE dbo.lasercutdp SET endTime = @endTime WHERE codiceTubo = @codiceTubo";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@endTime", DateTime.Now);
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        
                        comm.ExecuteNonQuery();
                    }
                    //insereting the bar codes into the queue for the next step
                    _queueSald.Enqueue(codiceBarre);
                    _queueSald.Enqueue(idLotto);
                    //and signaling it to another thread
                    _signalSald.Set();
                }
            }
        }

        public void setAccumuloSald2(SqlConnection conn, ConcurrentQueue<object> _queueLC2, AutoResetEvent _signalLC2, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC2.WaitOne();
                object codiceBarreTemp, idLottoTemp, idAssegnTemp;
                string[] codiceBarre;
                int idLotto, idAssegnazione;
                while(_queueLC2.TryDequeue(out codiceBarreTemp))
                {
                    _queueLC2.TryDequeue(out idLottoTemp);
                    _queueLC2.TryDequeue(out idAssegnTemp);

                    codiceBarre = (string[])codiceBarreTemp;
                    idLotto = (int)idLottoTemp;
                    idAssegnazione = (int)idAssegnTemp;

                    SqlCommand comm;
                    string query = "UPDATE dbo.percorsiveicoli SET tempoArrivo = @tempoArrivo WHERE id = @id";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@tempoArrivo", DateTime.Now);
                    comm.Parameters.AddWithValue("@id", idAssegnazione);

                    comm.ExecuteNonQuery();
                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(10000);

                    //transponting the tubes from the storage to the welder (saldatrice)
                    Console.WriteLine("STORING FOR WELMING");
                    //updating the storage that contains the tubes to be welmed.
                    for (int i = 0; i < codiceBarre.Length; i++)
                    {
                        query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        comm.Parameters.AddWithValue("@descrizione", "");
                        comm.Parameters.AddWithValue("@diametro", 5.0);
                        comm.Parameters.AddWithValue("@peso", 10.2);
                        comm.Parameters.AddWithValue("@lunghezza", 1.7);
                        
                        comm.ExecuteNonQuery();

                        query = "UPDATE dbo.lasercutdp SET endTime = @endTime WHERE codiceTubo = @codiceTubo";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@endTime", DateTime.Now);
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        
                        comm.ExecuteNonQuery();
                    }
                    //insereting the bar codes into the queue for the next step
                    _queueSald.Enqueue(codiceBarre);
                    _queueSald.Enqueue(idLotto);
                    //and signaling it to another thread
                    _signalSald.Set();
                }
            }
        }

        public void setAccumuloSald3(SqlConnection conn, ConcurrentQueue<object> _queueLC3, AutoResetEvent _signalLC3, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald)
        {
            while (true)
            {
                _signalLC3.WaitOne();
                object codiceBarreTemp, idLottoTemp, idAssegnTemp;
                string[] codiceBarre;
                int idLotto, idAssegnazione;
                while(_queueLC3.TryDequeue(out codiceBarreTemp))
                {
                    _queueLC3.TryDequeue(out idLottoTemp);
                    _queueLC3.TryDequeue(out idAssegnTemp);

                    codiceBarre = (string[])codiceBarreTemp;
                    idLotto = (int)idLottoTemp;
                    idAssegnazione = (int)idAssegnTemp;

                    SqlCommand comm;
                    string query = "UPDATE dbo.percorsiveicoli SET tempoArrivo = @tempoArrivo WHERE id = @id";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@tempoArrivo", DateTime.Now);
                    comm.Parameters.AddWithValue("@id", idAssegnazione);
                    
                    comm.ExecuteNonQuery();
                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(10000);

                    //transponting the tubes from the storage to the welder (saldatrice)
                    Console.WriteLine("STORING FOR WELMING");
                    //updating the storage that contains the tubes to be welmed.
                    for (int i = 0; i < codiceBarre.Length; i++)
                    {
                        query = "INSERT INTO dbo.accumulosaldaturadp (codiceTubo, descrizione, diametro, peso, lunghezza) VALUES (@codiceTubo, @descrizione, @diametro, @peso, @lunghezza)";

                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        comm.Parameters.AddWithValue("@descrizione", "");
                        comm.Parameters.AddWithValue("@diametro", 5.0);
                        comm.Parameters.AddWithValue("@peso", 10.2);
                        comm.Parameters.AddWithValue("@lunghezza", 1.7);
                        
                        comm.ExecuteNonQuery();

                        //updating the lasercut table for each tube
                        query = "UPDATE dbo.lasercutdp SET endTime = @endTime WHERE codiceTubo = @codiceTubo";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@endTime", DateTime.Now);
                        comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[i]);
                        
                        comm.ExecuteNonQuery();
                    }
                    //insereting the bar codes into the queue for the next step
                    _queueSald.Enqueue(codiceBarre);
                    _queueSald.Enqueue(idLotto);
                    //and signaling it to another thread
                    _signalSald.Set();
                }
            }
        }
    }
}
