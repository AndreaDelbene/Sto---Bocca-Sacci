using System;
using System.Collections.Generic;
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

        public void getMPS(SqlConnection conn, int i)
        {
            while(true)
            {
                Console.WriteLine("porcodioMR "+ i);
                Thread.Sleep(2000);
            }
        }
    }
}
