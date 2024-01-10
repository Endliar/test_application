using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_application
{
    public class Student : INotifyPropertyChanged
    {
        private int studentId;
        [Column("student_id")]public int StudentId { get { return studentId; } set { studentId = value; OnPropertyChanged("StudentId"); } }
        private string? fullName;
        [Column("full_name")]public string FullName { get { return fullName; } set { fullName = value; OnPropertyChanged(nameof(FullName)); } }
        private int physicsGrade;
        [Column("physics_grade")]public int PhysicsGrade { get { return physicsGrade; } set { physicsGrade = value; OnPropertyChanged(nameof(PhysicsGrade)); } }
        private int mathGrade;
        [Column("math_grade")]public int MathGrade { get { return mathGrade; } set { mathGrade = value; OnPropertyChanged(nameof(MathGrade)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
