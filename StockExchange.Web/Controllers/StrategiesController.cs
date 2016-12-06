﻿using System;
using System.Web.Mvc;
using StockExchange.Business.Services;
using StockExchange.Web.Models;

namespace StockExchange.Web.Controllers
{
    [Authorize]
    public class StrategiesController : Controller
    {
        private readonly IPriceService _priceService;

        public StrategiesController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        // GET: Strategies
        public ActionResult Index()
        {
            var model = GetViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateStrategy(StrategyViewModel model)
        {

            return RedirectToAction("Index");
        }

        private StrategyViewModel GetViewModel()
        {
            var model = new StrategyViewModel
            {
                Companies = _priceService.GetAllCompanies(),
                StartDate = new DateTime(2006, 01, 01),
                EndDate = DateTime.Today
            };
            return model;
        }
    }
}