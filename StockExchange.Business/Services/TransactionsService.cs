﻿using StockExchange.Business.ErrorHandling;
using StockExchange.Business.Exceptions;
using StockExchange.Business.Extensions;
using StockExchange.Business.Models.Filters;
using StockExchange.Business.Models.Paging;
using StockExchange.Business.Models.Transaction;
using StockExchange.Business.ServiceInterfaces;
using StockExchange.DataAccess.IRepositories;
using StockExchange.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExchange.Business.Services
{
    /// <summary>
    /// Provides methods for operating on user transactions
    /// </summary>
    public class TransactionsService : ITransactionsService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionsRepository _transactionsRepository;

        /// <summary>
        /// Creates a new instance of <see cref="TransactionsService"/>
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="transactionsRepository"></param>
        public TransactionsService(IUserRepository userRepository, ITransactionsRepository transactionsRepository)
        {
            _userRepository = userRepository;
            _transactionsRepository = transactionsRepository;
        }

        /// <inheritdoc />
        public async Task<PagedList<UserTransactionDto>> GetTransactions(int userId, PagedFilterDefinition<TransactionFilter> filter)
        {
            var allTransactions = await _transactionsRepository.GetAllUserTransactions(userId);
            var transactions = allTransactions.Select(t => new UserTransactionDto
                {
                    Date = t.Date,
                    Price = t.Price,
                    Quantity = t.Quantity < 0 ? -t.Quantity : t.Quantity,
                    Action = t.Quantity < 0 ? Action.Sell : Action.Buy,
                    Id = t.Id,
                    CompanyName = t.Company.Code,
                    Total = t.Quantity < 0 ? -t.Quantity * t.Price : t.Quantity * t.Price,
                    Profit = 0
                }).ToList();
            var pagedTransactions = transactions.ToPagedList(filter.Start, filter.Length);
            foreach (var pagedTransaction in pagedTransactions)
            {
                if (pagedTransaction.Action == Action.Buy) continue;
                var pastTransactions = transactions
                    .Where(item => item.CompanyName == pagedTransaction.CompanyName && item.Date < pagedTransaction.Date)
                    .OrderBy(item => item.Date)
                    .ToList();
                var quantity = 0m;
                var price = 0m;
                foreach (var pastTransaction in pastTransactions)
                {
                    if (pastTransaction.Action == Action.Buy)
                    {
                        price = (price * quantity + pastTransaction.Price * pastTransaction.Quantity) /
                                (quantity + pastTransaction.Quantity);
                        quantity += pastTransaction.Quantity;
                    }
                    else
                    {
                        quantity -= pastTransaction.Quantity;
                    }
                }
                pagedTransaction.Profit = pagedTransaction.Quantity * (pagedTransaction.Price - price);
            }
            return pagedTransactions;
        }

        /// <inheritdoc />
        public async Task<int> GetTransactionsCount(int userId)
        {
            return await _transactionsRepository.GetTransactionsCount(userId);
        }

        /// <inheritdoc />
        public async Task AddTransaction(UserTransactionDto dto)
        {
            var user = await _userRepository.GetUserWithTransactions(dto.UserId);
            if (user == null)
                throw new BusinessException(nameof(dto.UserId), "User does not exist", ErrorStatus.DataNotFound);
            VerifyTransaction(dto, user);
            user.Budget -= dto.Quantity * dto.Price;
            user.Transactions.Add(new UserTransaction
            {
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                Date = dto.Date,
                Price = dto.Price,
                Quantity = dto.Quantity
            });
            await _userRepository.Save();
        }
        
        /// <inheritdoc />
        public async Task<Dictionary<int, List<UserTransaction>>> GetTransactionsByCompany(int userId)
        {
            return await _transactionsRepository.GetTransactionsByCompany(userId);
        }

        private static void VerifyTransaction(UserTransactionDto dto, User user)
        {
            if (dto.Quantity < 0)
            {
                int currentlyOwnedStocksCount = user.Transactions.Where(t => t.CompanyId == dto.CompanyId)
                    .Sum(t => t.Quantity);
                if (currentlyOwnedStocksCount < -dto.Quantity)
                {
                    throw new BusinessException($"Cannot sell more stocks than currently owned ({currentlyOwnedStocksCount})");
                }
            }
            else if (user.Budget < dto.Price * dto.Quantity)
            {
                throw new BusinessException($"Not enough free budget (currently {user.Budget})");
            }
        }
    }
}
