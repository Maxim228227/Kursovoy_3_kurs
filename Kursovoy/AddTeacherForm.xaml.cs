using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;

namespace Kursovoy
{
    public partial class AddTeacherForm : Window
    {
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";
        private Action _loadTeachers; // Делегат для обновления списка учителей

        public AddTeacherForm(Action loadTeachers)
        {
            InitializeComponent();
            _loadTeachers = loadTeachers; // Сохраняем ссылку на метод
        }

        private void GetPositionIDButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder positionsInfo = new StringBuilder();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT PositionID, PositionName FROM Positions";
                    SqlCommand command = new SqlCommand(query, connection);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int positionID = reader.GetInt32(0);
                            string positionName = reader.GetString(1);
                            positionsInfo.AppendLine($"ID должность: {positionID} ---> Должность: {positionName}");
                        }
                    }

                    if (positionsInfo.Length > 0)
                    {
                        MessageBox.Show(positionsInfo.ToString(), "Информация о должностях");
                    }
                    else
                    {
                        MessageBox.Show("Должности не найдены.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении информации о должностях: " + ex.Message);
                }
            }
        }

        private void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string positionIDStr = PositionIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(positionIDStr))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!int.TryParse(positionIDStr, out int positionID))
            {
                MessageBox.Show("Некорректный ID должности.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Teachers (FullName, PositionID) 
                        VALUES (@FullName, @PositionID)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FullName", fullName);
                        command.Parameters.AddWithValue("@PositionID", positionID);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Учитель добавлен.");
                        _loadTeachers(); // Обновляем список учителей
                        this.Close(); // Закрываем форму после добавления
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении учителя: " + ex.Message);
                }
            }
        }
    }
}