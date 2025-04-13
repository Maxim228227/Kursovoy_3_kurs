using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
                    Console.WriteLine(" 1 = " + delenie[0] + " 2 = " + delenie[1] + " 3 = " + delenie[2]);

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
    t.FullName,
    q.Discription,
    c.ServiceType,
    c.UnitPrice,
    f.DateIssued
FROM 
    Fines f
JOIN 
    Teachers t ON f.TeacherID = t.TeacherID
JOIN 
    Costs c ON f.CostID = c.CostID
JOIN 
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
                            string fullName = reader.GetString(1);
                            string discription = reader.GetString(2);
                            string serviceType = reader.GetString(3);
                            decimal unitPrice = reader.GetDecimal(4);
                            DateTime dateIssued = reader.GetDateTime(5);

                            Console.WriteLine($"ID: {fineId}, Преподаватель: {fullName}, Предмет: {discription}, Услуга: {serviceType}, Цена: {unitPrice} рублей, Дата: {dateIssued.ToShortDateString()}");

                            debts.Add($"ID: {fineId} | Преподаватель: {fullName} | Предмет: {discription} | Услуга: {serviceType} | Цена: {unitPrice} рублей | Дата: {dateIssued.ToShortDateString()}");
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
    }
}