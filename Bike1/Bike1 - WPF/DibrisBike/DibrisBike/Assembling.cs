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
    class Assembling
    {
        public Assembling()
        {
        }

        public void startAssembling(SqlConnection conn, ConcurrentQueue<int> _queueAssemb, AutoResetEvent _signalAssemb)
        {
            _signalAssemb.WaitOne();
            int idAssemblaggio, idLotto;
            _queueAssemb.TryDequeue(out idAssemblaggio);
            _queueAssemb.TryDequeue(out idLotto);

            Thread.Sleep(5000);

            string query = "INSERT INTO prodottifinitidp (idAssemblaggio) VALUES (@idAssemblaggio)";
            
            SqlCommand comm = new SqlCommand(query, conn);
            //state is "finisheddry"; from now on the data will be handled by another table
            comm.Parameters.Clear();
            comm.Parameters.AddWithValue("@idAssemblaggio", idAssemblaggio);

            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();

            comm.ExecuteNonQuery();

            query = "SELECT quantitaDesiderata, quantitaProdotta FROM dbo.statoordini WHERE idLotto = @idLotto";
            comm = new SqlCommand(query, conn);
            SqlDataReader reader;
            comm.Parameters.Clear();

            comm.Parameters.AddWithValue("@idLotto", idLotto);
            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();
            
            reader = comm.ExecuteReader();
            reader.Read();
            int quantitaDesiderata = (int)reader["quantitaDesiderata"];
            int quantitaProdotta = (int)reader["quantitaProdotta"];

            if(quantitaDesiderata == quantitaProdotta + 1)
            {
                //and the state of orders.
                query = "UPDATE dbo.statoordini SET stato = @stato, quantitaProdotta = @quantitaProdotta, dueDateEffettiva = @dueDateEffettiva WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@stato", "finished");
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                comm.Parameters.AddWithValue("@quantitaProdotta", quantitaProdotta + 1);
                comm.Parameters.AddWithValue("@dueDateEffettiva", DateTime.Now.ToString());

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                Console.WriteLine("FINISHED");
            }
            else
            {
                //and the state of orders.
                query = "UPDATE dbo.statoordini SET quantitaProdotta = @quantitaProdotta WHERE idLotto = @idLotto";
                comm = new SqlCommand(query, conn);
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@idLotto", idLotto);
                comm.Parameters.AddWithValue("@quantitaProdotta", quantitaProdotta + 1);

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();
            }
            reader.Close();
        }
    }
}
