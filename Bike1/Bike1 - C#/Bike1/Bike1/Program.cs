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
            "Server=SIMONE-PC\\SQLEXPRESS;" +
            "Database=stodb;" +
            "Integrated Security=True";

            conn = con;
            Thread t1 = new Thread(new ThreadStart(getMPSCaller));
            Thread t2 = new Thread(new ThreadStart(getRawMaterial));

            t1.Start();
            t2.Start();
        }

        static void getMPSCaller()
        {
            MPS mps = new MPS();
            mps.getMPS(conn,1);
        }

        static void getRawMaterial()
        {
            RawMaterial rawMaterial = new RawMaterial(conn);
            rawMaterial.getRawFromFile(@"C:\Users\Simone\Desktop\rawMaterial.xlsx");
        }
    }
}
