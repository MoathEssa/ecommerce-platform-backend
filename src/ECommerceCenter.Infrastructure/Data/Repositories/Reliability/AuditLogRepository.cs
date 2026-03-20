using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Domain.Entities.Reliability;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Reliability;

public class AuditLogRepository(AppDbContext context)
    : GenericRepository<AuditLog>(context), IAuditLogRepository
{
}
