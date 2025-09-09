using System;
using System.Windows;

namespace Kursovoy
{
    public partial class EditStudentWindow : Window
    {
        public int StudentId { get; private set; }
        public string UserID { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Grupa { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public string Major { get; private set; }
        public string Address { get; private set; }
        public string FormStudies { get; private set; }
        public string TypeStudies { get; private set; }
        public string IsInternational { get; private set; }

        public EditStudentWindow(int studentId, string userID, string firstName, string lastName, string grupa, string phone,
            string email, string major, string address, string formStudies, string typeStudies, string isInternational)
        {
            InitializeComponent();

            StudentId = studentId;
            UserID = userID;
            FirstName = firstName;
            LastName = lastName;
            Grupa = grupa;
            Phone = phone;
            Email = email;
            Major = major;
            Address = address;
            FormStudies = formStudies;
            TypeStudies = typeStudies;
            IsInternational = isInternational;

            // Заполняем поля на форме текущими значениями
            UserIDTextBox.Text = UserID;
            FirstNameTextBox.Text = FirstName;
            LastNameTextBox.Text = LastName;
            GrupaTextBox.Text = Grupa;
            PhoneTextBox.Text = Phone;
            EmailTextBox.Text = Email;
            MajorTextBox.Text = Major;
            AddressTextBox.Text = Address;
            FormStudiesTextBox.Text = FormStudies;
            TypeStudiesTextBox.Text = TypeStudies;
            IsInternationalTextBox.Text = IsInternational;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на обязательность полей
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Имя и фамилия не могут быть пустыми.");
                return;
            }

            // Сохраняем данные, если все поля валидны
            FirstName = FirstNameTextBox.Text;
            LastName = LastNameTextBox.Text;
            Grupa = GrupaTextBox.Text;
            Phone = PhoneTextBox.Text;
            Email = EmailTextBox.Text;
            Major = MajorTextBox.Text;
            Address = AddressTextBox.Text;
            FormStudies = FormStudiesTextBox.Text;
            TypeStudies = TypeStudiesTextBox.Text;
            IsInternational = IsInternationalTextBox.Text;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
