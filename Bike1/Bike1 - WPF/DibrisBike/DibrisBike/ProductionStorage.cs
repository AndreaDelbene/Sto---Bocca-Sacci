using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DibrisBike
{
    class ProductionStorage
    {
        public ProductionStorage()
        {
        }

        public void updateProductionStorage(SqlConnection conn)
        {
            string query = "SELECT * FROM dbo.statoordini WHERE stato = @stato";
            SqlCommand comm = new SqlCommand(query, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(comm);

            comm.Parameters.Clear();
            comm.Parameters.AddWithValue("@stato", "finished");

            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();

            comm.ExecuteNonQuery();

            DataTable table = new DataTable();
            adapter.Fill(table);

            int[] id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();

            if(id.Length>0)
            {
                int[] idLotto = (from DataRow r in table.Rows select (int)r["idLotto"]).ToArray();
                DateTime[] startEffettiva = (from DataRow r in table.Rows select (DateTime)r["startEffettiva"]).ToArray();
                DateTime[] dueDateEffettiva = (from DataRow r in table.Rows select (DateTime)r["dueDateEffettiva"]).ToArray();
                int[] quantita = (from DataRow r in table.Rows select (int)r["quantitaProdotta"]).ToArray();
                string[] tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                string[] stato = (from DataRow r in table.Rows select (string)r["stato"]).ToArray();

                for(int i=0;i<id.Length;i++)
                {
                    query = "INSERT INTO dbo.storicoproduzione (idLotto, idStatoOrdini, startTime, finishTime, quantita, tipoTelaio) VALUES (idLotto, @idStatoOrdini, @startTime, @finishTime, @quantita, @tipoTelaio)";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@idLotto", idLotto[i]);
                    comm.Parameters.AddWithValue("@idStatoOrdini", id[i]);
                    comm.Parameters.AddWithValue("@startTime", startEffettiva[i]);
                    comm.Parameters.AddWithValue("@dueDateEffettiva", dueDateEffettiva[i]);
                    comm.Parameters.AddWithValue("@quantita", quantita[i]);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                }
            }
            Thread.Sleep(60000);
        }
    }
}
