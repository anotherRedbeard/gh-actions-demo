using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Web.Models;
using BudgetTracker.Web.Services;

namespace BudgetTracker.Web.Controllers;

public class BudgetController : Controller
{
    private readonly BudgetApiClient _apiClient;

    public BudgetController(BudgetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var budgets = await _apiClient.GetAsync<List<Budget>>("budgets") ?? new List<Budget>();
        return View(budgets);
    }

    public async Task<IActionResult> Details(string id)
    {
        var budget = await _apiClient.GetAsync<Budget>($"budgets/{id}");
        if (budget == null)
            return NotFound();
            
        return View(budget);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Budget budget)
    {
        if (ModelState.IsValid)
        {
            await _apiClient.PostAsync<Budget>("budgets", budget);
            return RedirectToAction(nameof(Index));
        }
        return View(budget);
    }
}
