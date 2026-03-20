using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjCategory;

/// <summary>
/// Represents one node in the subtree to recursively import alongside the parent.
/// Children can be nested to any depth.
/// </summary>
public record ImportCjSubtreeItem(
    string Id,
    string Name,
    List<ImportCjSubtreeItem> Children);

/// <summary>
/// Imports a CJDropshipping category into the local store catalogue atomically.
/// <list type="bullet">
///   <item>TargetLevel = 1 — import L1 root and all descendants in <c>SubTree</c>.</item>
///   <item>TargetLevel = 2 — optionally find-or-create L1 parent (<c>IncludeChain</c>),
///     then import L2 and all descendants in <c>SubTree</c>.</item>
///   <item>TargetLevel = 3 (default) — existing leaf-only behaviour.</item>
/// </list>
/// Returns the created root category ID.
/// </summary>
public record ImportCjCategoryCommand(
    string LeafId,
    string LeafName,
    string? Level1Id,
    string Level1Name,
    string? Level2Id,
    string Level2Name,
    bool IncludeChain,
    List<ImportCjSubtreeItem>? SubTree = null,
    bool MakeActive = false,
    /// <summary>Which CJ level is being imported: 1 = L1, 2 = L2, 3 = L3 (default).</summary>
    int TargetLevel = 3) : IRequest<Result<int>>;
