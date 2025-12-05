using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Web.Services;
using BudgetTracker.Web.Models;

namespace BudgetTracker.Web.Controllers;

public class DashboardController : Controller
{
    private readonly BudgetApiClient _apiClient;

    public DashboardController(BudgetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var budgets = await _apiClient.GetAsync<List<Budget>>("budgets");
        var transactions = await _apiClient.GetAsync<List<Transaction>>("transactions");
        var savingsGoals = await _apiClient.GetAsync<List<SavingsGoal>>("savings-goals");

        ViewBag.Budget = budgets?.FirstOrDefault();
        ViewBag.Transactions = transactions?.Take(10).ToList();
        ViewBag.SavingsGoals = savingsGoals;

        return View();
    }
}
