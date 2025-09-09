using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using CsvHelper;
using System.Globalization;
using System.Diagnostics;

// Классы для данных
public class PaymentRecord
{
    public string PaymentID { get; set; }
    public string StudentName { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string SubjectName { get; set; }
    public string NumberCard { get; set; }
    public int Discaunt { get; set; }

}

public class FineRecord
{
    public string FineID { get; set; }
    public string StudentName { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? DateIssued { get; set; }
    public string ServiceType { get; set; }
    public int Discount { get; set; }
}

namespace Kursovoy.Pages
{
    public partial class BugalteriaPage : Page
    {
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";

        public BugalteriaPage()
        {
            InitializeComponent();
            if (!ПроверитьПодключение())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте строку подключения и доступность сервера.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ЗагрузитьДанныеСводки();
            ЗагрузитьОтчеты();
        }

        private bool ПроверитьПодключение()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ЗагрузитьДанныеСводки()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Общий доход
                    string incomeQuery = "SELECT SUM(Amount) FROM Payments WHERE YEAR(PaymentDate) = 2025";
                    SqlCommand incomeCmd = new SqlCommand(incomeQuery, conn);
                    object totalIncome = incomeCmd.ExecuteScalar();
                    TotalIncomeText.Text = totalIncome != DBNull.Value ? $"{Convert.ToDecimal(totalIncome):N2} руб" : "0 руб";

                    // Количество студентов
                    string studentCountQuery = "SELECT COUNT(*) FROM Students WHERE YEAR(EnrollmentDate) <= 2025";
                    SqlCommand studentCountCmd = new SqlCommand(studentCountQuery, conn);
                    StudentCountText.Text = Convert.ToInt32(studentCountCmd.ExecuteScalar()).ToString();

                    // Количество иностранных студентов
                    string intlStudentQuery = "SELECT COUNT(*) FROM Students WHERE IsInternational = 1 AND YEAR(EnrollmentDate) <= 2025";
                    SqlCommand intlStudentCmd = new SqlCommand(intlStudentQuery, conn);
                    InternationalStudentCountText.Text = Convert.ToInt32(intlStudentCmd.ExecuteScalar()).ToString();

                    // Количество преподавателей
                    string teacherCountQuery = "SELECT COUNT(*) FROM Teachers";
                    SqlCommand teacherCountCmd = new SqlCommand(teacherCountQuery, conn);
                    TeacherCountText.Text = Convert.ToInt32(teacherCountCmd.ExecuteScalar()).ToString();

                    // Общая сумма штрафов
                    string finesQuery = "SELECT SUM(c.UnitPrice) FROM Fines f JOIN Costs c ON f.CostID = c.CostID WHERE YEAR(f.DateIssued) = 2025 OR f.DateIssued IS NULL";
                    SqlCommand finesCmd = new SqlCommand(finesQuery, conn);
                    object totalFines = finesCmd.ExecuteScalar();
                    TotalFinesText.Text = totalFines != DBNull.Value ? $"{Convert.ToDecimal(totalFines):N2} руб" : "0 руб";

                    // Средний платеж
                    string avgPaymentQuery = "SELECT AVG(Amount) FROM Payments WHERE YEAR(PaymentDate) = 2025";
                    SqlCommand avgPaymentCmd = new SqlCommand(avgPaymentQuery, conn);
                    object avgPayment = avgPaymentCmd.ExecuteScalar();
                    AveragePaymentText.Text = avgPayment != DBNull.Value ? $"{Convert.ToDecimal(avgPayment):N2} руб" : "0 руб";

                    // Количество платежей
                    string totalPaymentsQuery = "SELECT COUNT(*) FROM Payments WHERE YEAR(PaymentDate) = 2025";
                    SqlCommand totalPaymentsCmd = new SqlCommand(totalPaymentsQuery, conn);
                    TotalPaymentsText.Text = Convert.ToInt32(totalPaymentsCmd.ExecuteScalar()).ToString();

                    // Студенты с неоплаченными штрафами
                    string unpaidFinesQuery = "SELECT COUNT(DISTINCT f.StudentID) \r\nFROM Fines f \r\nWHERE f.DateIssued IS NOT NULL \r\nAND YEAR(f.DateIssued) = YEAR(GETDATE());";
                    SqlCommand unpaidFinesCmd = new SqlCommand(unpaidFinesQuery, conn);
                    StudentsWithFinesText.Text = Convert.ToInt32(unpaidFinesCmd.ExecuteScalar()).ToString();

                    // Тренд доходов
                    string incomeTrendQuery = @"
                        SELECT SUM(p.Amount) AS CurrentYear
                        FROM Payments p
                        WHERE YEAR(p.PaymentDate) = 2025
                        UNION ALL
                        SELECT SUM(p.Amount) AS PreviousYear
                        FROM Payments p
                        WHERE YEAR(p.PaymentDate) = 2024";
                    SqlCommand incomeTrendCmd = new SqlCommand(incomeTrendQuery, conn);
                    SqlDataReader reader = incomeTrendCmd.ExecuteReader();
                    decimal currentYearIncome = 0, previousYearIncome = 0;
                    if (reader.Read()) currentYearIncome = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.Read()) previousYearIncome = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    reader.Close();
                    decimal incomeChange = previousYearIncome != 0 ? ((currentYearIncome - previousYearIncome) / previousYearIncome * 100) : 0;
                    IncomeTrendText.Text = $"{incomeChange:N2}%";

                    // Просроченные платежи
                    string overduePaymentsQuery = "SELECT COUNT(*) FROM Fines p WHERE DATEDIFF(day, p.DateIssued, GETDATE()) > 30 AND YEAR(p.DateIssued) = 2025";
                    SqlCommand overduePaymentsCmd = new SqlCommand(overduePaymentsQuery, conn);
                    OverduePaymentsText.Text = Convert.ToInt32(overduePaymentsCmd.ExecuteScalar()).ToString();

                    // Средний долг по штрафам
                    string avgFineDebtQuery = @"
                        SELECT AVG(Amount) AS AverageDebt
FROM Payments;";
                    SqlCommand avgFineDebtCmd = new SqlCommand(avgFineDebtQuery, conn);
                    object avgFineDebt = avgFineDebtCmd.ExecuteScalar();
                    AverageFineDebtText.Text = avgFineDebt != DBNull.Value ? $"{Convert.ToDecimal(avgFineDebt):N2} руб" : "0 руб";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сводки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ЗагрузитьОтчеты()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Отчет по платежам за 2025 год с зашифрованным номером карты
                    string paymentsQuery = @"
    SELECT p.PaymentID AS PaymentID, s.FirstName + ' ' + s.LastName AS StudentName, p.Amount AS Amount, p.Discaunt AS Discaunt,
           p.PaymentDate AS PaymentDate, p.SubjectName AS SubjectName, c.NumberCard AS NumberCard
FROM Payments p
JOIN Students s ON p.StudentID = s.StudentID
JOIN Cards c ON p.CardID = c.CardID
WHERE YEAR(p.PaymentDate) = 2025";

                    SqlDataAdapter paymentsAdapter = new SqlDataAdapter(paymentsQuery, conn);
                    DataTable paymentsDt = new DataTable();
                    paymentsAdapter.Fill(paymentsDt);

                    foreach (DataRow row in paymentsDt.Rows)
                    {
                        Debug.WriteLine($"Discaunt: {row["Discaunt"]}"); // Отладка

                        string numberCard = row["NumberCard"].ToString();
                        if (!string.IsNullOrEmpty(numberCard) && numberCard.Length > 4)
                        {
                            string maskedCard = new string('X', numberCard.Length - 4) + numberCard.Substring(numberCard.Length - 4);
                            row["NumberCard"] = maskedCard;
                        }
                    }
                    PaymentsDataGrid.ItemsSource = paymentsDt.DefaultView;

                    // Отчет по штрафам за 2025 год с ServiceType
                    string finesQuery = @"
                        SELECT f.FineID AS FineID, s.FirstName + ' ' + s.LastName AS StudentName, c.UnitPrice AS UnitPrice,
                               f.Discount AS Discount,
                               f.DateIssued AS DateIssued, c.ServiceType AS ServiceType
                        FROM Fines f
                        JOIN Students s ON f.StudentID = s.StudentID
                        JOIN Costs c ON f.CostID = c.CostID
                        WHERE YEAR(f.DateIssued) = 2025 OR f.DateIssued IS NULL";
                    SqlDataAdapter finesAdapter = new SqlDataAdapter(finesQuery, conn);
                    DataTable finesDt = new DataTable();
                    finesAdapter.Fill(finesDt);
                    FinesDataGrid.ItemsSource = finesDt.DefaultView;

                    // Итоговая сумма платежей
                    string totalQuery = "SELECT SUM(Amount) FROM Payments WHERE YEAR(PaymentDate) = 2025";
                    SqlCommand totalCmd = new SqlCommand(totalQuery, conn);
                    object totalSum = totalCmd.ExecuteScalar();
                    TotalReportSumText.Text = totalSum != DBNull.Value ? $"{Convert.ToDecimal(totalSum):N2} руб" : "0 руб";

                    // Средний платеж
                    string avgQuery = "SELECT AVG(Amount) FROM Payments WHERE YEAR(PaymentDate) = 2025";
                    SqlCommand avgCmd = new SqlCommand(avgQuery, conn);
                    object avgSum = avgCmd.ExecuteScalar();
                    AverageReportPaymentText.Text = avgSum != DBNull.Value ? $"{Convert.ToDecimal(avgSum):N2} руб" : "0 руб";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке отчетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window exportChoiceWindow = new Window
                {
                    Title = "Выбор таблицы для экспорта",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(10) };
                RadioButton paymentsRadio = new RadioButton { Content = "История платежей", IsChecked = true, Margin = new Thickness(0, 0, 0, 10) };
                RadioButton finesRadio = new RadioButton { Content = "История штрафов", Margin = new Thickness(0, 0, 0, 10) };
                Button exportButton = new Button { Content = "Экспортировать", Width = 100, Margin = new Thickness(0, 10, 0, 0) };

                panel.Children.Add(paymentsRadio);
                panel.Children.Add(finesRadio);
                panel.Children.Add(exportButton);
                exportChoiceWindow.Content = panel;

                exportButton.Click += (s, args) =>
                {
                    DataGrid selectedGrid = paymentsRadio.IsChecked == true ? PaymentsDataGrid : FinesDataGrid;
                    if (selectedGrid.Items.Count == 0)
                    {
                        MessageBox.Show("Выбранная таблица пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        exportChoiceWindow.Close();
                        return;
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PDF файлы (*.pdf)|*.pdf",
                        FileName = paymentsRadio.IsChecked == true ? "PaymentsReport.pdf" : "FinesReport.pdf"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        Document doc = new Document(PageSize.A4.Rotate());
                        PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                        doc.Open();

                        // Регистрация шрифта с поддержкой кириллицы
                        BaseFont baseFont = BaseFont.CreateFont("c:\\windows\\fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font font = new Font(baseFont, 12);

                        PdfPTable table = new PdfPTable(selectedGrid.Columns.Count);
                        table.WidthPercentage = 100;

                        // Добавление заголовков
                        foreach (DataGridColumn column in selectedGrid.Columns)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(column.Header.ToString(), font));
                            table.AddCell(cell);
                        }

                        // Добавление данных
                        foreach (var item in selectedGrid.Items)
                        {
                            if (item is DataRowView row)
                            {
                                foreach (DataGridColumn column in selectedGrid.Columns)
                                {
                                    string columnName = column.Header.ToString();
                                    if (row.Row.Table.Columns.Contains(columnName))
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(row[columnName]?.ToString() ?? "", font));
                                        table.AddCell(cell);
                                    }
                                    else
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase("", font));
                                        table.AddCell(cell);
                                    }
                                }
                            }
                        }

                        doc.Add(table);
                        doc.Close();
                        MessageBox.Show("PDF отчет успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    exportChoiceWindow.Close();
                };

                exportChoiceWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Диалог выбора таблицы
                Window exportChoiceWindow = new Window
                {
                    Title = "Выбор таблицы для экспорта",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(10) };
                RadioButton paymentsRadio = new RadioButton { Content = "История платежей", IsChecked = true, Margin = new Thickness(0, 0, 0, 10) };
                RadioButton finesRadio = new RadioButton { Content = "История штрафов", Margin = new Thickness(0, 0, 0, 10) };
                Button exportButton = new Button { Content = "Экспортировать", Width = 100, Margin = new Thickness(0, 10, 0, 0) };

                panel.Children.Add(paymentsRadio);
                panel.Children.Add(finesRadio);
                panel.Children.Add(exportButton);
                exportChoiceWindow.Content = panel;

                exportButton.Click += (s, args) =>
                {
                    DataGrid selectedGrid = paymentsRadio.IsChecked == true ? PaymentsDataGrid : FinesDataGrid;
                    if (selectedGrid.Items.Count == 0)
                    {
                        MessageBox.Show("Выбранная таблица пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        exportChoiceWindow.Close();
                        return;
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "CSV файлы (*.csv)|*.csv",
                        FileName = paymentsRadio.IsChecked == true ? "PaymentsReport.csv" : "FinesReport.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var records = new List<object>();
                        foreach (var item in selectedGrid.Items)
                        {
                            DataRowView row = item as DataRowView;
                            if (row != null)
                            {
                                if (paymentsRadio.IsChecked == true)
                                {
                                    int discount;
                                    int.TryParse(row["Discaunt"]?.ToString(), out discount); // Пробуем преобразовать в int, если не удалось, discount будет 0

                                    records.Add(new PaymentRecord
                                    {
                                        PaymentID = row["PaymentID"]?.ToString(),
                                        StudentName = row["StudentName"]?.ToString(),
                                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0m,
                                        PaymentDate = row["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(row["PaymentDate"]) : DateTime.MinValue,
                                        SubjectName = row["SubjectName"]?.ToString(),
                                        NumberCard = row["NumberCard"]?.ToString(),
                                        Discaunt = discount // Присваиваем значение типа int
                                    });
                                }
                                else
                                {
                                    int discount;
                                    int.TryParse(row["Discount"]?.ToString(), out discount); // Пробуем преобразовать в int

                                    records.Add(new FineRecord
                                    {
                                        FineID = row["FineID"]?.ToString(),
                                        StudentName = row["StudentName"]?.ToString(),
                                        UnitPrice = row["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["UnitPrice"]) : 0m,
                                        DateIssued = row["DateIssued"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["DateIssued"]) : null,
                                        ServiceType = row["ServiceType"]?.ToString(),
                                        Discount = discount // Присваиваем значение типа int
                                    });
                                }
                            }
                        }

                        // Используем UTF-8 с BOM для корректной поддержки кириллицы
                        using (var writer = new StreamWriter(saveFileDialog.FileName, false, new System.Text.UTF8Encoding(true)))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            if (paymentsRadio.IsChecked == true)
                                csv.WriteRecords(records.Cast<PaymentRecord>());
                            else
                                csv.WriteRecords(records.Cast<FineRecord>());
                        }

                        MessageBox.Show("CSV отчет успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    exportChoiceWindow.Close();
                };

                exportChoiceWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Диалог выбора таблицы
                Window printChoiceWindow = new Window
                {
                    Title = "Выбор таблицы для печати",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(10) };
                RadioButton paymentsRadio = new RadioButton { Content = "История платежей", IsChecked = true, Margin = new Thickness(0, 0, 0, 10) };
                RadioButton finesRadio = new RadioButton { Content = "История штрафов", Margin = new Thickness(0, 0, 0, 10) };
                Button printButton = new Button { Content = "Печать", Width = 100, Margin = new Thickness(0, 10, 0, 0) };

                panel.Children.Add(paymentsRadio);
                panel.Children.Add(finesRadio);
                panel.Children.Add(printButton);
                printChoiceWindow.Content = panel;

                printButton.Click += (s, args) =>
                {
                    DataGrid selectedGrid = paymentsRadio.IsChecked == true ? PaymentsDataGrid : FinesDataGrid;
                    if (selectedGrid.Items.Count == 0)
                    {
                        MessageBox.Show("Выбранная таблица пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        printChoiceWindow.Close();
                        return;
                    }

                    System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintVisual(selectedGrid, paymentsRadio.IsChecked == true ? "Отчет по платежам" : "Отчет по штрафам");
                    }
                    printChoiceWindow.Close();
                };

                printChoiceWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterByDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window dateFilterWindow = new Window
                {
                    Title = "Фильтр по дате",
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(10) };
                DatePicker startDatePicker = new DatePicker { Margin = new Thickness(0, 0, 0, 10), DisplayDate = new DateTime(2025, 1, 1) };
                DatePicker endDatePicker = new DatePicker { Margin = new Thickness(0, 0, 0, 10), DisplayDate = DateTime.Now };
                Button applyButton = new Button { Content = "Применить", Width = 100, Margin = new Thickness(0, 10, 0, 0) };

                panel.Children.Add(new TextBlock { Text = "Дата начала:", Margin = new Thickness(0, 0, 0, 5) });
                panel.Children.Add(startDatePicker);
                panel.Children.Add(new TextBlock { Text = "Дата окончания:", Margin = new Thickness(0, 0, 0, 5) });
                panel.Children.Add(endDatePicker);
                panel.Children.Add(applyButton);
                dateFilterWindow.Content = panel;

                applyButton.Click += (s, args) =>
                {
                    if (startDatePicker.SelectedDate.HasValue && endDatePicker.SelectedDate.HasValue)
                    {
                        DateTime startDate = startDatePicker.SelectedDate.Value;
                        DateTime endDate = endDatePicker.SelectedDate.Value;
                        ФильтроватьОтчетыПоДатам(startDate, endDate);
                        dateFilterWindow.Close();
                    }
                    else
                    {
                        MessageBox.Show("Пожалуйста, выберите обе даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                dateFilterWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ФильтроватьОтчетыПоДатам(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Фильтрация платежей
                    string paymentsQuery = @"
                        SELECT p.PaymentID AS PaymentID, s.FirstName + ' ' + s.LastName AS StudentName, p.Amount AS Amount, p.Discaunt,
                               p.PaymentDate AS PaymentDate, p.SubjectName AS SubjectName, c.NumberCard AS NumberCard
                        FROM Payments p
                        JOIN Students s ON p.StudentID = s.StudentID
                        JOIN Cards c ON p.CardID = c.CardID
                        WHERE p.PaymentDate BETWEEN @StartDate AND @EndDate";
                    SqlDataAdapter paymentsAdapter = new SqlDataAdapter(paymentsQuery, conn);
                    paymentsAdapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                    paymentsAdapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                    DataTable paymentsDt = new DataTable();
                    paymentsAdapter.Fill(paymentsDt);

                    // Зашифровать номер карты
                    foreach (DataRow row in paymentsDt.Rows)
                    {
                        string numberCard = row["NumberCard"].ToString();
                        if (!string.IsNullOrEmpty(numberCard) && numberCard.Length > 4)
                        {
                            string maskedCard = new string('X', numberCard.Length - 4) + numberCard.Substring(numberCard.Length - 4);
                            row["NumberCard"] = maskedCard;
                        }
                    }
                    PaymentsDataGrid.ItemsSource = paymentsDt.DefaultView;

                    // Фильтрация штрафов
                    string finesQuery = @"
                        SELECT f.FineID AS FineID, s.FirstName + ' ' + s.LastName AS StudentName, c.UnitPrice AS UnitPrice, f.Discount,
                               f.DateIssued AS DateIssued, c.ServiceType AS ServiceType
                        FROM Fines f
                        JOIN Students s ON f.StudentID = s.StudentID
                        JOIN Costs c ON f.CostID = c.CostID
                        WHERE (f.DateIssued BETWEEN @StartDate AND @EndDate OR f.DateIssued IS NULL)";
                    SqlDataAdapter finesAdapter = new SqlDataAdapter(finesQuery, conn);
                    finesAdapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                    finesAdapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                    DataTable finesDt = new DataTable();
                    finesAdapter.Fill(finesDt);
                    FinesDataGrid.ItemsSource = finesDt.DefaultView;

                    // Итоговая сумма платежей
                    string totalQuery = "SELECT SUM(Amount) FROM Payments WHERE PaymentDate BETWEEN @StartDate AND @EndDate";
                    SqlCommand totalCmd = new SqlCommand(totalQuery, conn);
                    totalCmd.Parameters.AddWithValue("@StartDate", startDate);
                    totalCmd.Parameters.AddWithValue("@EndDate", endDate);
                    object totalSum = totalCmd.ExecuteScalar();
                    TotalReportSumText.Text = totalSum != DBNull.Value ? $"{Convert.ToDecimal(totalSum):N2} руб" : "0 руб";

                    // Средний платеж
                    string avgQuery = "SELECT AVG(Amount) FROM Payments WHERE PaymentDate BETWEEN @StartDate AND @EndDate";
                    SqlCommand avgCmd = new SqlCommand(avgQuery, conn);
                    avgCmd.Parameters.AddWithValue("@StartDate", startDate);
                    avgCmd.Parameters.AddWithValue("@EndDate", endDate);
                    object avgSum = avgCmd.ExecuteScalar();
                    AverageReportPaymentText.Text = avgSum != DBNull.Value ? $"{Convert.ToDecimal(avgSum):N2} руб" : "0 руб";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}