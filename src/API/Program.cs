// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

// Use the slim builder so we can publish our API with native AoT.
var builder = WebApplication.CreateSlimBuilder(args);

// Add services to generate OpenAPI documents.
builder.Services.AddOpenApi();

// Add TimeProvider for getting the current time in our API endpoint.
builder.Services.AddSingleton(TimeProvider.System);

// Configure the JSON serializer to use the JSON source generator
// and pretty-print JSON responses for readability.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.WriteIndented = true;
});

// Build our application pipeline.
var app = builder.Build();

var api = app.MapGroup("api");

// Define our Minimal API endpoint that returns the current date and time in various formats.
api.MapGet("time", (TimeProvider timeProvider) =>
{
    var utcNow = timeProvider.GetUtcNow();

    var time = new CurrentTime(
        utcNow,
        utcNow.ToString("r"),
        utcNow.ToUnixTimeSeconds(),
        utcNow.UtcDateTime.ToString("u"),
        utcNow.UtcDateTime.ToString("U"));

    return TypedResults.Ok(time);
});

// Map endpoint to return the OpenAPI document for our API.
app.MapOpenApi();

// Add SwaggerUI using Swashbuckle.AspNetCore to view our OpenAPI document at the root of the application.
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "";
    options.SwaggerEndpoint("openapi/v1.json", "Time API");
});

// Run the API!
app.Run();

// Define the API response for our API endpoint.

/// <summary>
/// Represents the current date and time.
/// </summary>
/// <param name="Timestamp">The timestamp for the response for which the times are generated.</param>
/// <param name="Rfc1123">The current UTC date and time in RFC1123 format.</param>
/// <param name="UnixSeconds">The number of seconds since the UNIX epoch.</param>
/// <param name="UniversalSortable">The current UTC date and time in universal sortable format.</param>
/// <param name="UniversalFull">The current UTC date and time in universal full format.</param>
public record CurrentTime(
    DateTimeOffset Timestamp,
    string Rfc1123,
    long UnixSeconds,
    string UniversalSortable,
    string UniversalFull);

// Define the JSON source generator context for our API models.
[JsonSerializable(typeof(CurrentTime))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
