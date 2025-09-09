using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;

namespace Kursovoy
{
    public partial class EditTeacherWindow : Window
    {
        public string FullName { get; private set; }
        public int PositionID { get; private set; }
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        public EditTeacherWindow(string fullName, int positionID)
        {
            InitializeComponent();
            FullNameTextBox.Text = fullName;
            PositionIDTextBox.Text = positionID.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Поле ФИО учителя не должно быть пустым.");
                return;
            }

            if (!int.TryParse(PositionIDTextBox.Text, out int positionID))
            {
                MessageBox.Show("ID должности должен быть числом.");
                return;
            }

            FullName = FullNameTextBox.Text;
            PositionID = positionID;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
    }
}
