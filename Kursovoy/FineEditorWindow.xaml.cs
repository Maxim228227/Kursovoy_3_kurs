using System;
using System.Windows;

namespace Kursovoy
{
    public partial class FineEditorWindow : Window
    {
        public string FirstName => FirstNameTextBox.Text;
        public string LastName => LastNameTextBox.Text;
        public string Teacher => TeacherTextBox.Text;
        public string ServiceType => ServiceTypeTextBox.Text;
        public string UnitPrice => UnitPriceTextBox.Text;
        public string Discount => DiscountTextBox.Text;
        public string Description => DescriptionTextBox.Text;
        public string DateIssued => DateIssuedTextBox.Text;

        public FineEditorWindow(string firstName, string lastName, string teacher, string serviceType,
                                string unitPrice, string discount, string description, string dateIssued)
        {
            InitializeComponent();

            FirstNameTextBox.Text = firstName;
            LastNameTextBox.Text = lastName;
            TeacherTextBox.Text = teacher;
            ServiceTypeTextBox.Text = serviceType;
            UnitPriceTextBox.Text = unitPrice;
            DiscountTextBox.Text = discount;
            DescriptionTextBox.Text = description;
            DateIssuedTextBox.Text = dateIssued;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UnitPriceTextBox.Text) || string.IsNullOrWhiteSpace(DateIssuedTextBox.Text))
            {
                MessageBox.Show("Поля 'Сумма Штрафа' и 'Дата Выдачи' обязательны.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
