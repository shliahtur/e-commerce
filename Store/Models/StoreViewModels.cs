using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Store.Models
{
    public class Product
    {  

        [HiddenInput(DisplayValue = false)]
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "Введите название товара")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Добавьте описание")]
        public string Description { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
        public int Price { get; set; }

        [Required]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }
        public DateTime PostedOn { get; set; }
        public int NetLikeCount { get; set; }


        public IEnumerable<Comment> comment { get; set; }
        public IEnumerable<Reply> reply { get; set; }
        public ICollection<ProductPicture> ProductPictures { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
        public ICollection<ProductLike> ProductLikes { get; set; }
        public Dictionary<string, string> Details { get; set; }
    }

    public partial class ProductProperty
    {
        [Key]
        public string Id { get; set; }
        public string PropertyName { get; set; }
        public bool Checked { get; set; }
        public string UrlSeo { get; set; }
        public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; }
    }

    public class ProductPropertyValue
    {
        [Key]
        public string ProductId { get; set; }
        public string ProductPropertyId { get; set; }
        public string PropertyValueId { get; set; }
        public bool Checked { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public PropertyValue PropertyValue { get; set; }
        public Product Product { get; set; }
        public ICollection<ProductProperty> ProductProperties { get; set; }

    }

    public class PropertyValue
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
        public Product Product { get; set; }
        public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; }
    }





    public class ProductPicture
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public byte[] Image { get; set; }
        public Product Product { get; set; }
    }


    public class Category
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        public bool Checked { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
        public ICollection<CategoryProperty> CategoryProperties { get; set; }
    }


    public class CategoryProperty
    {
        [Key]
        [Column(Order = 0)]
        public string CategoryId { get; set; }
        [Key]
        [Column(Order = 1)]
        public string PropertyId { get; set; }
        public bool Checked { get; set; }
        public Category Category { get; set; } 
        public ProductProperty ProductProperty { get; set; }
    }


    public class ProductCategory
    {
        [Key]
        [Column(Order = 0)]
        public string ProductId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CategoryId { get; set; }

        public bool Checked { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }

    }
    public class Comment
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Body { get; set; }
        [DefaultValue(0)]
        public int NetLikeCount { get; set; }
        [DefaultValue(false)]
        public bool Deleted { get; set; }
        public Product Product { get; set; }
        public ICollection<Reply> Replies { get; set; }

        public ICollection<CommentLike> CommentLikes { get; set; }
    }
    public class Reply
    {
        public string Id { get; set; }
        public string CommentId { get; set; }
        public string ProductId { get; set; }
        public string ParentReplyId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Body { get; set; }
        [DefaultValue(false)]
        public bool Deleted { get; set; }
        public Product Product { get; set; }
        public Comment Comment { get; set; }
        public ICollection<ReplyLike> ReplyLikes { get; set; }
    }

    public class ProductLike
    {
        [Key]
        public string ProductId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }

        public Product Product { get; set; }
    }

    public class CommentLike
    {
        [Key]
        public string CommentId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }
        public Comment Comment { get; set; }
    }
    public class ReplyLike
    {
        [Key]
        public string ReplyId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }
        public Reply Reply { get; set; }

    }
    public class StoreViewModel
    {
        public DateTime PostedOn { get; set; }
        public int TotalProducts { get; set; }
        public List<string> Category { get; set; }
        public Product Product { get; set; }
        public int Price { get; set; }
        public string ProductId { get; set; }
        public string Description { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int ProductDislikes { get; set; }
        public int ProductLikes { get; set; }
        public IList<Category> ProductCategories { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public string UrlSlug { get; set; }
        public CommentViewModel CommentViewModel { get; set; }
        public Dictionary<string, string> DetailPair { get; set; }
        public List<string> PropertyValue { get; set; }
        public PagedList.IPagedList<StoreViewModel> PagedStoreViewModel { get; set; }
    }



    public class ProductViewModel
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public string UrlSeo { get; set; }
        public int ProductDislikes { get; set; }
        public int ProductLikes { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public IList<ProductProperty> ProductProperties { get; set; }
        public IList<ProductPropertyValue> ProductPropertyValues { get; set; }
        public IList<ProductPicture> Pictures { get; set; }
        public IList<Category> ProductCategories { get; set; }
        public IList<ProductProperty> CategoryProperties { get; set; }
        public IList<Category> Categories { get; set; }
        public CommentViewModel CommentViewModel { get; set; }
        public Dictionary<string, string> DetailPairs { get; set; }
    }
    public class CategoryViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UrlSeo { get; set; }
        public IList<Category> Categories { get; set; }
        public IList<ProductProperty> CategoryProperties { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public IList<ProductProperty> ProductProperties { get; set; }

    }
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public string CategoryId { get; set; }
        public string PropertyId { get; set; }
        public string ProductId { get; set; }
        public string UrlSeo { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public IList<Category> ProductCategories { get; set; }
        public IList<ProductProperty> ProductProperties { get; set; }
        public IList<ProductProperty> CategoryProperties { get; set; }

    }
    public class AllProductsViewModel
    {
        public IList<Category> ProductCategories { get; set; }
        public int Price { get; set; }
        public string ProductId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public ProductProperty ProductProperty { get; set; }
        public ProductPropertyValue ProductPropertyValue { get; set; }
        public IList<ProductProperty> CategoryProperties { get; set; }
        public ProductProperty CategoryProperty { get; set; }
        public DateTime Date { get; set; }
        public bool Checked { get; set; }
        public string UrlSlug { get; set; }
        public Dictionary<string, List<string>> Detail { get; set; }


    }
    public class CommentViewModel
    {
        public CommentViewModel() { }
        public CommentViewModel(Comment comment)
        {
            Comment = comment;
        }
        public IList<Comment> Comments { get; set; }
        public string ID { get; set; }
        public Comment Comment { get; set; }
        public string UrlSeo { get; set; }
        public DateTime DateTime { get; set; }
        public IList<CommentViewModel> ChildReplies { get; set; }
        public string Body { get; set; }
        public string Id { get; set; }
        public string ParentReplyId { get; set; }
        public string UserName { get; set; }
    }
}