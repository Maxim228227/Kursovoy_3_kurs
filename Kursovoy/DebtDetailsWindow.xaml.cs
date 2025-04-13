using System.Windows;

namespace KursovoyClient
{
    public partial class DebtDetailsWindow : Window
    {
        public DebtDetailsWindow(string details)
        {
            InitializeComponent(); // Инициализация компонентов
            DetailsTextBlock.Text = details; // Устанавливаем текст в TextBlock
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрываем окно
        }
    }
}