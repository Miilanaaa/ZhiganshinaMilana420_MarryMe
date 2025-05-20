using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhiganshinaMilana420_MarryMe.DB
{
    public partial class Users
    {
        [NotMapped] // Это свойство не будет сохраняться в БД
        public bool IsUsersDismissed
        {
            get { return Dismissed == false; }
        }
    }
}
