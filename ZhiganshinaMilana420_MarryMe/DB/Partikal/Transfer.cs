using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZhiganshinaMilana420_MarryMe.DB
{
    public partial class Transfer
    {
        [NotMapped]
        public byte[] DisplayPhoto
        {
            get
            {
                // Возвращаем первое фото или null, если фото нет
                return TransferPhoto.FirstOrDefault()?.Photo;
            }
        }

        [NotMapped]
        public BitmapImage DisplayPhotoImage
        {
            get
            {
                if (DisplayPhoto == null) return null;

                var image = new BitmapImage();
                using (var mem = new MemoryStream(DisplayPhoto))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
        }
    }
}
