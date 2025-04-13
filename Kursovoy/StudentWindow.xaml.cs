using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic; // Для InputBox


namespace KursovoyClient
{
    public partial class StudentWindow : Window
    {
        public string UserID { get; set; }

        string fullMasiv;
        private List<CardInfo> boundCards = new List<CardInfo>();

        public StudentWindow(string userID, string firstName, string lastName)
        {
            InitializeComponent();
            UserID = userID;
            UserIDTextBlock.Text = $"{firstName} {lastName}"; // Проверка правильности отображения
            LoadPayments(); // Загрузка платежей из базы данных
            LoadDebts(); // Загрузка долгов
            //ShowMessage(userID + "dgdfg");
            ViewCarts();
            CardsListBox.ItemsSource = boundCards;
        }

        public class CardInfo
        {
            public string Name { get; set; }
            public string Number { get; set; }
            public string Expiry { get; set; }

            public string MaskedNumber => $"**** **** **** {Number.Substring(Number.Length - 4)}";
        }


        private void LoadPayments()
        {
            string response = SendUdpMessage($"getPayments {UserID}");
            if (!string.IsNullOrEmpty(response))
            {
                PaymentsListBox.Items.Clear();
                var payments = response.Split(';');
                foreach (var payment in payments)
                {
                    PaymentsListBox.Items.Add(payment);
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить платежи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDebts()
        {
            string response = SendUdpMessage($"getDebts {UserID}");
            fullMasiv = response;
            if (!string.IsNullOrEmpty(response))
            {
                DebtsListBox.Items.Clear();
                var debts = response.Split(';');

                foreach (var debt in debts)
                {
                    // Проверяем, что длина строки больше 5 символов
                    if (debt.Length > 5)
                    {
                        // Обрезаем первые 5 символов
                        string visibleDebt = debt.Substring(5).Trim();
                        string[] parts = visibleDebt.Split('|');

                        // Добавляем только 2-й и 3-й элементы
                        if (parts.Length >= 3)
                        {
                            DebtsListBox.Items.Add($"{parts[1].Trim()} | {parts[2].Trim()}");
                        }
                        else if (parts.Length == 2)
                        {
                            DebtsListBox.Items.Add(parts[1].Trim()); // Добавляем только второй элемент
                        }
                    }
                    else
                    {
                        // Если строка слишком короткая, можно добавить обработку, например, пропустить
                        DebtsListBox.Items.Add(string.Empty); // Или просто пропустить
                    }
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить долги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string SendUdpMessage(string message)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true; // Включение широковещательной рассылки
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 12345); // Замените на адрес вашего сервера

                // Ожидание ответа
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = udpClient.Receive(ref remoteEP);
                return Encoding.UTF8.GetString(receiveBytes); // Возврат ответа
            }
        }

        private void ViewPaymentInfo_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListBox.SelectedItem != null)
            {
                string selectedPayment = PaymentsListBox.SelectedItem.ToString();
                MessageBox.Show($"Информация о платеже: {selectedPayment}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите платеж для просмотра информации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindCard_Click(object sender, RoutedEventArgs e)
        {
            var bindCardWindow = new BindCardWindow
            {
                Owner = this
            };

            if (bindCardWindow.ShowDialog() == true)
            {
                string cardName = bindCardWindow.CardName;
                string cardNumber = bindCardWindow.CardNumber;
                string expiry = bindCardWindow.ExpiryDate;
                string result = $"{cardName}|{cardNumber}|{expiry}";
                string response = SendUdpMessage($"bindCart {UserID} {result}");
                if(response == "true")
                {
                    // Здесь можно отправить данные на сервер или сохранить
                    MessageBox.Show(
                        $"Карта привязана:\nНазвание: {cardName}\nНомер: {cardNumber}\nСрок действия: {expiry}",
                        "Успешно",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                }
                else if(response == "false")
                {
                    MessageBox.Show("Проверьте данные карты");
                }
                    
            }
        }



        private void PaymentsDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListBox.SelectedItem != null)
            {
                string selectedPayment = PaymentsListBox.SelectedItem.ToString();
                MessageBox.Show($"Подробности о платеже: {selectedPayment}", "Подробности платежа", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись из списка платежей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DebtsDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DebtsListBox.SelectedItem != null)
            {
                string selectedDebt = DebtsListBox.SelectedItem.ToString();

                // Разделяем строку на части
                string[] parts = selectedDebt.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                // Находим полную строку, соответствующую выбранному элементу
                string[] allDebts = fullMasiv.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string fullDebt = allDebts.FirstOrDefault(d => d.Contains(parts[0].Trim()));

                if (fullDebt != null)
                {
                    // Формируем текст для отображения
                    StringBuilder detailsMessage = new StringBuilder();
                    string[] debtDetails = fullDebt.Split('|'); // Предполагается, что детали также разделены символом '|'
                    for (int i = 1; i < debtDetails.Length; i++)
                    {
                        detailsMessage.AppendLine(debtDetails[i].Trim()); // Добавляем каждую часть с новой строки
                    }

                    // Создаем и открываем новое окно с деталями
                    DebtDetailsWindow detailsWindow = new DebtDetailsWindow(detailsMessage.ToString());
                    detailsWindow.ShowDialog(); // Открываем как диалоговое окно
                }
                else
                {
                    MessageBox.Show("Не удалось найти детали для выбранного долга.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись из списка долгов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Pay_Item_Click(object sender, RoutedEventArgs e)
        {
            if (DebtsListBox.SelectedItem != null)
            {

                MessageBox.Show("Оплата прошла успешна !");

                //string selectedDebt = DebtsListBox.SelectedItem.ToString();

                //// Разделяем строку на части
                //string[] parts = selectedDebt.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                //// Формируем текст для отображения, начиная со второго элемента
                //StringBuilder detailsMessage = new StringBuilder();
                //for (int i = 0; i < parts.Length; i++) // Начинаем с 1, чтобы пропустить первый элемент
                //{
                //    detailsMessage.AppendLine(parts[i].Trim()); // Добавляем каждую часть с новой строки
                //}

                //// Создаем и открываем новое окно с деталями
                //DebtDetailsWindow detailsWindow = new DebtDetailsWindow(detailsMessage.ToString());
                //detailsWindow.ShowDialog(); // Открываем как диалоговое окно
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись из списка долгов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewCarts()
        {
            boundCards.Clear();
            CardsListBox.ItemsSource = null;

            string connectionString = @"Server=MAXIM\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"; // Укажи своё
            int currentUserId = Convert.ToInt32(UserID); // Получи его из авторизации/логина

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT NameCard, NumberCard, SrokDeistvia FROM Cards WHERE UserID = @UserID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", currentUserId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        boundCards.Add(new CardInfo
                        {
                            Name = reader["NameCard"].ToString(),
                            Number = reader["NumberCard"].ToString(),
                            Expiry = reader["SrokDeistvia"].ToString()
                        });
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки карт: " + ex.Message);
                }
            }

            CardsListBox.ItemsSource = boundCards;
        }

        public class BoundCard
        {
            public int Id { get; set; } // ID в базе данных
            public string Name { get; set; }
            public string Number { get; set; } // Полный номер
            public string MaskedNumber => "**** **** **** " + Number.Substring(Number.Length - 4);
            public string Expiry { get; set; }
        }

        private void DeleteCard_Click(object sender, RoutedEventArgs e)
        {
            if (CardsListBox.SelectedItem is CardInfo selectedCard)
            {
                var confirm = MessageBox.Show($"Удалить карту {selectedCard.Number}?",
                                              "Подтверждение",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection("Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"))
                        {
                            connection.Open();
                            string query = "DELETE FROM Cards WHERE NumberCard = @NumberCard AND UserID = @UserID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@NumberCard", selectedCard.Number);
                                command.Parameters.AddWithValue("@UserID", Convert.ToInt32(UserID));

                                int result = command.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    MessageBox.Show("Карта удалена.");
                                    ViewCarts(); // обновить список
                                }
                                else
                                {
                                    MessageBox.Show("Карта не найдена или не принадлежит пользователю.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите карту для удаления.");
            }
        }



        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}