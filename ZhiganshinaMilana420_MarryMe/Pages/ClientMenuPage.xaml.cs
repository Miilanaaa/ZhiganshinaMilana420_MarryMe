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
    /// Логика взаимодействия для ClientMenuPage.xaml
    /// </summary>
    public partial class ClientMenuPage : Page
    {
        public static List<Couple> couples {  get; set; }
        public static List<Gromm> gromms { get; set; }
        public static List<Bride> brides { get; set; }
        public ClientMenuPage()
        {
            InitializeComponent();
            CoupleLV.ItemsSource = new List<Couple>(DbConnection.MarryMe.Couple.ToList());
            gromms = new List<Gromm>(DbConnection.MarryMe.Gromm.ToList());
            brides = new List<Bride>(DbConnection.MarryMe.Bride.ToList());
            this.DataContext = this;
        }

        private void CoupleLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoupleLV.SelectedItem is Couple couple)
            {
                couple = CoupleLV.SelectedItem as Couple;
                NavigationService.Navigate(new EditClientPage(couple));
            }
        }
    }
}
