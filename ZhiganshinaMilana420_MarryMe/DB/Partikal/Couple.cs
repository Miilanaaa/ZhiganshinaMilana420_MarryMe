using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhiganshinaMilana420_MarryMe.DB
{
    public partial class Couple
    {
        [NotMapped] // Это свойство не будет сохраняться в БД
        public bool IsActive
        {
            get { return WeddingStatusId == 1; }
        }
        public string BrideImage => WeddingStatusId == 1 ? "/Images/bride.png" : "/Images/bride_1.png";
        public string HeartImage => WeddingStatusId == 1 ? "/Images/heart.png" : "/Images/heart_1.png";
        public string GroomImage => WeddingStatusId == 1 ? "/Images/groom.png" : "/Images/bride_2.png";
    }
}
