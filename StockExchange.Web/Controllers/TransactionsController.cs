﻿using StockExchange.Business.Models;
using StockExchange.Business.ServiceInterfaces;
using StockExchange.Web.Filters;
using StockExchange.Web.Helpers;
using StockExchange.Web.Models.Transactions;
using System;
using System.Web.Mvc;

namespace StockExchange.Web.Controllers
{
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IPriceService _priceService;

        public TransactionsController(ITransactionsService transactionsService, IPriceService priceService)
        {
            _transactionsService = transactionsService;
            _priceService = priceService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new AddTransactionViewModel
            {
                Companies = _priceService.GetAllCompanies()
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult TransactionsTable()
        {
            var transactions = _transactionsService.GetUserTransactions(CurrentUserId);
            return PartialView("_TransactionsTable", transactions);
        }

        [HttpPost]
        [HandleJsonError]
        public ActionResult AddTransaction(AddTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return new JsonNetResult(ModelState);
            }

            var dto = BuildUserTransactionDto(model);
            _transactionsService.AddUserTransaction(dto);
            return new JsonNetResult(true);
        }

        private UserTransactionDto BuildUserTransactionDto(AddTransactionViewModel model)
        {
            return new UserTransactionDto
            {
                Date = DateTime.Now,
                CompanyId = model.SelectedCompanyId,
                Price = model.Price,
                Quantity = model.TransactionType == TransactionActionType.Buy ? model.Quantity : -model.Quantity,
                UserId = CurrentUserId
            };
        }
    }
}