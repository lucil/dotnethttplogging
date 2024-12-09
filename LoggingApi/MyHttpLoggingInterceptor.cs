using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.HttpLogging;

namespace LoggingApi
{
    internal sealed class MyHttpLoggingInterceptor : IHttpLoggingInterceptor
    {
        public async ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
        {
            // reading body in post
            if (logContext.HttpContext.Request.Method == "POST")
            {
                var requestBody = await ReadRequestBodyAsync(logContext.HttpContext.Request);
                var entityId = ExtractEntityId(requestBody);
                if (entityId != null)
                {
                    logContext.AddParameter("EntityId", entityId);
                }
            }
            else
            {
                var entityId = ExtractEntityIdFromQueryOrRoute(logContext.HttpContext.Request);

                if (entityId != null)
                {
                    logContext.AddParameter("EntityId", entityId);
                }
            }

            return;
        }

        public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
        {
            return default;
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            // Enable buffering so the stream can be read multiple times.
            request.EnableBuffering();

            // Read the stream as a string.
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();

                // Reset the stream position to allow further reads.
                request.Body.Position = 0;

                return body;
            }
        }

        private string? ExtractEntityId(string requestBody)
        {
            try
            {
                // Parse the JSON string into an anonymous object
                var jsonDocument = JsonDocument.Parse(requestBody);

                // Extract "entityId" from the JSON
                if (jsonDocument.RootElement.TryGetProperty("entityId", out var entityIdProperty))
                {
                    return entityIdProperty.GetString();
                }
            }
            catch (JsonException)
            {
                return "No entityId found in request";
            }

            return null;
        }
        private string? ExtractEntityIdFromQueryOrRoute(HttpRequest request)
        {
            // Check query string first
            if (request.Query.TryGetValue("entityId", out var queryValue))
            {
                return queryValue.ToString();
            }

            // Check route parameters
            if (request.RouteValues.TryGetValue("entityId", out var routeValue))
            {
                return routeValue?.ToString();
            }

            return null;
        }

    }
}