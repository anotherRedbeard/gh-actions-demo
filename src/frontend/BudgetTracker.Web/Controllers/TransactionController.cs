using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Web.Models;
using BudgetTracker.Web.Services;

namespace BudgetTracker.Web.Controllers;

public class TransactionController : Controller
{
    private readonly BudgetApiClient _apiClient;

    public TransactionController(BudgetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _apiClient.GetAsync<List<Transaction>>("transactions") ?? new List<Transaction>();
        return View(transactions);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            await _apiClient.PostAsync<Transaction>("transactions", transaction);
            return RedirectToAction(nameof(Index));
        }
        return View(transaction);
    }
}
