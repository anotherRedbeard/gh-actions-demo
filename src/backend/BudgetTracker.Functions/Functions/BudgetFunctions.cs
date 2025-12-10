using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using BudgetTracker.Functions.Models;
using BudgetTracker.Functions.Services;
using System.Net;

namespace BudgetTracker.Functions;

public class BudgetFunctions
{
    private readonly ILogger<BudgetFunctions> _logger;
    private readonly DataService _dataService;

    public BudgetFunctions(ILogger<BudgetFunctions> logger, DataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("GetBudgets")]
    [OpenApiOperation(operationId: "GetBudgets", tags: new[] { "Budget" }, Summary = "Get all budgets")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Budget>), Description = "List of all budgets")]
    public IActionResult GetBudgets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "budgets")] HttpRequest req)
    {
        _logger.LogInformation("Getting all budgets, from source");
        
        // Add CORS headers
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        
        var budgets = _dataService.GetBudgets();
        return new OkObjectResult(budgets);
    }

    [Function("GetBudget")]
    [OpenApiOperation(operationId: "GetBudget", tags: new[] { "Budget" }, Summary = "Get budget by ID")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Budget ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Budget), Description = "Budget found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Budget not found")]
    public IActionResult GetBudget(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "budgets/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation($"Getting budget with id: {id}");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var budget = _dataService.GetBudget(id);
        
        if (budget == null)
            return new NotFoundResult();
            
        return new OkObjectResult(budget);
    }

    [Function("CreateBudget")]
    [OpenApiOperation(operationId: "CreateBudget", tags: new[] { "Budget" }, Summary = "Create a new budget")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Budget), Required = true, Description = "Budget to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(Budget), Description = "Budget created successfully")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Invalid budget data")]
    public async Task<IActionResult> CreateBudget(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "budgets")] HttpRequest req)
    {
        // Handle CORS preflight
        if (req.Method == "OPTIONS")
        {
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "*");
            return new OkResult();
        }
        
        _logger.LogInformation("Creating new budget");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var budget = await req.ReadFromJsonAsync<Budget>();
        if (budget == null)
            return new BadRequestObjectResult("Invalid budget data");
            
        _dataService.AddBudget(budget);
        return new CreatedResult($"/api/budgets/{budget.Id}", budget);
    }
}
