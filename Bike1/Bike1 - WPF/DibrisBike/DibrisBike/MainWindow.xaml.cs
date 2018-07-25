using System;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using Microsoft.Win32;
using System.Data;
using System.Windows.Media;

namespace DibrisBike
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Connection
        static private SqlConnection conn;
        // queues and signals for MPS - Routing
        static private readonly ConcurrentQueue<object> _queue = new ConcurrentQueue<object>();
        static private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        static private readonly AutoResetEvent _signalError = new AutoResetEvent(false);
        static private readonly AutoResetEvent _singalErrorRM = new AutoResetEvent(false);
        static private bool flagError = false;
        //queues and signals for Routing - WelmStorage
        static private readonly ConcurrentQueue<object> _queueLC1 = new ConcurrentQueue<object>();
        static private readonly ConcurrentQueue<object> _queueLC2 = new ConcurrentQueue<object>();
        static private readonly ConcurrentQueue<object> _queueLC3 = new ConcurrentQueue<object>();
        static private readonly AutoResetEvent _signalLC1 = new AutoResetEvent(false);
        static private readonly AutoResetEvent _signalLC2 = new AutoResetEvent(false);
        static private readonly AutoResetEvent _signalLC3 = new AutoResetEvent(false);
        //queues and signals for WelmStorage - Welming
        static private readonly ConcurrentQueue<object> _queueSald = new ConcurrentQueue<object>();
        static private readonly AutoResetEvent _signalSald = new AutoResetEvent(false);
        //queues and signals for Welming - Furnace
        static private readonly ConcurrentQueue<int> _queueForno = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalForno = new AutoResetEvent(false);
        //queues and signals for Furnace - PaintStorage
        static private readonly ConcurrentQueue<int> _queueToPaint = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalToPaint = new AutoResetEvent(false);
        //queues and signals for PaintStorage - Painting
        static private readonly ConcurrentQueue<object> _queuePast = new ConcurrentQueue<object>();
        static private readonly AutoResetEvent _signalPast = new AutoResetEvent(false);
        static private readonly ConcurrentQueue<object> _queueMetal = new ConcurrentQueue<object>();
        static private readonly AutoResetEvent _signalMetal = new AutoResetEvent(false);
        //queues and signals for Painting - Drying
        static private readonly ConcurrentQueue<int> _queueEssic = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalEssic = new AutoResetEvent(false);
        //queues and signals for Drying - Assembling
        static private readonly ConcurrentQueue<int> _queueAssemb = new ConcurrentQueue<int>();
        static private readonly AutoResetEvent _signalAssemb = new AutoResetEvent(false);
        static private readonly AutoResetEvent _signalErrorEssic = new AutoResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            SqlConnection con = new SqlConnection();
            //SIMONE - PC\\SQLEXPRESS
            //LAPTOP - DT8KB2TQ;
            con.ConnectionString =
            "Server=SIMONE-PC\\SQLEXPRESS;" +
            "Database=stodb;" +
            "Integrated Security=True;" +
            "MultipleActiveResultSets=true;";

            conn = con;
            conn.Open();


            Thread t1 = new Thread(new ThreadStart(getMPSCaller));
            Thread t2 = new Thread(new ThreadStart(routingMagazzinoCaller));
            Thread t3 = new Thread(new ThreadStart(printStatoOrdini));
            Thread t41 = new Thread(new ThreadStart(accumuloSaldCaller1));
            Thread t42 = new Thread(new ThreadStart(accumuloSaldCaller2));
            Thread t43 = new Thread(new ThreadStart(accumuloSaldCaller3));
            Thread t5 = new Thread(new ThreadStart(saldCaller));
            Thread t6 = new Thread(new ThreadStart(furnaceCaller));
            Thread t7 = new Thread(new ThreadStart(accumuloPaintCaller));
            Thread t81 = new Thread(new ThreadStart(pastPaintCaller));
            Thread t82 = new Thread(new ThreadStart(metalPaintCaller));
            Thread t9 = new Thread(new ThreadStart(dryCaller));
            Thread t10 = new Thread(new ThreadStart(assembCaller));
            Thread t11 = new Thread(new ThreadStart(signalErrorChangeListener));
            Thread t12 = new Thread(new ThreadStart(checkFinishedCaller));

            t1.Start();
            t2.Start();
            t3.Start();
            t41.Start();
            t42.Start();
            t43.Start();
            t5.Start();
            t6.Start();
            t7.Start();
            t81.Start();
            t82.Start();
            t9.Start();
            t10.Start();
            t11.Start();
            t12.Start();
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
                    {
                        conn.Open();
                    }
                    else
                    {
                        while (conn.State == ConnectionState.Connecting)
                        {
                            // wait
                        }
                        comm.ExecuteNonQuery();

                        SqlDataAdapter adapter = new SqlDataAdapter(comm);
                        DataTable table = new DataTable();
                        if (table != null)
                        {
                            adapter.Fill(table);
                            statoOrdiniGrid.ItemsSource = table.DefaultView;
                            adapter.Update(table);
                        }
                    }
                });
                Thread.Sleep(2000);
            }
        }

        static void routingMagazzinoCaller()
        {
            Routing rm = new Routing();
            rm.routingMagazzino(conn, _queue, _signal, _queueLC1, _queueLC2, _queueLC3, _signalLC1, _signalLC2, _signalLC3, _signalError, _singalErrorRM);
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
                MPSPathLabel.Content = MPSFilePath;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    getMPS(MPSFilePath);
                }));
                thread.Start();
            }
        }

        private void getMPS(string mpsFilePath)
        {
            MPS mps = new MPS();
            mps.getMPSFromFile(mpsFilePath, conn);
            updateLabel(MPSPathLabel, "MPS caricato con successo");
        }

        private void RMChooser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Excel File|*.xlsx";
            fileDialog.DefaultExt = ".xlsx";
            Nullable<bool> dialogOk = fileDialog.ShowDialog();

            String rawMaterialFilePath = string.Empty;
            if (dialogOk == true)
            {
                rawMaterialFilePath = fileDialog.FileName;
                RMPathLabel.Content = rawMaterialFilePath;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    getRawMaterial(rawMaterialFilePath);
                }));
                thread.Start();
            }
        }

        private void getRawMaterial(String path)
        {
            RawMaterial rawMaterial = new RawMaterial(conn, _signalError);
            rawMaterial.getRawFromFile(path);
            updateLabel(RMPathLabel, "Raw Material caricato con successo");
        }

        private void updateLabel(Label label, string message)
        {
            Action action = () => label.Content = message;
            Dispatcher.Invoke(action);
        }

        private void updateMPSLabel(string message)
        {
            Action action = () => MPSPathLabel.Content = message;
            Dispatcher.Invoke(action);
        }

        static void accumuloSaldCaller1()
        {
            WelmStorage aS = new WelmStorage();
            aS.setAccumuloSald1(conn, _queueLC1, _signalLC1, _queueSald, _signalSald);
        }

        static void accumuloSaldCaller2()
        {
            WelmStorage aS = new WelmStorage();
            aS.setAccumuloSald2(conn, _queueLC2, _signalLC2, _queueSald, _signalSald);
        }

        static void accumuloSaldCaller3()
        {
            WelmStorage aS = new WelmStorage();
            aS.setAccumuloSald3(conn, _queueLC3, _signalLC3, _queueSald, _signalSald);
        }

        static void saldCaller()
        {
            Welming sald = new Welming();
            sald.startSaldatura(conn, _queueSald, _signalSald, _queueForno, _signalForno);
        }

        static void furnaceCaller()
        {
            Furnace fur = new Furnace();
            fur.startCooking(conn, _queueForno, _signalForno, _queueToPaint, _signalToPaint);
        }

        static void accumuloPaintCaller()
        {
            PaintStorage ap = new PaintStorage();
            ap.setAccumuloPaint(conn, _queueToPaint, _signalToPaint, _queuePast, _queueMetal, _signalPast, _signalMetal);
        }

        private void ordiniModify_Click(object sender, RoutedEventArgs e)
        {
            ModifyOrdiniPage modifyOrdiniPage = new ModifyOrdiniPage();
            modifyOrdiniPage.Show();
        }

        static void pastPaintCaller()
        {
            Painting paint = new Painting();
            paint.startPaintingPast(conn, _queuePast, _signalPast, _queueEssic, _signalEssic);
        }

        static void metalPaintCaller()
        {
            Painting paint = new Painting();
            paint.startPaintingMetal(conn, _queueMetal, _signalMetal, _queueEssic, _signalEssic);
        }

        static void dryCaller()
        {
            Drying dry = new Drying();
            dry.startDrying(conn, _queueEssic, _signalEssic, _queueAssemb, _signalAssemb, _signalErrorEssic);
        }

        static void assembCaller()
        {
            Assembling assemb = new Assembling();
            assemb.startAssembling(conn, _queueAssemb, _signalAssemb);
        }

        static void checkFinishedCaller()
        {
            ProductionStorage ps = new ProductionStorage();
            ps.updateProductionStorage(conn);
        }

        private void signalErrorChangeListener()
        {
            while (true)
            {
                this.Dispatcher.BeginInvoke(
                    new Action(
                        delegate ()
                        {
                            RMAlert.Fill = new SolidColorBrush(Colors.ForestGreen);
                            RMAlert.Stroke = new SolidColorBrush(Colors.ForestGreen);
                            RMAlertLabel.Content = "Raw material sufficienti";
                        }
                        ));

                _singalErrorRM.WaitOne();

                this.Dispatcher.BeginInvoke(
                    new Action(
                        delegate ()
                        {
                            RMAlert.Fill = new SolidColorBrush(Colors.OrangeRed);
                            RMAlert.Stroke = new SolidColorBrush(Colors.OrangeRed);
                            RMAlertLabel.Content = "Raw material insufficienti";
                        }
                        ));
            }
        }

        private void seeFinishedProducts_Click(object sender, RoutedEventArgs e)
        {
            FinishProductPage finishProductPage = new FinishProductPage();
            finishProductPage.Show();
        }
    }
}