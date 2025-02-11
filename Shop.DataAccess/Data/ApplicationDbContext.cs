
using Shop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Shop.DataAccess.Data
{
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
                
        }

        public DbSet<Category> Categories {  get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Notify> Notify { get; set; }
        public DbSet<CreditCard> CreditCard { get; set; }

        public DbSet<ApplicationUser>ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { id = 1, Name = "Action", DisplayOrder = 1},
                new Category { id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { id = 3, Name = "Fantasy", DisplayOrder = 3 }
                );
            modelBuilder.Entity<Product>().HasData(
              new Product { 
                  id = 1, 
                  GameName = "Kingdom Hearts", 
                  Description = "KINGDOM HEARTS follows the main protagonist Sora, a Keyblade wielder, as he travels to many Disney worlds with Donald and Goofy to stop the Heartless invasion by sealing each world’s keyhole and restore peace to the realms."
              ,   Price=20,
                  ReleaseYear=2003, 
                  CategoryId=1,
                  ImageUrl = "",
                  Platform="PS4",
                  Stock=20,
                  PEGI=12,
                  SoldSoFar=0,
                  BestSeller=false,
                  SKU="#04001001"
              },
              new Product
              {
                  id = 2,
                  GameName = "Batman: Arkham Knight",
                  Description = "While Scarecrow returns for revenge against Batman and plans to unleash his fear toxin all over Gotham City, a new antagonist, the mysterious Arkham Knight, confronts Batman and challenges his supremacy over the streets of the city."
              ,
                  Price = 15,
                  ReleaseYear = 2015,
                  CategoryId=1,
                  ImageUrl="",
                  Platform = "PS4",
                  Stock = 20,
                  PEGI = 16,
                  SoldSoFar = 0,
                  BestSeller = false,
                  SKU = "#04001002"

              }
              );
        }
    }
}
