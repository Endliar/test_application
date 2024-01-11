using Npgsql;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace test_application
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();

        public MainWindow()
        {
            InitializeComponent();
            DataGrid1.ItemsSource = Students;
            RefreshStudentList();
        }

        private async Task Data()
        {
            var connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611";
            await using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    var sql = @"SELECT s.student_id, s.full_name, g.physics_grade, g.math_grade FROM students s JOIN grades g ON s.student_id = g.student_id";
                    await using (var command = new NpgsqlCommand(sql, con))
                    {
                        var dt = new DataTable();
                        await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            dt.Load(reader);
                        }
                        DataGrid1.DataContext = dt;
                        DataGrid1.ItemsSource = dt.AsDataView();
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void RefreshStudentList()
        {
            string connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611";
            string sql = "SELECT students.student_id, full_name, physics_grade, math_grade FROM students INNER JOIN grades ON students.student_id = grades.student_id;";

            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            try
            {
                con.Open();

                using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, con);
                using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();

                Students.Clear();

                while(npgsqlDataReader.Read())
                {
                    Student student = new Student() { StudentId = npgsqlDataReader.GetInt32(0), FullName = npgsqlDataReader.GetString(1), PhysicsGrade = npgsqlDataReader.GetInt32(2), MathGrade = npgsqlDataReader.GetInt32(3) };
                    Students.Add(student);
                }
            } catch (NpgsqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            } finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Data();
        }

        private void click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611";

            int.TryParse(textBox_physics_grade.Text, out int physiscsGrade);
            int.TryParse(textBox_math_grade.Text, out int mathGrade);
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);

            try
            {
                con.Open();

                string sqlStudent = "INSERT INTO students (full_name) VALUES (@name) RETURNING student_id";
                using NpgsqlCommand commandStudent = new NpgsqlCommand(sqlStudent, con);
                commandStudent.Parameters.AddWithValue("@name", textBox_full_name.Text);

                int studentId = (int)commandStudent.ExecuteScalar();

                string sqlGrades = "INSERT INTO grades (student_id, physics_grade, math_grade) VALUES (@student_id, @physicsgrade, @mathgrade)";
                using NpgsqlCommand command = new NpgsqlCommand(sqlGrades, con);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@physicsgrade", physiscsGrade);
                command.Parameters.AddWithValue("@mathgrade", mathGrade);

                command.ExecuteNonQuery();
            } catch (NpgsqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            } catch (System.IO.IOException ex)
            {
                MessageBox.Show("IO errorL " + ex.Message);
            } finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }

            RefreshStudentList();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editedStudent = e.Row.Item as Student;
                if (editedStudent != null)
                {
                    UpdateStudent(editedStudent);
                }
            }
        }

        private void UpdateStudent(Student student)
        {
            string connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611";
            string sql = $"UPDATE students SET full_name = @fullname, physics_grade = @physicsgrade, math_grade = @mathgrade WHERE student_id = @studentid";

            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            try
            {
                con.Open();
                using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, con);
                npgsqlCommand.Parameters.AddWithValue("@studentid", student.StudentId);
                npgsqlCommand.Parameters.AddWithValue("@fullname", student.FullName);
                npgsqlCommand.Parameters.AddWithValue("@physicsgrade", student.PhysicsGrade);
                npgsqlCommand.Parameters.AddWithValue("@mathgrade", student.MathGrade);
                npgsqlCommand.ExecuteNonQuery();
            } catch (NpgsqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            } finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }

}
