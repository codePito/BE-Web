//using Microsoft.AspNetCore.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using System.Net;
//using System.Text.Json;

//namespace WebApp.Controller.Controllers.Middleware
//{
//    public class GlobalExceptionHandler : IExceptionHandler
//    {
//        private readonly ILogger<GlobalExceptionHandler> _logger;
//        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
//        {
//            _logger = logger;
//        }
//        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
//        {
//            _logger.LogError(exception, "An unhandled exception occurred {Message}", exception.Message);

//            var problemDetails = new ProblemDetails
//            {
//                Instance = httpContext.Request.Path,
//            };

//            switch (exception)
//            {
//                case ArgumentException argEx:
//                    problemDetails.Title = "Invalid Argument";
//                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
//                    problemDetails.Detail = argEx.Message;
//                    break;

//                case UnauthorizedAccessException unauthEx:
//                    problemDetails.Title = "Unauthorized";
//                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
//                    problemDetails.Detail = unauthEx.Message;
//                    break;

//                case KeyNotFoundException _:
//                    problemDetails.Title = "Resource Not Found";
//                    problemDetails.Status = (int)HttpStatusCode.NotFound;
//                    problemDetails.Detail = "The requested resource was not found.";
//                    break;

//                case InvalidOperationException invalidOpEx:
//                    problemDetails.Title = "Invalid Operation";
//                    problemDetails.Status = (int)HttpStatusCode.Conflict;
//                    problemDetails.Detail = invalidOpEx.Message;
//                    break;

//                default:
//                    problemDetails.Title = "Internal Server Error";
//                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
//                    problemDetails.Detail = "An unexpected error occurred. Please try again later.";
//                    break;
//            }

//            httpContext.Response.StatusCode = problemDetails.Status.Value;
//            httpContext.Response.ContentType = "application/json";

//            var json = JsonSerializer.Serialize(problemDetails);
//            await httpContext.Response.WriteAsync(json, cancellationToken);

//            return true;
//        }
//    }
//}
