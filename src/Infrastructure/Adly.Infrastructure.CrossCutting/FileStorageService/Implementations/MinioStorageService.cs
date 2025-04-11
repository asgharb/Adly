using Adly.Application.Contracts.FileService.Interfaces;
using Adly.Application.Contracts.FileService.Models;
using FileTypeChecker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.DataModel.Args;

namespace Adly.Infrastructure.CrossCutting.FileStorageService.Implementations;

internal class MinioStorageService(
    IMinioClient minioClient,
    [FromKeyedServices("SasMinioClient")] IMinioClient sasClient) : IFileService
{
    private const string AdlyBucketName = "adlyfiles";


    private async Task CreateBucketIfMissing(CancellationToken cancellationToken = default)
    {
        var checkBucketExistsArg = new BucketExistsArgs().WithBucket(AdlyBucketName);

        if (await minioClient.BucketExistsAsync(checkBucketExistsArg, cancellationToken))
            return;


        var createBucketArg = new MakeBucketArgs().WithBucket(AdlyBucketName);

        await minioClient.MakeBucketAsync(createBucketArg, cancellationToken);
    }

    public async Task<List<SaveFileModelResult>> SaveFilesAsync(List<SaveFileModel> files,
        CancellationToken cancellationToken = default)
    {
        await CreateBucketIfMissing(cancellationToken);

        var result = new List<SaveFileModelResult>();

        foreach (var file in files)
        {
            await using var memoryStream = new MemoryStream(Convert.FromBase64String(file.Base64File));

            var fileName = $"{Guid.NewGuid():N}.{FileTypeValidator.GetFileType(memoryStream).Extension}";

            var fileType = !string.IsNullOrEmpty(file.FileContent) ? file.FileContent : "application/octet-stream";

            memoryStream.Position = 0;

            var createFileArgs = new PutObjectArgs()
                .WithBucket(AdlyBucketName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithObject(fileName)
                .WithContentType(
                    fileType);

            await minioClient.PutObjectAsync(createFileArgs, cancellationToken);

            result.Add(new(fileName, fileType));
        }

        return result;
    }

    public async Task<List<GetFileModel>> GetFilesByNameAsync(List<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        await CreateBucketIfMissing(cancellationToken);

        var result = new List<GetFileModel>();


        foreach (var fileName in fileNames)
        {
            var objectInfo = new StatObjectArgs()
                .WithBucket(AdlyBucketName)
                .WithObject(fileName);

            var objectInfoResult = await minioClient.StatObjectAsync(objectInfo, cancellationToken);

            if (objectInfoResult is null)
                continue;

            var sasUrlArgs = new PresignedGetObjectArgs()
                .WithBucket(AdlyBucketName)
                .WithObject(fileName)
                .WithExpiry((int)TimeSpan.FromMinutes(30).TotalSeconds);

            var fileUrl = await sasClient.PresignedGetObjectAsync(sasUrlArgs);

            result.Add(new(fileUrl, objectInfoResult.ContentType, objectInfoResult.ObjectName));
        }

        return result;
    }

    public async Task RemoveFilesAsync(string[] fileNames, CancellationToken cancellationToken = default)
    {
        await CreateBucketIfMissing(cancellationToken);


        foreach (var fileName in fileNames)
        {
            var objectInfo = new StatObjectArgs()
                .WithBucket(AdlyBucketName)
                .WithObject(fileName);

            var objectInfoResult = await minioClient.StatObjectAsync(objectInfo, cancellationToken);

            if (objectInfoResult is null)
                continue;

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(AdlyBucketName)
                .WithObject(objectInfoResult.ObjectName);

            await minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
        }
    }
}