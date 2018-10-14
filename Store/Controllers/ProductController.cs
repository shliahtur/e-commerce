using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.Models;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PagedList;
using System.IO;

namespace Store.Controllers
{
    public class ProductController : Controller
    {

        private IStoreRepository _storeRepository;    
        public static List<StoreViewModel> productList = new List<StoreViewModel>();
        public static List<AllProductsViewModel> allProductsList = new List<AllProductsViewModel>();
        public static List<AllProductsViewModel> checkCatList = new List<AllProductsViewModel>();
        public static List<AllProductsViewModel> checkDetailList = new List<AllProductsViewModel>();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public int PageSize = 4;

        public ProductController()
        {
            _storeRepository = new StoreRepository(new EFDbContext());
        }
        public ProductController(IStoreRepository storeRepository, ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _storeRepository = storeRepository;
            UserManager = userManager;
            SignInManager = signInManager;

        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;    
            }
        }
        #region Index

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(int? page, string sortOrder, string searchString, string[] searchCategory, int? minPrice, int? maxPrice, string propertyValue)
        {
            checkCatList.Clear();
            checkDetailList.Clear();
            CreateCategoryList();
            CreateDetailList();


            StoreViewModel model = new StoreViewModel();
            model.PagedStoreViewModel = CreatePagedStoreViewModel(page, sortOrder, searchString, searchCategory, minPrice, maxPrice, propertyValue);
            return View(model);
        }
    
        #endregion Index

        #region Products/AllProducts
        [HttpGet]
        [AllowAnonymous]

        public ActionResult Products(int? page, string sortOrder, string searchString, string[] searchCategory, int? minPrice, int? maxPrice, string propertyValue)
        {
            checkCatList.Clear();
            checkDetailList.Clear();
            CreateCategoryList();
            CreateDetailList();
           


            StoreViewModel model = new StoreViewModel();
            model.PagedStoreViewModel = CreatePagedStoreViewModel(page, sortOrder, searchString, searchCategory, minPrice, maxPrice, propertyValue);
            return PartialView(model);
        }


        public IPagedList<StoreViewModel> CreatePagedStoreViewModel(int? page, string sortOrder, string searchString, string[] searchCategory, int? minPrice, int? maxPrice, string propertyValue)
        {
            productList.Clear();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchCategory = searchCategory;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.PropertyValue = propertyValue;


            var products = _storeRepository.GetProducts();
            foreach (var product in products)
            {
                var productCategories = GetProductCategories(product);
                var propertyValues = _storeRepository.GetPropertyValues();
                var likes = _storeRepository.LikeDislikeCount("productlike", product.Id);
                var dislikes = _storeRepository.LikeDislikeCount("productdislike", product.Id);

                productList.Add(new StoreViewModel() {
                    Product = product,
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    ProductCategories = productCategories,
                    ProductLikes = likes,
                    ProductDislikes = dislikes,
                    PostedOn = product.PostedOn,
                    UrlSlug = product.UrlSeo,
                    PropertyValue = product.PropertyValue
                    });
            }
            if (minPrice != null && maxPrice != null)
            {
                productList = productList.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();
            }

            if (searchString != null)
            {
                productList = productList.Where(x => x.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }

            if (searchCategory != null)
            {
                List<StoreViewModel> newlist = new List<StoreViewModel>();
                foreach (var catName in searchCategory)
                {
                    foreach (var item in productList)
                    {
                        if (item.ProductCategories.Where(x => x.Name == catName).Any())
                        {
                            newlist.Add(item);
                        }
                    }
                    foreach (var item in checkCatList)
                    {
                        if (item.Category.Name == catName)
                        {
                            item.Checked = true;
                        }
                    }
                }
                productList = productList.Intersect(newlist).ToList();
            }
            if(detailPair != null)
            {
                List<StoreViewModel> newlist = new List<StoreViewModel>();
                foreach (var detail in detailPair)
                {
                    foreach (var item in productList)
                    {
                        if (item.DetailPair.Where(x => x.Equals(detail)).Any())
                        {
                            newlist.Add(item);
                        }
                    }
                   
                }
                productList = productList.Intersect(newlist).ToList();
            }

            switch (sortOrder)
            {
                case "date_desc":
                    productList = productList.OrderByDescending(x => x.PostedOn).ToList();
                    break;
                case "Name":
                    productList = productList.OrderBy(x => x.Name).ToList();
                    break;
                default:
                    productList = productList.OrderBy(x => x.PostedOn).ToList();
                    break;
            }

            int pageSize = 6;
            int pageNumber = (page ?? 1);
            return productList.ToPagedList(pageNumber, pageSize);
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AllProducts(int? page, string sortOrder, string searchString, string[] searchCategory, int? minPrice, int? maxPrice)
        {
            allProductsList.Clear();
            checkCatList.Clear();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchCategory = searchCategory;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            var products = _storeRepository.GetProducts();
            foreach (var product in products)
            {
                var productCategories = GetProductCategories(product);
                allProductsList.Add(new AllProductsViewModel() { ProductId = product.Id, Date = product.PostedOn, Name = product.Name, Description = product.Description, Price = product.Price, ProductCategories = productCategories, UrlSlug = product.UrlSeo });
            }

            if (searchString != null)
            {
                allProductsList = allProductsList.Where(x => x.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }

            CreateCategoryList();

            if (minPrice != null && maxPrice != null)
            {
                productList = productList.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();
            }


            if (searchCategory != null)
            {
                List<AllProductsViewModel> newlist = new List<AllProductsViewModel>();
                foreach (var catName in searchCategory)
                {
                    foreach (var item in allProductsList)
                    {
                        if (item.ProductCategories.Where(x => x.Name == catName).Any())
                        {
                            newlist.Add(item);
                        }
                    }
                    foreach (var item in checkCatList)
                    {
                        if (item.Category.Name == catName)
                        {
                            item.Checked = true;
                        }
                    }
                }
                allProductsList = allProductsList.Intersect(newlist).ToList();
            }

           
            

            switch (sortOrder)
            {
                case "date_desc":
                    allProductsList = allProductsList.OrderByDescending(x => x.Date).ToList();
                    break;
                case "Name":
                    allProductsList = allProductsList.OrderBy(x => x.Name).ToList();
                    break;
                case "name_desc":
                    allProductsList = allProductsList.OrderByDescending(x => x.Name).ToList();
                    break;
                default:
                    allProductsList = allProductsList.OrderBy(x => x.Date).ToList();
                    break;

            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View("AllProducts", allProductsList.ToPagedList(pageNumber, pageSize));

        }
        #endregion

        #region Product

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Product(string sortOrder, string slug)
        {
            ProductViewModel model = new ProductViewModel();
            var products = GetProducts();
            var productid = _storeRepository.GetProductIdBySlug(slug);
            var product = _storeRepository.GetProductById(productid);
            var pictures =  GetProductPictures(product);
            model.ID = product.Id;
            model.Pictures = pictures;
            model.ProductCount = products.Count();
            model.UrlSeo = product.UrlSeo;
            model.Name = product.Name;
            model.Description = product.Description;
            model.Price = product.Price;
            model.DetailPairs = _storeRepository.GetProductDetails(product.Id);
            model.ProductLikes = _storeRepository.LikeDislikeCount("productlike", product.Id);
            model.ProductDislikes = _storeRepository.LikeDislikeCount("productdislike", product.Id);
            model.CommentViewModel = CreateCommentViewModel(product.Id, sortOrder);
            return View(model);
        }

        public ActionResult UpdateProductLike(string productid, string slug, string username, string likeordislike, string sortorder)
        {
            _storeRepository.UpdateProductLike(productid, username, likeordislike);
            return RedirectToAction("Product", new { slug = slug, sortorder = sortorder });
        }

        [HttpGet]
        public ActionResult AddPictureToProduct(string productid, string slug)
        {
                ProductViewModel model = new ProductViewModel();
                model.ID = productid;
                model.UrlSeo = slug; 
                return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPictureToProduct(string productid, string slug, ProductPicture pic, HttpPostedFileBase uploadImage)
        {
            CreateProductViewModel(slug);
            _storeRepository.AddPictureToProduct(productid, pic, uploadImage);
            return RedirectToAction("EditProduct", new { slug = slug });
        }

        [HttpGet]
        public ActionResult AddPropertyValueToProduct(string categoryid, string productid, string slug)
        {
            ProductViewModel model = new ProductViewModel();  
            model.ID = productid;
            model.UrlSeo = slug;
            model.ProductProperties = _storeRepository.GetProductPropertiesByCatId(categoryid);
            return PartialView(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPropertyValueToProduct(string productId, string slug, PropertyValue ppv, string propertyId, string value)
        {
            CreateProductViewModel(slug);
            //_storeRepository.AddPropertyValueToProduct(ppv, productId, propertyId, value);
            return RedirectToAction("EditProduct", new { slug = slug });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RemovePictureFromProduct(string slug, string productid)
        {
            CreateProductViewModel(slug);
            _storeRepository.RemovePictureFromProduct(productid);
            return RedirectToAction("EditProduct", new { slug = slug });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EditProduct(string slug)
        {
            var model = CreateProductViewModel(slug);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditProduct(ProductViewModel model)
        {
            var product = _storeRepository.GetProductById(model.ID);
            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.UrlSeo = model.UrlSeo;
       
            _storeRepository.Save();

            return RedirectToAction("Product", new { slug = model.UrlSeo });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DeleteProduct(ProductViewModel model, string productid)
        {
            model.ID = productid;
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(string productId)
        {
            _storeRepository.DeleteProductandComponents(productId);
            return RedirectToAction("Index", "Product");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddNewProduct()
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var products = _storeRepository.GetProducts();
            if (products.Count != 0)
            {
                foreach (var product in products)
                {
                    var productid = product.Id;
                    Int32.TryParse(productid, out num);
                    numlist.Add(num);
                }
                numlist.Sort();
                num = numlist.Last();
                num++;
            }
            else
            {
                num = 1;
            }
            var newid = num.ToString();
            ProductViewModel model = new ProductViewModel();
            model.ID = newid;
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddNewProduct(ProductViewModel model)
        {
            var product = new Product
            {
                Id = model.ID,
                Name = model.Name,
                PostedOn = DateTime.Now,
                Description = model.Description,
                Price = model.Price,
                UrlSeo = model.UrlSeo,
             };
                _storeRepository.AddNewProduct(product);
           
            return RedirectToAction("EditProduct", "Product", new { slug = model.UrlSeo });
        }

        

         [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddNewProperty(string categoryid, bool callfromcategory)
        {
            if (categoryid != null && callfromcategory)
            {
                CategoryViewModel model = new CategoryViewModel();
                model.Id = categoryid;
                return View(model);
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewProperty(string categoryid, string propName, string propUrlSeo)
        {
            if (categoryid != null)
            {
                _storeRepository.AddNewProperty(propName, propUrlSeo);
                return RedirectToAction("AddPropertyToCategory", new { categoryid = categoryid });
            }
            else
            {
                _storeRepository.AddNewProperty(propName, propUrlSeo);
                return RedirectToAction("Categories", "Product");
            }
        }
        [Authorize(Roles = "Admin")]
        public ActionResult RemovePropertyFromCategory(string slug, string categoryid, string propName)
        {
            CreateCategoryViewModel(slug);
            _storeRepository.RemovePropertyFromCategory(categoryid, propName);
            return RedirectToAction("EditCategory", new { slug = slug });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddNewCategory(string productid, bool callfromproduct)
        {
            if (productid != null && callfromproduct)
            {
                ProductViewModel model = new ProductViewModel();
                model.ID = productid;
                return View(model);
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCategory(string productid, string catName, string catUrlSeo, string catDesc)
        {
            if (productid != null)
            {
                _storeRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("AddCategoryToProduct", new { productid = productid });
            }
            else
            {
                _storeRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("Categories", "Product");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EditCategory(string slug)
        {
            var model = CreateCategoryViewModel(slug);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditCategory(CategoryViewModel model)
        {
            var category = _storeRepository.GetCategoryById(model.Id);
            category.Name = model.Name;
            category.UrlSeo = model.UrlSeo;

            _storeRepository.Save();

            return RedirectToAction("Categories", new { slug = model.UrlSeo });
        }


        [Authorize(Roles = "Admin")]
        public ActionResult RemoveCategoryFromProduct(string slug, string productid, string catName)
        {
            CreateProductViewModel(slug);
            _storeRepository.RemoveCategoryFromProduct(productid, catName);
            return RedirectToAction("EditProduct", new { slug = slug });
        }
        [Authorize(Roles = "Admin")]
        public ActionResult RemovePropertyValueFromProduct(string slug, string productid, string propValue)
        {
            CreateProductViewModel(slug);
            _storeRepository.RemovePropertyValueFromProduct(productid, propValue);
            return RedirectToAction("EditProduct", new { slug = slug });
        }
       

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Categories()
        {
            checkCatList.Clear();
            CreateCategoryList();

            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCategories(string[] categoryNames)
        {
            if (categoryNames != null)
            {
                foreach (var catName in categoryNames)
                {
                    var category = _storeRepository.GetCategories().Where(x => x.Name == catName).FirstOrDefault();
                    _storeRepository.RemoveCategory(category);
                }
            }
         
            return RedirectToAction("Categories", "Product");

       }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddPropertyToCategory(string categoryId)
        {
            CategoryViewModel model = new CategoryViewModel();
            model.Id = categoryId;
            model.ProductProperties = _storeRepository.GetProductProperties();
            return View(model);

        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPropertyToCategory(CategoryViewModel model)
        {
            var category = _storeRepository.GetCategoryById(model.Id);
            var categoryProps = _storeRepository.GetCategoryProperties(category);
            List<string> pPropIds = new List<string>();
            foreach (var pProp in categoryProps)
            {
                pPropIds.Add(pProp.Id);
            }
            var newProps = model.ProductProperties.Where(x => x.Checked == true).ToList();
            List<string> nPropIds = new List<string>();
            foreach (var pProp in newProps)
            {
                nPropIds.Add(pProp.Id);
            }
            if (!pPropIds.SequenceEqual(nPropIds))
            {
                foreach (var pProp in categoryProps)
                {
                    _storeRepository.RemoveCategoryProperties(model.Id, pProp.Id);
                }
                foreach (var prop in model.ProductProperties)
                {
                    CategoryProperty categoryProperty = new CategoryProperty();
                    if (prop.Checked == true)
                    {
                        categoryProperty.CategoryId = model.Id;
                        categoryProperty.PropertyId = prop.Id;
                        categoryProperty.Checked = true;
                        _storeRepository.AddCategoryProperties(categoryProperty);
                    }
                }
                _storeRepository.Save();
        }
            return RedirectToAction("EditCategory", new { slug = category.UrlSeo });

        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddCategoryToProduct(string productid)
        {
            ProductViewModel model = new ProductViewModel();
            model.ID = productid;
            model.Categories = _storeRepository.GetCategories();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategoryToProduct(ProductViewModel model)
        {
            var product = _storeRepository.GetProductById(model.ID);
            var productCats = _storeRepository.GetProductCategories(product);
            List<string> pCatIds = new List<string>();
            foreach (var pCat in productCats)
            {
                pCatIds.Add(pCat.Id);
            }
            var newCats = model.Categories.Where(x => x.Checked == true).ToList();
            List<string> nCatIds = new List<string>();
            foreach (var pCat in newCats)
            {
                nCatIds.Add(pCat.Id);
            }
            if (!pCatIds.SequenceEqual(nCatIds))
            {
                foreach (var pCat in productCats)
                {
                    _storeRepository.RemoveProductCategories(model.ID, pCat.Id);
                }
                foreach (var cat in model.Categories)
                {
                    ProductCategory productCategory = new ProductCategory();
                    if (cat.Checked == true)
                    {
                        productCategory.ProductId = model.ID;
                        productCategory.CategoryId = cat.Id;
                        productCategory.Checked = true;
                        _storeRepository.AddProductCategories(productCategory);
                    }
                }
                _storeRepository.Save();
            }
            return RedirectToAction("EditProduct", new { slug = product.UrlSeo });
        }

        #endregion Product


        [ChildActionOnly]
        public ActionResult Comments(string productId, string sortOrder)
        {
            return PartialView();
        }
        public CommentViewModel CreateCommentViewModel(string productId, string sortOrder)
        {
            CommentViewModel model = new CommentViewModel();
            model.ID = productId;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.BestSortParm = sortOrder == "Best" ? "best_desc" : "Best";

            var comments = _storeRepository.GetCommentsByPageId(productId).OrderByDescending(d => d.DateTime).ToList();
            foreach (var comment in comments)
            {
                var likes = LikeDislikeCount("commentlike", comment.Id);
                var dislikes = LikeDislikeCount("commentdislike", comment.Id);
                comment.NetLikeCount = likes - dislikes;
                if (comment.Replies != null) comment.Replies.Clear();
                List<CommentViewModel> replies = _storeRepository.GetParentReplies(comment);
                foreach (var reply in replies)
                {
                    var rep = _storeRepository.GetReplyById(reply.Id);
                    comment.Replies.Add(rep);
                }
            }

            if (productId.Contains("product"))
            {
                model.UrlSeo = _storeRepository.GetProductById(productId).UrlSeo;
            }


            switch (sortOrder)
            {
                case "date_asc":
                    comments = comments.OrderBy(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                    break;
                case "Best":
                    comments = comments.OrderByDescending(x => x.NetLikeCount).ToList();
                    ViewBag.BestSortLink = "active";
                    break;
                case "best_desc":
                    comments = comments.OrderBy(x => x.NetLikeCount).ToList();
                    ViewBag.BestSortLink = "active";
                    break;
                default:
                    comments = comments.OrderByDescending(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                    break;
            }

            model.Comments = comments;
            return model;
        }
        public PartialViewResult Replies()
        {
            return PartialView();
        }
        public PartialViewResult ChildReplies()
        {
            return PartialView();
        }


        public ActionResult UpdateCommentLike(string commentid, string username, string likeordislike, string slug, string sortorder)
        {
            if (username != null)
            {
                _storeRepository.UpdateCommentLike(commentid, username, likeordislike);
            }
            return RedirectToAction("Product", new { slug = slug, sortorder = sortorder });
        }
        public ActionResult UpdateReplyLike(string replyid, string username, string likeordislike, string sortorder)
        {
            if (username != null)
            {
                _storeRepository.UpdateReplyLike(replyid, username, likeordislike);
            }
            var slug = _storeRepository.GetProductByReply(replyid).UrlSeo;
            return RedirectToAction("Product", "Product", new { slug = slug, sortorder = sortorder });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewComment(string commentBody, string comUserName, string slug, string productId)
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var comments = _storeRepository.GetComments().ToList();
            if (comments.Count() != 0)
            {
                foreach (var cmnt in comments)
                {
                    var comid = cmnt.Id;
                    Int32.TryParse(comid.Replace("cmt", ""), out num);
                    numlist.Add(num);
                }
                numlist.Sort();
                num = numlist.Last();
                num++;
            }
            else
            {
                num = 1;
            }
            var newid = "cmt" + num.ToString();
            var comment = new Comment()
            {
                Id = newid,
                ProductId = productId,
                DateTime = DateTime.Now,
                UserName = comUserName,
                Body = commentBody,
                NetLikeCount = 0
            };
            _storeRepository.AddNewComment(comment);

            if (productId.Contains("product"))
            {
                return RedirectToAction("Product","Product", new { slug = slug });
            }
            else
            {
                return RedirectToAction("Index", "Product");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewParentReply(string replyBody, string comUserName, string commentid, string slug)
        {
            var comDelChck = CommentDeleteCheck(commentid);
            if (!comDelChck)
            {
                List<int> numlist = new List<int>();
                int num = 0;
                var replies = _storeRepository.GetReplies().ToList();
                if (replies.Count != 0)
                {
                    foreach (var rep in replies)
                    {
                        var repid = rep.Id;
                        Int32.TryParse(repid.Replace("rep", ""), out num);
                        numlist.Add(num);
                    }
                    numlist.Sort();
                    num = numlist.Last();
                    num++;
                }
                else
                {
                    num = 1;
                }
                var newid = "rep" + num.ToString();
                var reply = new Reply()
                {
                    Id = newid,
                    CommentId = commentid,
                    ParentReplyId = null,
                    DateTime = DateTime.Now,
                    UserName = comUserName,
                    Body = replyBody,
                };
                _storeRepository.AddNewReply(reply);
            }

            var productId = _storeRepository.GetPageIdByComment(commentid);
            if (productId.Contains("product"))
            {
                return RedirectToAction("Product", new { slug = slug });
            }
            else
            {
                return RedirectToAction("Index", "Product");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewChildReply(string preplyid, string comUserName, string replyBody)
        {
            var repDelCheck = ReplyDeleteCheck(preplyid);
            var preply = _storeRepository.GetReplyById(preplyid);
            if (!repDelCheck)
            {
                List<int> numlist = new List<int>();
                int num = 0;
                var replies = _storeRepository.GetReplies().ToList();
                if (replies.Count != 0)
                {
                    foreach (var rep in replies)
                    {
                        var repid = rep.Id;
                        Int32.TryParse(repid.Replace("rep", ""), out num);
                        numlist.Add(num);
                    }
                    numlist.Sort();
                    num = numlist.Last();
                    num++;
                }
                else
                {
                    num = 1;
                }
                var newid = "rep" + num.ToString();
                var reply = new Reply()
                {
                    Id = newid,
                    CommentId = preply.CommentId,
                    ParentReplyId = preply.Id,
                    DateTime = DateTime.Now,
                    UserName = comUserName,
                    Body = replyBody,
                };
                _storeRepository.AddNewReply(reply);
            }
            var productId = _storeRepository.GetPageIdByComment(preply.CommentId);
            if (productId.Contains("product"))
            {
                return RedirectToAction("Product", new { slug = _storeRepository.GetUrlSeoByReply(preply) });
            }
            else
            {
                return RedirectToAction("Index", "Product");
            }
        }



        [HttpGet]
        public async Task<ActionResult> EditComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _storeRepository.GetCommentById(commentid);
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                model.Body = comment.Body;
                return View(model);
            }
            else
            {
                return RedirectToAction("Product", new { slug = _storeRepository.GetProducts().Where(x => x.Id == comment.ProductId).FirstOrDefault().UrlSeo });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditComment(string commentid, string commentBody)
        {
            var comment = _storeRepository.GetCommentById(commentid);
            comment.Body = commentBody;
            comment.DateTime = DateTime.Now;
            _storeRepository.Save();
            return RedirectToAction("Product","Product", new { slug = _storeRepository.GetProducts().Where(x => x.Id == comment.ProductId).FirstOrDefault().UrlSeo });
        }


        [HttpGet]
        public async Task<ActionResult> DeleteComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _storeRepository.GetCommentById(commentid);
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Product", new { slug = _storeRepository.GetProducts().Where(x => x.Id == comment.ProductId).FirstOrDefault().UrlSeo });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(string commentid)
        {
            var comment = _storeRepository.GetCommentById(commentid);
            var productid = comment.ProductId;
            var repliesList = _storeRepository.GetParentReplies(comment);
            if (repliesList.Count() == 0)
            {
                _storeRepository.DeleteComment(commentid);
            }
            else
            {
                comment.DateTime = DateTime.Now;
                comment.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                comment.Deleted = true;
                _storeRepository.Save();
            }
            return RedirectToAction("Product", new { slug = _storeRepository.GetProducts().Where(x => x.Id == productid).FirstOrDefault().UrlSeo });
        }


        [HttpGet]
        public async Task<ActionResult> EditReply(CommentViewModel model, string replyid)
        {
            var user = await GetCurrentUserAsync();
            var reply = _storeRepository.GetReplyById(replyid);
            if (reply.UserName == user.UserName)
            {
                model.Id = replyid;
                model.Body = reply.Body;
                return View(model);
            }
            else
            {
                return RedirectToAction("Product", new { slug = _storeRepository.GetUrlSeoByReply(reply) });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditReply(string replyid, string replyBody)
        {
            var reply = _storeRepository.GetReplyById(replyid);
            reply.Body = replyBody;
            reply.DateTime = DateTime.Now;
            _storeRepository.Save();
            return RedirectToAction("Product", new { slug = _storeRepository.GetUrlSeoByReply(reply) });
        }


        [HttpGet]
        public async Task<ActionResult> DeleteReply(CommentViewModel model, string replyid)
        {
            var user = await GetCurrentUserAsync();
            var reply = _storeRepository.GetReplyById(replyid);
            if (reply.UserName == user.UserName)
            {
                model.Id = replyid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Product", new { slug = _storeRepository.GetUrlSeoByReply(reply) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReply(string replyid)
        {
            var reply = _storeRepository.GetReplyById(replyid);
            var repliesList = _storeRepository.GetChildReplies(reply);
            var productid = _storeRepository.GetUrlSeoByReply(reply);
            if (repliesList.Count() == 0)
            {
                _storeRepository.DeleteReply(replyid);
            }
            else
            {
                reply.DateTime = DateTime.Now;
                reply.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                reply.Deleted = true;
                _storeRepository.Save();
            }
            return RedirectToAction("Product", new { slug = _storeRepository.GetProducts().Where(x => x.Id == productid).FirstOrDefault().UrlSeo });
        }
        #region Helpers

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await UserManager.FindByIdAsync(User.Identity.GetUserId());
        }


        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            return _storeRepository.GetChildReplies(parentReply);
        }


        public bool CommentDeleteCheck(string commentid)
        {
            return _storeRepository.CommentDeleteCheck(commentid);
        }
        public bool ReplyDeleteCheck(string replyid)
        {
            return _storeRepository.ReplyDeleteCheck(replyid);
        }
        public static string TimePassed(DateTime postDate)
        {
            string date = null;
            double dateDiff = 0.0;
            var timeDiff = DateTime.Now - postDate;
            var yearPassed = timeDiff.TotalDays / 365;
            var monthPassed = timeDiff.TotalDays / 30;
            var dayPassed = timeDiff.TotalDays;
            var hourPassed = timeDiff.TotalHours;
            var minutePassed = timeDiff.TotalMinutes;
            var secondPassed = timeDiff.TotalSeconds;
            if (Math.Floor(yearPassed) > 0)
            {
                dateDiff = Math.Floor(yearPassed);
                date = dateDiff == 1 ? dateDiff + " год назад" : dateDiff + " лет назад";
            }
            else
            {
                if (Math.Floor(monthPassed) > 0)
                {
                    dateDiff = Math.Floor(monthPassed);
                    date = dateDiff == 1 ? dateDiff + " месяц назад" : dateDiff + " месяцев назад";
                }
                else
                {
                    if (Math.Floor(dayPassed) > 0)
                    {
                        dateDiff = Math.Floor(dayPassed);
                        date = dateDiff == 1 ? dateDiff + " день назад" : dateDiff + " дней назад";
                    }
                    else
                    {
                        if (Math.Floor(hourPassed) > 0)
                        {
                            dateDiff = Math.Floor(hourPassed);
                            date = dateDiff == 1 ? dateDiff + " час назад" : dateDiff + " часов назад";
                        }
                        else
                        {
                            if (Math.Floor(minutePassed) > 0)
                            {
                                dateDiff = Math.Floor(minutePassed);
                                date = dateDiff == 1 ? dateDiff + " минуту назад" : dateDiff + " минут назад";
                            }
                            else
                            {
                                dateDiff = Math.Floor(secondPassed);
                                date = dateDiff == 1 ? dateDiff + " секунду назад" : dateDiff + " секунд назад";
                            }
                        }
                    }
                }
            }

            return date;
        }
        public string[] NewCommentDetails(string username)
        {
            string[] newCommentDetails = new string[3];
            newCommentDetails[0] = "td" + username; //comText
            newCommentDetails[1] = "tdc" + username; //comTextdiv
            newCommentDetails[2] = "tb" + username; //comTextBtn
            return newCommentDetails;
        }

        public string[] CommentDetails(Comment comment)
        {
            string[] commentDetails = new string[17];
            commentDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(comment.UserName);//username
            commentDetails[1] = "/Content/images/profile/" + commentDetails[0] + ".png?time=" + DateTime.Now.ToString();//imgUrl
            commentDetails[2] = TimePassed(comment.DateTime);//passed time
            commentDetails[3] = comment.DateTime.ToLongDateString().Replace(comment.DateTime.DayOfWeek.ToString() + ", ", "");//comment date
            commentDetails[4] = "gp" + comment.Id; //grandparentid
            commentDetails[5] = "mc" + comment.Id; //maincommentId
            commentDetails[6] = "crp" + comment.Id; //repliesId
            commentDetails[7] = "cex" + comment.Id; //commentExpid
            commentDetails[8] = "ctex" + comment.Id; //ctrlExpId
            commentDetails[9] = "ctflg" + comment.Id; //ctrlFlagId
            commentDetails[10] = "sp" + comment.Id; //shareParentId
            commentDetails[11] = "sc" + comment.Id; //shareChildId
            commentDetails[12] = "td" + comment.Id; //comText
            commentDetails[13] = "tdc" + comment.Id; //comTextdiv
            commentDetails[14] = "rpl" + comment.Id; //Reply
            commentDetails[15] = "cc1" + comment.Id; //commentControl
            commentDetails[16] = "cc2" + comment.Id; //commentMenu
            return commentDetails;
        }
        public string[] ReplyDetails(string replyId)
        {
            string[] replyDetails = new string[17];
            var reply = _storeRepository.GetReplyById(replyId);
            replyDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(reply.UserName);//username
            replyDetails[1] = "/Content/images/profile/" + replyDetails[0] + ".png?time=" + DateTime.Now.ToString(); //imgUrl
            replyDetails[2] = TimePassed(reply.DateTime); //passed time
            replyDetails[3] = reply.DateTime.ToLongDateString().Replace(reply.DateTime.DayOfWeek.ToString() + ", ", ""); //reply date
            replyDetails[4] = "gp" + replyId; //grandparentid
            replyDetails[5] = "rp" + replyId; //parentreplyId
            replyDetails[6] = "crp" + replyId; //repliesId
            replyDetails[7] = "cex" + replyId; //commentExpid
            replyDetails[8] = "ctex" + replyId; //ctrlExpId
            replyDetails[9] = "ctflg" + replyId; //ctrlFlagId
            replyDetails[10] = "sp" + replyId; //shareParentId
            replyDetails[11] = "sc" + replyId; //shareChildId;
            replyDetails[12] = "td" + replyId; //comText
            replyDetails[13] = "tdc" + replyId; //comTextdiv
            replyDetails[14] = "rpl" + replyId; //Reply
            replyDetails[15] = "cc1" + replyId; //commentControl
            replyDetails[16] = "cc2" + replyId; //commentMenu

            return replyDetails;
        }
        public int LikeDislikeCount(string typeAndlike, string id)
        {
            switch (typeAndlike)
            {
                case "commentlike":
                    return _storeRepository.LikeDislikeCount("commentlike", id);
                case "commentdislike":
                    return _storeRepository.LikeDislikeCount("commentdislike", id);
                case "replylike":
                    return _storeRepository.LikeDislikeCount("replylike", id);
                case "replydislike":
                    return _storeRepository.LikeDislikeCount("replydislike", id);
                default:
                    return 0;
            }
        }
        public IList<Product> GetProducts()
        {
            return _storeRepository.GetProducts();
        }
        public IList<ProductPicture> GetProductPictures(Product product)
        {
            return _storeRepository.GetProductPictures(product);
        }
        public IList<Category> GetProductCategories(Product product)
        {
            return _storeRepository.GetProductCategories(product);
        }
        public void CreateCategoryList()
        {
            foreach (var ct in _storeRepository.GetCategories())
            {
                checkCatList.Add(new AllProductsViewModel { Category = ct, Checked = false });
            }
        }
        public void CreateDetailList()
        {     
                checkDetailList.Add(new AllProductsViewModel { Detail = _storeRepository.GetDetails() });
        }



        public ProductViewModel CreateProductViewModel(string slug)
        {
            ProductViewModel model = new ProductViewModel();
            var productid = _storeRepository.GetProductIdBySlug(slug);    
            var product = _storeRepository.GetProductById(productid);    
            model.ID = productid;
            model.Name = product.Name;
            model.Pictures = _storeRepository.GetProductPictures(product).ToList();
            model.UrlSeo = product.UrlSeo;
            model.Price = product.Price;
            model.ProductCategories = _storeRepository.GetProductCategories(product).ToList();
            model.DetailPairs = _storeRepository.GetProductDetails(productid);
            model.Description = product.Description;
            return model;
        }
            public CategoryViewModel CreateCategoryViewModel(string slug)
        {
            CategoryViewModel model = new CategoryViewModel();
            var categoryid = _storeRepository.GetCategoryIdBySlug(slug);
            var category = _storeRepository.GetCategoryById(categoryid);
            model.Id = categoryid;
            model.Name = category.Name;
            model.UrlSeo = category.UrlSeo;
            model.CategoryProperties = _storeRepository.GetCategoryProperties(category).ToList();
            return model;
        }

        #endregion

    }
}