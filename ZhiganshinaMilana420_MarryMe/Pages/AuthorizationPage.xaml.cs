using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
using ZhiganshinaMilana420_MarryMe.DB.Partikal;
using static MaterialDesignThemes.Wpf.Theme;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public class UserInfo
    {
        public static Users User { get; set; }
    }
    public partial class AuthorizationPage : Page
    {
        //private TextBox visiblePasswordTextBox;

        public static List<Users> users { get; set; } 
        public AuthorizationPage()
        {
            InitializeComponent();
            //InitializePasswordVisibilityControls();
        }

        //private void InitializePasswordVisibilityControls()
        //{
        //    // Создаем TextBox для отображения пароля
        //    visiblePasswordTextBox = new TextBox
        //    {
        //        Visibility = Visibility.Collapsed,
        //        Width = PasswordTb.Width,
        //        Height = PasswordTb.Height
        //    };

        //    // Добавляем его в тот же Grid, где находится PasswordBox
        //    var grid = (Grid)PasswordTb.Parent;
        //    Grid.SetColumn(visiblePasswordTextBox, 0);
        //    grid.Children.Add(visiblePasswordTextBox);
        //}

        //private void TogglePasswordBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    isPasswordVisible = !isPasswordVisible;

        //    if (isPasswordVisible)
        //    {
        //        // Показываем пароль
        //        visiblePasswordTextBox.Text = PasswordTb.Password;
        //        visiblePasswordTextBox.Visibility = Visibility.Visible;
        //        PasswordTb.Visibility = Visibility.Collapsed;
        //        TogglePasswordBtn.Content = "🙈";
        //    }
        //    else
        //    {
        //        // Скрываем пароль
        //        PasswordTb.Password = visiblePasswordTextBox.Text;
        //        PasswordTb.Visibility = Visibility.Visible;
        //        visiblePasswordTextBox.Visibility = Visibility.Collapsed;
        //        TogglePasswordBtn.Content = "👁";
        //    }
        //}


        

        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ResetValidationStyles()
        {
            LoginTbx.BorderBrush = Brushes.LightGray;
            PasswordPbx.BorderBrush = Brushes.LightGray;
            txtError.Text = "";
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetValidationStyles();
            bool isValid = true;
            string login = LoginTbx.Text.Trim();
            string password = PasswordPbx.Password.Trim();

            users = new List<Users>(DbConnection.MarryMe.Users.ToList());

            if (string.IsNullOrEmpty(LoginTbx.Text))
            {
                ApplyErrorStyle(LoginTbx);
                isValid = false;
            }
            if (string.IsNullOrEmpty(PasswordPbx.Password))
            {
                ApplyErrorStyle(PasswordPbx);
                isValid = false;
            }
            if (!isValid)
            {
                txtError.Text = "Заполните все обязательные поля!";
                return;
            }
            if (password.Length < 3)
            {
                txtError.Text = "Пароль должен содержать минимум 3 символа!";
                return;
            }
            Users currentUser = users.FirstOrDefault(i => i.Login == login && i.Password == password);
            if (currentUser != null)
            {
                UserInfo.User = currentUser;
                NavigationService.Navigate(new MenuPage(currentUser));
                return;
            }
                txtError.Text = "Неверные данные"; 
                //MessageBox.Show("Неверные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
