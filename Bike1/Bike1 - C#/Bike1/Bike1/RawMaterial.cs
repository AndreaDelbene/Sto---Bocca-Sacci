using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Bike1
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
            columnName = new List<string>();
            columnName.Add("@codiceBarre");
            columnName.Add("@descrizione");
            columnName.Add("@diametro");
            columnName.Add("@peso");
            columnName.Add("@lunghezza");
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
            String campo;
            String nomeCampo;
            for (int i = 2; i <= rowCount; i++)
            {
                comm.Parameters.Clear();
                for (int j = 1; j <= colCount; j++)
                {
                    if (j == 1)
                        Console.Write("\r\n");
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        /*String colName = (String)xlRange.Cells[0, j].Value2.toString();
                        if (columnName.Contains(colName))
                        {
                            comm.Parameters.Add("@"+colName, xlRange.Cells[i, j].Value2);
                        }*/
                        campo = xlRange.Cells[i, j].Value2.ToString();
                        Console.Write(campo + "\t");
                        comm.Parameters.Add(columnName[j-1], campo);
                    }
                }
                conn.Open();
                try
                {
                    int result = comm.ExecuteNonQuery();
                    if (result < 0)
                    {
                        Console.WriteLine("Errore nell'inserimento dei raw material");
                    }
                }
                catch(SqlException e)
                {
                    //Console.WriteLine(e.Errors);
                    Console.WriteLine("Errore nell'inserimento dei raw material");
                }
                conn.Close();
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
        }
    }
}
