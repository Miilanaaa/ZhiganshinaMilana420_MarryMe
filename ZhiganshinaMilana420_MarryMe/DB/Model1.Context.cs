﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MarryMeEntities : DbContext
    {
        public MarryMeEntities()
            : base("name=MarryMeEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Accessory> Accessory { get; set; }
        public virtual DbSet<AccessoryPhoto> AccessoryPhoto { get; set; }
        public virtual DbSet<AccessoryType> AccessoryType { get; set; }
        public virtual DbSet<Bouquet> Bouquet { get; set; }
        public virtual DbSet<BouquetPhoto> BouquetPhoto { get; set; }
        public virtual DbSet<BouquetType> BouquetType { get; set; }
        public virtual DbSet<Bride> Bride { get; set; }
        public virtual DbSet<Cake> Cake { get; set; }
        public virtual DbSet<CakePhoto> CakePhoto { get; set; }
        public virtual DbSet<CakeType> CakeType { get; set; }
        public virtual DbSet<Clothing> Clothing { get; set; }
        public virtual DbSet<ClothingPhoto> ClothingPhoto { get; set; }
        public virtual DbSet<ClothingType> ClothingType { get; set; }
        public virtual DbSet<Couple> Couple { get; set; }
        public virtual DbSet<CoupleFavorites> CoupleFavorites { get; set; }
        public virtual DbSet<Decoration> Decoration { get; set; }
        public virtual DbSet<DecorationPhoto> DecorationPhoto { get; set; }
        public virtual DbSet<Dress> Dress { get; set; }
        public virtual DbSet<DressTypy> DressTypy { get; set; }
        public virtual DbSet<Gender> Gender { get; set; }
        public virtual DbSet<Gromm> Gromm { get; set; }
        public virtual DbSet<Host> Host { get; set; }
        public virtual DbSet<HostBookingDates> HostBookingDates { get; set; }
        public virtual DbSet<HostPhoto> HostPhoto { get; set; }
        public virtual DbSet<Musician> Musician { get; set; }
        public virtual DbSet<MusicianBookingDates> MusicianBookingDates { get; set; }
        public virtual DbSet<MusicianPhoto> MusicianPhoto { get; set; }
        public virtual DbSet<MusicianType> MusicianType { get; set; }
        public virtual DbSet<PhotoDress> PhotoDress { get; set; }
        public virtual DbSet<PhotographerType> PhotographerType { get; set; }
        public virtual DbSet<PhotographerVideographer> PhotographerVideographer { get; set; }
        public virtual DbSet<PhotographerVideographerBookingDates> PhotographerVideographerBookingDates { get; set; }
        public virtual DbSet<PhotographerVideographerPhoto> PhotographerVideographerPhoto { get; set; }
        public virtual DbSet<Restaurant> Restaurant { get; set; }
        public virtual DbSet<RestaurantBookingDates> RestaurantBookingDates { get; set; }
        public virtual DbSet<RestaurantPhoto> RestaurantPhoto { get; set; }
        public virtual DbSet<RestaurantType> RestaurantType { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Stylist> Stylist { get; set; }
        public virtual DbSet<StylistBookingDates> StylistBookingDates { get; set; }
        public virtual DbSet<StylistPhoto> StylistPhoto { get; set; }
        public virtual DbSet<StylistType> StylistType { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<TaskUsers> TaskUsers { get; set; }
        public virtual DbSet<Transfer> Transfer { get; set; }
        public virtual DbSet<TransferBookingDates> TransferBookingDates { get; set; }
        public virtual DbSet<TransferPhoto> TransferPhoto { get; set; }
        public virtual DbSet<TransferType> TransferType { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<WeddingStatus> WeddingStatus { get; set; }
    }
}
