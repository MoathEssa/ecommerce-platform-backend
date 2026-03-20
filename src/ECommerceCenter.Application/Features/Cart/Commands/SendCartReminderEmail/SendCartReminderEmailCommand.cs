using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.SendCartReminderEmail;

public record SendCartReminderEmailCommand(
    string ToEmail,
    string Subject,
    string Body) : IRequest<Result>;
