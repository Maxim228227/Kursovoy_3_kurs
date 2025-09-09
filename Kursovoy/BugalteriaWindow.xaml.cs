using System.Windows;
using Kursovoy.Pages; // Подключите пространство имен для BugalteriaPage

namespace Kursovoy
{
    public partial class BugalteriaWindow : Window
    {
        public BugalteriaWindow()
        {
            InitializeComponent();
            MainFrame.Content = new BugalteriaPage(); // Загружаем страницу
        }
    }
}