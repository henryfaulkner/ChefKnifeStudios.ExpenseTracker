using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.BL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ChefKnifeStudios.ExpenseTracker.Shared;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI.EndpointGroups;

public static class SemanticEndpoints
{
    public static IEndpointRouteBuilder MapSemanticEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/semantic")
            .WithName("Semantic");

        group.MapPost("/scan-receipt", async (
            HttpRequest request,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] ISemanticService semanticService,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(SemanticEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                // Read the uploaded file
                if (!request.Form.Files.Any())
                {
                    return Results.BadRequest("No file was uploaded.");
                }
                var formFile = request.Form.Files[0];
                if (formFile == null)
                {
                    return Results.BadRequest("File upload failed.");
                }

                using var stream = formFile.OpenReadStream();
                var resultData = await semanticService.ScanReceiptAsync(stream, Guid.Parse(appId), cancellationToken);
                return Results.Ok(resultData);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error: {Message}. AppId: {AppId}", ex.Message, appId);
                return Results.Problem(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred. AppId: {AppId}", appId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ScanReceipt")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<List<ReceiptDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/text-to-expense", async (
            HttpRequest request,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] ISemanticService semanticService,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(SemanticEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                string prompt;
                using (var reader = new StreamReader(request.Body))
                {
                    prompt = await reader.ReadToEndAsync();
                }

                var result = await semanticService.TextToExpenseAsync(prompt, Guid.Parse(appId), cancellationToken);
                return Results.Ok(result);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error: {Message}. AppId: {AppId}", ex.Message, appId);
                return Results.Problem(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred. AppId: {AppId}", appId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("TextToExpense")
        .Accepts<TextToExpenseRequestDTO>("application/json")
        .Produces<TextToExpenseResponseDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/label-receipt-details", async (
            HttpRequest request,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] ISemanticService semanticService,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(SemanticEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                // Read the receipt JSON input from the request body
                string prompt;
                using (var reader = new StreamReader(request.Body))
                {
                    prompt = await reader.ReadToEndAsync();
                }

                var result = await semanticService.LabelReceiptDetailsAsync(prompt, Guid.Parse(appId), cancellationToken);
                return Results.Ok(result);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error: {Message}. AppId: {AppId}", ex.Message, appId);
                return Results.Problem(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred. AppId: {AppId}", appId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("LabelReceiptJson")
        .Accepts<ReceiptDTO>("application/json")
        .Produces<ReceiptLabelsDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("semantic-embedding", async (
            HttpRequest request,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] ISemanticService semanticService,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(SemanticEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                string reqBody;
                using (var reader = new StreamReader(request.Body))
                {
                    reqBody = await reader.ReadToEndAsync();
                }
                var reqDTO = JsonSerializer.Deserialize<ReceiptLabelsDTO>(reqBody, Shared.JsonOptions.Get());
                if (reqDTO == null) throw new ApplicationException("Invalid request body");

                var result = await semanticService.CreateSemanticEmbeddingAsync(reqDTO, Guid.Parse(appId), cancellationToken);
                return Results.Ok(result);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error: {Message}. AppId: {AppId}", ex.Message, appId);
                return Results.Problem(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred. AppId: {AppId}", appId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateSemanticEmbedding")
        .Accepts<ReceiptLabelsDTO>("application/json")
        .Produces<SemanticEmbeddingDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("expense/search", async (
            HttpRequest request,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] ISemanticService semanticService,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(SemanticEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                string reqBody;
                using (var reader = new StreamReader(request.Body))
                {
                    reqBody = await reader.ReadToEndAsync();
                }
                var reqDTO = JsonSerializer.Deserialize<ExpenseSearchDTO>(reqBody, Shared.JsonOptions.Get());
                if (reqDTO is null) throw new ApplicationException("Invalid request body");

                var result = await semanticService.SearchExpensesAsync(reqDTO, Guid.Parse(appId), cancellationToken);
                return Results.Ok(result);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error: {Message}. AppId: {AppId}", ex.Message, appId);
                return Results.Problem(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred. AppId: {AppId}", appId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SearchExpenses")
        .Accepts<ExpenseSearchDTO>("application/json")
        .Produces<ExpenseSearchResponseDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}