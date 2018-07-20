using System;
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
    class RawMaterial
    {

        SqlConnection conn;
        private String query;
        private SqlCommand comm;
        List<String> columnName;

        public RawMaterial(SqlConnection conn)
        {
            this.conn = conn;
            query = "INSERT INTO dbo.magazzinomateriali (codiceBarre,descrizione,diametro,peso,lunghezza) VALUES (@codiceBarre,@descrizione,@diametro,@peso,@lunghezza)";
            comm = new SqlCommand(query, conn);
        }

        public void getRawFromFile(String pathToFile)
        {
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
                        Console.Write("\r\n");
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        if (j == 1)
                        {
                            String campo = xlRange.Cells[i, j].Value2.ToString();
                            comm.Parameters.Add("@codiceBarre", campo);
                            Console.Write(campo + "\t");
                        }
                        else if (j == 2)
                        {
                            String campo = xlRange.Cells[i, j].Value2.ToString();
                            comm.Parameters.Add("@descrizione", campo);
                            Console.Write(campo + "\t");
                        }
                        else if (j == 3)
                        {
                            float campo = (float)xlRange.Cells[i, j].Value2;
                            comm.Parameters.Add("@diametro", campo);
                            Console.Write(campo.ToString() + "\t");
                        }
                        else if (j == 4)
                        {
                            float campo = (float)xlRange.Cells[i, j].Value2;
                            comm.Parameters.Add("@peso", campo);
                            Console.Write(campo.ToString() + "\t");
                        }
                        else
                        {
                            float campo = (float)xlRange.Cells[i, j].Value2;
                            comm.Parameters.Add("@lunghezza", campo);
                            Console.Write(campo.ToString() + "\t");
                        }
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

            Console.WriteLine("Lettura e salvattagio raw material completata");
        }
    }
}
