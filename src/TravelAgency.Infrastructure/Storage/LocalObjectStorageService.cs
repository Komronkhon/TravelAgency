using Microsoft.Extensions.Configuration;
using TravelAgency.Application.Interfaces;

namespace TravelAgency.Infrastructure.Storage;

public sealed class LocalObjectStorageService : IFileStorageService
{
    private readonly string _bucketRootPath;

    public LocalObjectStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["StorageSettings:BucketPath"] ?? "App_Data/DocubinetBucket";
        _bucketRootPath = Path.GetFullPath(configuredPath);
        Directory.CreateDirectory(_bucketRootPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string storageKey, string contentType, CancellationToken ct = default)
    {
        var fullPath = GetSafePhysicalPath(storageKey);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        // Потоковая запись с асинхронным буфером 64 КБ без загрузки в LOH
        await using var outputStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 65536,
            useAsync: true);

        await fileStream.CopyToAsync(outputStream, ct);
        return storageKey;
    }

    public Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = GetSafePhysicalPath(storageKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Объект с ключом '{storageKey}' не найден в бакете хранилища.");
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 65536,
            useAsync: true);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = GetSafePhysicalPath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    private string GetSafePhysicalPath(string storageKey)
    {
        var relativePath = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_bucketRootPath, relativePath));

        // Защита от уязвимости Path Traversal (../../)
        if (!fullPath.StartsWith(_bucketRootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Недопустимый ключ объекта хранилища.");
        }

        return fullPath;
    }
}