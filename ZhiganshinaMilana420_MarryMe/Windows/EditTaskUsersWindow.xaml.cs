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
    /// Логика взаимодействия для EditTaskUsersWindow.xaml
    /// </summary>
    public partial class EditTaskUsersWindow : Window
    {
        public static Users use { get; set; }
        Users contextUsers;
        TaskUsers currentTask; // Добавляем поле для хранения редактируемой задачи
        public static TaskUsers taskUsers1 = new TaskUsers();

        public EditTaskUsersWindow(Users users, TaskUsers task)
        {
            InitializeComponent();
            contextUsers = users;
            currentTask = task; // Сохраняем задачу
            use = users;
            taskUsers1 = task;
            NameEmployeeTb.Text = $"Редактирование задачи сотрудника {contextUsers.Surname} {contextUsers.Name}!";

            // Заполняем поля данными из задачи
            DescriptionTb.Text = currentTask.Description;
            DateEndDp.SelectedDate = currentTask.DateEnd;

            this.DataContext = this;
        }
        private void DateEndDp_CalendarOpened(object sender, RoutedEventArgs e)
        {
            // Устанавливаем минимальную дату - сегодня
            DateEndDp.DisplayDateStart = DateTime.Today;

            // Если выбрана прошедшая дата, сбрасываем на сегодня
            if (DateEndDp.SelectedDate < DateTime.Today)
            {
                DateEndDp.SelectedDate = DateTime.Today;
            }
        }

        private void ClouseBtt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddTaksUserBt_Click(object sender, RoutedEventArgs e)
        {
            if (DateEndDp.Text == "" || DescriptionTb.Text == "")
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                DateTime today = DateTime.Today;
                TaskUsers taskUsers = taskUsers1;

                taskUsers.DateAppointment = Convert.ToDateTime(today.ToString("dd.MM.yyyy"));
                taskUsers.DateEnd = Convert.ToDateTime(DateEndDp.Text);
                taskUsers.Description = DescriptionTb.Text;
                taskUsers.Done = false;
                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show($"Задача сотрудника {contextUsers.Surname} {contextUsers.Name} отредактированна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Закрываем окно после успешного редакитрования
            }
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
