using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder;
using ZhiganshinaMilana420_MarryMe.Windows;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для TaskPage.xaml
    /// </summary>
    public partial class TaskPage : Page
    {
        public static List<TaskUsers> taskUsers { get; set; }
        Users contextUsers;
        public TaskPage(Users users)
        {
            InitializeComponent();
            contextUsers = users;
            DateTime today = DateTime.Today;
            taskUsers = DbConnection.MarryMe.TaskUsers.Where(i => i.UserId == users.Id && i.DateEnd == today).ToList();
            TaskUserLV.ItemsSource = taskUsers;
            DateTaskDp.Text = today.ToString();

            this.DataContext = this;
        }

        

        private void AddTaskUserBt_Click(object sender, RoutedEventArgs e)
        {
            AddTaskUserWindow addTaskUserWindow = new AddTaskUserWindow(contextUsers);
            addTaskUserWindow.Closed += (s, args) =>
            {
                RefreshTaskList(); // Обновляем список при закрытии окна
            };
            addTaskUserWindow.ShowDialog();
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TaskUserAddPage());
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void DateTaskDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateTaskDp.SelectedDate == null)
            {
                return;
            }
            DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.MinValue;

            var filteredItems = DbConnection.MarryMe.TaskUsers.Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id).ToList();
            TaskUserLV.ItemsSource = filteredItems;
        }
        public void RefreshTaskList()
        {
            if (DateTaskDp.SelectedDate != null)
            {
                DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.Today;
                taskUsers = new List<TaskUsers>(DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id).ToList());
                TaskUserLV.ItemsSource = taskUsers;
            }
            else
            {
                DateTime today = DateTime.Today;
                taskUsers = new List<TaskUsers>(DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == today && i.UserId == contextUsers.Id).ToList());
                TaskUserLV.ItemsSource = taskUsers;
            }
        }
        private void EditBt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button.DataContext as TaskUsers;

            if (task != null)
            {
                // Проверяем, что текущий пользователь - администратор задачи
                if (task.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете редактировать только задачи, которые создали вы.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что дата задачи еще не прошла
                if (task.DateEnd < DateTime.Today)
                {
                    MessageBox.Show("Редактирование задач с прошедшей датой запрещено.",
                          "Ограничение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                EditTaskUsersWindow editTaskUsersWindow = new EditTaskUsersWindow(contextUsers, task);
                editTaskUsersWindow.Closed += (s, args) =>
                {
                    RefreshTaskList();
                };
                editTaskUsersWindow.ShowDialog();
            }
        }
        private void DelateBt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskUsers taskToDelete)
            {
                // Проверяем, что текущий пользователь - администратор задачи
                if (taskToDelete.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете удалять только задачи, которые создали сами.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что дата задачи еще не прошла
                if (taskToDelete.DateEnd < DateTime.Today)
                {
                    MessageBox.Show("Удаление задач с прошедшей датой запрещено.",
                          "Ограничение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту задачу?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.TaskUsers.Remove(taskToDelete);
                        DbConnection.MarryMe.SaveChanges();
                        RefreshTaskList();
                        MessageBox.Show("Задача успешно удалена!",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить задачу: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
