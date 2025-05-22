using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ZhiganshinaMilana420_MarryMe.Windows;
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для TaskUserAddPage.xaml
    /// </summary>
    public partial class TaskUserAddPage : Page
    {
        public static List<Users> users {  get; set; }
        public TaskUserAddPage()
        {
            InitializeComponent();
            users = new List<Users>(DbConnection.MarryMe.Users.Where(i => i.Id != UserInfo.User.Id)).ToList();
            UsersLV.ItemsSource = users;
            ShowFreeCheckBox.IsChecked = false;
            ShowTakenCheckBox.IsChecked = true;
            this.DataContext = this;
        }

        private void Refresh()
        {
            List<Users> aviaries = DbConnection.MarryMe.Users.Where(i => i.Id != UserInfo.User.Id).ToList();
            if (SearchTb.Text.Length > 0)
            {
                aviaries = aviaries.Where(u => u.Surname.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())
                                                     || u.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())).ToList();
            }
            if ((bool)!ShowFreeCheckBox.IsChecked)
            {
                aviaries = aviaries.Where(x => (bool)!x.Dismissed).ToList();
            }
            if ((bool)!ShowTakenCheckBox.IsChecked)
            {
                aviaries = aviaries.Where(x => (bool)x.Dismissed).ToList();
            }
            UsersLV.ItemsSource = aviaries;
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void TaskUserBt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Users users)
            {
                NavigationService.Navigate(new TaskPage(users));
            }
        }
        
        private void AddEmployyeBt_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.Closed += (s, args) =>
            {
                // Обновляем данные после закрытия окна
                users = new List<Users>(DbConnection.MarryMe.Users.Where(i => i.Id != UserInfo.User.Id && i.Dismissed == false)).ToList();
                UsersLV.ItemsSource = users;
            };
            addEmployeeWindow.ShowDialog();
        }

        private void Delate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Users employee)
            {
                // Создаем окно подтверждения с указанием причины увольнения
                var dismissalWindow = new DismissalWindow(employee);
                if (dismissalWindow.ShowDialog() == true)
                {
                    try
                    {
                        // Генерируем приказ об увольнении
                        GenerateDismissalOrder(employee, dismissalWindow.DismissalReason, dismissalWindow.DismissalArticle);

                        employee.Dismissed = true;
                        employee.Login = null;
                        employee.Password = null;
                        DbConnection.MarryMe.SaveChanges();

                        // Обновляем список сотрудников
                        users = new List<Users>(DbConnection.MarryMe.Users.Where(i => i.Id != UserInfo.User.Id && i.Dismissed == false)).ToList();
                        UsersLV.ItemsSource = users;

                        MessageBox.Show("Сотрудник уволен. Приказ об увольнении создан.",
                                      "Успешно",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при увольнении: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
        }

        private void GenerateDismissalOrder(Users employee, string reason, string article)
        {
            try
            {
                // Путь к папкам
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string baseFolderPath = Path.Combine(desktopPath, "Приказы MarryMe");
                string ordersFolderPath = Path.Combine(baseFolderPath, "Приказы об увольнении сотрудника");

                // Создаем папки, если их нет
                Directory.CreateDirectory(baseFolderPath);
                Directory.CreateDirectory(ordersFolderPath);

                // Открываем шаблон документа
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                string templatePath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "Приказы об увольнении сотрудника.docx");

                var document = wordApp.Documents.Open(templatePath);
                wordApp.Visible = true;

                // Заполняем поля в документе
                FillField(document, "номер_приказа", $"DISM-{employee.Id}-{DateTime.Now:yyyyMMdd}");

                FillField(document, "день", DateTime.Now.Day.ToString());
                FillField(document, "месяц", GetMonthNameGenitive(DateTime.Now.Month));
                FillField(document, "год", DateTime.Now.Year.ToString());
                
                if (employee.DeviceDate.HasValue)
                {
                    FillField(document, "дата_работы", employee.DeviceDate.Value.Day.ToString());
                    FillField(document, "месяц_работы", GetMonthNameGenitive(employee.DeviceDate.Value.Month));
                    FillField(document, "год_работы", employee.DeviceDate.Value.Year.ToString());
                }
                else
                {
                    // Использовать текущую дату, если DeviceDate не задан
                    FillField(document, "дата_работы", DateTime.Now.Day.ToString());
                    FillField(document, "месяц_работы", GetMonthNameGenitive(DateTime.Now.Month));
                    FillField(document, "год_работы", DateTime.Now.Year.ToString());
                }
                FillField(document, "день", DateTime.Now.Day.ToString());
                FillField(document, "месяц", GetMonthNameGenitive(DateTime.Now.Month));
                FillField(document, "год", DateTime.Now.Year.ToString());

                FillField(document, "фио_сотрудника", $"{employee.Surname} {employee.Name} {employee.Patronymic}");
                FillField(document, "id_сотрудника", employee.Id.ToString());
                FillField(document, "должность сотрудника", employee.Role?.Name ?? "");
                FillField(document, "основания_увольнения", $"{article} - {reason}");
                FillField(document, "фио_сотрудника", $"{employee.Surname} {employee.Name[0]}.{employee.Patronymic[0]}.");

                // Сохраняем документ
                string fileName = $"Приказ об увольнении {employee.Surname} {employee.Name[0]}.{employee.Patronymic[0]}. от {DateTime.Now:dd.MM.yyyy}.docx";
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
                throw new Exception($"Ошибка при создании приказа об увольнении: {ex.Message}");
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

        private void ShowFreeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void ShowFreeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        private void ShowTakenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void ShowTakenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
