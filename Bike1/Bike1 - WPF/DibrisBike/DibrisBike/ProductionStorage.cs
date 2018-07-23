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
            }
            Thread.Sleep(60000);
        }
    }
}
