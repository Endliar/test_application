using Npgsql;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace test_application
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=0611";
        private ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            DataGrid1.ItemsSource = Students;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Data();
        }

        private async Task Data()
        {
            await using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    var sql = @"SELECT s.student_id, s.full_name, s.phone_number, g.physics_grade, g.math_grade FROM students s JOIN grades g ON s.student_id = g.student_id";
                    await using (var command = new NpgsqlCommand(sql, con))
                    {
                        await using (var reader  = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection)) 
                        {
                            Students.Clear();
                            while (await reader.ReadAsync())
                            {
                                var studentId = reader.GetInt32(reader.GetOrdinal("student_id"));
                                var fullName = reader.GetString(reader.GetOrdinal("full_name"));
                                var physicsGrade = reader.GetInt32(reader.GetOrdinal("physics_grade"));
                                var mathGrade = reader.GetInt32(reader.GetOrdinal("math_grade"));
                                var phoneNumber = reader.GetString(reader.GetOrdinal("phone_number"));

                                Students.Add(new Student
                                {
                                    StudentId = studentId,
                                    FullName = fullName,
                                    PhysicsGrade = physicsGrade,
                                    MathGrade = mathGrade,
                                    PhoneNumber = phoneNumber
                                });
                            }
                        }
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
            string sql = "SELECT students.student_id, full_name, physics_grade, math_grade, phone_number FROM students LEFT JOIN grades ON students.student_id = grades.student_id;";
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            Students.Clear();
            try
            {
                con.Open();
                using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, con);
                using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();

                while(npgsqlDataReader.Read())
                {
                    Student student = new Student() { StudentId = npgsqlDataReader.GetInt32(0), FullName = npgsqlDataReader.GetString(1), PhysicsGrade = npgsqlDataReader.GetInt32(2), MathGrade = npgsqlDataReader.GetInt32(3), PhoneNumber = npgsqlDataReader.GetString(4) };
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

        private void textBox_phone_number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void click(object sender, RoutedEventArgs e)
        {
            int.TryParse(textBox_physics_grade.Text, out int physiscsGrade);
            int.TryParse(textBox_math_grade.Text, out int mathGrade);
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);

            try
            {
                con.Open();

                string sqlStudent = "INSERT INTO students (full_name, phone_number) VALUES (@name, @phone) RETURNING student_id";
                using NpgsqlCommand commandStudent = new NpgsqlCommand(sqlStudent, con);
                commandStudent.Parameters.AddWithValue("@name", textBox_full_name.Text);
                commandStudent.Parameters.AddWithValue("@phone", textBox_phone_number.Text);

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
                var cellContent = e.EditingElement as TextBox;
                var editedCellValue = cellContent.Text;
                var editedStudent = e.Row.Item as Student;

                var columnName = e.Column.SortMemberPath;
                if (columnName == "FullName")
                {
                    editedStudent.FullName = editedCellValue;
                }
                else if (columnName == "PhysicsGrade")
                {
                    editedStudent.PhysicsGrade = int.TryParse(editedCellValue, out int newGrade) ? newGrade : editedStudent.PhysicsGrade;
                }
                else if (columnName == "MathGrade")
                {
                    editedStudent.MathGrade = int.TryParse(editedCellValue, out int newGrade) ? newGrade : editedStudent.MathGrade;
                }
                else if (columnName == "PhoneNumber")
                {
                    editedStudent.PhoneNumber = editedCellValue;
                }

                if (editedStudent != null)
                {
                    UpdateStudent(editedStudent);
                }
            }
        }

        private void UpdateStudent(Student student)
        {
            string sqlUpdateStudent = "UPDATE students SET full_name = @fullname, phone_number = @phonenumber WHERE student_id = @studentid";
            string sqlUpdateGrades = "UPDATE grades SET physics_grade = @physicsgrade, math_grade = @mathgrade WHERE student_id = @studentid";

            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            try
            {
                con.Open();
                
                using NpgsqlCommand cmdUpdateStudent = new NpgsqlCommand(sqlUpdateStudent, con);
                cmdUpdateStudent.Parameters.AddWithValue("@studentid", student.StudentId);
                cmdUpdateStudent.Parameters.AddWithValue("@fullname", student.FullName);
                cmdUpdateStudent.Parameters.AddWithValue("@phonenumber", student.PhoneNumber);
                cmdUpdateStudent.ExecuteNonQuery();
                
                using NpgsqlCommand cmdUpdateGrades = new NpgsqlCommand(sqlUpdateGrades, con);
                cmdUpdateGrades.Parameters.AddWithValue("@studentid", student.StudentId);
                cmdUpdateGrades.Parameters.AddWithValue("@physicsgrade", student.PhysicsGrade);
                cmdUpdateGrades.Parameters.AddWithValue("@mathgrade", student.MathGrade);
                cmdUpdateGrades.ExecuteNonQuery();
            } catch (NpgsqlException ex) {
                MessageBox.Show("Database error: " + ex.Message);
            } finally {
                if (con.State == ConnectionState.Open) {
                    con.Close();
                }
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudent = DataGrid1.SelectedItem as Student;
            if (selectedStudent == null)
            {
                MessageBox.Show("Выберите студента для удаления");
                return;
            }

            string sqlDeleteStudent = "DELETE FROM students WHERE student_id = @studentid";
            string sqlDeleteGrades = "DELETE FROM grades WHERE student_id = @studentid";

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (NpgsqlTransaction trans = con.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand cmdDeleteGrades = new NpgsqlCommand(sqlDeleteGrades, con))
                        {
                            cmdDeleteGrades.Parameters.AddWithValue("@studentid", selectedStudent.StudentId);
                            cmdDeleteGrades.ExecuteNonQuery();
                        }
                        using (NpgsqlCommand cmdDeleteStudent = new NpgsqlCommand(sqlDeleteStudent, con))
                        {
                            cmdDeleteStudent.Parameters.AddWithValue("@studentid", selectedStudent.StudentId);
                            cmdDeleteStudent.ExecuteNonQuery();
                        }

                        trans.Commit();
                        Students.Remove(selectedStudent);
                    } catch (NpgsqlException ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Не удалось удалить студента. Возможно, у него есть связанные данные." + ex.Message);
                    }
                }
            }
        }
    }

}
    