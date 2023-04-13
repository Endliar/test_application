using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
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

namespace test_application
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NpgsqlConnection con = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611");
            try
            {
                con.Open();
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.Message);
                con.Close();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Error");
                con.Close();
            }

            string sql = $"SELECT * FROM test WHERE name = '{textBox_name.Text}'";
            NpgsqlCommand command = new NpgsqlCommand(sql, con);
            string name1 = "maxim";
            string email1 = "maximus@outlook.com";
            int age = 20;
            NpgsqlCommand new_command = new NpgsqlCommand("insert into test values ('@name', '@email', '@age')", con);
            command.Parameters.AddWithValue(new NpgsqlParameter("@name", name1));
            command.Parameters.AddWithValue(new NpgsqlParameter("@email", email1));
            command.Parameters.AddWithValue(new NpgsqlParameter("@age", age));
            new_command.ExecuteNonQuery();

            command.Parameters.Add(new NpgsqlParameter("@name", textBox_name.Text));

            DataTable dt = new DataTable();
            ;
            dt.Load(command.ExecuteReader(CommandBehavior.CloseConnection));
            DataGrid1.DataContext = dt;
            DataGrid1.ItemsSource = dt.AsDataView();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
