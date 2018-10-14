using System.Collections.Generic;
using System.Linq;
using System;
using System.Web;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;

namespace Store.Models
{
    public class StoreRepository : IStoreRepository, IDisposable
    {

        private EFDbContext _context;
        public StoreRepository(EFDbContext context)
        {
            _context = context;
        }

        /* Product  _______________________________________________________________________________________________*/

        public IQueryable<Product> Products
        {
            get { return _context.Products; }
        }

        public IList<Product> GetProducts()
        {
            return _context.Products.ToList();
        }

        public Product GetProductById(string productId)
        {
            return _context.Products.Find(productId);
        }

        public string GetProductIdBySlug(string slug)
        {
            return _context.Products.Where(x => x.UrlSeo == slug).FirstOrDefault().Id;
        }

        public void AddNewProduct(Product product)
        {
            _context.Products.Add(product);
            Save();
        }

        public void DeleteProductandComponents(string productid)
        {
            var productCategories = _context.ProductCategories.Where(p => p.ProductId == productid).ToList();
            var productComments = _context.Comments.Where(p => p.ProductId == productid).ToList();
            List<Reply> productReplies = new List<Reply>();
            foreach (var comment in productComments)
            {
                var replies = _context.Replies.Where(x => x.CommentId == comment.Id).ToList();
                foreach (var reply in replies)
                {
                    productReplies.Add(reply);
                }
            }
            var product = _context.Products.Find(productid);
            foreach (var pc in productCategories) _context.ProductCategories.Remove(pc);
            foreach (var pcom in productComments)
            {
                var commentLikes = _context.CommentLikes.Where(x => x.CommentId == pcom.Id).ToList();
                foreach (var cl in commentLikes) _context.CommentLikes.Remove(cl);
                _context.Comments.Remove(pcom);
            }
            foreach (var pr in productReplies)
            {
                var replyLikes = _context.ReplyLikes.Where(x => x.ReplyId == pr.Id).ToList();
                foreach (var rl in replyLikes) _context.ReplyLikes.Remove(rl);
                _context.Replies.Remove(pr);
            }
            _context.Products.Remove(product);
            Save();
        }




        /* Picture and Likes  _______________________________________________________________________________________*/

        public IList<ProductPicture> GetProductPictures(Product product)
        {
            var pictures = _context.ProductPictures.Where(p => p.ProductId == product.Id).ToList();
            List<ProductPicture> pics = new List<ProductPicture>();
            foreach (var picture in pictures)
            {
                pics.Add(picture);

            }
            return pics;
        }

        public void AddPictureToProduct(string productid, ProductPicture pic, HttpPostedFileBase uploadImage)
        {
            List<int> numlist = new List<int>();
            int num = 1;
            if (uploadImage != null)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                {
                    imageData = binaryReader.ReadBytes(uploadImage.ContentLength);
                }
                pic.Image = imageData;

                var check = _context.ProductPictures.Where(x => x.ProductId == productid && x.Image == imageData).Any();
                if (!check)
                {
                    while (_context.ProductPictures.Where(x => x.Id == num).Any())
                    {
                        num++;
                    }

                    var picture = new ProductPicture { Id = num, ProductId = productid, Image = imageData };
                    _context.ProductPictures.Add(picture);
                    Save();
                }
            }
        }

        public void RemovePictureFromProduct(string productid)
        {
            var picture = _context.ProductPictures.Where(p => p.ProductId == productid).FirstOrDefault();
            _context.ProductPictures.Remove(picture);
            Save();
        }

        public void UpdateProductLike(string productid, string username, string likeordislike)
        {
            var productLike = _context.ProductLikes.Where(x => x.Username == username && x.ProductId == productid).FirstOrDefault();
            if (productLike != null)
            {
                switch (likeordislike)
                {
                    case "like":
                        if (productLike.Like == false) { productLike.Like = true; productLike.Dislike = false; }
                        else productLike.Like = false;
                        break;
                    case "dislike":
                        if (productLike.Dislike == false) { productLike.Dislike = true; productLike.Like = false; }
                        else productLike.Dislike = false;
                        break;
                }
                if (productLike.Like == false && productLike.Dislike == false) _context.ProductLikes.Remove(productLike);
            }
            else
            {
                switch (likeordislike)
                {
                    case "like":
                        productLike = new ProductLike() { ProductId = productid, Username = username, Like = true, Dislike = false };
                        _context.ProductLikes.Add(productLike);
                        break;
                    case "dislike":
                        productLike = new ProductLike() { ProductId = productid, Username = username, Like = false, Dislike = true };
                        _context.ProductLikes.Add(productLike);
                        break;
                }
            }
            var product = _context.Products.Where(x => x.Id == productid).FirstOrDefault();
            product.NetLikeCount = LikeDislikeCount("productlike", productid) - LikeDislikeCount("productdislike", productid);
            Save();
        }

        public int LikeDislikeCount(string typeAndlike, string id)
        {
            switch (typeAndlike)
            {
                case "productlike":
                    return _context.ProductLikes.Where(p => p.ProductId == id && p.Like == true).Count();
                case "productdislike":
                    return _context.ProductLikes.Where(p => p.ProductId == id && p.Dislike == true).Count();
                case "commentlike":
                    return _context.CommentLikes.Where(p => p.CommentId == id && p.Like == true).Count();
                case "commentdislike":
                    return _context.CommentLikes.Where(p => p.CommentId == id && p.Dislike == true).Count();
                case "replylike":
                    return _context.ReplyLikes.Where(p => p.ReplyId == id && p.Like == true).Count();
                case "replydislike":
                    return _context.ReplyLikes.Where(p => p.ReplyId == id && p.Dislike == true).Count();
                default:
                    return 0;
            }
        }



        /* Category  _________________________________________________________________________________________________*/

        public IList<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public Category GetCategoryById(string categoryid)
        {
            return _context.Categories.Find(categoryid);
        }

        public string GetCategoryIdBySlug(string slug)
        {
            return _context.Categories.Where(x => x.UrlSeo == slug).FirstOrDefault().Id;
        }

        public void RemoveCategory(Category category)
        {
            var productCategories = _context.ProductCategories.Where(x => x.CategoryId == category.Id).ToList();
            foreach (var productCat in productCategories)
            {
                _context.ProductCategories.Remove(productCat);
            }
            _context.Categories.Remove(category);
            Save();
        }

        public void AddNewCategory(string catName, string catUrlSeo, string catDesc)
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var categories = _context.Categories.ToList();
            foreach (var cat in categories)
            {
                var catid = cat.Id;
                Int32.TryParse(catid.Replace("cat", ""), out num);
                numlist.Add(num);
            }
            numlist.Sort();
            num = numlist.Last();
            num++;
            var newid = "cat" + num.ToString();
            var category = new Category { Id = newid, Name = catName, Description = catDesc, UrlSeo = catUrlSeo, Checked = false };
            _context.Categories.Add(category);
            Save();
        }

        public void RemoveCategoryFromProduct(string productid, string catName)
        {
            var catid = _context.Categories.Where(x => x.Name == catName).Select(x => x.Id).FirstOrDefault();
            var cat = _context.ProductCategories.Where(x => x.ProductId == productid && x.CategoryId == catid).FirstOrDefault();
            _context.ProductCategories.Remove(cat);
            Save();
        }


        /* ProductCategory   __________________________________________________________________________________________*/

        public IList<Category> GetProductCategories(Product product)
        {
            var categoryIds = _context.ProductCategories.Where(p => p.ProductId == product.Id).Select(p => p.CategoryId).ToList();
            List<Category> categories = new List<Category>();
            foreach (var catId in categoryIds)
            {
                categories.Add(_context.Categories.Where(p => p.Id == catId).FirstOrDefault());
            }
            return categories;
        }

        public void AddProductCategories(ProductCategory productCategory)
        {
            _context.ProductCategories.Add(productCategory);
        }

        public void RemoveProductCategories(string productid, string categoryid)
        {
            ProductCategory productCategory = _context.ProductCategories.Where(x => x.ProductId == productid && x.CategoryId == categoryid).FirstOrDefault();
            _context.ProductCategories.Remove(productCategory);
            Save();
        }





        /* ProductPropertyValue ____________________________________________________________________________________________*/



        public Dictionary<string, List<string>> GetDetails()
        {


            var Result = new Dictionary<string, List<string>>();
            List<Dictionary<string, string>> Details = new List<Dictionary<string, string>>();


            string _connectionString = ConfigurationManager.ConnectionStrings["StoreDb"].ConnectionString;    
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "Select  ProductProperty.PropertyName, PropertyValue.Value from ProductPropertyValue" +
                    " Join ProductProperty on ProductProperty.Id = ProductPropertyValue.ProductPropertyId " +
                    "Join PropertyValue on PropertyValue.Id = ProductPropertyValue.PropertyValueId";
             
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        while (reader.Read()) 
                        {

                               string Property = (string)reader.GetValue(0);
                               string Value = (string)reader.GetValue(1);

                           
                        var temp = new List<Dictionary<string, string>>
                               {
                                new Dictionary<string, string>
                                {
                                    [Property] = Value

                                }
                            };

                         Details.Add(temp.First());

                        }

                        Result = Details.SelectMany(item => item)
                        .GroupBy(item => item.Key)
                        .ToDictionary(key => key.Key, value => value.Select(i => i.Value).ToList());

                        reader.NextResult();
                        reader.Close();
                    }
                }

                return Result;

            }
        }



        public Dictionary<string, string> GetProductDetails(string productid)
        {
            string _connectionString = ConfigurationManager.ConnectionStrings["StoreDb"].ConnectionString;
            Dictionary<string, string> ProductDetailsPair = new Dictionary<string, string>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "Select ProductProperty.PropertyName, PropertyValue.Value from ProductPropertyValue" +
                    " Join ProductProperty on ProductProperty.Id = ProductPropertyValue.ProductPropertyId " +
                    "Join PropertyValue on PropertyValue.Id = ProductPropertyValue.PropertyValueId Where ProductPropertyValue.ProductId = @id";
                cmd.Parameters.AddWithValue("@id", productid);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        while (reader.Read()) // построчно считываем данные
                        {
                            string Property = (string)reader.GetValue(0);
                            string Value = (string)reader.GetValue(1);
                            ProductDetailsPair.Add(Property, Value);
                        }

                        reader.NextResult();
                        reader.Close();
                    }
                }

                return ProductDetailsPair;

            }
        }

        public void AddProductPropertyValues(ProductPropertyValue productPropertyValue)
        {
            _context.ProductPropertyValues.Add(productPropertyValue);
        }

        //public void AddPropertyValueToProduct(PropertyValue ppv, string productid, string propertyId, string value)
        //{
         
        //        var propValue = new ProductPropertyValue { Id = num, PropertyId = propertyId, ProductId = productid, PropertyValue = value };
        //        _context.PropertyValues.Add(propValue);
        //        Save();

            
        //}

        public void RemoveProductPropertyValues(string propertyid, string valueid)
        {
            ProductPropertyValue productPropertyValue = _context.ProductPropertyValues.Where(x => x.ProductPropertyId == propertyid && x.PropertyValueId == valueid).FirstOrDefault();
            _context.ProductPropertyValues.Remove(productPropertyValue);
            Save();
        }

      

        /* ProductProperty  __________________________________________________________________________________________________*/



        public IList<ProductProperty> GetProductProperties()
        {
            return _context.ProductProperties.ToList();
        }

        public void AddNewProperty(string propName, string propUrlSeo)
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var properties = _context.ProductProperties.ToList();
            foreach (var prop in properties)
            {
                var propid = prop.Id;
                Int32.TryParse(propid.Replace("prop", ""), out num);
                numlist.Add(num);
            }
            numlist.Sort();
            num = numlist.Last();
            num++;
            var newid = "prop" + num.ToString();
            var property = new ProductProperty { Id = newid, PropertyName = propName, UrlSeo = propUrlSeo, Checked = false };
            _context.ProductProperties.Add(property);
            Save();
        }



        /* PropertyValue   ___________________________________________________________________________________________________*/


        public IList<PropertyValue> GetPropertyValues()
        {
            return _context.PropertyValues.ToList();
        }
        public IList<PropertyValue> GetPropertyValuesByProductId(string productid)
        {
            List<PropertyValue> propValues = new List<PropertyValue>();

            string _connectionString = ConfigurationManager.ConnectionStrings["StoreDb"].ConnectionString;
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select Value from PropertyValue " +
                    "join ProductPropertyValue on PropertyValue.Id = ProductPropertyValue.PropertyValueId " +
                    "Where ProductPropertyValue.Product_Id  = @id";
                cmd.Parameters.AddWithValue("@id", productid);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        while (reader.Read()) // построчно считываем данные
                        {
                            string Property = (string)reader.GetValue(0);
                            propValues.Add(Property);
                        }

                        reader.NextResult();
                        reader.Close();
                    }
                }

                return propValues;


            }
        }

            public void RemovePropertyValueFromProduct(string productid, string value)
        {
            var valueId = _context.PropertyValues.Where(x => x.Value == value).Select(x => x.Id).FirstOrDefault();
            var propertyValue = _context.PropertyValues.Where(x => x.ProductId == productid && x.Id == valueId).FirstOrDefault();
            _context.PropertyValues.Remove(propertyValue);
            Save();
        }


        /* CategoryProperty    ________________________________________________________________________________________________*/


        public IList<ProductProperty> GetCategoryProperties(Category category)
        {
            var propIds = _context.CategoryProperties.Where(p => p.CategoryId == category.Id).Select(p => p.PropertyId).ToList();
            List<ProductProperty> properties = new List<ProductProperty>();
            foreach (var propId in propIds)
            {
                properties.Add(_context.ProductProperties.Where(p => p.Id == propId).FirstOrDefault());
            }
            return properties;
        }

        public IList<ProductProperty> GetProductPropertiesByCatId(string categoryId)
        {
            var propIds = _context.CategoryProperties.Where(p => p.CategoryId == categoryId).Select(p => p.PropertyId).ToList();
            List<ProductProperty> properties = new List<ProductProperty>();
            foreach (var propId in propIds)
            {
                properties.Add(_context.ProductProperties.Where(p => p.Id == propId).FirstOrDefault());
            }
            return properties;
        }

        public void AddCategoryProperties(CategoryProperty categoryProperty)
        {
            _context.CategoryProperties.Add(categoryProperty);
        }

        public void RemoveCategoryProperties(string categoryid, string propertyid)
        {
            CategoryProperty categoryProperty = _context.CategoryProperties.Where(x => x.CategoryId == categoryid && x.PropertyId == propertyid).FirstOrDefault();
            _context.CategoryProperties.Remove(categoryProperty);
            Save();
        }

        public void RemovePropertyFromCategory(string categoryid, string propName)
        {
            var propid = _context.ProductProperties.Where(x => x.PropertyName == propName).Select(x => x.Id).FirstOrDefault();
            var prop = _context.CategoryProperties.Where(x => x.CategoryId == categoryid && x.PropertyId == propid).FirstOrDefault();
            _context.CategoryProperties.Remove(prop);
            Save();
        }



        #region comments and replies
        /* Comment and Reply    ________________________________________________________________________________________________*/

        public IList<Comment> GetProductComments(Product product)
        {
            return _context.Comments.Where(p => p.ProductId == product.Id).ToList();
        }
        public IList<Comment> GetCommentsByPageId(string productId)
        {
            return _context.Comments.Where(p => p.ProductId == productId).ToList();
        }
        public List<CommentViewModel> GetParentReplies(Comment comment)
        {
            var parentReplies = _context.Replies.Where(p => p.Id == comment.Id && p.ParentReplyId == null).ToList();
            List<CommentViewModel> parReplies = new List<CommentViewModel>();
            foreach (var pr in parentReplies)
            {
                var chReplies = GetChildReplies(pr);
                parReplies.Add(new CommentViewModel() { Body = pr.Body, ParentReplyId = pr.ParentReplyId, DateTime = pr.DateTime, Id = pr.Id, UserName = pr.UserName, ChildReplies = chReplies });
            }
            return parReplies;
        }
        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            List<CommentViewModel> chldReplies = new List<CommentViewModel>();
            if (parentReply != null)
            {
                var childReplies = _context.Replies.Where(p => p.ParentReplyId == parentReply.Id).ToList();
                foreach (var reply in childReplies)
                {
                    var chReplies = GetChildReplies(reply);
                    chldReplies.Add(new CommentViewModel() { Body = reply.Body, ParentReplyId = reply.ParentReplyId, DateTime = reply.DateTime, Id = reply.Id, UserName = reply.UserName, ChildReplies = chReplies });
                }
            }
            return chldReplies;
        }
        public Reply GetReplyById(string id)
        {
            return _context.Replies.Where(p => p.Id == id).FirstOrDefault();
        }
        public string GetUrlSeoByReply(Reply reply)
        {
            var productId = _context.Comments.Where(x => x.Id == reply.CommentId).Select(x => x.ProductId).FirstOrDefault();
            return _context.Products.Where(x => x.Id == productId).Select(x => x.UrlSeo).FirstOrDefault();
        }  
        public bool CommentDeleteCheck(string commentid)
        {
            return _context.Comments.Where(x => x.Id == commentid).Select(x => x.Deleted).FirstOrDefault();
        }
        public bool ReplyDeleteCheck(string replyid)
        {
            return _context.Replies.Where(x => x.Id == replyid).Select(x => x.Deleted).FirstOrDefault();
        }
        public void UpdateCommentLike(string commentid, string username, string likeordislike)
        {
            var commentLike = _context.CommentLikes.Where(x => x.Username == username && x.CommentId == commentid).FirstOrDefault();
            if (commentLike != null)
            {
                switch (likeordislike)
                {
                    case "like":
                        if (commentLike.Like == false) { commentLike.Like = true; commentLike.Dislike = false; }
                        else commentLike.Like = false;
                        break;
                    case "dislike":
                        if (commentLike.Dislike == false) { commentLike.Dislike = true; commentLike.Like = false; }
                        else commentLike.Dislike = false;
                        break;
                }
                if (commentLike.Like == false && commentLike.Dislike == false) _context.CommentLikes.Remove(commentLike);
            }
            else
            {
                switch (likeordislike)
                {
                    case "like":
                        commentLike = new CommentLike() { CommentId = commentid, Username = username, Like = true, Dislike = false };
                        _context.CommentLikes.Add(commentLike);
                        break;
                    case "dislike":
                        commentLike = new CommentLike() { CommentId = commentid, Username = username, Like = false, Dislike = true };
                        _context.CommentLikes.Add(commentLike);
                        break;
                }
            }
            var comment = _context.Comments.Where(x => x.Id == commentid).FirstOrDefault();
            comment.NetLikeCount = LikeDislikeCount("commentlike", commentid) - LikeDislikeCount("commentdislike", commentid);
            Save();
        }
        public void UpdateReplyLike(string replyid, string username, string likeordislike)
        {
            var replyLike = _context.ReplyLikes.Where(x => x.Username == username && x.ReplyId == replyid).FirstOrDefault();
            if (replyLike != null)
            {
                switch (likeordislike)
                {
                    case "like":
                        if (replyLike.Like == false) { replyLike.Like = true; replyLike.Dislike = false; }
                        else replyLike.Like = false;
                        break;
                    case "dislike":
                        if (replyLike.Dislike == false) { replyLike.Dislike = true; replyLike.Like = false; }
                        else replyLike.Dislike = false;
                        break;
                }
                if (replyLike.Like == false && replyLike.Dislike == false) _context.ReplyLikes.Remove(replyLike);
            }
            else
            {
                switch (likeordislike)
                {
                    case "like":
                        replyLike = new ReplyLike() { ReplyId = replyid, Username = username, Like = true, Dislike = false };
                        _context.ReplyLikes.Add(replyLike);
                        break;
                    case "dislike":
                        replyLike = new ReplyLike() { ReplyId = replyid, Username = username, Like = false, Dislike = true };
                        _context.ReplyLikes.Add(replyLike);
                        break;
                }
            }
            Save();
        }
        public Product GetProductByReply(string replyid)
        {
            var commentId = _context.Replies.Where(x => x.Id == replyid).Select(x => x.CommentId).FirstOrDefault();
            var productId = _context.Comments.Where(x => x.Id == commentId).Select(x => x.Id).FirstOrDefault();
            return _context.Products.Where(x => x.Id == productId).FirstOrDefault();
        }
        public string GetPageIdByComment(string commentId)
        {
            return _context.Comments.Where(x => x.Id == commentId).Select(x => x.ProductId).FirstOrDefault();
        }
        public IList<Comment> GetComments()
        {
            return _context.Comments.ToList();
        }
        public IList<Reply> GetReplies()
        {
            return _context.Replies.ToList();
        }
        public void AddNewComment(Comment comment)
        {
            _context.Comments.Add(comment);
            Save();
        }
        public void AddNewReply(Reply reply)
        {
            _context.Replies.Add(reply);
            Save();
        }
        public Comment GetCommentById(string id)
        {
            return _context.Comments.Where(p => p.Id == id).FirstOrDefault();
        }
        public void DeleteComment(string commentid)
        {
            var comment = _context.Comments.Where(x => x.Id == commentid).FirstOrDefault();
            _context.Comments.Remove(comment);
            Save();
        }
        public void DeleteReply(string replyid)
        {
            var reply = _context.Replies.Where(x => x.Id == replyid).FirstOrDefault();
            _context.Replies.Remove(reply);
            Save();
        }
        #endregion



        public void Save()
        {
            try
            {

                _context.SaveChanges();

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }
        }


        #region dispose
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

     

       




        #endregion dispose
    }
}
