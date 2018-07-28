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
    public partial class ModifyOrdiniPage : Window
    {

        private List<String> list = new List<string>();
        private string index;
        private DataTable table;
        private SqlConnection conn;
        private int idLotto;
        private int produced, max;

        public ModifyOrdiniPage()
        {
            InitializeComponent();

            conn = new SqlConnection();
            //SIMONE-PC\\SQLEXPRESS;
            //LAPTOP-DT8KB2TQ;
            conn.ConnectionString =
            "Server=LAPTOP-DT8KB2TQ;" +
            "Database=stodb;" +
            "Integrated Security=True;" +
            "MultipleActiveResultSets=true;";

            conn.Open();

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
            if (conn != null && conn.State == ConnectionState.Closed)
                conn.Open();
            String query = "SELECT * FROM dbo.statoordini WHERE stato!='finished' AND stato!='stored'";
            SqlCommand comm = new SqlCommand(query, conn);

            comm.ExecuteNonQuery();

            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            table = new DataTable();
            adapter.Fill(table);
            statoordiniGridModify.ItemsSource = table.DefaultView;
            GetStatoordiniColumnsNames(table);
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

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            modificationPanel.Visibility = Visibility.Visible;
            if (statoordiniGridModify.SelectedItem == null)
                return;
            DataRowView dr = statoordiniGridModify.SelectedItem as DataRowView;
            DataRow dr1 = dr.Row;
            idLotto = (int)dr1["idLotto"];
            produced = (int)dr1["quantitaProdotta"];
            max = (int)dr1["quantitaDesiderata"];
            newValueTextBox.Text = Convert.ToString(dr1["quantitaDesiderata"]);
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            if (!newValueTextBox.Text.Contains(",") && !newValueTextBox.Text.Contains(".")) {    // check if the value is integer
                int newValue = Int32.Parse(newValueTextBox.Text);
                if (newValue > 0)
                {

                    // Calculating how many items are currently in the queue
                    string queryNumberOfProduct = "SELECT idPezzo FROM dbo.routing WHERE idLotto=(@idLotto) ORDER BY idPezzo ASC";
                    SqlCommand comm = new SqlCommand(queryNumberOfProduct, conn);
                    comm.Parameters.AddWithValue("@idLotto", idLotto);
                    comm.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(comm);
                    table = new DataTable();
                    adapter.Fill(table);

                    List<String> numList = new List<string>();
                    foreach (DataRow dr in table.Rows)
                    {
                        String number = (String)dr["idPezzo"];
                        number = number.Substring(number.LastIndexOf("-") + 1);
                        number.Trim(' ');
                        numList.Add(number);
                    }
                    List<int> idPezziList = numList.Select(int.Parse).ToList();
                    int producing = idPezziList.Max();
                    Console.WriteLine("Telai in produzione del lotto " + idLotto + ": " + producing);


                    if (newValue > producing)  // check if the value is less then the produced item
                    {
                        /*String query = "UPDATE dbo.statoordini SET quantitaDesiderata=(@newQuantita) WHERE idLotto=(@idLotto)";
                        SqlCommand comm = new SqlCommand(query, conn);
                        comm.Parameters.AddWithValue("@newQuantita", Int32.Parse(newValueTextBox.Text));
                        comm.Parameters.AddWithValue("@idLotto", idLotto);

                        if (conn != null && conn.State == ConnectionState.Closed)
                            conn.Open();

                        comm.ExecuteNonQuery();*/

                        string query = "UPDATE dbo.mps SET quantita=(@newQuantita), modified=1 WHERE id=(@id)";
                        SqlCommand comm2 = new SqlCommand(query, conn);
                        comm2.Parameters.AddWithValue("@newQuantita", Int32.Parse(newValueTextBox.Text));
                        comm2.Parameters.AddWithValue("@id", idLotto);


                        comm2.ExecuteNonQuery();
                        conn.Close();
                        FillDataGrid(conn);

                        modificationPanel.Visibility = Visibility.Hidden;
                        this.Close();
                    }
                    else
                    {
                        int possibleValue;
                        if (producing >= max)
                            possibleValue = max;
                        else
                            possibleValue = producing;
                        errorLabel.Content = "Il valore inserito è superiore alla quantità di elementi già prodotti o che sono in produzione\n" +
                            "Il limite massimo impostabile è " + possibleValue;

                        FillDataGrid(conn);
                    }
                }
                else
                {
                    errorLabel.Content = "Non è possibile inserire un valore null o minore di zero";
                }
            }
            else
            {
                errorLabel.Content = "Non è possibile inserire un numero non intero";
            }
        }
    }
}
