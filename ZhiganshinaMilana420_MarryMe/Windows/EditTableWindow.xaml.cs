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
using System.Windows.Shapes;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditTableWindow.xaml
    /// </summary>
    public partial class EditTableWindow : Window
    {
        public string ShapeName { get; private set; }
        public double ShapeWidth { get; private set; }
        public double ShapeHeight { get; private set; }
        public int SeatsCount { get; private set; }

        public EditTableWindow(string currentName, double currentWidth, double currentHeight, int currentSeats = 8)
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
            WidthTextBox.Text = currentWidth.ToString();
            HeightTextBox.Text = currentHeight.ToString();
            SeatsTextBox.Text = currentSeats.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) &&
                double.TryParse(HeightTextBox.Text, out double height) &&
                int.TryParse(SeatsTextBox.Text, out int seats))
            {
                ShapeName = NameTextBox.Text;
                ShapeWidth = width;
                ShapeHeight = height;
                SeatsCount = seats;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
