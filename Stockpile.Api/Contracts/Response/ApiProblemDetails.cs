using Microsoft.AspNetCore.Mvc;

namespace Stockpile.Api.Contracts.Response;

public static class ApiProblemDetails
{
    public static ProblemDetails Unauthorized(string detail = "Authentication is required.") =>
        Create(StatusCodes.Status401Unauthorized, "Unauthorized", detail);

    public static ProblemDetails Forbidden(string detail = "You do not have access to this resource.") =>
        Create(StatusCodes.Status403Forbidden, "Forbidden", detail);

    public static ProblemDetails NotFound(string detail = "The requested resource was not found.") =>
        Create(StatusCodes.Status404NotFound, "Not Found", detail);

    public static ProblemDetails BadRequest(string detail = "The request is invalid.") =>
        Create(StatusCodes.Status400BadRequest, "Bad Request", detail);

    private static ProblemDetails Create(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
