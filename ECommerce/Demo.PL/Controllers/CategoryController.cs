using AutoMapper;
using Demo.BBL.Interfaces;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Demo.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.PL.Controllers
{

    [Authorize(Roles = SD.Admin + "," + SD.Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {

            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var mappedCategory = _mapper.Map<IEnumerable<Category>,
                IEnumerable<CategoryViewModel>>(categories);
            return View(mappedCategory);
        }


        public IActionResult Create()
        {
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {

            if (model.Name == model.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder can't match the name.");
            }

            var categoriesDb =await _unitOfWork.Repository<Category>().GetAllAsync();
            var categories = categoriesDb.Select(i => i.Name.ToLower());
            if (categories.Contains(model.Name.ToLower()))
            {
                TempData["error"] = "This category exists.";
                return View();
            }

            if (ModelState.IsValid)
            {
                var cat = _mapper.Map<CategoryViewModel, Category>(model);

                await _unitOfWork.Repository<Category>().AddAsync(cat);
                await _unitOfWork.CompleteAsync();
                TempData["success"] = "Created Successfully.";

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id.Value);
            if (category is null)
            {
                return BadRequest();
            }

            var mapped = _mapper.Map<Category, CategoryViewModel>(category);
            return View(mapped);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var categoriesDb = await _unitOfWork.Repository<Category>().GetAllAsync();
            var categories = categoriesDb.Select(i => i.Name.ToLower());
            if (categories.Contains(model.Name.ToLower()))
            {
                TempData["error"] = "This category exists.";
                return View();
            }


            if (ModelState.IsValid)
            {
                var cat = _mapper.Map<CategoryViewModel, Category>(model);

                _unitOfWork.Repository<Category>().Update(cat);
                await _unitOfWork.CompleteAsync();

                TempData["success"] = "Updated Successfully.";

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id.Value);
            if (category is null)
            {
                return BadRequest();
            }

            var mapped = _mapper.Map<Category, CategoryViewModel>(category);
            return View(mapped);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                var category = _mapper.Map<CategoryViewModel, Category>(model);

                if (category == null)
                {
                    return NotFound();
                }
                _unitOfWork.Repository<Category>().Delete(category);
                await _unitOfWork.CompleteAsync();

                TempData["success"] = "Deleted Successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }




            return View(model);
        }
    }
}
