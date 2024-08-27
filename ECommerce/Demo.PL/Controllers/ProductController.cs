using AutoMapper;
using Azure.Core.Serialization;
using Demo.BBL.Interfaces;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Demo.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Demo.PL.Controllers
{
    [Authorize(Roles = SD.Admin + "," + SD.Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            var mappedProducts = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductViewModel>>(products);
            return View(mappedProducts);
        }



        #region Create

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model, List<IFormFile> files)
        {
            ViewBag.Categories = await _unitOfWork.Repository<Category>().GetAllAsync();

            try
            {


                var product = _mapper.Map<ProductViewModel, Product>(model);

                await _unitOfWork.Repository<Product>().AddAsync(product);
                await _unitOfWork.CompleteAsync();

                foreach (var file in files)
                {
                    var path = Path.Combine("Products", "Product-" + product.Id.ToString());

                    var (fileName, imagePath) = await FileSettings.AddOrUpdateFile(file, path);

                    var image = new ProductImage
                    {
                        ImageName = fileName,
                        
                        ProductId = product.Id,

                    };

                    product.ProductImages.Add(image);


                }
                await _unitOfWork.CompleteAsync();


                TempData["success"] = "Created Successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return BadRequest("An Error:( -> " + e.Message);
            }

            return View(model);
        }
        #endregion


        #region Details
        public async Task<IActionResult> Details(int? id, string viewName = "Details")
        {
            if (id is null)
            {
                return BadRequest();
            }

            var product = await _unitOfWork.Repository<Product>()
                .GetWithFilterAsync(p=>p.Id==id.Value, includeProperty: "ProductImages");

            if (product is null)
            {
                return NotFound();
            }

            var mappedProduct = _mapper.Map<Product, ProductViewModel>(product);

            ViewBag.CategoryName = _unitOfWork.Repository<Category>()
                .GetByIdAsync(mappedProduct.CategoryId.Value)
                .Result
                .Name;

            return View(viewName, mappedProduct);

        }

        #endregion


        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.Categories = await _unitOfWork.Repository<Category>().GetAllAsync();

            return await Details(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([FromRoute]int id, ProductViewModel model, List<IFormFile> files)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ViewBag.Categories = await _unitOfWork.Repository<Category>().GetAllAsync();

            try
            {
                var oldProduct = await _unitOfWork.Repository<Product>()
                    .GetWithFilterAsync(i=> i.Id==id, includeProperty: "ProductImages");

                if (files.Count() > 0)
                {

                    foreach (var file in files)
                    {
                        var path = Path.Combine("Products", "Product-" + oldProduct.Id.ToString());

                        var (fileName, imagePath) = await FileSettings.AddOrUpdateFile(file, path);

                        var image = new ProductImage
                        {
                            ImageName = fileName,

                            ProductId = oldProduct.Id,

                        };

                        oldProduct.ProductImages.Add(image);
                    }
                    await _unitOfWork.CompleteAsync();
                }


                _unitOfWork.DetachEntity(oldProduct);


                var product = _mapper.Map<ProductViewModel, Product>(model);

                _unitOfWork.Repository<Product>().Update(product);

                await _unitOfWork.CompleteAsync();

                TempData["success"] = "Updated Successfully.";

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        #endregion


        #region Delete

        public async Task<IActionResult> Delete(int? id)
        {
            return await Details(id, "Delete");
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int? id, ProductViewModel model)
        {
            if (id is null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                var product = _mapper.Map<ProductViewModel, Product>(model);

                var path = Path.Combine("Products", "Product-"+ product.Id);
                if (Path.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                _unitOfWork.Repository<Product>().Delete(product);

                await _unitOfWork.CompleteAsync();

                TempData["success"] = "Deleted Successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion


        
        public async Task<IActionResult> DeleteImage(int? imageId)
        {
            var image = await _unitOfWork.Repository<ProductImage>().GetByIdAsync(imageId.Value);

            var Id = image.ProductId;

            if (image is not null)
            {
                var ImagePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","Images", "Products", "Product-"+Id,image.ImageName);

                FileSettings.DeleteFile(ImagePath);

                _unitOfWork.Repository<ProductImage>().Delete(image);
                await _unitOfWork.CompleteAsync();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Edit), new { id = Id });
        }

    }
}
