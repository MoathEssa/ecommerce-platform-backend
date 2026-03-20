using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.SendCartReminderEmail;

public class SendCartReminderEmailCommandHandler(IEmailService emailService)
    : IRequestHandler<SendCartReminderEmailCommand, Result>
{
    public async Task<Result> Handle(
        SendCartReminderEmailCommand request,
        CancellationToken cancellationToken)
    {
        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #333;">Cart Reminder</h2>
                <div style="padding: 16px; background: #f9f9f9; border-radius: 8px; margin: 16px 0;">
                    {System.Net.WebUtility.HtmlEncode(request.Body).Replace("\n", "<br/>")}
                </div>
                <p style="color: #666; font-size: 12px;">This is an automated message from our store.</p>
            </div>
            """;

        await emailService.SendAsync(request.ToEmail, request.Subject, htmlBody, cancellationToken);

        return Result.Success();
    }
}
