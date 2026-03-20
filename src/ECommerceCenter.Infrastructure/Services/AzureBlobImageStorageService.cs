using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace ECommerceCenter.Infrastructure.Services;

public sealed class AzureBlobImageStorageService : IImageStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly StorageSharedKeyCredential _sharedKeyCredential;
    private readonly string _accountBaseUrl;
    private readonly IHttpClientFactory _httpClientFactory;

    public AzureBlobImageStorageService(
        IOptions<BlobStorageSettings> options,
        IHttpClientFactory httpClientFactory)
    {
        var s = options.Value;
        _sharedKeyCredential = new StorageSharedKeyCredential(s.AccountName, s.AccountKey);

        var serviceUri = new Uri($"https://{s.AccountName}.blob.core.windows.net");
        _serviceClient      = new BlobServiceClient(serviceUri, _sharedKeyCredential);
        _accountBaseUrl     = $"https://{s.AccountName}.blob.core.windows.net";
        _httpClientFactory  = httpClientFactory;
    }

    /// <inheritdoc/>
    public async Task<string> UploadImageAsync(
        Stream content, string folder, string fileName, string contentType,
        CancellationToken ct = default)
    {
        var containerName = folder.TrimEnd('/');
        var blobName      = $"{Guid.NewGuid():N}-{SanitizeFileName(fileName)}";
        var blobClient    = _serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

        // Generate a short-lived write SAS (15 min) — never leaves the server
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName          = blobName,
            Resource          = "b",
            ExpiresOn         = DateTimeOffset.UtcNow.AddMinutes(15),
            ContentType       = contentType,
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var uploadUrl = $"{blobClient.Uri}?{sasBuilder.ToSasQueryParameters(_sharedKeyCredential)}";

        // Stream the bytes from the API directly to Azure Storage
        using var http    = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
        {
            Content = new StreamContent(content),
        };
        request.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        request.Headers.Add("x-ms-blob-type", "BlockBlob");

        var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        // Container is public — return the permanent plain URL (no SAS)
        return $"{_accountBaseUrl}/{containerName}/{blobName}";
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        // URL format: https://account.blob.core.windows.net/{container}/{blobname}
        var path  = new Uri(url).AbsolutePath.TrimStart('/');
        var slash = path.IndexOf('/');
        if (slash < 0) return;

        var containerName = path[..slash];
        var blobName      = path[(slash + 1)..];
        if (string.IsNullOrWhiteSpace(blobName)) return;

        var blobClient = _serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized    = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "image" : sanitized;
    }
}
