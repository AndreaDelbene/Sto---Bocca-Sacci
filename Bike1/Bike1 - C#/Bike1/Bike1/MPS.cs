using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bike1
{

    class MPS
    {

        public MPS()
        {
        }

        public void getMPS(SqlConnection conn)
        {
            while(true)
            {
                string query = "SELECT * FROM stodb.dbo.mps WHERE running = 0";
                SqlCommand comm = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                conn.Open();
                DataTable table = new DataTable();
                adapter.Fill(table);
                int[] id, quantita, priorita;
                string[] tipoTelaio, colore;
                DateTime[] startDate, dueDate;
                Byte[] running;
                id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();
                startDate = (from DataRow r in table.Rows select (DateTime)r["startDate"]).ToArray();
                dueDate = (from DataRow r in table.Rows select (DateTime)r["dueDate"]).ToArray();
                quantita = (from DataRow r in table.Rows select (int)r["quantita"]).ToArray();
                tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                colore = (from DataRow r in table.Rows select (string)r["colore"]).ToArray();
                priorita = (from DataRow r in table.Rows select (int)r["priorita"]).ToArray();
                running = (from DataRow r in table.Rows select (Byte)r["running"]).ToArray();

                string[] quantitaTubi = new string[id.Length];

                conn.Close();
                for(int i = 0; i < id.Length; i++)
                {
                    query = "INSERT INTO stodb.dbo.statoordini (idLotto, startPianificata, startEffettiva, dueDatePianificata, quantitaDesiderata, quantitaProdotta, tipoTelaio, stato, descrizione) " +
                        "VALUES(@idLotto, @startPianificata, @startEffettiva, @dueDatePianificata, @quantitaDesiderata, @quantitaProdotta, @tipoTelaio, @stato, @descrizione)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@idLotto", id[i]);
                    comm.Parameters.AddWithValue("@startPianificata", startDate[i]);
                    comm.Parameters.AddWithValue("@startEffettiva", startDate[i]);
                    comm.Parameters.AddWithValue("@dueDatePianificata", dueDate[i]);
                    comm.Parameters.AddWithValue("@quantitaDesiderata", quantita[i]);
                    comm.Parameters.AddWithValue("@quantitaProdotta", 0);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);
                    comm.Parameters.AddWithValue("@stato", "running");
                    comm.Parameters.AddWithValue("@descrizione", "");
                    conn.Open();
                    comm.ExecuteNonQuery();
                    conn.Close();

                    query = "UPDATE stodb.dbo.mps SET running = 1 WHERE id = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@idLotto", id[i]);

                    conn.Open();
                    comm.ExecuteNonQuery();
                    conn.Close();

                    query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);
                    
                    conn.Open();

                    comm.ExecuteNonQuery();

                    SqlDataReader reader = comm.ExecuteReader();
                    quantitaTubi[i] = (string)reader["quantitaTubi"];
                    conn.Close();
                    Console.WriteLine(i);

                }
                Thread.Sleep(2000);
            }
        }
    }
}
