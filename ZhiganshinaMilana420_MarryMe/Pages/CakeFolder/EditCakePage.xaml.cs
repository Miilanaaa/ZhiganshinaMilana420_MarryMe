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
    /// Логика взаимодействия для EditCakePage.xaml
    /// </summary>
    public partial class EditCakePage : Page
    {
        public static Cake cake1 = new Cake();
        Cake contextCake;
        public static Cake cak { get; set; }
        public EditCakePage(Cake cake)
        {
            InitializeComponent();
            cak = cake;
            contextCake = cake;
            cake1 = cake;
            this.DataContext = this;
            if (cake1 != null)
            {
                NameTb.Text = contextCake.Name;
                PriceTb.Text = contextCake.Price.ToString();
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

        private void AddBt_Click(object sender, RoutedEventArgs e)
        {
            Cake cake = cak;
            if (NameTb.Text == "" || PriceTb.Text == "" || TestImg.Source == null)
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                cake.Name = NameTb.Text;
                cake.Price = Convert.ToInt32(PriceTb.Text);
                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show("Данные изменены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new CakePage());
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CakePage());
        }
    }
}
