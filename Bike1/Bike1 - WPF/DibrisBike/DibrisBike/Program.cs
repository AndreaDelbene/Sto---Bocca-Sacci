﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bike1
{

    class Program
    {
        static SqlConnection conn;
        //private readonly ConcurrentQueue<Item> _queue = new ConcurrentQueue<Item>();
        //private readonly AutoResetEvent _signal = new AutoResetEvent();


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
            Thread t3 = new Thread(new ThreadStart(routingMagazzinoCaller));

            t1.Start();
            t3.Start();
            t2.Start();
        }

        static void getMPSCaller()
        {
            MPS mps = new MPS();
            //mps.getMPS(conn, _queue, _signal);
        }

        static void getRawMaterial()
        {
            RawMaterial rawMaterial = new RawMaterial(conn);
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"excelFiles\rawMaterial.xlsx");
            rawMaterial.getRawFromFile(path);
        }

        static void routingMagazzinoCaller()
        {
            Routing rm = new Routing();
            //rm.routingMagazzino(conn, _queue, _signal);
        }


        /*void ProducerThread()
        {
            while (ShouldRun)
            {
                Item item = GetNextItem();
                _queue.Enqueue(item);
                _signal.Set();
            }

        }

        void ConsumerThread()
        {
            while (ShouldRun)
            {
                _signal.WaitOne();

                Item item = null;
                while (_queue.TryDequeue(out item))
                {
                    // do stuff
                }
            }
        }*/
    }
}
