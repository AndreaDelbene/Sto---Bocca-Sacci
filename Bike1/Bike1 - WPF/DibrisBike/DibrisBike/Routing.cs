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

        public void routingMagazzino(SqlConnection conn, ConcurrentQueue<int[]> _queue, AutoResetEvent _signal, ConcurrentQueue<object> _queueLC1, ConcurrentQueue<object> _queueLC2, ConcurrentQueue<object> _queueLC3, AutoResetEvent _signalLC1, AutoResetEvent _signalLC2, AutoResetEvent _signalLC3)
        {
            while (true)
            {
                //waiting until some data comes from the queue
                _signal.WaitOne();
                //getting it then
                int[] idLotto,quantitaTubi;
                _queue.TryDequeue(out idLotto);
                _queue.TryDequeue(out quantitaTubi);

                string[] tipoTelaio = new string[idLotto.Length];

                for (int i = 0; i < idLotto.Length; i++)
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
                        for(int j=0;j<quantitaTubi.Length;j++)
                        {
                            comm = new SqlCommand(query, conn);
                            query = "DELETE FROM dbo.magazzinomateriali WHERE codiceBarre = @codiceBarre";
                            comm.Parameters.AddWithValue("@codiceBarre", codiceBarre[j]);

                            if (conn != null && conn.State == ConnectionState.Closed)
                                conn.Open();

                            comm.ExecuteNonQuery();
                        }
                        //waiting until the stuff passes under the Quality Control Area
                        Thread.Sleep(5000);

                        //Going for the Laser Cut then
                        Console.WriteLine("LASER CUT");

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

                        comm = new SqlCommand(query, conn);
                        //getting a random number to select in which Laser Cut send the set of tubes.
                        //alternatively it's possible to do a control on queue's dimensions and pick the lowest one.
                        Random r = new Random();
                        int rInt = r.Next(1, 4);
                        bool flag = false;

                        //preparing the insertion into the routing table
                        query = "INSERT INTO dbo.routing (idLotto,idPezzo,step,durata,durataSetUp,opMacchina) VALUES (@idLotto,@idPezzo,@step,@durata,@durataSetUp,@opMacchina)";
                        comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                        comm.Parameters.AddWithValue("@idPezzo", codiceBarre[i]);
                        comm.Parameters.AddWithValue("@step", 1);
                        comm.Parameters.AddWithValue("@durata", 5);
                        comm.Parameters.AddWithValue("@durataSetUp", 0);


                        switch (tipoTelaio[i])
                        {
                            case "graziella":
                                comm.Parameters.AddWithValue("@opMacchina", rInt);
                                flag = true;
                                break;

                            case "corsa":
                                comm.Parameters.AddWithValue("@opMacchina", rInt);
                                flag = true;
                                break;

                            case "mbike":
                                comm.Parameters.AddWithValue("@opMacchina", 3);
                                break;

                            case "personalizzato":
                                comm.Parameters.AddWithValue("@opMacchina", 3);
                                break;

                            default:
                                break;
                        }

                        //and executing the command
                        comm.ExecuteNonQuery();

                        //TODO: Complete the routing process--------------------------------------------------------------------------------------------------

                        query = "UPDATE stodb.dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.AddWithValue("@stato", "cutting");
                        comm.Parameters.AddWithValue("@idLotto", idLotto);
                        if (conn != null && conn.State == ConnectionState.Closed)
                            conn.Open();

                        comm.ExecuteNonQuery();

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
                        //launch exception on storage?
                        Console.WriteLine("NOT ENOUGH RAW MATERIALS");
                        //TODO: signal to wait --------------------------------------------------------------------------------------
                    }
                    //conn.Close();
                }
                //sleeping the thread for 2 secs
                Thread.Sleep(10000);
            }
        }
    }
}
