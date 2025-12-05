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

public class SavingsGoalFunctions
{
    private readonly ILogger<SavingsGoalFunctions> _logger;
    private readonly DataService _dataService;

    public SavingsGoalFunctions(ILogger<SavingsGoalFunctions> logger, DataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("GetSavingsGoals")]
    [OpenApiOperation(operationId: "GetSavingsGoals", tags: new[] { "SavingsGoal" }, Summary = "Get all savings goals")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<SavingsGoal>), Description = "List of all savings goals")]
    public IActionResult GetSavingsGoals(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "savings-goals")] HttpRequest req)
    {
        _logger.LogInformation("Getting all savings goals");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var goals = _dataService.GetSavingsGoals();
        return new OkObjectResult(goals);
    }

    [Function("GetSavingsGoal")]
    [OpenApiOperation(operationId: "GetSavingsGoal", tags: new[] { "SavingsGoal" }, Summary = "Get savings goal by ID")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Savings goal ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SavingsGoal), Description = "Savings goal found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Savings goal not found")]
    public IActionResult GetSavingsGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "savings-goals/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation($"Getting savings goal with id: {id}");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var goal = _dataService.GetSavingsGoal(id);
        
        if (goal == null)
            return new NotFoundResult();
            
        return new OkObjectResult(goal);
    }

    [Function("CreateSavingsGoal")]
    [OpenApiOperation(operationId: "CreateSavingsGoal", tags: new[] { "SavingsGoal" }, Summary = "Create a new savings goal")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SavingsGoal), Required = true, Description = "Savings goal to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(SavingsGoal), Description = "Savings goal created successfully")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Invalid savings goal data")]
    public async Task<IActionResult> CreateSavingsGoal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "savings-goals")] HttpRequest req)
    {
        if (req.Method == "OPTIONS")
        {
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "*");
            return new OkResult();
        }
        
        _logger.LogInformation("Creating new savings goal");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var goal = await req.ReadFromJsonAsync<SavingsGoal>();
        if (goal == null)
            return new BadRequestObjectResult("Invalid savings goal data");
            
        _dataService.AddSavingsGoal(goal);
        return new CreatedResult($"/api/savings-goals/{goal.Id}", goal);
    }
}
