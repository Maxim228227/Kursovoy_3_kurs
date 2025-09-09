using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using KursovoyServer.Models;

namespace KursovoyServer
{
    class Program
    {
        private static int port = 12345; // Порт для сервера
        // Строка подключения к базе данных
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        static void Main(string[] args)
        {
            UdpClient udpServer = new UdpClient(port);
            Console.WriteLine("Сервер запущен. Ожидание запросов...");

            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                byte[] receivedData = udpServer.Receive(ref remoteEP); // Получение данных
                string receivedMessage = Encoding.UTF8.GetString(receivedData);
                Console.WriteLine($"Получено сообщение: {receivedMessage}");

                // Обработка команды
                string response = ProcessCommand(receivedMessage);
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                udpServer.Send(responseData, responseData.Length, remoteEP); // Отправка ответа
            }
        }

        /// <summary>
        /// Обрабатывает входящие команды.
        /// </summary>
        /// <param name="command">Входная команда.</param>
        /// <returns>Ответ в виде строки.</returns>
        private static string ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');

            if (parts.Length == 0)
            {
                return "Введите команду.";
            }
            Console.WriteLine(" 1 = " + parts[0] + " 2 = " + parts[1] + " 3/" + parts[0].ToLower() + "/");

            switch (parts[0].ToLower())
            {

                case "login":
                    if (parts.Length != 3)
                    {
                        return "Использование: login <username> <password>";
                    }

                    
                    string username = parts[1];
                    string password = parts[2];
                    string role;
                    int? userId;
                    string firstName;
                    string lastName;

                    //if (parts[1] == "admin" || parts[2] == "admin")
                    //{
                    //    role = "admin";
                    //    return $"Успешный вход.{role}";
                    //}

                    bool isValid = ValidateUser(username, password, out role, out userId, out firstName, out lastName);
                    return isValid ? $"Успешный вход. Ваша роль: {role}, ID: {userId}, Фамилия: {firstName}, Имя: {lastName}" : "Неверные учетные данные";

                case "getdebts":
                    if (parts.Length != 2)
                    {
                        return "Использование: getDebts <userId>";
                    }
                    
                    Console.WriteLine(" 1 = " + parts[0] + " 2 = " + parts[1] /*+ " 3 = " + parts[2] + " 4 = " + parts[3]*/);
                    // Извлечение userId из команды
                    if (int.TryParse(parts[1], out int userIdToFetch))
                    {
                        var programInstance = new Program(); // Создаем экземпляр класса
                        return programInstance.GetDebts(userIdToFetch);
                    }
                    else
                    {
                        return "Неверный формат UserID.";
                    }

                case "bindcart":

                    string[] delenie = parts[2].Split('|');
                  //  Console.WriteLine(" 1 = " + delenie[0] + " 2 = " + delenie[1] + " 3 = " + delenie[2]);

                    int userID;
                    if (int.TryParse(parts[1].Trim(), out userID))
                    {
                        // Преобразование успешно, переменная number содержит целое значение
                    }
                    else
                    {
                        // Обработка ошибки, если преобразование не удалось
                    }
                    bool result = AddCards(userID, delenie[0].ToString(), delenie[1].ToString(), delenie[2].ToString());

                    return result ? "true" : "false";

                case "payfine":

                    Console.WriteLine(" 1 = " + parts[0] + " 2 = " + parts[1] + " 3/" + parts[2] + "/" + parts[3] + "/" + parts[4] + "/" + parts[5] + "/"
                + parts[6] + "/" + parts[7] + "/");
                    var regex = new Regex(@"payfine ID: (\d+) (\d+) Цена: ([\d,]+)");
                    var match = regex.Match(command);
                    // Регулярное выражение для извлечения информации между "Предмет:" и "Скидка:"
                    var regex1 = new Regex(@"Предмет:\s*(.*?)\s*Cкидка:");
                    var match1 = regex1.Match(command);

                   
                    if (match.Success)
                    {
                        int fineID = int.Parse(match.Groups[1].Value);
                        int studentID_pay = int.Parse(match.Groups[2].Value);
                        string amountStr = match.Groups[3].Value.Replace(',', '.');
                        string numberCard = parts[7].ToString();
                        string subject = parts[8].ToString();

                        if(subject.Trim() == "Оплата")
                        {
                            subject = subject + " за обучение";
                        }

                        if (match1.Success)
                        {
                            subject = match1.Groups[1].Value.Trim();
                            Console.WriteLine("Предмет: " + subject);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось найти Предмет.");
                        }
                        //string discaunt = parts[9].ToString();
                        int discaunt = int.Parse(parts[parts.Length - 1]);

                        if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                        {
                            Console.WriteLine($"Оплата штрафа ID:{fineID}, Студент ID:{studentID_pay}, Сумма: {amount}, Скидка: {discaunt}, предмет: |{subject}|");

                            bool paymentAdded = AddPayment(studentID_pay, fineID, amount, numberCard, subject, discaunt);
                            if (paymentAdded && RemoveFine(fineID))
                            {
                                return "OK";
                            }
                            return "ERROR: Ошибка при обработке оплаты";
                        }
                        return "ERROR: Неверный формат суммы";
                    }
                    return "ERROR: Неверный формат команды";


                case "getpayments":
                    {
                        string UserID = parts[1].ToString();
                        // Извлекаем StudentID на основании UserID
                        int studentID = GetStudentID(UserID);
                        if (studentID == -1)
                        {
                            return "ERROR: Студент не найден";
                        }
                        Console.WriteLine("Этап 1 ="+UserID+"|");
                        // Получаем платежи для данного StudentID
                        var payments = GetPaymentsByStudentID(studentID);
                        if (payments != null && payments.Count > 0)
                        {
                            // Формируем ответ с платежами
                            StringBuilder response = new StringBuilder();
                            foreach (var payment in payments)
                            {
                                response.AppendLine($"PaymentID: {payment.PaymentID}, StudentID: {payment.StudentID}, " +
                                                    $"Amount: {payment.Amount}, PaymentDate: {payment.PaymentDate:dd.MM.yyyy}, " +
                                                    $"CardNumber: {payment.CardNumber}, SubjectName: {payment.SubjectName}, Discaunt: {payment.Discaunt}");
                            }
                            Console.WriteLine("Этап 2 ="+ response.ToString());
                            return response.ToString();
                        }

                        return "ERROR: Платежи не найдены";
                    }

                default:
                    return "Неизвестная команда.";
            }
        }

        private static bool AddCards(int userID, string nameCards, string numberCards, string srokDeistviy)
        {


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Открываем соединение
                connection.Open();

                // SQL-запрос для добавления данных
                string query = "INSERT INTO Cards (UserID, NameCard, NumberCard, SrokDeistvia) VALUES (@UserID, @NameCard, @NumberCard, @SrokDeistvia)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Добавляем параметры
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.Parameters.AddWithValue("@NameCard", nameCards);
                    command.Parameters.AddWithValue("@NumberCard", numberCards);
                    command.Parameters.AddWithValue("@SrokDeistvia", srokDeistviy);

                    try
                    {
                        // Выполняем команду и возвращаем true, если добавление прошло успешно
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; // Возвращаем true, если хотя бы одна строка добавлена
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибок
                        Console.WriteLine("Ошибка при добавлении карты: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет учетные данные пользователя в базе данных.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="role">Роль пользователя (выходной параметр).</param>
        /// <param name="userId">ID пользователя (выходной параметр).</param>
        /// <returns>true, если учетные данные верны; иначе false.</returns>
        private static bool ValidateUser(string username, string password, out string role, out int? userId, out string firstName, out string lastName)
        {
            role = string.Empty;
            userId = null;
            firstName = string.Empty;
            lastName = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string quri = @"SELECT *
FROM Users
WHERE Username = @username AND Password = @password AND RoleID = 1;";
                using (SqlCommand cmd = new SqlCommand(quri, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = "admin";
                            userId = (int)reader["UserID"];
                            firstName = "Кто";
                            lastName = "Вы";
                            Console.WriteLine($"Роль = {role}, Пользовательский ID = {userId}, Имя = {firstName}, Фамилия = {lastName}");
                            return true;
                        }
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string quri = @"SELECT *
FROM Users
WHERE Username = @username AND Password = @password AND RoleID = 2;";
                using (SqlCommand cmd = new SqlCommand(quri, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = "Bugalter";
                            userId = (int)reader["UserID"];
                            firstName = "Кто";
                            lastName = "Вы";
                            Console.WriteLine($"Роль = {role}, Пользовательский ID = {userId}, Имя = {firstName}, Фамилия = {lastName}");
                            return true;
                        }
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
                {
                connection.Open();
                string query = @"
            SELECT u.RoleID, u.UserID, s.FirstName, s.LastName 
            FROM Users u 
            JOIN Students s ON s.UserID = u.UserID 
            WHERE u.Username = @username AND u.Password = @password";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = GetRoleName((int)reader["RoleID"]);
                            userId = (int)reader["UserID"];
                            firstName = reader["FirstName"].ToString();
                            lastName = reader["LastName"].ToString();
                            Console.WriteLine($"Роль = {role}, Пользовательский ID = {userId}, Имя = {firstName}, Фамилия = {lastName}");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Получает название роли по ID роли.
        /// </summary>
        /// <param name="roleId">ID роли.</param>
        /// <returns>Название роли.</returns>
        private static string GetRoleName(int roleId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT RoleName FROM Roles WHERE RoleID=@roleId";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@roleId", roleId);
                    return (string)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Получает информацию о пользователе по его UserID.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <returns>Информация о пользователе.</returns>
        private static string GetUserInfo(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Username, RoleID FROM Users WHERE UserID=@userId";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"].ToString();
                            int roleId = (int)reader["RoleID"];
                            string roleName = GetRoleName(roleId);
                            return $"Пользователь: {username}, Роль: {roleName}";
                        }
                        else
                        {
                            return "Пользователь не найден.";
                        }
                    }
                }
            }
        }

        private string GetDebts(int userId)
        {
            var debts = new List<string>();

            Console.WriteLine("|" + userId + "|");
            // Укажите свою строку подключения

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Обновленный SQL-запрос
                string query = @"
SELECT 
    f.FineID,
    t.FullName AS TeacherName,
    q.Discription AS SubjectDescription,
    c.ServiceType,
    c.UnitPrice,
    f.DateIssued,
f.Discount
FROM 
    Fines f
LEFT JOIN 
    Teachers t ON f.TeacherID = t.TeacherID
LEFT JOIN 
    Costs c ON f.CostID = c.CostID
LEFT JOIN 
    AcademicSubjects q ON f.SubjectID = q.SubjectID
JOIN 
    Students s ON f.StudentID = s.StudentID  -- Соединение с таблицей Students
WHERE 
    s.UserId = 5;  -- Фильтрация по UserId из таблицы Students"; // Фильтрация по userId
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId); // Добавляем параметр
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int fineId = reader.GetInt32(0);

                            // Проверка на NULL и присвоение значений по умолчанию
                            string fullName = reader.IsDBNull(1) ? "Нет данных" : reader.GetString(1);
                            string discription = reader.IsDBNull(2) ? "Нет данных" : reader.GetString(2);
                            string serviceType = reader.IsDBNull(3) ? "Нет данных" : reader.GetString(3);
                            decimal unitPrice = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            DateTime dateIssued = reader.GetDateTime(5);
                            int discount;
                            if (reader.IsDBNull(6))
                            {
                                discount = 0; // значение по умолчанию, если данных нет
                            }
                            else
                            {
                                discount = reader.GetInt32(6); // получаем значение как int
                            }

                            Console.WriteLine($"ID: {fineId}, Преподаватель: {fullName}, Предмет: {discription}, Услуга: {serviceType}, Цена: {unitPrice} рублей, Дата: {dateIssued.ToShortDateString()}  | Cкидка: |{discount}| ");

                            debts.Add($"ID: {fineId} | Преподаватель: {fullName} | Предмет: {discription} | Услуга: {serviceType} | Цена: {unitPrice} рублей | Дата: {dateIssued.ToShortDateString()} | Cкидка: {discount} ");
                        }
                    }
                }
            }

            if (debts.Count == 0)
            {
                return "Нет долгов в системе.";
            }

            return string.Join(";", debts);
        }

        private static bool AddPayment(int userId, int fineId, decimal amount, string numberCard, string subject, int discaunt)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем StudentID по UserID
                    int studentId;
                    string studentQuery = @"SELECT StudentID FROM Students WHERE UserID = @UserID";

                    using (SqlCommand studentCmd = new SqlCommand(studentQuery, connection))
                    {
                        studentCmd.Parameters.AddWithValue("@UserID", userId);
                        object result = studentCmd.ExecuteScalar();

                        if (result != null)
                        {
                            studentId = Convert.ToInt32(result);
                        }
                        else
                        {
                            Console.WriteLine("Студент не найден.");
                            return false;
                        }
                    }

                    // Получаем CardID по номеру карты
                    int cardId;
                    string cardQuery = @"SELECT CardID FROM Cards WHERE NumberCard = @NumberCard";

                    using (SqlCommand cardCmd = new SqlCommand(cardQuery, connection))
                    {
                        cardCmd.Parameters.AddWithValue("@NumberCard", numberCard);
                        object result = cardCmd.ExecuteScalar();

                        if (result != null)
                        {
                            cardId = Convert.ToInt32(result);
                        }
                        else
                        {
                            Console.WriteLine("Карта не найдена.");
                            return false;
                        }
                    }

                    Console.WriteLine($"Номер карты = {numberCard} CardID = {cardId}");

                    // Запрос на вставку
                    string query = @"INSERT INTO Payments 
                             (StudentID, Amount, PaymentDate, CardID, SubjectName, Discaunt) 
                             VALUES 
                             (@StudentID, @Amount, @PaymentDate, @CardID, @SubjectName, @Discaunt)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", studentId); // Используем найденный StudentID
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CardID", cardId); // Используем найденный CardID
                        cmd.Parameters.AddWithValue("@SubjectName", subject);
                        cmd.Parameters.AddWithValue("@Discaunt", discaunt);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении платежа: {ex.Message}");
                return false;
            }
        }

        private static bool RemoveFine(int fineId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка существования штрафа перед удалением
                    string checkQuery = "SELECT 1 FROM Fines WHERE FineID = @FineID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@FineID", fineId);
                        if (checkCmd.ExecuteScalar() == null)
                        {
                            Console.WriteLine($"Штраф с ID {fineId} не найден");
                            return false;
                        }
                    }

                    // Удаление штрафа
                    string deleteQuery = "DELETE FROM Fines WHERE FineID = @FineID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@FineID", fineId);
                        int affectedRows = deleteCmd.ExecuteNonQuery();

                        // Проверка успешности удаления
                        if (affectedRows > 0)
                        {
                            Console.WriteLine($"Штраф с ID {fineId} успешно удален");
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении штрафа: {ex.Message}");
                return false;
            }
        }

        // Метод для получения StudentID
        private static int GetStudentID(string userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT StudentID FROM Students WHERE UserID = @UserID", connection);
                command.Parameters.AddWithValue("@UserID", userId);

                var result = command.ExecuteScalar();
                Console.WriteLine("Этап 3 ="+result+"|");
                return result != null ? (int)result : -1; // Возвращаем -1, если студент не найден
            }
        }

        // Метод для получения платежей с номером карты
        private static List<Payment> GetPaymentsByStudentID(int studentID)
        {
            var payments = new List<Payment>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
            SELECT p.PaymentID, p.StudentID, p.Amount, p.PaymentDate, c.NumberCard, p.SubjectName, p.Discaunt 
            FROM Payments p
            JOIN Cards c ON p.CardID = c.CardID 
            WHERE p.StudentID = @StudentID", connection);

                command.Parameters.AddWithValue("@StudentID", studentID);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentID = reader.GetInt32(0),
                            StudentID = reader.GetInt32(1),
                            Amount = reader.GetDecimal(2),
                            PaymentDate = reader.GetDateTime(3),
                            CardNumber = reader.IsDBNull(4) ? null : reader.GetString(4), // Получаем номер карты
                            SubjectName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Discaunt = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6) // Проверка на NULL и получение значения как Int32
                        });
                    }
                }
            }

            return payments;
        }

        public class Payment
        {
            public int PaymentID { get; set; }
            public int StudentID { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; } // Добавлено поле для даты платежа
            public string CardNumber { get; set; } // Добавлено поле для номера карты
            public string SubjectName { get; set; }
            public int? Discaunt { get; set; }
        }



    }
}