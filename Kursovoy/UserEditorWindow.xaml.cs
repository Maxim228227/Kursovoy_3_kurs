using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;

namespace Kursovoy
{
    public partial class UserEditorWindow : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int RoleID { get; private set; }
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        public UserEditorWindow(string login, string password, int roleId)
        {
            InitializeComponent();
            LoginTextBox.Text = login;
            PasswordTextBox.Text = password;
            RoleIDTextBox.Text = roleId.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Поля логина и пароля не должны быть пустыми.");
                return;
            }

            if (!int.TryParse(RoleIDTextBox.Text, out int roleId))
            {
                MessageBox.Show("ID роли должен быть числом.");
                return;
            }

            Username = LoginTextBox.Text;
            Password = PasswordTextBox.Text;
            RoleID = roleId;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Role_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder positionsInfo = new StringBuilder();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT RoleID, RoleName FROM Roles";
                    SqlCommand command = new SqlCommand(query, connection);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int roleID = reader.GetInt32(0);
                            string roleName = reader.GetString(1);
                            positionsInfo.AppendLine($"ID должность: {roleID} ---> Должность: {roleName}");
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
    }
}
