using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace DibrisBike
{
    class ProductionStorage
    {
        public ProductionStorage()
        {
        }

        public void updateProductionStorage(SqlConnection conn)
        {
            while(true)
            {
                //getting the orders that are finished
                string query = "SELECT * FROM dbo.statoordini WHERE stato = @stato";
                SqlCommand comm = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "finished");

                while (conn.State == ConnectionState.Executing || conn.State == ConnectionState.Fetching)
                {
                }

                comm.ExecuteNonQuery();

                DataTable table = new DataTable();
                adapter.Fill(table);

                int[] id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();
                //and if there are any
                if (id.Length > 0)
                {
                    //let's get the infos
                    int[] idLotto = (from DataRow r in table.Rows select (int)r["idLotto"]).ToArray();
                    DateTime[] startEffettiva = (from DataRow r in table.Rows select (DateTime)r["startEffettiva"]).ToArray();
                    DateTime[] dueDateEffettiva = (from DataRow r in table.Rows select (DateTime)r["dueDateEffettiva"]).ToArray();
                    int[] quantita = (from DataRow r in table.Rows select (int)r["quantitaProdotta"]).ToArray();
                    string[] tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                    string[] stato = (from DataRow r in table.Rows select (string)r["stato"]).ToArray();

                    //and insert everyone of them into the production storage
                    for (int i = 0; i < id.Length; i++)
                    {
                        query = "INSERT INTO dbo.storicoproduzione (idLotto, idStatoOrdine, startTime, endTime, quantita, tipoTelaio) VALUES (@idLotto, @idStatoOrdine, @startTime, @endTime, @quantita, @tipoTelaio)";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                        comm.Parameters.AddWithValue("@idStatoOrdine", id[i]);
                        comm.Parameters.AddWithValue("@startTime", startEffettiva[i]);
                        comm.Parameters.AddWithValue("@endTime", dueDateEffettiva[i]);
                        comm.Parameters.AddWithValue("@quantita", quantita[i]);
                        comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                        while (conn.State == ConnectionState.Executing || conn.State == ConnectionState.Fetching)
                        {
                        }

                        comm.ExecuteNonQuery();

                        Console.WriteLine("STORED");

                        //updating the state of the order.
                        query = "UPDATE dbo.statoordini SET stato = @stato WHERE idLotto = @idLotto";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@stato", "stored");
                        comm.Parameters.AddWithValue("@idLotto", idLotto[i]);

                        while (conn.State == ConnectionState.Executing || conn.State == ConnectionState.Fetching)
                        {
                        }

                        comm.ExecuteNonQuery();

                    }
                }
                //and sleeping for a given period
                Thread.Sleep(60000);
            }
        }
    }
}
