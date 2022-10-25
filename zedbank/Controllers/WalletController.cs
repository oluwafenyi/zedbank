using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using zedbank.Database;
using zedbank.Exceptions;
using zedbank.Models;
using zedbank.Services;
using zedbank.Validators;

namespace zedbank.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly Context _context;
        private readonly ILogger _logger;
        
        public WalletController(Context context, ILogger<WalletController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpPost, Route("{id}/fund"), Authorize]
        public async Task<ActionResult<Object>> FundWallet(long id, WalletTransactionDto walletTransactionDto)
        {
            var wallet = await WalletService.GetWallet(id, _context);
            if (wallet == null)
            {
                return NotFound(new { Detail = "resource was not found on this server" });
            }

            var user = await AuthService.GetAuthUser(User, _context);
            if (wallet.OwnerId != user.Id)
            {
                return StatusCode(403, new { Detail = "you are not authorized to make this request" });
            }
            
            var validator = new WalletTransactionValidator();
            var result = await validator.ValidateAsync(walletTransactionDto);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                await WalletService.FundWallet(wallet, walletTransactionDto, _context);
            }
            catch (TransactionException e)
            {
                _logger.LogError("an error occurred while attempting to fund wallet: {e}", e.ToString());
                return Problem("an error occurred");
            }

            return Ok(new { Detail = "wallet funding successful" });
        }
        
        [HttpPost, Route("{id}/withdraw"), Authorize]
        public async Task<ActionResult<Object>> WithdrawFromWallet(long id, WalletTransactionDto walletTransactionDto)
        {
            var wallet = await WalletService.GetWallet(id, _context);
            if (wallet == null)
            {
                return NotFound(new { Detail = "resource was not found on this server" });
            }

            var user = await AuthService.GetAuthUser(User, _context);
            if (wallet.OwnerId != user.Id)
            {
                return StatusCode(403, new { Detail = "you are not authorized to make this request" });
            }
            
            var validator = new WalletTransactionValidator();
            var result = await validator.ValidateAsync(walletTransactionDto);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                await WalletService.WithdrawFromWallet(wallet, walletTransactionDto, _context);
            }
            catch (TransactionOverdraftException)
            {
                return BadRequest(new { Detail = "amount greater than wallet balance" });
            }
            catch (TransactionException e)
            {
                _logger.LogError("an error occurred while attempting to withdraw from wallet: {e}", e.ToString());
                return Problem("an error occurred");
            }

            return Ok(new { Detail = "wallet withdrawal successful" });
        }

        [HttpGet, Route("{id}/transactions"), Authorize]
        public async Task<ActionResult<List<TransactionDto>>> GetWalletTransactionHistory(long id)
        {
            var wallet = await WalletService.GetWallet(id, _context);
            if (wallet == null)
            {
                return NotFound(new { Detail = "resource was not found on this server" });
            }

            var user = await AuthService.GetAuthUser(User, _context);
            if (wallet.OwnerId != user.Id)
            {
                return StatusCode(403, new { Detail = "you are not authorized to make this request" });
            }

            var transactions = await WalletService.GetWalletTransactions(wallet, _context);
            var dtos = transactions.Select(t => new TransactionDto(t)).ToList();
            return Ok(dtos);
        }
    }
}
