using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
    /// Logica di interazione per ModifyOrdiniPage.xaml
    /// </summary>
    public partial class FinishProductPage : Window
    {

        private List<String> list = new List<string>();
        private string index;
        private DataTable table;
        private SqlConnection conn;
        private int idLotto;

        public FinishProductPage()
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

            FillDataGrid(conn);
            SetFieldsComboBox();
    }

        private void SetFieldsComboBox()
        {
            this.fields.ItemsSource = list;
            this.fields.SelectedIndex = 0;
        }

        private void FillDataGrid(SqlConnection conn)
        {
            String query = "SELECT * FROM dbo.storicoproduzione";
            SqlCommand comm = new SqlCommand(query, conn);
            conn.Open();

            comm.ExecuteNonQuery();

            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            table = new DataTable();
            adapter.Fill(table);
            finishedProductsGrid.ItemsSource = table.DefaultView;
            GetStatoordiniColumnsNames(table);
            //conn.Close();
        }

        private void GetStatoordiniColumnsNames(DataTable table)
        {
            for(int i = 0; i < table.Columns.Count; i++)
            {            
                list.Add(table.Columns[i].ColumnName.ToString());
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

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
