using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
                string query = "SELECT * FROM dbo.mps WHERE running = 0";
                SqlCommand comm = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                //problems with other threads trying to open a connection already opened
                if (conn != null && conn.State == ConnectionState.Closed)
                    conn.Open();

                comm.ExecuteNonQuery();

                DataTable table = new DataTable();
                adapter.Fill(table);
                //getting then the data from the table
                int[] id, quantita, priorita;
                string[] tipoTelaio, colore, linea;
                DateTime[] startDate, dueDate;
                Byte[] running;

                //if we have more than one order, they may have different priorities
                priorita = (from DataRow r in table.Rows select (int)r["priorita"]).ToArray();
                
                if (priorita.Length > 1)
                {
                    //sorting the rows on the "priorita" column
                    DataView dv = table.DefaultView;
                    dv.Sort = "priorita desc";
                    DataTable sortedTable = dv.ToTable();
                    //then getting the data
                    id = (from DataRow r in sortedTable.Rows select (int)r["id"]).ToArray();
                    startDate = (from DataRow r in sortedTable.Rows select (DateTime)r["startDate"]).ToArray();
                    dueDate = (from DataRow r in sortedTable.Rows select (DateTime)r["dueDate"]).ToArray();
                    quantita = (from DataRow r in sortedTable.Rows select (int)r["quantita"]).ToArray();
                    tipoTelaio = (from DataRow r in sortedTable.Rows select (string)r["tipoTelaio"]).ToArray();
                    colore = (from DataRow r in sortedTable.Rows select (string)r["colore"]).ToArray();
                    linea = (from DataRow r in sortedTable.Rows select (string)r["linea"]).ToArray();
                    priorita = (from DataRow r in sortedTable.Rows select (int)r["priorita"]).ToArray();
                    running = (from DataRow r in sortedTable.Rows select (Byte)r["running"]).ToArray();
                }
                else
                {
                    //else we get data normally from the table.
                    id = (from DataRow r in table.Rows select (int)r["id"]).ToArray();
                    startDate = (from DataRow r in table.Rows select (DateTime)r["startDate"]).ToArray();
                    dueDate = (from DataRow r in table.Rows select (DateTime)r["dueDate"]).ToArray();
                    quantita = (from DataRow r in table.Rows select (int)r["quantita"]).ToArray();
                    tipoTelaio = (from DataRow r in table.Rows select (string)r["tipoTelaio"]).ToArray();
                    colore = (from DataRow r in table.Rows select (string)r["colore"]).ToArray();
                    linea = (from DataRow r in table.Rows select (string)r["linea"]).ToArray();
                    running = (from DataRow r in table.Rows select (Byte)r["running"]).ToArray();
                }

                int[] quantitaTubi = new int[id.Length];

           

                //conn.Close();
                //for each element in the table we got back from the first request
                for (int i = 0; i < id.Length; i++)
                {
                    //I update the 'statoordini' table in the DB
                    query = "INSERT INTO dbo.statoordini (idLotto, startPianificata, startEffettiva, dueDatePianificata, quantitaDesiderata, quantitaProdotta, tipoTelaio, stato, descrizione) " +
                        "VALUES(@idLotto, @startPianificata, @startEffettiva, @dueDatePianificata, @quantitaDesiderata, @quantitaProdotta, @tipoTelaio, @stato, @descrizione)";

                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
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

                    //I set then the flag to 1 into the 'mps' table
                    query = "UPDATE stodb.dbo.mps SET running = 1 WHERE id = @idLotto";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@idLotto", id[i]);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();
                    //conn.Close();

                    //and I check how many stuff I need for that kind of bike
                    query = "SELECT quantitaTubi FROM dbo.ricette WHERE tipoTelaio = @tipoTelaio";
                    comm = new SqlCommand(query, conn);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@tipoTelaio", tipoTelaio[i]);

                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();
                    
                    SqlDataReader reader = comm.ExecuteReader();

                    reader.Read();

                    quantitaTubi[i] = (int)reader["quantitaTubi"];
                    
                    //conn.Close();
                    reader.Close();

                }
                if (id.Length > 0)
                {
                    //queue=FIFO, i save in it the amount of ids, tubes and other stuff.
                    _queue.Enqueue(id);
                    _queue.Enqueue(quantitaTubi);
                    _queue.Enqueue(linea);
                    _queue.Enqueue(quantita);
                    _signal.Set();
                    //the stuff passes under the Quality Control Area
                    Console.WriteLine("ACQ");
                }
                //conn.Close();
                Thread.Sleep(2000);
            }
        }

        internal void getMPSFromFile(string pathToFile, SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand();
            String query = "INSERT INTO dbo.mps (startDate,dueDate,quantita,tipoTelaio,colore,linea,priorita,running) VALUES (@startDate,@dueDate,@quantita,@tipoTelaio,@colore,@linea,@priorita,@running)";
            comm = new SqlCommand(query, conn);

            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(pathToFile);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            //iterate over the rows and columns
            //excel is not zero based!!
            bool flagError;
            for (int i = 2; i <= rowCount; i++)
            {
                flagError = false;
                comm.Parameters.Clear();
                for (int j = 1; j <= colCount; j++)
                {
                    switch (j)
                    {
                        case 1:
                            String temp = Convert.ToString(xlRange.Cells[i, j].Value2);
                            if (temp != null)
                            {
                                double campo1 = double.Parse(temp);
                                DateTime date1 = DateTime.FromOADate(campo1);
                                comm.Parameters.AddWithValue("@dueDate", date1);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;

                        case 2:
                            String campo2 = xlRange.Cells[i, j].Value2.ToString();
                            DateTime date2 = DateTime.ParseExact(campo2, "MM/dd/yy HH:mm:ss", null);
                            comm.Parameters.AddWithValue("@dueDate", date2);
                            break;

                        case 3:
                            String campo4 = (String)xlRange.Cells[i, j].Value2;
                            if (campo4 != null)
                            {
                                comm.Parameters.AddWithValue("@tipoTelaio", campo4);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;

                        case 4:
                            String campo5 = (String)xlRange.Cells[i, j].Value2;
                            if (campo5 != null)
                            {
                                comm.Parameters.AddWithValue("@colore", campo5);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;

                        case 5:
                            String linea = (String)xlRange.Cells[i, j].Value2;
                            if (linea != null)
                            {
                                comm.Parameters.AddWithValue("@linea", linea);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;
                        case 6:
                            Object campo6 = xlRange.Cells[i, j].Value2;
                            if (campo6 != null)
                            {
                                comm.Parameters.AddWithValue("@priorita", (int)(double)campo6);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;

                        case 7:
                            Object campodef = xlRange.Cells[i, j].Value2;
                            if (campodef != null)
                            {
                                int campodefInt = Int32.Parse(Convert.ToString(campodef));
                                comm.Parameters.AddWithValue("@running", campodefInt);
                            }
                            else
                            {
                                flagError = true;
                            }
                            break;
                    }
                }

                if (!flagError)
                {
                    comm.Parameters.AddWithValue("@startDate", DateTime.Now);

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

            Console.WriteLine("Lettura e salvataggio MPS completato");
        }
    }
}