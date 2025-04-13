using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Kursovoy;

namespace KursovoyClient
{
    public partial class MainWindow : Window
    {
        private const int Port = 12345; // Порт сервера
        private const string ServerAddress = "127.0.0.1"; // Адрес сервера (измените при необходимости)

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Проверка на заполненность полей
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Пожалуйста, введите имя пользователя и пароль.");
                return;
            }

            string message = $"login {username} {password}";
            string response = SendUdpMessage(message);

            // Проверка ответа от сервера
            if (string.IsNullOrEmpty(response))
            {
                ShowMessage("Пользователь не найден.");
            }
            else if (response.StartsWith("Успешный вход"))
            {
                // Извлечение userID из ответа
                string[] parts = response.Split(new[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3 && parts[1].Trim() == "Student")
                {
                    // Получаем userID 5
                    string userID = parts[3].Trim();
                    string firstName = parts[5].Trim();
                    string lastName = parts[7].Trim();

                    // Открытие окна для студентов с передачей userID
                    StudentWindow studentWindow = new StudentWindow(userID, firstName, lastName);
                    studentWindow.Show();
                    this.Close(); // Закрыть текущее окно
                }
                else
                {
                    ShowMessage("Неверный формат ответа от сервера.");
                }
            }
            else
            {
                ShowMessage(response); // Показать успешный ответ
            }
        }

        private string SendUdpMessage(string message)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true; // Включение широковещательной рассылки
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                udpClient.Send(sendBytes, sendBytes.Length, ServerAddress, Port); // Отправка на сервер

                // Ожидание ответа
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = udpClient.Receive(ref remoteEP);
                return Encoding.UTF8.GetString(receiveBytes); // Возврат ответа
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}