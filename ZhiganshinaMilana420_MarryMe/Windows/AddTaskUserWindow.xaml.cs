using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ZhiganshinaMilana420_MarryMe.Pages;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddTaskUserWindow.xaml
    /// </summary>
    public partial class AddTaskUserWindow : Window
    {
        Users contextUsers;
        public AddTaskUserWindow(Users users)
        {
            InitializeComponent();
            contextUsers = users;
            NameEmployeeTb.Text = $"Задача сотрудника {contextUsers.Surname} {contextUsers.Name}!";
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

        private void AddTaksUserBt_Click(object sender, RoutedEventArgs e)
        {
            if (DateEndDp.Text == "" || DescriptionTb.Text == "")
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                DateTime today = DateTime.Today;
                TaskUsers taskUsers = new TaskUsers();

                taskUsers.DateAppointment = Convert.ToDateTime(today.ToString("dd.MM.yyyy"));
                taskUsers.DateEnd = Convert.ToDateTime(DateEndDp.Text);
                taskUsers.Description = DescriptionTb.Text;
                taskUsers.UserId = contextUsers.Id;
                taskUsers.Done = false;
                taskUsers.AdminId = UserInfo.User.Id;
                taskUsers.AdminLastName = UserInfo.User.Surname;
                taskUsers.AdminFirstName = UserInfo.User.Name;
                taskUsers.AdminPhoto = UserInfo.User.Photo;
                DbConnection.MarryMe.TaskUsers.Add(taskUsers);
                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show($"Задача назначена сотруднику {contextUsers.Surname} {contextUsers.Name}!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Закрываем окно после успешного добавления
            }
        }

        private void ClouseBtt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
