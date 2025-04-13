using System.Text.RegularExpressions;
using System.Windows;

namespace KursovoyClient
{
    public partial class BindCardWindow : Window
    {
        public string CardName { get; private set; }
        public string CardNumber { get; private set; }
        public string ExpiryDate { get; private set; }

        public BindCardWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            CardName = CardNameTextBox.Text.Trim();
            CardNumber = CardNumberTextBox.Text.Trim();
            ExpiryDate = ExpiryDateTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(CardName) ||
                string.IsNullOrWhiteSpace(CardNumber) ||
                string.IsNullOrWhiteSpace(ExpiryDate))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Простая проверка формата номера карты
            if (!Regex.IsMatch(CardNumber, @"^\d{16}$"))
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка формата даты MM/YY
            if (!Regex.IsMatch(ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                MessageBox.Show("Срок действия должен быть в формате MM/YY.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
