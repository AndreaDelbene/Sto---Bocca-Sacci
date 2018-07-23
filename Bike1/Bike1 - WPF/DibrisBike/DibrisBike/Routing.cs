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
    class Routing
    {
        public Routing()
        {
        }

        public void routingMagazzino(SqlConnection conn, ConcurrentQueue<object> _queue, AutoResetEvent _signal, ConcurrentQueue<object> _queueLC1, ConcurrentQueue<object> _queueLC2, 
            ConcurrentQueue<object> _queueLC3, AutoResetEvent _signalLC1, AutoResetEvent _signalLC2, AutoResetEvent _signalLC3, AutoResetEvent _signalError, bool flagError)
        {
            while (true)
            {
                //waiting until some data comes from the queue
                _signal.WaitOne();
                //getting it then
                object idLottoTemp, quantitaTubiTemp, lineaTemp, quantitaOrdineTemp;
                int[] idLotto, quantitaTubi, quantitaOrdine;
                string[] linea;
                _queue.TryDequeue(out idLottoTemp);
                _queue.TryDequeue(out quantitaTubiTemp);
                _queue.TryDequeue(out lineaTemp);
                _queue.TryDequeue(out quantitaOrdineTemp);

                idLotto = (int[])idLottoTemp;
                quantitaTubi = (int[])quantitaTubiTemp;
                linea = (string[])lineaTemp;
                quantitaOrdine = (int[])quantitaOrdineTemp;

                string[] tipoTelaio = new string[idLotto.Length];

                for (int i = 0; i < idLotto.Length; i++)
                {
                    //we need to cycle on how many bikes the customer asked for
                    for(int j = 0; j < quantitaOrdine[i]; j++)
                    {
                        //and checking, for each request, whenever I have still tubes in the storage
                        string query = "SELECT TOP (@quantita) * FROM dbo.magazzinomateriali";
                        SqlCommand comm = new SqlCommand(query, conn);

                        comm.Parameters.AddWithValue("@quantita", quantitaTubi[i]);

                        SqlDataAdapter adapter = new SqlDataAdapter(comm);

                        if (conn != null && conn.State == ConnectionState.Closed)
                            conn.Open();

                        comm.ExecuteNonQuery();

                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        string[] codiceBarre;

                        codiceBarre = (from DataRow r in table.Rows select (string)r["codiceBarre"]).ToArray();

                        //If i have it, i proceed in updating the 'routing' table
                        if (table.Rows.Count == quantitaTubi[i])
                        {
                            //deleting every tube I get from the storage.
                            for (int k = 0; k < quantitaTubi.Length; k++)
                            {
                                comm = new SqlCommand(query, conn);
                                query = "DELETE FROM dbo.magazzinomateriali WHERE codiceBarre = @codiceBarre";
                                comm.Parameters.AddWithValue("@codiceBarre", codiceBarre[k]);

                                if (conn != null && conn.State == ConnectionState.Closed)
                                    conn.Open();

                                comm.ExecuteNonQuery();
                            }
                            
                            //selecting the frame (telaio) type
                            query = "SELECT tipoTelaio FROM dbo.mps WHERE id = @idLotto";

                            comm = new SqlCommand(query, conn);
                            SqlDataReader reader;

                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);

                            if (conn != null && conn.State == ConnectionState.Closed)
                                conn.Open();

                            reader = comm.ExecuteReader();
                            reader.Read();

                            tipoTelaio[i] = (string)reader["tipoTelaio"];

                            reader.Close();

                            comm = new SqlCommand(query, conn);
                            //getting a random number to select in which Laser Cut send the set of tubes.
                            //alternatively it's possible to do a control on queue's dimensions and pick the lowest one.
                            Random r = new Random();

                            int rInt = r.Next(1, 4);
                            bool flag = false;
                            int idPercorso = 0;

                            //preparing the insertion into the routing table
                            //Laser Cut step
                            query = "INSERT INTO dbo.routing (idLotto,idPezzo,step,durata,durataSetUp,opMacchina) VALUES (@idLotto,@idPezzo,@step,@durata,@durataSetUp,@opMacchina)";
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 1);
                            comm.Parameters.AddWithValue("@durata", 9);
                            comm.Parameters.AddWithValue("@durataSetUp", 1);


                            switch (tipoTelaio[i])
                            {
                                case "graziella":
                                    comm.Parameters.AddWithValue("@opMacchina", rInt);
                                    flag = true;
                                    idPercorso = rInt;
                                    break;

                                case "corsa":
                                    comm.Parameters.AddWithValue("@opMacchina", rInt);
                                    flag = true;
                                    idPercorso = rInt;
                                    break;

                                case "mbike":
                                    comm.Parameters.AddWithValue("@opMacchina", 3);
                                    idPercorso = 3;
                                    break;

                                case "personalizzato":
                                    comm.Parameters.AddWithValue("@opMacchina", 3);
                                    idPercorso = 3;
                                    break;

                                default:
                                    break;
                            }

                            //and executing the command
                            comm.ExecuteNonQuery();

                            //and keeping updating the routing 
                            //Welming step
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 2);
                            comm.Parameters.AddWithValue("@durata", 8);
                            comm.Parameters.AddWithValue("@durataSetUp", 1);
                            comm.Parameters.AddWithValue("@opMacchina", 9);


                            comm.ExecuteNonQuery();
                            //Furnace Step
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 3);
                            comm.Parameters.AddWithValue("@durata", 8);
                            comm.Parameters.AddWithValue("@durataSetUp", 0);
                            comm.Parameters.AddWithValue("@opMacchina", 10);


                            comm.ExecuteNonQuery();
                            //Painting Step
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 4);
                            comm.Parameters.AddWithValue("@durata", 5);
                            comm.Parameters.AddWithValue("@durataSetUp", 2);

                            if (linea[i].CompareTo("pastello") == 0)
                            {
                                comm.Parameters.AddWithValue("@opMacchina", 11);
                            }
                            else if (linea[i].CompareTo("metallizzato") == 0)
                            {
                                comm.Parameters.AddWithValue("@opMacchina", 12);
                            }

                            comm.ExecuteNonQuery();
                            //Drying Step
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 5);
                            comm.Parameters.AddWithValue("@durata", 6);
                            comm.Parameters.AddWithValue("@durataSetUp", 0);
                            comm.Parameters.AddWithValue("@opMacchina", 13);


                            comm.ExecuteNonQuery();
                            //Assembling Step
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                            comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                            comm.Parameters.AddWithValue("@step", 6);
                            comm.Parameters.AddWithValue("@durata", 4);
                            comm.Parameters.AddWithValue("@durataSetUp", 1);
                            comm.Parameters.AddWithValue("@opMacchina", 14);


                            comm.ExecuteNonQuery();

                            //waiting until the stuff passes under the Quality Control Area
                            Thread.Sleep(5000);

                            query = "INSERT INTO dbo.percorsiveicoli (idPercorso, idVeicolo, tempoAssegnazione, tempoPartenza) VALUES (@idPercorso, @idVeicolo, @tempoAssegnazione, @tempoPartenza)";
                            comm = new SqlCommand(query, conn);
                            comm.Parameters.Clear();
                            string idVeicolo = "AGV" + idPercorso.ToString();
                            comm.Parameters.AddWithValue("@idPercorso", idPercorso+1);
                            comm.Parameters.AddWithValue("@idVeicolo", idVeicolo);
                            comm.Parameters.AddWithValue("@tempoAssegnazione", DateTime.Now.ToString());
                            comm.Parameters.AddWithValue("@tempoPartenza", DateTime.Now.ToString());
                            if (conn != null && conn.State == ConnectionState.Closed)
                                conn.Open();

                            comm.ExecuteNonQuery();

                            query = "SELECT TOP 1 id FROM dbo.perocorsiveicoli";
                            comm = new SqlCommand(query, conn);
                            comm.Parameters.Clear();

                            if (conn != null && conn.State == ConnectionState.Closed)
                                conn.Open();

                            reader = comm.ExecuteReader();
                            reader.Read();

                            int idAssegnazione = (int)reader["id"];

                            reader.Close();


                            //Going for the Laser Cut then
                            Console.WriteLine("LASER CUT");

                            query = "UPDATE stodb.dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                            comm = new SqlCommand(query, conn);
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@stato", "cutting");
                            comm.Parameters.AddWithValue("@idLotto", idLotto);
                            if (conn != null && conn.State == ConnectionState.Closed)
                                conn.Open();

                            comm.ExecuteNonQuery();


                            for(int k=0;k<quantitaTubi.Length;k++)
                            {
                                comm = new SqlCommand(query, conn);
                                query = "INSERT INTO dbo.lasercutdp (codiceTubo, idAssegnazione, startTime) VALUES (@codiceTubo, @idAssegnazione, @startTime)";
                                comm.Parameters.AddWithValue("@codiceTubo", codiceBarre[k]);
                                comm.Parameters.AddWithValue("@idAssegnazione", idAssegnazione);
                                comm.Parameters.AddWithValue("@startTime", DateTime.Now.ToString());

                                if (conn != null && conn.State == ConnectionState.Closed)
                                    conn.Open();

                                comm.ExecuteNonQuery();
                            }
                            if (flag)
                            {
                                //updating the interested LC's queue
                                if (rInt == 1)
                                {
                                    _queueLC1.Enqueue(codiceBarre);
                                    _queueLC1.Enqueue(idLotto[0]);
                                    //signaling the service after the laser cut.
                                    _signalLC1.Set();
                                }
                                else if (rInt == 2)
                                {
                                    _queueLC2.Enqueue(codiceBarre);
                                    _queueLC2.Enqueue(idLotto[0]);
                                    _signalLC2.Set();

                                }
                                else
                                {
                                    _queueLC3.Enqueue(codiceBarre);
                                    _queueLC3.Enqueue(idLotto[0]);
                                    _signalLC3.Set();
                                }

                            }
                            else
                            {
                                _queueLC3.Enqueue(codiceBarre);
                                _queueLC3.Enqueue(idLotto[0]);
                                //signaling the service after the laser cut.
                                _signalLC3.Set();
                            }

                        }
                        else
                        {
                            //launch exception on storage
                            Console.WriteLine("NOT ENOUGH RAW MATERIALS");
                            //and waiting for someone inserting some.
                            flagError = true;
                            _signalError.WaitOne();
                            flagError = false;
                        }
                        //conn.Close();
                    }

                }
                //sleeping the thread for 2 secs
                Thread.Sleep(10000);
            }
        }
    }
}
