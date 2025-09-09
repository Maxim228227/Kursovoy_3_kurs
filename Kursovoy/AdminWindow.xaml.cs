using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Collections.Generic;
using System.Text;




namespace Kursovoy
{
    public partial class AdminWindow : Window
    {
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadStudents();
            LoadTeachers();
            LoadPayments();
            LoadFines();
            LoadCosts();
        }

        private void LoadCosts()
        {
            using (SqlConnection conn = new SqlConnection("Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"))
            {
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Costs", conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                CostsDataGrid.ItemsSource = table.DefaultView;
            }
        }
        private void DeleteCost_Click(object sender, RoutedEventArgs e)
        {
            if (CostsDataGrid.SelectedItem is DataRowView row)
            {
                int costId = Convert.ToInt32(row["CostID"]);
                using (SqlConnection conn = new SqlConnection("Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Costs WHERE CostID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", costId);
                    cmd.ExecuteNonQuery();
                }
                LoadCosts();
            }
        }

        private void AddCost_Click(object sender, RoutedEventArgs e)
        {
            var window = new CostEditorWindow();
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                LoadCosts();
            }
        }



        private void EditCost_Click(object sender, RoutedEventArgs e)
        {
            if (CostsDataGrid.SelectedItem is DataRowView row)
            {
                var window = new CostEditorWindow(row);
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    LoadCosts();
                }
            }
        }

        //private void EditFineButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (FinesDataGrid.SelectedItem is DataRowView row)
        //    {
        //        int fineID = Convert.ToInt32(row["FineID"]);
        //        int studentID = Convert.ToInt32(row["StudentID"]);
        //        int teacherID = Convert.ToInt32(row["TeacherID"]);
        //        decimal costID = Convert.ToDecimal(row["CostID"]);
        //        string dateIssued = Convert.ToDateTime(row["DateIssued"]).ToString("yyyy-MM-dd");
        //        int subjectID = Convert.ToInt32(row["SubjectID"]);
        //        int discount = Convert.ToInt32(row["Discount"]);

        //        var window = new FineEditorWindow(
        //            fineID,
        //            studentID,
        //            teacherID,
        //            costID,
        //            dateIssued,
        //            subjectID,
        //            discount
        //        );

        //        window.Owner = this;

        //        if (window.ShowDialog() == true)
        //        {
        //            using (SqlConnection connection = new SqlConnection(connectionString))
        //            {
        //                try
        //                {
        //                    connection.Open();
        //                    string query = "UPDATE Fines SET StudentID = @StudentID, TeacherID = @TeacherID, CostID = @CostID, DateIssued = @DateIssued, SubjectID = @SubjectID, Discount = @Discount WHERE FineID = @FineID";
        //                    SqlCommand command = new SqlCommand(query, connection);
        //                    command.Parameters.AddWithValue("@StudentID", window.StudentID);
        //                    command.Parameters.AddWithValue("@TeacherID", window.TeacherID);
        //                    command.Parameters.AddWithValue("@CostID", window.CostID);
        //                    command.Parameters.AddWithValue("@DateIssued", window.DateIssued);
        //                    command.Parameters.AddWithValue("@SubjectID", window.SubjectID);
        //                    command.Parameters.AddWithValue("@Discount", window.Discount);
        //                    command.Parameters.AddWithValue("@FineID", fineID);
        //                    command.ExecuteNonQuery();

        //                    MessageBox.Show("Штраф обновлён.");
        //                    LoadFines(); // Обновляем список штрафов
        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("Ошибка при редактировании штрафа: " + ex.Message);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Пожалуйста, выберите штраф для редактирования.");
        //    }
        //}



        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования.");
                return;
            }

            DataRowView row = UsersDataGrid.SelectedItem as DataRowView;
            int userId = Convert.ToInt32(row["UserID"]);
            string currentLogin = row["Username"].ToString();
            string currentPassword = row["Password"].ToString();
            string Role = row["RoleName"].ToString().Trim();
            int currentRoleId = 0;

            if (Role == "Admin")
            {
                currentRoleId = 1;
            }
            else if(Role == "Bugalter")
            {
                currentRoleId = 2;
            }
            else if(Role == "Student")
            {
                currentRoleId = 3;
            }

                UserEditorWindow editor = new UserEditorWindow(currentLogin, currentPassword, currentRoleId);
            if (editor.ShowDialog() == true)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "UPDATE Users SET Username = @Login, Password = @Password, RoleID = @RoleID WHERE UserID = @UserID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Login", editor.Username);
                        command.Parameters.AddWithValue("@Password", editor.Password);
                        command.Parameters.AddWithValue("@RoleID", editor.RoleID);
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Пользователь обновлён.");
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при редактировании пользователя: " + ex.Message);
                    }
                }
            }
        }


        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите студента для редактирования.");
                return;
            }

            DataRowView row = StudentsDataGrid.SelectedItem as DataRowView;
            int studentId = Convert.ToInt32(row["StudentID"]);
            string currentUserID = row["UserID"] != DBNull.Value ? row["UserID"].ToString() : string.Empty;
            string currentName = row["FirstName"].ToString();
            string currentSurname = row["LastName"].ToString();
            string currentGruppa = row["Grupa"].ToString();
            string currentPhone = row["Phone"].ToString();
            string currentEmail = row["Email"].ToString();
            string currentMajor = row["Major"].ToString();
            string currentAddress = row["Address"].ToString();
            string currentFormStudies = row["FormStudies"].ToString();
            string currentTypeStudies = row["TypeStudies"].ToString();
            string currentIsInternational = row["IsInternational"].ToString();

            // Открытие окна редактирования студента
            EditStudentWindow editor = new EditStudentWindow(studentId, currentUserID, currentName, currentSurname, currentGruppa,
                currentPhone, currentEmail, currentMajor, currentAddress, currentFormStudies, currentTypeStudies, currentIsInternational);

            if (editor.ShowDialog() == true)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = @"
                UPDATE Students 
                SET UserID = @UserID, 
                    FirstName = @FirstName, 
                    LastName = @LastName, 
                    Grupa = @Grupa,
                    Phone = @Phone, 
                    Email = @Email, 
                    Major = @Major, 
                    Address = @Address, 
                    FormStudies = @FormStudies,
                    TypeStudies = @TypeStudies,
                    IsInternational = @IsInternational 
                WHERE StudentID = @StudentID";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@UserID", string.IsNullOrEmpty(editor.UserID) ? (object)DBNull.Value : editor.UserID);
                        command.Parameters.AddWithValue("@FirstName", editor.FirstName);
                        command.Parameters.AddWithValue("@LastName", editor.LastName);
                        command.Parameters.AddWithValue("@Grupa", editor.Grupa);
                        command.Parameters.AddWithValue("@Phone", editor.Phone);
                        command.Parameters.AddWithValue("@Email", editor.Email);
                        command.Parameters.AddWithValue("@Major", editor.Major);
                        command.Parameters.AddWithValue("@Address", editor.Address);
                        command.Parameters.AddWithValue("@FormStudies", editor.FormStudies);
                        command.Parameters.AddWithValue("@TypeStudies", editor.TypeStudies);
                        command.Parameters.AddWithValue("@IsInternational", editor.IsInternational);
                        command.Parameters.AddWithValue("@StudentID", studentId);

                        command.ExecuteNonQuery();

                        MessageBox.Show("Данные студента обновлены.");
                        LoadStudents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при редактировании студента: " + ex.Message);
                    }
                }
            }
        }


        private void EditTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите учителя для редактирования.");
                return;
            }

            DataRowView row = TeachersDataGrid.SelectedItem as DataRowView;
            int teacherId = Convert.ToInt32(row["TeacherID"]);
            string currentFullName = row["FullName"].ToString();
            string Position = row["PositionName"].ToString().Trim();
            int currentPositionID = 0;
            if(Position == "Професор, доктор наук")
            {
                currentPositionID = 1;
            }
            else if(Position == "Доцент, кандидат наук")
            {
                currentPositionID = 2;
            }
            else if(Position == "Лица, не имеющие ученной степени")
            {
                currentPositionID = 3;
            }
            else if(Position == "Университет")
            {
                currentPositionID = 4;
            }

                // Открываем окно редактирования учителя
                EditTeacherWindow editor = new EditTeacherWindow(currentFullName, currentPositionID);
            if (editor.ShowDialog() == true)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "UPDATE Teachers SET FullName = @FullName, PositionID = @PositionID WHERE TeacherID = @TeacherID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@FullName", editor.FullName);
                        command.Parameters.AddWithValue("@PositionID", editor.PositionID);
                        command.Parameters.AddWithValue("@TeacherID", teacherId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Учитель обновлён.");
                        LoadTeachers(); // Обновляем список учителей
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при редактировании учителя: " + ex.Message);
                    }
                }
            }
        }

        // Пользователи
        private void LoadUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT u.UserID, u.Username, u.Password, r.RoleName\r\nFROM dbo.Users u\r\nJOIN dbo.Roles r ON u.RoleID = r.RoleID";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    UsersDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
                }
            }
        }


        // метод добавления нового преподавателя в базу данных
        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна для добавления учителя с передачей метода обновления списка учителей
            AddTeacherForm teacherForm = new AddTeacherForm(LoadTeachers);
            teacherForm.Show();
        }



        private void AddUser_Click(object sender, RoutedEventArgs e)
        {

            var window = new AddUsers(LoadUsers);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                LoadCosts();
            }




            //string login = Interaction.InputBox("Введите логин:", "Добавление пользователя");
            //string password = Interaction.InputBox("Введите пароль:", "Добавление пользователя");
            //string roleIdStr = Interaction.InputBox("Введите ID роли:", "Добавление пользователя");

            //if (!int.TryParse(roleIdStr, out int roleId))
            //{
            //    MessageBox.Show("ID роли должен быть числом.");
            //    return;
            //}

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        connection.Open();
            //        string query = "INSERT INTO Users (Username, Password, RoleID) VALUES (@Login, @Password, @RoleID)";
            //        SqlCommand command = new SqlCommand(query, connection);
            //        command.Parameters.AddWithValue("@Login", login);
            //        command.Parameters.AddWithValue("@Password", password);
            //        command.Parameters.AddWithValue("@RoleID", roleId);
            //        command.ExecuteNonQuery();
            //        MessageBox.Show("Пользователь добавлен успешно.");
            //        LoadUsers();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("Ошибка при добавлении пользователя: " + ex.Message);
            //    }
            //}
        }

     

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.");
                return;
            }

            DataRowView row = UsersDataGrid.SelectedItem as DataRowView;
            int userId = Convert.ToInt32(row["UserID"]);

            if (MessageBox.Show("Вы уверены, что хотите удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM Users WHERE UserID = @UserID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Пользователь удалён.");
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message);
                    }
                }
            }
        }

        // Студенты
        private void LoadStudents()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Students";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    StudentsDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки студентов: " + ex.Message);
                }
            }
        }


        //Остановка на обед/ужин
        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна для студентов с передачей метода обновления
            AddStudentForm studentForm = new AddStudentForm(LoadStudents);
            studentForm.Show();
        }



        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента для удаления.");
                return;
            }

            DataRowView row = StudentsDataGrid.SelectedItem as DataRowView;
            int studentId = Convert.ToInt32(row["StudentID"]);

            if (MessageBox.Show("Вы уверены, что хотите удалить студента?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM Students WHERE StudentID = @StudentID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@StudentID", studentId);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Студент удалён.");
                        LoadStudents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении студента: " + ex.Message);
                    }
                }
            }
        }

        // Загрузка преподавателей:
        private void LoadTeachers() 
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT t.TeacherID, t.FullName, p.PositionName\r\nFROM dbo.Teachers t\r\nJOIN dbo.Positions p ON t.PositionID = p.PositionID";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    TeachersDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
                }
            }
        }
        
        // Загрузка платежей:
        private void LoadPayments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Payments";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    PaymentsDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
                }
            }
        }

        // Загрузка штрафов:
        private void LoadFines()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
              SELECT 
    f.FineID AS [ИдентификаторШтрафа], 
    s.FirstName AS [Фамилия], 
    s.LastName AS [Имя], 
    t.FullName AS [ФИО Учителя], 
    c.UnitPrice AS [Сумма Штрафа], 
    f.DateIssued AS [Дата Выдачи], 
    f.Discount AS [Скидка],
    a.Discription AS [Описание],
    c.ServiceType AS [Тип Услуги]  -- Добавлено поле ТипУслуги
FROM 
    dbo.Fines f
JOIN 
    dbo.Students s ON f.StudentID = s.StudentID
LEFT JOIN 
    dbo.Teachers t ON f.TeacherID = t.TeacherID
LEFT JOIN 
    dbo.Costs c ON f.CostID = c.CostID
LEFT JOIN 
    dbo.AcademicSubjects a ON a.SubjectID = f.SubjectID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    FinesDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки штрафов: " + ex.Message);
                }
            }
        }

       


        // метод удаления преподавателя из базы данных
        private void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите учителя для удаления.");
                return;
            }

            DataRowView row = TeachersDataGrid.SelectedItem as DataRowView;
            int teacherId = Convert.ToInt32(row["TeacherID"]);

            if (MessageBox.Show("Вы уверены, что хотите удалить этого учителя?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM Teachers WHERE TeacherID = @TeacherID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@TeacherID", teacherId);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Данные учителя успешно удалены.");
                        LoadTeachers(); // Обновляем список учителей
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении учителя: " + ex.Message);
                    }
                }
            }
        }
        private void ExportSelectedPaymentToTXT_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный элемент из DataGrid
            if (PaymentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите платеж для экспорта.");
                return;
            }

            DataRowView selectedRow = (DataRowView)PaymentsDataGrid.SelectedItem;

            // Открываем диалоговое окно для выбора места сохранения
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Сохранить отчет как"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Получаем данные из выбранной строки
                    int paymentId = Convert.ToInt32(selectedRow["PaymentID"]);
                    int studentId = Convert.ToInt32(selectedRow["StudentID"]);
                    decimal amount = Convert.ToDecimal(selectedRow["Amount"]);
                    DateTime paymentDate = Convert.ToDateTime(selectedRow["PaymentDate"]);
                    string subjectName = selectedRow["SubjectName"].ToString();

                    // Формируем чек для выбранного платежа
                    StringBuilder receipt = new StringBuilder();
                    receipt.AppendLine("Чек оплаты");
                    receipt.AppendLine("-------------------");

                    string universityName = "Полесский государственный университет"; // Замените на реальное значение
                    string studentName = GetStudentNameById(studentId); // Метод для получения имени студента
                    string maskedCardNumber = GetMaskedCardNumberByPaymentId(paymentId); // Метод для получения номера карты

                    receipt.AppendLine($"Получатель платежа: {universityName}");
                    receipt.AppendLine($"Фамилия и имя студента: {studentName}");
                    receipt.AppendLine($"Номер карты: {maskedCardNumber}");
                    receipt.AppendLine($"Предмет: {subjectName}"); // Предмет
                    receipt.AppendLine($"Дата оплаты: {paymentDate.ToShortDateString()}");
                    receipt.AppendLine($"Начислено: {amount}");
                    receipt.AppendLine($"Итого оплачено: {amount}");
                    receipt.AppendLine("-------------------");
                    receipt.AppendLine("Спасибо за оплату!");

                    // Записываем чек в текстовый файл
                    writer.WriteLine(receipt.ToString());
                }

                MessageBox.Show("Отчет по выбранному платежу успешно экспортирован в TXT.");
            }
        }

        // Пример метода для получения имени студента по его ID
        private string GetStudentNameById(int studentId)
        {
            string studentName = "Не найдено"; // Значение по умолчанию

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT FirstName, LastName FROM Students WHERE StudentID = @StudentID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentID", studentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string firstName = reader["FirstName"].ToString();
                        string lastName = reader["LastName"].ToString();
                        studentName = $"{firstName} {lastName}"; // Формируем полное имя
                    }
                }
            }

            return studentName;
        }

        private string GetMaskedCardNumberByPaymentId(int paymentId)
        {
            string maskedCardNumber = "Не найдено"; // Значение по умолчанию

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT c.NumberCard 
            FROM Cards c
            JOIN Payments p ON c.CardID = p.CardID
            WHERE p.PaymentID = @PaymentID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentID", paymentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string numberCard = reader["NumberCard"].ToString();
                        maskedCardNumber = MaskCardNumber(numberCard); // Маскируем номер карты
                    }
                }
            }

            return maskedCardNumber;
        }

        // Метод для маскировки номера карты
        private string MaskCardNumber(string numberCard)
        {
            if (numberCard.Length > 4)
            {
                return $"**** **** **** {numberCard.Substring(numberCard.Length - 4)}"; // Маскируем все, кроме последних 4 цифр
            }
            return numberCard; // Если номер карты слишком короткий
        }
        private void AddFineButton_Click(object sender, RoutedEventArgs e)
        {
            var addFinePage = new AddFinePage();
            addFinePage.FineAdded += () => LoadFines(); // Подписываемся на событие

            var window = new Window
            {
                Content = addFinePage,
                Title = "Добавить штраф",
                Width = 570,
                Height = 400
            };
            window.ShowDialog();
        }
        private void DeleteFine_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный штраф из DataGrid
            if (FinesDataGrid.SelectedItem is DataRowView selectedRow)
            {
                // Запрос подтверждения
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить выбранный штраф?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                int fineId = Convert.ToInt32(selectedRow["ИдентификаторШтрафа"]);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Fines WHERE FineID = @FineID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FineID", fineId);

                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Штраф успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadFines(); // Обновляем DataGrid
                            }
                            else
                            {
                                MessageBox.Show("Штраф не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Общая ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите штраф для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
