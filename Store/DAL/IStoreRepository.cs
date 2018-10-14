using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Store.Models;

namespace Store
{
    public interface IStoreRepository : IDisposable
    {

        /* Product */
        IQueryable<Product> Products { get; }
        IList<Product> GetProducts();
        Product GetProductById(string productid);
        string GetProductIdBySlug(string slug);
        void AddNewProduct(Product product);   
        void DeleteProductandComponents(string productid);

       

        /* Category */
        IList<Category> GetCategories();
        Category GetCategoryById(string categoryid);
        string GetCategoryIdBySlug(string slug);
        void AddNewCategory(string catName, string catUrlSeo, string catDesc);
        void RemoveCategory(Category category);
        void RemoveCategoryFromProduct(string productid, string catName);


        /* ProductCategory */
        IList<Category> GetProductCategories(Product product);
        void AddProductCategories(ProductCategory productCategory);
        void RemoveProductCategories(string productid, string categoryid);




        /* ProductPropertyValue */
        Dictionary<string, List<string>> GetDetails();
        Dictionary<string, string> GetProductDetails(string productid);
        void AddProductPropertyValues(ProductPropertyValue productPropertyValue);
        //void AddPropertyValueToProduct(PropertyValue ppv, string productid, string propertyId, string value);
        void RemoveProductPropertyValues(string propertyid, string valueid);



        /* ProductProperty */
        IList<ProductProperty> GetProductProperties();
        void AddNewProperty(string propName, string propUrlSeo);



        /* PropertyValue */
        IList<PropertyValue> GetPropertyValues();
        IList<PropertyValue> GetPropertyValuesByProductId(string productid);
        void RemovePropertyValueFromProduct(string productid, string value);



        /* CategoryProperty */
        IList<ProductProperty> GetCategoryProperties(Category category);  
        IList<ProductProperty> GetProductPropertiesByCatId(string categoryId);
        void AddCategoryProperties(CategoryProperty categoryProperty);
        void RemoveCategoryProperties(string categoryid, string propertyid);
        void RemovePropertyFromCategory(string categoryid, string propName);



        /* Picture and likes */
        IList<ProductPicture> GetProductPictures(Product product);
        void AddPictureToProduct(string productid, ProductPicture pic, HttpPostedFileBase uploadImage);
        void RemovePictureFromProduct(string productid);
        void UpdateProductLike(string productid, string username, string likeordislike);
        int LikeDislikeCount(string typeAndlike, string id);



        /* Comment and reply */
        IList<Comment> GetProductComments(Product product);
        List<CommentViewModel> GetParentReplies(Comment comment);
        List<CommentViewModel> GetChildReplies(Reply parentReply);
        Reply GetReplyById(string id);
        bool CommentDeleteCheck(string commentid);
        bool ReplyDeleteCheck(string replyid);
        string GetPageIdByComment(string commentId);
        void UpdateCommentLike(string commentid, string username, string likeordislike);
        void UpdateReplyLike(string replyid, string username, string likeordislike);
        Product GetProductByReply(string replyid);
        IList<Comment> GetCommentsByPageId(string productId);
        IList<Comment> GetComments();
        IList<Reply> GetReplies();
        void AddNewComment(Comment comment);
        void AddNewReply(Reply reply);
        Comment GetCommentById(string id);
        void DeleteComment(string commentid);
        void DeleteReply(string replyid);
        string GetUrlSeoByReply(Reply reply);
        void Save();
    }
}