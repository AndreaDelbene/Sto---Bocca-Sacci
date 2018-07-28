using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace DibrisBike
{
    class WeldStorage
    {
        public WeldStorage()
        {
        }

        public void setAccumuloSald1(SqlConnection conn, ConcurrentQueue<object> _queueLC1, AutoResetEvent _signalLC1, ConcurrentQueue<object> _queueSald, AutoResetEvent _signalSald, AutoResetEvent _signalErrorLC1, AutoResetEvent _signalWaitErrorLC1, AutoResetEvent _signalFixLC1, ConcurrentQueue<Boolean> _queueBlockLC1)
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

                    //first cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 1 + "_P2");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    // Simulating probability of error
                    // signal to the thread that generate the error
                    _signalWaitErrorLC1.Set();
                    bool result = _signalErrorLC1.WaitOne(5000);
                    if (result)
                    {
                        Boolean block;
                        _queueBlockLC1.TryDequeue(out block);
                        if (block)
                        {
                            // if the error blocks the system then wait until repair
                            _signalFixLC1.WaitOne();
                        }
                    }
                    Console.WriteLine(result);

                    //second cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 1 + "_P3");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    _signalWaitErrorLC1.Set();
                    result = _signalErrorLC1.WaitOne(5000);
                    Console.WriteLine(result);

                    //transponting the tubes from the storage to the welder (saldatrice)
                    //Console.WriteLine("STORING FOR WELDING");
                    //updating the storage that contains the tubes to be welded.
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
                    
                    //first cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 2 + "_P2");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(5000);

                    //second cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 2 + "_P3");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(5000);

                    //transponting the tubes from the storage to the welder (saldatrice)
                    //Console.WriteLine("STORING FOR WELDING");
                    //updating the storage that contains the tubes to be welded.
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

                    //first cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 3 + "_P2");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(5000);

                    //second cut
                    query = "INSERT INTO dbo.processirt (type, date, value) VALUES (@type, @date, @value)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@type", "LC00" + 3 + "_P3");
                    comm.Parameters.AddWithValue("@date", DateTime.Now);
                    comm.Parameters.AddWithValue("@value", 0);

                    comm.ExecuteNonQuery();

                    //sleep the Thread (simulating laser cut)
                    Thread.Sleep(5000);
                    //transponting the tubes from the storage to the welder (saldatrice)
                    //Console.WriteLine("STORING FOR WELDING");
                    //updating the storage that contains the tubes to be welded.
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
