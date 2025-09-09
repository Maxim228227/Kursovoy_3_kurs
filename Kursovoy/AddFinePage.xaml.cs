using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kursovoy
{
    public partial class AddFinePage : Page
    {
        public event Action FineAdded;

        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        public AddFinePage()
        {
            InitializeComponent();
        }

        private void ShowDataWindow(string title, string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    Window dataWindow = new Window
                    {
                        Title = title,
                        Width = 600,
                        Height = 400,
                        Content = new ScrollViewer
                        {
                            Content = new DataGrid
                            {
                                ItemsSource = dt.DefaultView,
                                AutoGenerateColumns = true,
                                CanUserAddRows = false,
                                IsReadOnly = true
                            }
                        }
                    };
                    dataWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void ShowStudents_Click(object sender, RoutedEventArgs e)
        {
            ShowDataWindow("Список студентов",
                @"SELECT 
            *
        FROM Students");
        }

        private void ShowTeachers_Click(object sender, RoutedEventArgs e)
        {
            ShowDataWindow("Список преподавателей",
                @"SELECT 
            t.TeacherID AS 'ID',
            t.FullName AS 'ФИО',
            p.PositionName AS 'Должность'
        FROM Teachers t
        LEFT JOIN Positions p ON t.PositionID = p.PositionID");
        }

        private void ShowCosts_Click(object sender, RoutedEventArgs e)
        {
            ShowDataWindow("Список стоимостей",
                @"SELECT 
            c.CostID AS 'ID',
            c.ServiceType AS 'Тип услуги',
            c.UnitPrice AS 'Стоимость',
            p.PositionName AS 'Позиция',
            CASE c.IsForeign 
                WHEN 1 THEN 'Иностранная' 
                ELSE 'Белорусская' 
            END AS 'Тип'
        FROM Costs c
        LEFT JOIN Positions p ON c.PositionID = p.PositionID");
        }

        private void ShowSubjects_Click(object sender, RoutedEventArgs e)
        {
            ShowDataWindow("Список предметов",
                @"SELECT 
           *
        FROM AcademicSubjects");
        }

        // Остальные методы остаются без изменений
        private void AddFine_Click(object sender, RoutedEventArgs e)
        {
            // Проверка корректности введенных данных
            if (!int.TryParse(StudentIdTextBox.Text, out int studentId) ||
    !int.TryParse(TeacherIdTextBox.Text, out int teacherId) ||
    !int.TryParse(CostIdTextBox.Text, out int costId) ||
    !int.TryParse(SubjectIdTextBox.Text, out int subjectId) ||
    !int.TryParse(DiscauntTextBox.Text, out int discaunt) ||
    !DateTime.TryParse(DateTextBox.Text, out DateTime date))
            {
                ResultTextBlock.Text = "Ошибка: Некорректный формат ID";
                ResultTextBlock.Foreground = Brushes.Red;
                return;
            }

            string query = @"
        INSERT INTO Fines 
            (StudentID, TeacherID, CostID, Dateissued, SubjectID, Discount) 
        VALUES 
            (@StudentID, @TeacherID, @CostID, @DateIssued, @SubjectID, @Discount)";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", studentId);
                        command.Parameters.AddWithValue("@TeacherID", teacherId);
                        command.Parameters.AddWithValue("@CostID", costId);
                        command.Parameters.AddWithValue("@DateIssued", date);
                        command.Parameters.AddWithValue("@SubjectID", subjectId);
                        command.Parameters.AddWithValue("@Discount", discaunt);


                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ResultTextBlock.Text = "Штраф успешно добавлен!";
                            FineAdded?.Invoke(); // Вызываем событие
                            ResultTextBlock.Foreground = Brushes.Green;

                            // Очистка полей
                            StudentIdTextBox.Clear();
                            TeacherIdTextBox.Clear();
                            CostIdTextBox.Clear();
                            SubjectIdTextBox.Clear();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Обработка ошибок через классический switch
                string errorMessage;
                switch (ex.Number)
                {
                    case 547:
                        errorMessage = "Ошибка внешнего ключа: Проверьте существование ID в связанных таблицах";
                        break;
                    default:
                        errorMessage = $"Ошибка SQL: {ex.Message}";
                        break;
                }

                ResultTextBlock.Text = errorMessage;
                ResultTextBlock.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = $"Ошибка: {ex.Message}";
                ResultTextBlock.Foreground = Brushes.Red;
            }
        }


        private void StudentIdTextBox_TextChanged(object sender, TextChangedEventArgs e) { /* ... */ }
        private void TeacherIdTextBox_TextChanged(object sender, TextChangedEventArgs e) { /* ... */ }
        private void CostIdTextBox_TextChanged(object sender, TextChangedEventArgs e) { /* ... */ }
        private void SubjectIdTextBox_TextChanged(object sender, TextChangedEventArgs e) { /* ... */ }
    }
}