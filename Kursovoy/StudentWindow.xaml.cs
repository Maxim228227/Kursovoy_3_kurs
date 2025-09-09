using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic; // Для InputBox
using Microsoft.Win32; // для SaveFileDialog
using System.IO; // для File
using iTextSharp.text; // для работы с PDF
using iTextSharp.text.pdf; // для работы с PDF
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Cryptography;



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
                var paymentLines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in paymentLines)
                {
                    // Извлекаем данные из строки сервера
                    var amountMatch = Regex.Match(line, @"Amount:\s*([\d,]+)");
                    var dateMatch = Regex.Match(line, @"PaymentDate:\s*(\d{2}\.\d{2}\.\d{4})");
                    var cardMatch = Regex.Match(line, @"CardNumber:\s*(\d{16})");
                    var subjectMatch = Regex.Match(line, @"SubjectName:\s*([^,]+)");
                    var discountMatch = Regex.Match(line, @"Discaunt:\s*([\d,]+)"); // Извлечение Discaunt

                    if (amountMatch.Success && dateMatch.Success && cardMatch.Success && subjectMatch.Success)
                    {
                        // Создаем объект Payment
                        var payment = new Payment
                        {
                            Amount = amountMatch.Groups[1].Value,
                            Date = dateMatch.Groups[1].Value,
                            FullCardNumber = cardMatch.Groups[1].Value,
                            MaskedCard = "**** **** **** " + cardMatch.Groups[1].Value.Substring(12),
                            SubjectName = subjectMatch.Groups[1].Value.Trim(),
                            Discaunt = discountMatch.Groups[1].Value
                        };

                        // Добавляем в ListBox (отобразится через ToString())
                        PaymentsListBox.Items.Add(payment);
                    }
                    //else
                    //{
                    //    MessageBox.Show($"Ошибка парсинга: {line}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить платежи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Payment
        {
            public string MaskedCard { get; set; }  // Замаскированный номер: "**** **** **** 4567"
            public string Amount { get; set; }      // Сумма: "26,07"
            public string Date { get; set; }        // Дата: "14.04.2025"
            public string FullCardNumber { get; set; } // Полный номер карты: "9112685346829119"
            public string SubjectName { get; set; } // Название предмета: "Криптография и охрана коммерческой информации"
            public string Discaunt { get; set; } // скидка 

            // Отображаемая строка в ListBox
            public override string ToString() => $"Карта: {MaskedCard}, Сумма: {Amount} руб., Дата: {Date}";
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
                 
                        // Проверяем количество частей и добавляем нужные данные
                        if (parts.Length >= 3)
                        {
                            string teacherInfo = parts[1].Trim();
                            string subjectInfo = parts[2].Trim();
                            string serviceInfo = parts[3].Trim();
                            string unitPrice = parts[4].Trim();
                            string date = parts[5].Trim();

                            int discount = 0; // Значение по умолчанию

                            // Проверяем, есть ли скидка (часть 6) и можно ли её корректно конвертировать
                            if (parts.Length > 6 && int.TryParse(parts[6].Trim().Replace("Cкидка:", "").Trim(), out int parsedDiscount))
                            {
                                discount = parsedDiscount;
                            }
                            string trimmedString = unitPrice.Substring(6, unitPrice.Length - 13);

                            //MessageBox.Show($"Значение = |{trimmedString}| |{unitPrice}|");

                            trimmedString = trimmedString.Replace(',', '.'); // замена запятой на точку для корректного преобразования
                            double price = 0;

                            if (double.TryParse(trimmedString, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                            {
                                // Успешное преобразование
                                Console.WriteLine(price);
                            }
                            else
                            {
                                // Обработка ошибки
                                Console.WriteLine("Ошибка: Не удалось преобразовать строку в double.");
                            }

                            double itogSumma = 0;
                            //Console.WriteLine($"ID: {fineId} | Преподаватель: {fullName} | Предмет: {discription} | Услуга: {serviceType} | Цена: {unitPrice} рублей | Дата: {dateIssued.ToShortDateString()}"");
                            // Если информации по преподавателю или предмету нет, выводим информацию о услуге и стоимости
                            if (teacherInfo == "Преподаватель: Нет данных" || subjectInfo == "Предмет: Нет данных")
                            {
                                itogSumma = (price * discount) / 100;
                                price = price - itogSumma;
                                date = date.Substring(5);
                                DebtsListBox.Items.Add($"{serviceInfo} | Стоимость: {price} | Скидка: {discount}% | Оплатить необходимо до: {date}" );
                            }
                            else
                            {
                                DebtsListBox.Items.Add($"{teacherInfo} | {subjectInfo}");
                            }
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
                        if (i == debtDetails.Length - 1) // Проверяем, является ли текущий элемент последним
                        {
                            detailsMessage.AppendLine(debtDetails[i].Trim() + "%"); // Добавляем % к последнему элементу
                        }
                        else
                        {
                            detailsMessage.AppendLine(debtDetails[i].Trim()); // Добавляем каждую часть с новой строки
                        }
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

        public class FineModel
        {
            public int FineID { get; set; }
            public decimal Cost { get; set; }
            public string SubjectName { get; set; }
            public DateTime DateIssued { get; set; }

            public override string ToString()
            {
                return $"{SubjectName} — {Cost}₽ (от {DateIssued:dd.MM.yyyy})";
            }
        }


        private void Pay_Item_Click(object sender, RoutedEventArgs e)
        {
            if (DebtsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите предмет для оплаты.");
                return;
            }

            if (boundCards == null || boundCards.Count == 0)
            {
                MessageBox.Show("Сначала привяжите хотя бы одну карту.");
                return;
            }

            // Получение данных по долгу
            var selectedDebtRaw = fullMasiv.Split(';')[DebtsListBox.SelectedIndex];
            if (string.IsNullOrWhiteSpace(selectedDebtRaw) || selectedDebtRaw.Length <= 5)
            {
                MessageBox.Show("Невозможно обработать выбранный долг.");
                return;
            }

            // Вывод строки для отладки
            Console.WriteLine($"Выбранная строка долга: {selectedDebtRaw}");

            // Извлечение частей строки
            string[] debtParts = selectedDebtRaw.Split('|'); // Используем разделитель '|'
            if (debtParts.Length < 3) // Проверяем, что есть как минимум 3 части
            {
                MessageBox.Show($"Ошибка при разборе данных долга. Ожидается как минимум 3 части, но получено: {debtParts.Length}");
                return;
            }

            // Получаем нужные значения
            string fineID = debtParts[0].Trim();  // ID находится на позиции 0
            string instructor = debtParts[1].Trim();  // Преподаватель на позиции 1
            string subject = debtParts[2].Trim();  // Предмет на позиции 2
            string discription = debtParts[3].Trim(); 
            string amount = debtParts[4].Trim();   // Сумма на позиции 2
            string discaunt = debtParts[6].Trim();
            //MessageBox.Show($"|{subject}|");

            string trimmedString = amount.Substring(6, amount.Length - 13);

            //MessageBox.Show($"Значение = |{trimmedString}| |{unitPrice}|");

            trimmedString = trimmedString.Replace(',', '.'); // замена запятой на точку для корректного преобразования
            double price = 0;

            if (double.TryParse(trimmedString, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                // Успешное преобразование
                Console.WriteLine(price);
            }
            else
            {
                // Обработка ошибки
                Console.WriteLine("Ошибка: Не удалось преобразовать строку в double.");
            }

            int discaunting = 0;
            double itogSumma = 0;
            if (int.TryParse(discaunt.Trim().Replace("Cкидка:", "").Trim(), out int parsedDiscount))
            {
                discaunting = parsedDiscount;
            }
            //MessageBox.Show($"1 = |{discaunting}|");
            //MessageBox.Show($"2 = |{amount}|");
            //MessageBox.Show($"3 = |{price}|");

            itogSumma = (price * discaunting) / 100;
            price = price - itogSumma;
            //MessageBox.Show($"4 = |{price}|");
            // Выбор карты
            var cardSelection = new StringBuilder("Выберите карту для оплаты:\n\n");
            for (int i = 0; i < boundCards.Count; i++)
            {
                cardSelection.AppendLine($"{i + 1}. {boundCards[i].Name} — {boundCards[i].MaskedNumber} (до {boundCards[i].Expiry})");
            }

            string input = Interaction.InputBox(cardSelection.ToString(), "Выбор карты", "1");

            if (!int.TryParse(input, out int cardIndex) || cardIndex < 1 || cardIndex > boundCards.Count)
            {
                MessageBox.Show("Неверный выбор карты.");
                return;
            }

            var selectedCard = boundCards[cardIndex - 1];
            string sub = subject.Substring(9);
            if (sub == "Нет данных")
            {
                subject = discription;
            }

            //MessageBox.Show($"|{subject}|");
            //if (subject.Length >= 7 && subject.Substring(0, 7) == "Услуга:")
            //{
            //    subject = subject.Substring(8);
            //}

            if (subject.Trim() == "Услуга: Оплата за обучение")
            {
                subject = subject.Substring(8);
            }
            //MessageBox.Show($"|{subject}|");
            var confirm = MessageBox.Show($"Оплатить \"{subject}\" с карты {selectedCard.MaskedNumber}?",
                                          "Подтвердить оплату",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                // Оплата
                //MessageBox.Show($"|{subject}|");
                //MessageBox.Show($"payfine {fineID} {UserID} Цена: {price} рублей {selectedCard.Number} {subject} {discaunt}");
                string payResponse = SendUdpMessage($"payfine {fineID} {UserID} Цена: {price} рублей {selectedCard.Number} {subject} {discaunt}");

                if (payResponse == "OK")
                {
                    LoadDebts();
                    LoadPayments();

                    amount = $"Цена: {price} рублей";
                    // Вывод чека
                    PrintReceipt(subject, amount, selectedCard.MaskedNumber, discaunt);
                }
                else
                {
                    MessageBox.Show("Не удалось выполнить оплату. Ответ: " + payResponse);
                }
            }
        }

        private void PrintReceipt(string subject, string amount, string maskedCardNumber, string discaunt)
        {
            // Получаем фамилию и имя студента
            int userIdInt;
            if (!int.TryParse(UserID, out userIdInt))
            {
                MessageBox.Show("Неверный формат UserID.");
                return;
            }

            string studentName = GetStudentName(userIdInt);
            string universityName = "Полесский государственный университет";
            string issueDate = DateTime.Now.ToString("dd.MM.yyyy");
            amount = amount.Substring(5);

            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("Чек оплаты");
            receipt.AppendLine("-------------------");
            receipt.AppendLine($"Получатель платежа: {universityName}");
            receipt.AppendLine($"Фамилия и имя студента: {studentName}");
            receipt.AppendLine($"Номер карты: {maskedCardNumber}");
            receipt.AppendLine($"{subject}"); // Добавлено поле для предмета
            receipt.AppendLine($"Дата оплаты: {issueDate}");
            receipt.AppendLine($"{discaunt} %");
            receipt.AppendLine($"Начислено: {amount}");
            receipt.AppendLine($"Итого оплачено: {amount}");
            receipt.AppendLine("-------------------");
            receipt.AppendLine("Спасибо за оплату!");

            // Запрос подтверждения сохранения
            var confirmSave = MessageBox.Show("Хотите сохранить чек?", "Сохранение чека", MessageBoxButton.YesNo);
            if (confirmSave == MessageBoxResult.No)
            {
                return; // Если пользователь не хочет сохранять, выходим
            }

            // Отображаем диалог для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Сохранить чек как"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Сохраняем чек в выбранное место
                    File.WriteAllText(saveFileDialog.FileName, receipt.ToString());
                    MessageBox.Show("Чек успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetStudentName(int userId)
        {
            using (SqlConnection connection = new SqlConnection("Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"))
            {
                connection.Open();
                string query = @"SELECT FirstName, LastName FROM Students WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string firstName = reader["FirstName"].ToString();
                            string lastName = reader["LastName"].ToString();
                            return $"{firstName} {lastName}";
                        }
                    }
                }
            }
            return "Неизвестный студент"; // Если студент не найден
        }

        private void PaymentsDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите платеж из списка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем объект Payment из выбранного элемента
            Payment selectedPayment = (Payment)PaymentsListBox.SelectedItem;

            // Извлекаем данные
            string subject = selectedPayment.SubjectName;
            string date = selectedPayment.Date;
            string amount = selectedPayment.Amount;
            string maskedCard = selectedPayment.MaskedCard;
            string discaunt = selectedPayment.Discaunt;

            if (discaunt.Length == 0)
            {
                MessageBox.Show($"предмет: {subject} Дата: {date} Сумма: {amount} Карта: {maskedCard}");
            }
            else {
                MessageBox.Show($"предмет: {subject} Дата: {date} Сумма: {amount} Карта: {maskedCard} Скидка: {discaunt}%");
            }
           

        }

        private void PrintCheck_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите платеж из списка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем объект Payment из выбранного элемента
            Payment selectedPayment = (Payment)PaymentsListBox.SelectedItem;

            // Извлекаем данные
            string subject = selectedPayment.SubjectName;
            string date = selectedPayment.Date;
            string amount = selectedPayment.Amount;
            string maskedCard = selectedPayment.MaskedCard;
            string discaunt = selectedPayment.Discaunt;
            if (discaunt.Length == 0)
            {
                discaunt = "0";
            }
            amount = amount.Substring(0, amount.Length - 1);
                MessageBox.Show($"предмет: {subject} Дата: {date} Сумма: {amount} Карта: {maskedCard} Скидка: {discaunt}%");
            // Печать чека
            // Получаем фамилию и имя студента
            int userIdInt;
            if (!int.TryParse(UserID, out userIdInt))
            {
                MessageBox.Show("Неверный формат UserID.");
                return;
            }

            if(subject.Trim() != "Оплата за обучение")
            {
                subject = "Предмет: " + subject;
            }
           
            string studentName = GetStudentName(userIdInt);
            string universityName = "Полесский государственный университет";

            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("Чек оплаты");
            receipt.AppendLine("-------------------");
            receipt.AppendLine($"Получатель платежа: {universityName}");
            receipt.AppendLine($"Фамилия и имя студента: {studentName}");
            receipt.AppendLine($"Номер карты: {maskedCard}");
            receipt.AppendLine($"{subject}");
            receipt.AppendLine($"Скидка: {discaunt}%");
            receipt.AppendLine($"Дата оплаты: {date}");
            receipt.AppendLine($"Итого оплачено: {amount} р.");
            receipt.AppendLine("-------------------");
            receipt.AppendLine("Спасибо за оплату!");

            //// Запрос подтверждения сохранения
            ///Проверка 0.2
            //var confirmSave = MessageBox.Show("Хотите сохранить чек?", "Сохранение чека", MessageBoxButton.YesNo);
            //if (confirmSave == MessageBoxResult.No)
            //{
            //    return; // Если пользователь не хочет сохранять, выходим
            //}

            // Отображаем диалог для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Сохранить чек как"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Сохраняем чек в выбранное место
                    File.WriteAllText(saveFileDialog.FileName, receipt.ToString());
                    MessageBox.Show("Чек успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void PrintPayments(string subject, string date, string amount, string maskedCardNumber, string discaunt)
        {
            // Получаем фамилию и имя студента
            int userIdInt;
            if (!int.TryParse(UserID, out userIdInt))
            {
                MessageBox.Show("Неверный формат UserID.");
                return;
            }

            string studentName = GetStudentName(userIdInt);
            string universityName = "Полесский государственный университет";
            amount = amount.Substring(5);

            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("Чек оплаты");
            receipt.AppendLine("-------------------");
            receipt.AppendLine($"Получатель платежа: {universityName}");
            receipt.AppendLine($"Фамилия и имя студента: {studentName}");
            receipt.AppendLine($"Номер карты: {maskedCardNumber}");
            receipt.AppendLine($"Предмет: {subject}");
            receipt.AppendLine($"Скидка: {discaunt}");
            receipt.AppendLine($"Дата оплаты: {date}");
            receipt.AppendLine($"Итого оплачено: {amount}");
            receipt.AppendLine("-------------------");
            receipt.AppendLine("Спасибо за оплату!");

            //// Запрос подтверждения сохранения
            ///Проверка 0.2
            //var confirmSave = MessageBox.Show("Хотите сохранить чек?", "Сохранение чека", MessageBoxButton.YesNo);
            //if (confirmSave == MessageBoxResult.No)
            //{
            //    return; // Если пользователь не хочет сохранять, выходим
            //}

            // Отображаем диалог для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Сохранить чек как"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Сохраняем чек в выбранное место
                    File.WriteAllText(saveFileDialog.FileName, receipt.ToString());
                    MessageBox.Show("Чек успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


        }


        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            string i = message;
            Console.WriteLine(message);
        }
    }
}