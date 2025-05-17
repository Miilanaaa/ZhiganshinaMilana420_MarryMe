using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.CakeFolder
{
    /// <summary>
    /// Логика взаимодействия для AddCakePage.xaml
    /// </summary>
    public partial class AddCakePage : Page
    {
        public static Cake cak = new Cake();
        public AddCakePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void AddBt_Click(object sender, RoutedEventArgs e)
        {
            if (NameTb.Text == "" || PriceTb.Text == "" || TestImg.Source == null)
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                cak.Name = NameTb.Text;
                cak.Price = Convert.ToInt32(PriceTb.Text);

                DbConnection.MarryMe.Cake.Add(cak);
                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show("Товар успешно добавлен!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new CakePage());
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
                cak.Photo = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            };
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CakePage());
        }
    }
}
