using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bike1
{
    class Program
    {
        static SqlConnection conn;

        static void Main(string[] args)
        {

            SqlConnection con = new SqlConnection();
            con.ConnectionString =
            "Server=LAPTOP-DT8KB2TQ;" +
            "Database=stodb;" +
            "Integrated Security=True";

            conn = con;
            Thread t1 = new Thread(new ThreadStart(getMPSCaller));

            t1.Start();
        }

        static void getMPSCaller()
        {
            MPS mps = new MPS();
            mps.getMPS(conn);
        }
    }
}
