using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;

namespace DibrisBike
{
    class PrintSO
    {

        private static String query = "SELECT * FROM dbo.statoordini";
        private SqlConnection conn;

        public PrintSO(SqlConnection conn)
        {
            this.conn = conn;
        }

        public DataTable startPrintingThread()
        {
            SqlCommand comm = new SqlCommand(query, conn);
            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();
            comm.ExecuteNonQuery();

            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
    }
}