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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditClientPage.xaml
    /// </summary>
    public partial class EditClientPage : Page
    {
        private Couple _currentCouple;

        public EditClientPage(Couple selectedCouple)
        {
            InitializeComponent();

            if (selectedCouple == null)
            {
                MessageBox.Show("Не выбрана пара для редактирования!");
                NavigationService.GoBack();
                return;
            }

            _currentCouple = selectedCouple;
            LoadCoupleData();
            ValidateFields(); // Проверка полей сразу при загрузке
        }

        private void LoadCoupleData()
        {
            try
            {
                // Загрузка данных жениха с проверкой на null
                var groom = _currentCouple?.Gromm;
                if (groom != null)
                {
                    SurnameGroomTb.Text = groom.Surname ?? "";
                    NameGroomTb.Text = groom.Name ?? "";
                    PatronymicGroomTb.Text = groom.Patronymic ?? "";
                    NumberTelGroomTb.Text = groom.PhoneNumber ?? "";
                    EmailGroomTb.Text = groom.Email ?? "";
                    PassportNumberGroomTb.Text = groom.PassportNumber ?? "";
                    PassportSeriesGroomTb.Text = groom.PassportSeries ?? "";
                    PassportAddressGroomTb.Text = groom.PassportAddress ?? "";
                    AddresssGroomTb.Text = groom.Addresss ?? "";
                }

                // Загрузка данных невесты с проверкой на null
                var bride = _currentCouple?.Bride;
                if (bride != null)
                {
                    SurnameBrideTb.Text = bride.Surname ?? "";
                    NameBrideTb.Text = bride.Name ?? "";
                    PatronymicBrideTb.Text = bride.Patronymic ?? "";
                    NumberTelBrideTb.Text = bride.PhoneNumber ?? "";
                    EmailBrideTb.Text = bride.Email ?? "";
                    PassportNumberBrideTb.Text = bride.PassportNumber ?? "";
                    PassportSeriesBrideTb.Text = bride.PassportSeries ?? "";
                    PassportAddressBrideTb.Text = bride.PassportAddress ?? "";
                    AddresssBrideTb.Text = bride.Addresss ?? "";
                }

                // Загрузка общих данных
                if (_currentCouple != null)
                {
                    DateDp.SelectedDate = _currentCouple.WeddingDate;
                    WeddingBudgetTb.Text = _currentCouple.WeddingBudget?.ToString() ?? "";
                    CountGuestsTb.Text = _currentCouple.NumberGuests?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            // Список всех обязательных полей
            var requiredFields = new List<Control>
            {
                // Жених
                SurnameGroomTb, NameGroomTb, PatronymicGroomTb,
                NumberTelGroomTb, EmailGroomTb,
                PassportNumberGroomTb, PassportSeriesGroomTb,
                PassportAddressGroomTb, AddresssGroomTb,
                
                // Невеста
                SurnameBrideTb, NameBrideTb, PatronymicBrideTb,
                NumberTelBrideTb, EmailBrideTb,
                PassportNumberBrideTb, PassportSeriesBrideTb,
                PassportAddressBrideTb, AddresssBrideTb,
                
                // Общие поля
                DateDp, WeddingBudgetTb, CountGuestsTb
            };

            foreach (var field in requiredFields)
            {
                if (field is System.Windows.Controls.TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else if (field is System.Windows.Controls.DatePicker datePicker && datePicker.SelectedDate == null)
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else
                {
                    ClearErrorStyle(field);
                }
            }

            return isValid;
        }

        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABAdB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        private void SaveBtt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                //MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Обновление данных жениха с проверкой на null
                if (_currentCouple?.Gromm != null)
                {
                    var groom = _currentCouple.Gromm;
                    groom.Surname = SurnameGroomTb.Text;
                    groom.Name = NameGroomTb.Text;
                    groom.Patronymic = PatronymicGroomTb.Text;
                    groom.PhoneNumber = NumberTelGroomTb.Text;
                    groom.Email = EmailGroomTb.Text;
                    groom.PassportNumber = PassportNumberGroomTb.Text;
                    groom.PassportSeries = PassportSeriesGroomTb.Text;
                    groom.PassportAddress = PassportAddressGroomTb.Text;
                    groom.Addresss = AddresssGroomTb.Text;
                }

                // Обновление данных невесты с проверкой на null
                if (_currentCouple?.Bride != null)
                {
                    var bride = _currentCouple.Bride;
                    bride.Surname = SurnameBrideTb.Text;
                    bride.Name = NameBrideTb.Text;
                    bride.Patronymic = PatronymicBrideTb.Text;
                    bride.PhoneNumber = NumberTelBrideTb.Text;
                    bride.Email = EmailBrideTb.Text;
                    bride.PassportNumber = PassportNumberBrideTb.Text;
                    bride.PassportSeries = PassportSeriesBrideTb.Text;
                    bride.PassportAddress = PassportAddressBrideTb.Text;
                    bride.Addresss = AddresssBrideTb.Text;
                }

                // Обновление общих данных
                if (_currentCouple != null)
                {
                    _currentCouple.WeddingDate = DateDp.SelectedDate ?? DateTime.Now;
                    _currentCouple.WeddingBudget = Convert.ToInt32(WeddingBudgetTb.Text);
                    _currentCouple.NumberGuests = Convert.ToInt32(CountGuestsTb.Text);
                }

                DbConnection.MarryMe.SaveChanges();
                //MessageBox.Show("Данные пары успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new ClientMenuPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitBtt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClientMenuPage());
        }
    }
}
