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
        public class Event
        {
            public string EventName { get; set; }
            public int EventId { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            string sql = $"SELECT * FROM test";
            NpgsqlCommand command = new NpgsqlCommand(sql, con);

            DataTable dt = new DataTable();
            dt.Load(command.ExecuteReader(CommandBehavior.CloseConnection));
            DataGrid1.DataContext = dt;
            DataGrid1.ItemsSource = dt.AsDataView();
        }

        private void click(object sender, RoutedEventArgs e)
        {
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
            
                string sql = "INSERT INTO test(name, email, age) VALUES (@name, @email, @age)";
                NpgsqlCommand command = new NpgsqlCommand(sql, con);
                command.Parameters.Add(new NpgsqlParameter("@name", textBox_name.Text));
                command.Parameters.Add(new NpgsqlParameter("@email", textBox_email.Text));
                string text_age = textBox_age.Text;
                int age;
                bool isConvToInt = int.TryParse(textBox_age.Text, out age);
                command.Parameters.Add(new NpgsqlParameter("@age", age));
                command.ExecuteNonQuery();
        }
    }

}
