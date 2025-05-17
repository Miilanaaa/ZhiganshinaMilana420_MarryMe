using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditUserPage.xaml
    /// </summary>
    public partial class EditUserPage : Page
    {
        private bool isPasswordVisible = false;
        private TextBox visiblePasswordTextBox;
        private Users contextUser;
        private byte[] photoBytes;

        public EditUserPage(Users user)
        {
            InitializeComponent();
            InitializePasswordVisibilityControls();
            contextUser = user;
            LoadUserData();
            LoadGender();
            this.DataContext = this;
            ValidateFields();
        }

        private void InitializePasswordVisibilityControls()
        {
            visiblePasswordTextBox = new TextBox
            {
                FontSize = 16,
                Width = 200,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var stackPanel = PasswordTb.Parent as StackPanel;
            stackPanel?.Children.Insert(0, visiblePasswordTextBox);
        }

        private void LoadUserData()
        {
            // Основные данные
            SurnameTb.Text = contextUser.Surname;
            NameTb.Text = contextUser.Name;
            PatronymicTb.Text = contextUser.Patronymic;
            LoginTb.Text = contextUser.Login;
            PasswordTb.Password = contextUser.Password;
            EmailTb.Text = contextUser.Email;

            // Даты и финансы
            BirthDateDp.SelectedDate = contextUser.BirthDate;

            // Фото
            if (contextUser.Photo != null)
            {
                photoBytes = contextUser.Photo;
                TestImg.Source = LoadImage(photoBytes);
            }
        }

        private void LoadGender()
        {
            if (contextUser.IdGender == 1) // Мужской
            {
                GenderMen.IsChecked = true;
                GenderGirl.IsChecked = false;
            }
            else if (contextUser.IdGender == 2) // Женский
            {
                GenderMen.IsChecked = false;
                GenderGirl.IsChecked = true;
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            // Список всех обязательных полей
            var requiredFields = new List<Control>
            {
                SurnameTb, NameTb, PatronymicTb,
                LoginTb, EmailTb, 
                BirthDateDp
            };

            foreach (var field in requiredFields)
            {
                if (field is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else if (field is ComboBox comboBox && comboBox.SelectedItem == null)
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else if (field is DatePicker datePicker && datePicker.SelectedDate == null)
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else
                {
                    ClearErrorStyle(field);
                }
            }

            // Проверка пароля
            if (string.IsNullOrEmpty(isPasswordVisible ? visiblePasswordTextBox.Text : PasswordTb.Password))
            {
                ApplyErrorStyle(PasswordTb);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(PasswordTb);
            }

            // Проверка пола
            if (!GenderMen.IsChecked.GetValueOrDefault() && !GenderGirl.IsChecked.GetValueOrDefault())
            {
                ApplyErrorStyle(GenderMen);
                ApplyErrorStyle(GenderGirl);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(GenderMen);
                ClearErrorStyle(GenderGirl);
            }

            // Проверка фото
            if (TestImg.Source == null)
            {
                ApplyErrorStyle(TestImg);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(TestImg);
            }

            return isValid;
        }

        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ApplyErrorStyle(Image image)
        {
            var parentBorder = image.Parent as Border;
            if (parentBorder != null)
            {
                parentBorder.BorderBrush = Brushes.Red;
                parentBorder.BorderThickness = new Thickness(2);
                parentBorder.ToolTip = "Добавьте фото";
            }
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABAdB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        private void ClearErrorStyle(Image image)
        {
            var parentBorder = image.Parent as Border;
            if (parentBorder != null)
            {
                parentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4C8BF"));
                parentBorder.BorderThickness = new Thickness(5);
                parentBorder.ToolTip = null;
            }
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                visiblePasswordTextBox.Text = PasswordTb.Password;
                visiblePasswordTextBox.Visibility = Visibility.Visible;
                PasswordTb.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "🙈";
            }
            else
            {
                PasswordTb.Password = visiblePasswordTextBox.Text;
                PasswordTb.Visibility = Visibility.Visible;
                visiblePasswordTextBox.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "👁";
            }
        }

        private void changeBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "*.png|*.png|*.jpeg|*.jpeg|*.jpg|*.jpg"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                ClearErrorStyle(TestImg);
            }
        }

        private void AddEmployyeBt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Обновление основных данных
                contextUser.Surname = SurnameTb.Text;
                contextUser.Name = NameTb.Text;
                contextUser.Patronymic = PatronymicTb.Text;
                contextUser.Login = LoginTb.Text;
                contextUser.Password = isPasswordVisible ? visiblePasswordTextBox.Text : PasswordTb.Password;
                contextUser.Email = EmailTb.Text;

                // Обновление дат и финансов
                contextUser.BirthDate = BirthDateDp.SelectedDate.Value;

                // Обновление роли и пола
                
                contextUser.IdGender = GenderMen.IsChecked.GetValueOrDefault() ? 1 : 2;

                // Обновление фото
                if (photoBytes != null)
                {
                    contextUser.Photo = photoBytes;
                }

                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new MenuPage(UserInfo.User));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoBackBtt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

