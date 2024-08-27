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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompanyController(IUnitOfWork unitOfWork, IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            var MappedCompanies = _mapper.Map<IEnumerable<Company>, IEnumerable<CompanyViewModel>>(companies);
            return View(MappedCompanies);
        }

        #region Create

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {

            try
            {

                var company = _mapper.Map<CompanyViewModel, Company>(model);

                await _unitOfWork.Repository<Company>().AddAsync(company);
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

            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id.Value);
            if (company is null)
            {
                return NotFound();
            }

            var mappedCompany = _mapper.Map<Company, CompanyViewModel>(company);

            return View(viewName, mappedCompany);

        }

        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {

            return await Details(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            try
            {
                var company = _mapper.Map<CompanyViewModel, Company>(model);

                _unitOfWork.Repository<Company>().Update(company);

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
        public async Task<ActionResult> Delete(int? id, CompanyViewModel model)
        {
            if (id is null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                var company = _mapper.Map<CompanyViewModel, Company>(model);

                _unitOfWork.Repository<Company>().Delete(company);

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


    }
}
