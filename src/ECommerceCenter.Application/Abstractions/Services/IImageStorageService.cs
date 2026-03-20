namespace ECommerceCenter.Application.Abstractions.Services;

public interface IImageStorageService
{
    /// <summary>
    /// Streams <paramref name="content"/> to Azure Blob Storage (SAS generated internally)
    /// and returns the permanent, public blob URL to store in the database.
    /// </summary>
    /// <param name="content">The image stream.</param>
    /// <param name="folder">Container / folder name (e.g. "products", "categories").</param>
    /// <param name="fileName">Original file name — a GUID prefix is added internally.</param>
    /// <param name="contentType">MIME type (e.g. "image/jpeg").</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> UploadImageAsync(
        Stream content, string folder, string fileName, string contentType,
        CancellationToken ct = default);

    /// <summary>Deletes the blob at the given URL. Gracefully ignores missing blobs.</summary>
    Task DeleteAsync(string url, CancellationToken ct = default);
}
