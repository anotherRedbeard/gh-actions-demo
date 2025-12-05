using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Web.Models;
using BudgetTracker.Web.Services;

namespace BudgetTracker.Web.Controllers;

public class SavingsGoalController : Controller
{
    private readonly BudgetApiClient _apiClient;

    public SavingsGoalController(BudgetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var goals = await _apiClient.GetAsync<List<SavingsGoal>>("savings-goals") ?? new List<SavingsGoal>();
        return View(goals);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(SavingsGoal goal)
    {
        if (ModelState.IsValid)
        {
            await _apiClient.PostAsync<SavingsGoal>("savings-goals", goal);
            return RedirectToAction(nameof(Index));
        }
        return View(goal);
    }
}
