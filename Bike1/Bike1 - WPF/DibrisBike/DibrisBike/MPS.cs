using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace DibrisBike
{

    class MPS
    {

        public MPS()
        {
        }

        public void getMPS(SqlConnection conn, ConcurrentQueue<object> _queue, AutoResetEvent _signal)
        {
            while (true)
            {
                //checking whenever a new MPS has been uploaded
                string query = "SELECT * FROM stodb.dbo.mps WHERE running = 0";
                SqlCommand comm = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                //problems with other threads trying to open a connection already opened
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                DataTable table = new DataTable();
                adapter.Fill(table);
                //getting then the data from the table
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

                int[] quantitaTubi = new int[id.Length];

                conn.Close();
                //for each element in the table we got back from the first request
                for (int i = 0; i < id.Length; i++)
                {
                    //I update the 'statoordini' table in the DB
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

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                    //conn.Close();

                    // i set then the flag to 1 into the 'mps' table
                    query = "UPDATE stodb.dbo.mps SET running = 1 WHERE id = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@idLotto", id[i]);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                    //conn.Close();

                    //and i check how many stuff i need for that kind of bike
                    query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();

                    SqlDataReader reader = comm.ExecuteReader();
                    quantitaTubi[i] = (int)reader["quantitaTubi"];
                    conn.Close();
                    Console.WriteLine(i);

                }
                if (id.Length != 0)
                {
                    //queue=FIFO, i save in it the amount of ids and tubes, and i sleep for the next 2 secs.
                    _queue.Enqueue(id);
                    _queue.Enqueue(quantitaTubi);
                    _signal.Set();
                }
                Thread.Sleep(2000);
            }
        }

        internal void getMPSFromFile(string pathToFile, SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand();
            String query = "INSERT INTO dbo.magazzinomateriali (start,dueDate,quantita,tipoTelaio,colore,priorita,running) VALUES (@start,@dueDate,@quantita,@tipoTelaio,@colore,@priorita,@running)";
            comm = new SqlCommand(query, conn);

            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(pathToFile);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            for (int i = 2; i <= rowCount; i++)
            {
                comm.Parameters.Clear();
                for (int j = 1; j <= colCount; j++)
                {
                    if (j == 1)
                    {
                        DateTime campo = (DateTime)xlRange.Cells[i, j].Value2.ToString();
                        comm.Parameters.Add("@start", campo);
                    }
                    else if (j == 2)
                    {
                        DateTime campo = (DateTime)xlRange.Cells[i, j].Value2.ToString();
                        comm.Parameters.Add("@dueDate", campo);
                    }
                    else if (j == 3)
                    {
                        int campo = (int)xlRange.Cells[i, j].Value2;
                        comm.Parameters.Add("@quantita", campo);
                    }
                    else if (j == 4)
                    {
                        String campo = (String)xlRange.Cells[i, j].Value2;
                        comm.Parameters.Add("@tipoTelaio", campo);
                    }
                    else if (j == 5)
                    {
                        String campo = (String)xlRange.Cells[i, j].Value2;
                        comm.Parameters.Add("@colore", campo);
                    }
                    else if (j == 6)
                    {
                        int campo = (int)xlRange.Cells[i, j].Value2;
                        comm.Parameters.Add("@priorita", campo);
                    }
                    else
                    {
                        int campo = (int)xlRange.Cells[i, j].Value2;
                        comm.Parameters.Add("@running", campo);
                    }
                }

                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();
                try
                {
                    int result = comm.ExecuteNonQuery();
                    if (result < 0)
                    {
                        Console.WriteLine("Errore nell'inserimento dei raw material: result = " + result);
                    }
                }
                catch (SqlException e)
                {
                    //Console.WriteLine(e.Errors);
                    Console.WriteLine(e.ToString());
                }
                //conn.Close();
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            Console.WriteLine("Lettura e salvattagio MPS completato");
        }
    }
}