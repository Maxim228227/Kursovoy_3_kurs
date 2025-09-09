using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Kursovoy
{
    public partial class AddUsers : Window
    {
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";
        private Action _loadUsers; // Делегат для обновления списка учителей

        public AddUsers(Action loadUsers)
        {
            InitializeComponent();
            _loadUsers = loadUsers; // Сохраняем ссылку на метод
        }

        private void GetPositionIDButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder positionsInfo = new StringBuilder();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT RoleID, RoleName \r\nFROM Roles";
                    SqlCommand command = new SqlCommand(query, connection);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int positionID = reader.GetInt32(0);
                            string positionName = reader.GetString(1);
                            positionsInfo.AppendLine($"ID : {positionID} ---> Роль: {positionName}");
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

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            string i = message;
            Console.WriteLine(message);
        }
        private void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = LoginTextBox.Text.Trim();
            string positionIDStr = PaswordTextBox.Text.Trim();
            if (!int.TryParse(RoleTextBox.Text, out int role))
            {
                MessageBox.Show("Ошибка: Некорректный формат ID");
                return;
            }

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(positionIDStr))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Проверяем, существует ли уже пользователь с таким именем
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", fullName);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Пользователь с таким именем уже существует.");
                            return; // Прерываем выполнение, если пользователь уже существует
                        }
                    }

                    // Если пользователь не существует, вставляем нового
                    string insertQuery = @"
        INSERT INTO Users (Username, Password, RoleID) 
        VALUES (@Username, @Password, @RoleID)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", fullName);
                        command.Parameters.AddWithValue("@Password", positionIDStr);
                        command.Parameters.AddWithValue("@RoleID", role);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Пользователь добавлен.");
                        _loadUsers(); // Обновляем список учителей
                        this.Close(); // Закрываем форму после добавления
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении пользователя: " + ex.Message);
                }
            }
        }
    }
}