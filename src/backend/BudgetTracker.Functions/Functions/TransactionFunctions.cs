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

public class TransactionFunctions
{
    private readonly ILogger<TransactionFunctions> _logger;
    private readonly DataService _dataService;

    public TransactionFunctions(ILogger<TransactionFunctions> logger, DataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("GetTransactions")]
    [OpenApiOperation(operationId: "GetTransactions", tags: new[] { "Transaction" }, Summary = "Get all transactions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Transaction>), Description = "List of all transactions")]
    public IActionResult GetTransactions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transactions")] HttpRequest req)
    {
        _logger.LogInformation("Getting all transactions");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var transactions = _dataService.GetTransactions();
        return new OkObjectResult(transactions);
    }

    [Function("GetTransaction")]
    [OpenApiOperation(operationId: "GetTransaction", tags: new[] { "Transaction" }, Summary = "Get transaction by ID")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Transaction ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Transaction), Description = "Transaction found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Transaction not found")]
    public IActionResult GetTransaction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transactions/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation($"Getting transaction with id: {id}");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var transaction = _dataService.GetTransaction(id);
        
        if (transaction == null)
            return new NotFoundResult();
            
        return new OkObjectResult(transaction);
    }

    [Function("CreateTransaction")]
    [OpenApiOperation(operationId: "CreateTransaction", tags: new[] { "Transaction" }, Summary = "Create a new transaction")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Transaction), Required = true, Description = "Transaction to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(Transaction), Description = "Transaction created successfully")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Invalid transaction data")]
    public async Task<IActionResult> CreateTransaction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "transactions")] HttpRequest req)
    {
        if (req.Method == "OPTIONS")
        {
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "*");
            return new OkResult();
        }
        
        _logger.LogInformation("Creating new transaction");
        
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        var transaction = await req.ReadFromJsonAsync<Transaction>();
        if (transaction == null)
            return new BadRequestObjectResult("Invalid transaction data");
            
        _dataService.AddTransaction(transaction);
        return new CreatedResult($"/api/transactions/{transaction.Id}", transaction);
    }
}
