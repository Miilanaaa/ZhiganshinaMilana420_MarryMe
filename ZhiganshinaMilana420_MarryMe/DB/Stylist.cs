//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZhiganshinaMilana420_MarryMe.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class Stylist
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stylist()
        {
            this.CoupleFavorites = new HashSet<CoupleFavorites>();
            this.StylistBookingDates = new HashSet<StylistBookingDates>();
            this.StylistPhoto = new HashSet<StylistPhoto>();
        }
    
        public int Id { get; set; }
        public string TeamName { get; set; }
        public Nullable<int> Price { get; set; }
        public string Description { get; set; }
        public Nullable<int> StylistTypeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CoupleFavorites> CoupleFavorites { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StylistBookingDates> StylistBookingDates { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StylistPhoto> StylistPhoto { get; set; }
        public virtual StylistType StylistType { get; set; }
    }
}
