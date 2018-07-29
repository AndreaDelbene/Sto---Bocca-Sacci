using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
using System.Windows.Shapes;

namespace DibrisBike
{
    /// <summary>
    /// Logica di interazione per ErrorPage.xaml
    /// </summary>
    public partial class ErrorPage : Window
    {

        private List<String> list = new List<string>();
        private string index;
        private DataTable table;
        private SqlConnection conn;
        private int id;
        private string machineType;
        private AutoResetEvent _signalFixLC1;

        public ErrorPage(AutoResetEvent _signalFixLC1)
        {
            InitializeComponent();

            conn = new SqlConnection();
            //SIMONE-PC\\SQLEXPRESS;
            //LAPTOP-DT8KB2TQ;
            conn.ConnectionString =
            "Server=SIMONE-PC\\SQLEXPRESS;" +
            "Database=stodb;" +
            "Integrated Security=True;" +
            "MultipleActiveResultSets=true;";

            conn.Open();

            SetFieldsComboBox();

            Thread thread = new Thread(new ThreadStart(FillDataGrid));

            this._signalFixLC1 = _signalFixLC1;

            thread.Start();
        }

        private void SetFieldsComboBox()
        {
            String query = "SELECT * FROM dbo.allarmirt";
            SqlCommand comm = new SqlCommand(query, conn);
            comm.ExecuteNonQuery();

            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            table = new DataTable();
            adapter.Fill(table);

            // Get columns name
            for (int i = 0; i < table.Columns.Count; i++)
            {
                list.Add(table.Columns[i].ColumnName.ToString());
            }

            fields.ItemsSource = list;
            fields.SelectedIndex = 0;
        }

        private void FillDataGrid()
        {
            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();
            String query = "SELECT * FROM dbo.allarmirt WHERE solved = 0";
            SqlCommand comm = new SqlCommand(query, conn);

            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    comm.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(comm);
                    table = new DataTable();
                    adapter.Fill(table);
                    errorGrid.ItemsSource = table.DefaultView;
                });
                Thread.Sleep(500);
            }
        }

        private void fields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            index = comboBox.SelectedItem as string;
        }

        private void keywordBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            (table.DefaultView as DataView).RowFilter = string.Format("convert({0},'System.String') LIKE '%{1}%'", index, keywordBox.Text);
        }

        private void errorGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            fixError.Visibility = Visibility.Visible;
            DataRowView dr = errorGrid.SelectedItem as DataRowView;
            DataRow dr1 = dr.Row;
            id = (int)dr1["id"];
            machineType = (String)dr1["type"];
            machineType = machineType.Substring(0,machineType.LastIndexOf("_"));
        }

        private void fixError_Click(object sender, RoutedEventArgs e)
        {
            string queryNumberOfProduct = "UPDATE dbo.allarmirt SET solved=1 WHERE id=(@id)";
            SqlCommand comm = new SqlCommand(queryNumberOfProduct, conn);
            comm.Parameters.AddWithValue("@id", id);
            comm.ExecuteNonQuery();
            switch (machineType)
            {
                case "LC001":
                    _signalFixLC1.Set();
                    break;
            }
            this.Close();
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
