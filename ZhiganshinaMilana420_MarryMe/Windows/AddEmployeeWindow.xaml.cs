using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        private bool isPasswordVisible = false;
        private TextBox visiblePasswordTextBox;
        public static List<Role> roles { get; set; }
        public event Action EmployeeAdded;
        public AddEmployeeWindow()
        {
            InitializeComponent();
            roles = new List<Role>(DbConnection.MarryMe.Role.ToList());
            
            this.DataContext = this;
            InitializePasswordVisibilityControls();
        }
        private bool IsAdult(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            // Проверяем, был ли уже день рождения в этом году
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age >= 18;
        }

        private void InitializePasswordVisibilityControls()
        {
            visiblePasswordTextBox = new TextBox
            {
                Width = 200,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var stackPanel = PasswordTb.Parent as StackPanel;
            stackPanel?.Children.Insert(0, visiblePasswordTextBox);
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            // Список обязательных полей
            var requiredFields = new List<Control>
            {
                SurnameTb, NameTb, PatronymicTb,
                EmailTb, SalaryTb, BirthDateDp,
                LoginTb, RoleCb
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
            if (BirthDateDp.SelectedDate != null)
            {
                if (!IsAdult(BirthDateDp.SelectedDate.Value))
                {
                    ApplyErrorStyle(BirthDateDp);
                    BirthDateDp.ToolTip = "Сотрудник должен быть совершеннолетним (18+ лет)";
                    isValid = false;
                }
                else
                {
                    ClearErrorStyle(BirthDateDp);
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

        private void AddEmployyeBt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var userinfo = DbConnection.MarryMe.Users.Where(i => i.Login == LoginTb.Text).FirstOrDefault();
                if (userinfo == null)
                {
                    Users newEmployee = new Users();
                    newEmployee.Surname = SurnameTb.Text;
                    newEmployee.Name = NameTb.Text;
                    newEmployee.Patronymic = PatronymicTb.Text;
                    newEmployee.RoleId = (RoleCb.SelectedItem as Role)?.Id ?? 0;
                    newEmployee.Email = EmailTb.Text;
                    newEmployee.IdGender = GenderMen.IsChecked.GetValueOrDefault() ? 1 : 2;
                    newEmployee.Salary = int.Parse(SalaryTb.Text);
                    newEmployee.BirthDate = BirthDateDp.SelectedDate.Value;
                    newEmployee.Dismissed = false;
                    newEmployee.Login = LoginTb.Text;
                    newEmployee.Password = isPasswordVisible ? visiblePasswordTextBox.Text : PasswordTb.Password;

                    DbConnection.MarryMe.Users.Add(newEmployee);
                    DbConnection.MarryMe.SaveChanges();

                    GenerateEmploymentOrder(newEmployee);

                    //MessageBox.Show("Сотрудник успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    EmployeeAdded?.Invoke();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Пользователь под таким логином существует, введите другой логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GenerateEmploymentOrder(Users employee)
        {
            try
            {
                // Путь к базовой папке на рабочем столе
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string baseFolderPath = Path.Combine(desktopPath, "Приказы MarryMe");
                string ordersFolderPath = Path.Combine(baseFolderPath, "Приказы о приеме на работу");

                // Создаем папки, если их нет
                if (!Directory.Exists(baseFolderPath))
                {
                    Directory.CreateDirectory(baseFolderPath);
                }
                if (!Directory.Exists(ordersFolderPath))
                {
                    Directory.CreateDirectory(ordersFolderPath);
                }

                // Открываем шаблон документа
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                string templatePath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "Приказ о приеме на работу.docx");

                var document = wordApp.Documents.Open(templatePath);
                wordApp.Visible = true;

                // Заполняем поля в документе
                FillField(document, "номер_договора", $"EMP-{employee.Id}-{DateTime.Now:yyyyMMdd}");
                FillField(document, "день", DateTime.Now.Day.ToString());
                FillField(document, "месяц", GetMonthNameGenitive(DateTime.Now.Month));
                FillField(document, "год", DateTime.Now.Year.ToString());
                FillField(document, "фио_сотрудника_полностью", $"{employee.Surname} {employee.Name} {employee.Patronymic}");
                FillField(document, "должность_сотрудника", (RoleCb.SelectedItem as Role)?.Name ?? "");
                FillField(document, "оклад_сотрудника", employee.Salary.ToString());
                FillField(document, "фио_работника", $"{employee.Surname} {employee.Name[0]}.{employee.Patronymic[0]}.");

                // Сохраняем документ
                string fileName = $"Приказ о приеме {employee.Surname} {employee.Name[0]}.{employee.Patronymic[0]}. от {DateTime.Now:dd.MM.yyyy}.docx";
                string savePath = Path.Combine(ordersFolderPath, fileName);

                document.SaveAs2(savePath);

                // Открываем папку с документом
                Process.Start(new ProcessStartInfo()
                {
                    FileName = ordersFolderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании приказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetMonthNameGenitive(int month)
        {
            string[] months = {
                "января", "февраля", "марта", "апреля", "мая", "июня",
                "июля", "августа", "сентября", "октября", "ноября", "декабря"
            };
            return months[month - 1];
        }

        private void FillField(Microsoft.Office.Interop.Word.Document document, string fieldName, string value)
        {
            try
            {
                var range = document.Content;
                range.Find.ClearFormatting();
                if (range.Find.Execute(FindText: fieldName))
                {
                    range.Text = value;
                }
            }
            catch { }
        }

        private void ClouseBtt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
