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

        static private SqlConnection conn;

        static private readonly ConcurrentQueue<int[]> _queue = new ConcurrentQueue<int[]>();
        static private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        static private readonly ConcurrentQueue<string[]> _queueLC1 = new ConcurrentQueue<string[]>();
        static private readonly ConcurrentQueue<string[]> _queueLC2 = new ConcurrentQueue<string[]>();
        static private readonly ConcurrentQueue<string[]> _queueLC3 = new ConcurrentQueue<string[]>();
        static private readonly AutoResetEvent _signalLC = new AutoResetEvent(false);

        static private readonly ConcurrentQueue<string[]> _queueSald = new ConcurrentQueue<string[]>();
        static private readonly AutoResetEvent _signalSald = new AutoResetEvent(false);

        static private readonly ConcurrentQueue<int> _queueForno = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalForno = new AutoResetEvent(false);

        static private readonly ConcurrentQueue<int> _queueToPrint = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalToPrint = new AutoResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            SqlConnection con = new SqlConnection();
            //SIMONE-PC\\SQLEXPRESS;
            con.ConnectionString =
            "Server=LAPTOP-DT8KB2TQ;" +
            "Database=stodb;" +
            "Integrated Security=True;" +
            "MultipleActiveResultSets=true";

            conn = con;
            conn.Open();
            Thread t1 = new Thread(new ThreadStart(getMPSCaller));
            Thread t2 = new Thread(new ThreadStart(routingMagazzinoCaller));
            Thread t3 = new Thread(new ThreadStart(printStatoOrdini));
            Thread t4 = new Thread(new ThreadStart(accumuloSaldCaller));
            Thread t5 = new Thread(new ThreadStart(saldCaller));
            Thread t6 = new Thread(new ThreadStart(furnaceCaller));

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();
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
            rm.routingMagazzino(conn, _queue, _signal, _queueLC1, _queueLC2, _queueLC3, _signalLC);
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

        static void accumuloSaldCaller()
        {
            AccumuloSald aS = new AccumuloSald();
            aS.setAccumuloSald(conn, _queueLC1, _queueLC2, _queueLC3, _signalLC, _queueSald, _signalSald);
        }

        static void saldCaller()
        {
            Saldatura sald = new Saldatura();
            sald.startSaldatura(conn, _queueSald, _signalSald, _queueForno, _signalForno);
        }

        static void furnaceCaller()
        {
            Furnace fur = new Furnace();
            fur.startCooking(conn, _queueForno, _signalForno, _queueToPrint, _signalToPrint);
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
