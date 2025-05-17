using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZhiganshinaMilana420_MarryMe.DB
{
    public partial class RestaurantBookingDates
    {
        public SolidColorBrush BackgroundColorProduct
        {
            get
            {
                bool isBooked = DbConnection.MarryMe.RestaurantBookingDates
                .Any(b => b.RestaurantId == this.RestaurantId &&
                         b.BookingDate == this.BookingDate &&
                         b.Status == true &&
                         b.Id != this.Id); // Исключаем текущую запись из проверки

                return isBooked ?
                    new SolidColorBrush(Colors.Red) :
                    new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
