namespace HireMindsAPI.Middleware
{
    // This middleware catches ALL unhandled exceptions in one place.
    // Instead of writing try-catch in every controller, this handles everything.
    public class GlobalExceptionMiddleware
    {
        // _next is the next middleware in the pipeline
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // This method runs for EVERY HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Try to pass the request to the next middleware/controller
                await _next(context);
            }
            catch (Exception ex)
            {
                // If any exception is thrown, we catch it here
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

                // Set response type to JSON
                context.Response.ContentType = "application/json";

                // Map exception type to HTTP status code
                int statusCode;
                string message;

                switch (ex)
                {
                    case KeyNotFoundException:
                        // Resource not found (e.g., Job with ID 999 doesn't exist)
                        statusCode = 404;
                        message = ex.Message;
                        break;

                    case ArgumentException:
                        // Bad input (e.g., wrong degree for a job application)
                        statusCode = 400;
                        message = ex.Message;
                        break;

                    case InvalidOperationException:
                        // Invalid action (e.g., applying to same job twice)
                        statusCode = 400;
                        message = ex.Message;
                        break;

                    case UnauthorizedAccessException:
                        // Not allowed to access this resource
                        statusCode = 401;
                        message = "Unauthorized access.";
                        break;

                    default:
                        // Any other error — don't expose internal details
                        statusCode = 500;
                        message = "DEBUG ERROR: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "");
                        break;
                }

                // Set the HTTP status code
                context.Response.StatusCode = statusCode;

                // Create a clean error response object
                var errorResponse = new
                {
                    StatusCode = statusCode,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                // Write the error as JSON to the response
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
