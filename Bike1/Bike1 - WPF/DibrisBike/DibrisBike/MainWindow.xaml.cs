using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Win32;
using System.Data;

namespace DibrisBike
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static SqlConnection conn;
        static private readonly ConcurrentQueue<int[]> _queue = new ConcurrentQueue<int[]>();
        static private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            SqlConnection con = new SqlConnection();
            con.ConnectionString =
            "Server=SIMONE-PC\\SQLEXPRESS;" +
            "Database=stodb;" +
            "Integrated Security=True;" +
            "MultipleActiveResultSets=true";

            conn = con;
            conn.Open();
            Thread t1 = new Thread(new ThreadStart(getMPSCaller));
            Thread t3 = new Thread(new ThreadStart(routingMagazzinoCaller));
            Thread t4 = new Thread(new ThreadStart(printStatoOrdini));

            t1.Start();
            t3.Start();
            t4.Start();
        }

        static void getMPSCaller()
        {
            MPS mps = new MPS();
            mps.getMPS(conn, _queue, _signal);
        }

        public void printStatoOrdini()
        {
            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    String query = "SELECT * FROM dbo.statoordini";
                    SqlCommand comm = new SqlCommand(query, conn);
                    if (conn != null && conn.State == ConnectionState.Closed)
                        conn.Open();

                    comm.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(comm);
                    DataTable table = new DataTable();
                    if (table != null)
                    {
                        adapter.Fill(table);
                        statoOrdiniGrid.ItemsSource = table.DefaultView;
                        adapter.Update(table);
                    }
                });
                Thread.Sleep(2000);
            }
        }

        static void routingMagazzinoCaller()
        {
            Routing rm = new Routing();
            rm.routingMagazzino(conn, _queue, _signal);
        }

        private void MPSChooser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Excel File|*.xlsx";
            fileDialog.DefaultExt = ".xlsx";
            Nullable<bool> dialogOk = fileDialog.ShowDialog();

            String MPSFilePath = string.Empty;
            if (dialogOk == true)
            {
                MPSFilePath = fileDialog.FileName;
                RMPathLabel.Content = MPSFilePath;
                getMPS(MPSFilePath);
            }
        }

        private void getMPS(string mpsFilePath)
        {
            MPS mps = new MPS();
            mps.getMPSFromFile(mpsFilePath, conn);
        }

        private void RMChooser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Excel File|*.xlsx";
            fileDialog.DefaultExt = ".xlsx";
            Nullable<bool> dialogOk = fileDialog.ShowDialog();

            String rawMaterialFilePath = string.Empty;
            if(dialogOk == true)
            {
                rawMaterialFilePath = fileDialog.FileName;
                RMPathLabel.Content = rawMaterialFilePath;
                getRawMaterial(rawMaterialFilePath);
            }
        }

        static void getRawMaterial(String path)
        {
            RawMaterial rawMaterial = new RawMaterial(conn);
            rawMaterial.getRawFromFile(path);
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
