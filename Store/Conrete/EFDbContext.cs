
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Store.Models
{
    public class EFDbContext : DbContext 
    {
        public EFDbContext() : base("StoreDb")
        {
        }
        public static EFDbContext Create()
        {
            return new EFDbContext();
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPicture> ProductPictures { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<ReplyLike> ReplyLikes { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<CategoryProperty> CategoryProperties { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductLike> ProductLikes { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<PropertyValue> PropertyValues { get; set; }
        public DbSet<ProductPropertyValue> ProductPropertyValues { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            Database.SetInitializer<EFDbContext>(null);
            base.OnModelCreating(modelBuilder);
           
        }
       
    }

}
