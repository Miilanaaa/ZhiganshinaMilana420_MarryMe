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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    public static class Clock
    {
        public static string CurrentTime => DateTime.Now.ToString("HH:mm:ss");
    }
    public partial class MenuPage : Page
    {
        public static Users users { get; set; }
        public static Users use { get; set; }
        public MenuPage(Users user)
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Обновление каждую секунду
            timer.Tick += Timer_Tick;
            timer.Start();
            DateTime today = DateTime.Today;
            DateTextBlock.Text = today.ToString("dd.MM.yyyy");
            UpdateTime();
            use = user;
            users = user;
            TBUser.Text = $"{user.Surname} {user.Name}";
            this.DataContext = this;

            if(user.RoleId == 1)
            {
                CollectionBtn.Visibility = Visibility.Visible;
                TaskBtn.Visibility = Visibility.Visible;

            }
            else
            {
                CollectionBtn.Visibility = Visibility.Hidden;
                TaskBtn.Visibility = Visibility.Hidden;
            }

            ContentFrame.Navigate(new HomePage(users));

        }

        private void UpdateTime()
        {
            TimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }
    
        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new HomePage(users));

        }

        private void CollectionBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CollectionPage());

        }

        private void ClientBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ClientMenuPage());
        }

        private void TaskBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new TaskUserAddPage());
        }

        private void OtchetBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы точно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new AuthorizationPage());
            }
        }

        private void ClouseBtt_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        private void RollUpBtt_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        //private void SmallScreenBtt_Click(object sender, RoutedEventArgs e)
        //{
        //    Window parentWindow = Window.GetWindow(this);
        //    if (parentWindow.WindowState == WindowState.Maximized)
        //    {
        //        parentWindow.WindowState = WindowState.Normal;
        //    }
        //    else
        //    {
        //        parentWindow.WindowState = WindowState.Maximized;
        //    }
        //}

        private void EditAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditUserPage(users));
        }

        private void ProectBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ReportsPage());
        }
    }
}