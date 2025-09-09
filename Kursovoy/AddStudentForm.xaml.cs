using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;

namespace Kursovoy
{
    public partial class AddStudentForm : Window
    {
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";
        private Action _loadStudents; // Делегат для обновления списка студентов

        // Конструктор с передачей метода для обновления списка студентов
        public AddStudentForm(Action loadStudents)
        {
            InitializeComponent();
            _loadStudents = loadStudents; // Сохраняем ссылку на метод
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            string userIDStr = UserIDTextBox.Text.Trim();
            int? userID = null; // Изначально null

            // Проверка, если UserID введен
            if (!string.IsNullOrEmpty(userIDStr) && int.TryParse(userIDStr, out int parsedUserID))
            {
                userID = parsedUserID; // Присваиваем значение, если оно корректное
            }

            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string birthDateStr = BirthDateTextBox.Text.Trim();
            string enrollmentDateStr = EnrollmentDateTextBox.Text.Trim();
            string major = MajorTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();
            string formStuding = FormStudiesTextBox.Text.Trim();
            string tipStudingText = TypeStudiesTextBox.Text.Trim();
            string gruppa = GroupTextBox.Text.Trim();
            string isInternationalStr = IsInternationalTextBox.Text.Trim();

            // Проверка формата дат
            if (!DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate) ||
                !DateTime.TryParseExact(enrollmentDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime enrollmentDate))
            {
                MessageBox.Show("Неверный формат даты. Используйте ДД.ММ.ГГГГ.");
                return;
            }

            // Проверка поля на международность
            bool isInternational = isInternationalStr.ToLower() == "да";
            bool tipStuding = tipStudingText.ToLower() == "да";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Students 
                        (UserID, FirstName, LastName, Grupa, BirthDate, EnrollmentDate, Major, Email, Phone, FormStudies, TypeStudies, Address, IsInternational)
                        VALUES 
                        (@UserID, @FirstName, @LastName, @Grupa, @BirthDate, @EnrollmentDate, @Major, @Email, @Phone, @FormStudies, @TypeStudies, @Address, @IsInternational)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметры
                        command.Parameters.AddWithValue("@UserID", (object)userID ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@Grupa", gruppa);
                        command.Parameters.AddWithValue("@BirthDate", birthDate);
                        command.Parameters.AddWithValue("@EnrollmentDate", enrollmentDate);
                        command.Parameters.AddWithValue("@Major", major);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@FormStudies", formStuding);
                        command.Parameters.AddWithValue("@TypeStudies", tipStuding);
                        command.Parameters.AddWithValue("@IsInternational", isInternational);

                        // Выполняем запрос
                        command.ExecuteNonQuery();
                        MessageBox.Show("Студент добавлен.");
                        _loadStudents(); // Обновляем список студентов
                        this.Close(); // Закрываем форму после добавления
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении студента: " + ex.Message);
                }
            }
        }
    }
}