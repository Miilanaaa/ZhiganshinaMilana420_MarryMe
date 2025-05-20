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
using static MaterialDesignThemes.Wpf.Theme;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddClientPage.xaml
    /// </summary>
    public partial class AddClientPage : Page
    {
        public AddClientPage()
        {
            InitializeComponent();
            DateDp.SelectedDateChanged += DateDp_SelectedDateChanged;
        }
        private void DateDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateDp.SelectedDate.HasValue)
            {
                DateTime minWeddingDate = DateTime.Today.AddMonths(1);
                if (DateDp.SelectedDate.Value < minWeddingDate)
                {
                    ApplyErrorStyle(DateDp);
                    DateDp.ToolTip = $"Дата свадьбы должна быть не ранее {minWeddingDate:dd.MM.yyyy}";
                }
                else
                {
                    ClearErrorStyle(DateDp);
                }
            }
        }
        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABAdB3")); // Default color
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
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
                    if (datePicker.SelectedDate == null)
                    {
                        ApplyErrorStyle(field);
                        isValid = false;
                    }
                    else if (datePicker == DateDp)
                    {
                        // Проверка что дата свадьбы не менее чем через месяц
                        DateTime minWeddingDate = DateTime.Today.AddMonths(1);
                        if (datePicker.SelectedDate.Value < minWeddingDate)
                        {
                            ApplyErrorStyle(field);
                            field.ToolTip = $"Дата свадьбы должна быть не ранее {minWeddingDate:dd.MM.yyyy}";
                            isValid = false;
                        }
                        else
                        {
                            ClearErrorStyle(field);
                        }
                    }
                }
                else
                {
                    ClearErrorStyle(field);
                }
            }

            // Дополнительные проверки
            if (!int.TryParse(CountGuestsTb.Text, out _))
            {
                ApplyErrorStyle(CountGuestsTb);
                CountGuestsTb.ToolTip = "Введите целое число";
                isValid = false;
            }

            if (!int.TryParse(WeddingBudgetTb.Text, out _))
            {
                ApplyErrorStyle(WeddingBudgetTb);
                WeddingBudgetTb.ToolTip = "Введите целое число";
                isValid = false;
            }

            return isValid;
        }

        private void AddCoupleBtt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                //MessageBox.Show("Заполните все обязательные поля!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                DateTime today = DateTime.Today;
                Couple couple = new Couple();
                Gromm gromm = new Gromm();
                Bride bride = new Bride();
                
                // Groom info
                gromm.Surname = SurnameGroomTb.Text;
                gromm.Name = NameGroomTb.Text;
                gromm.Patronymic = PatronymicGroomTb.Text;
                gromm.PhoneNumber = NumberTelGroomTb.Text;
                gromm.Email = EmailGroomTb.Text;
                gromm.PhoneNumber = NumberTelGroomTb.Text;
                gromm.PassportNumber = PassportNumberGroomTb.Text;
                gromm.PassportSeries = PassportSeriesGroomTb.Text;
                gromm.PassportAddress = PassportAddressGroomTb.Text;
                gromm.Addresss = AddresssGroomTb.Text;
                DbConnection.MarryMe.Gromm.Add(gromm);
                DbConnection.MarryMe.SaveChanges();

                // Bride info
                bride.Surname = SurnameBrideTb.Text;
                bride.Name = NameBrideTb.Text;
                bride.Patronymic = PatronymicBrideTb.Text;
                bride.PhoneNumber = NumberTelBrideTb.Text;
                bride.Email = EmailBrideTb.Text;
                bride.PhoneNumber = NumberTelBrideTb.Text;
                bride.PassportNumber = PassportNumberBrideTb.Text;
                bride.PassportSeries = PassportSeriesBrideTb.Text;
                bride.PassportAddress = PassportAddressBrideTb.Text;
                bride.Addresss = AddresssBrideTb.Text;
                DbConnection.MarryMe.Bride.Add(bride);
                DbConnection.MarryMe.SaveChanges();

                // Couple info
                couple.BrideId = bride.Id;
                couple.GroomId = gromm.Id;
                couple.WeddingStatusId = 1;
                couple.NumberGuests = Convert.ToInt32(CountGuestsTb.Text);
                couple.WeddingBudget = Convert.ToInt32(WeddingBudgetTb.Text);
                couple.WeddingDate = Convert.ToDateTime(DateDp.Text);
                couple.ContractDate = Convert.ToDateTime(today.ToString("dd.MM.yyyy"));
                DbConnection.MarryMe.Couple.Add(couple);
                DbConnection.MarryMe.SaveChanges();

                CoupleFavorites coupleFavorites = new CoupleFavorites();
                coupleFavorites.CoupleId = couple.Id;
                DbConnection.MarryMe.CoupleFavorites.Add(coupleFavorites);
                DbConnection.MarryMe.SaveChanges();
                
                //MessageBox.Show("Пара зарегистрирована!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new HomePage(UserInfo.User));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitBtt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage(UserInfo.User));
        }
    }
}
