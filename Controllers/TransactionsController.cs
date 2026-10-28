using Microsoft.AspNetCore.Mvc;
using BelajarAspNetMvc.Models;

namespace BelajarAspNetMvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var bankRecords = new List<Transaction>
        {
            new Deposit(100_000),
            new Withdrawal(25_000), 
            new Deposit(50_000),
            new Withdrawal(10_000),
        };

        var rows = new List<TransactionRow>();
        var currentBalance = 0m;

        foreach (var transaction in bankRecords)
        {
            currentBalance += transaction switch
            {
                Deposit d => d.Amount,
                Withdrawal w => -w.Amount,
                _=> 0m,
            };

            rows.Add(new TransactionRow(transaction, currentBalance));
        }

        return Ok(rows);
    }
}
