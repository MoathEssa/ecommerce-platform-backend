using System.Net;
using ECommerceCenter.Application.Common.ApiResponse;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class AppController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponseHandler.Success(result.Value!));

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound =>
                NotFound(ApiResponseHandler.NotFound<T>(result.Error.Message)),
            HttpStatusCode.Conflict =>
                Conflict(ApiResponseHandler.Conflict<T>(result.Error.Message)),
            HttpStatusCode.BadRequest =>
                BadRequest(ApiResponseHandler.BadRequest<T>(result.Error.Message)),
            HttpStatusCode.Unauthorized =>
                Unauthorized(ApiResponseHandler.Unauthorized<T>(result.Error.Message)),
            HttpStatusCode.Forbidden =>
                StatusCode(403, ApiResponseHandler.Forbidden<T>(result.Error.Message)),
            HttpStatusCode.UnprocessableEntity =>
                UnprocessableEntity(ApiResponseHandler.BadRequest<T>(result.Error.Message)),
            _ =>
                StatusCode(500, ApiResponseHandler.InternalServerError<T>())
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponseHandler.Success<string>(message: "Operation completed successfully."));

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound =>
                NotFound(ApiResponseHandler.NotFound<string>(result.Error.Message)),
            HttpStatusCode.Conflict =>
                Conflict(ApiResponseHandler.Conflict<string>(result.Error.Message)),
            HttpStatusCode.BadRequest =>
                BadRequest(ApiResponseHandler.BadRequest<string>(result.Error.Message)),
            HttpStatusCode.Unauthorized =>
                Unauthorized(ApiResponseHandler.Unauthorized<string>(result.Error.Message)),
            HttpStatusCode.Forbidden =>
                StatusCode(403, ApiResponseHandler.Forbidden<string>(result.Error.Message)),
            HttpStatusCode.UnprocessableEntity =>
                UnprocessableEntity(ApiResponseHandler.BadRequest<string>(result.Error.Message)),
            _ =>
                StatusCode(500, ApiResponseHandler.InternalServerError<string>())
        };
    }
}
