using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для DismissalWindow.xaml
    /// </summary>
    public partial class DismissalWindow : Window
    {
        public Users Employee { get; set; }
        public string DismissalReason { get; set; }
        public string DismissalArticle { get; set; }
        public string EmployeeFullName => $"{Employee.Surname} {Employee.Name} {Employee.Patronymic}";

        public DismissalWindow(Users employee)
        {
            InitializeComponent();
            Employee = employee;
            DataContext = this;
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonTb.Text))
            {
                MessageBox.Show("Укажите причину увольнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DismissalReason = ReasonTb.Text;
            DismissalArticle = (ArticleCb.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
