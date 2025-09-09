using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Kursovoy
{
    public partial class CostEditorWindow : Window
    {
        public int? CostID { get; set; } = null;
        private static string connectionString = "Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;";
        public CostEditorWindow()
        {
            InitializeComponent();
        }

        public CostEditorWindow(DataRowView row) : this()
        {
            CostID = Convert.ToInt32(row["CostID"]);
            ServiceTypeBox.Text = row["ServiceType"].ToString();
            PositionIDBox.Text = row["PositionID"].ToString();
            UnitPriceBox.Text = row["UnitPrice"].ToString();
            IsForeignBox.IsChecked = Convert.ToBoolean(row["IsForeign"]);
        }

        private void ShowDataWindow(string title, string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    Window dataWindow = new Window
                    {
                        Title = title,
                        Width = 600,
                        Height = 400,
                        Content = new ScrollViewer
                        {
                            Content = new DataGrid
                            {
                                ItemsSource = dt.DefaultView,
                                AutoGenerateColumns = true,
                                CanUserAddRows = false,
                                IsReadOnly = true
                            }
                        }
                    };
                    dataWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private void ShowPositions_Click(object sender, RoutedEventArgs e)
        {
            ShowDataWindow("Список должностей",
                @"SELECT PositionID AS 'ID должности', PositionName AS 'Название должности'
          FROM Positions");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string serviceType = ServiceTypeBox.Text;
            int positionId = int.Parse(PositionIDBox.Text);
            decimal unitPrice = decimal.Parse(UnitPriceBox.Text);
            bool isForeign = IsForeignBox.IsChecked ?? false;

            using (SqlConnection conn = new SqlConnection("Server=MAXIM\\SQLEXPRESS;Database=Kursovoi;Integrated Security=true;"))
            {
                conn.Open();
                SqlCommand cmd;

                if (CostID == null)
                {
                    cmd = new SqlCommand("INSERT INTO Costs (ServiceType, PositionID, UnitPrice, IsForeign) VALUES (@type, @position, @price, @foreign)", conn);
                }
                else
                {
                    cmd = new SqlCommand("UPDATE Costs SET ServiceType=@type, PositionID=@position, UnitPrice=@price, IsForeign=@foreign WHERE CostID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", CostID);
                }

                cmd.Parameters.AddWithValue("@type", serviceType);
                cmd.Parameters.AddWithValue("@position", positionId);
                cmd.Parameters.AddWithValue("@price", unitPrice);
                cmd.Parameters.AddWithValue("@foreign", isForeign);
                cmd.ExecuteNonQuery();
            }

            DialogResult = true;
        }
    }
}
