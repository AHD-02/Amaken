using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class DigitalOceanStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "amaken-images";
    public DigitalOceanStorageService(IOptions<DigitalOceanSpacesSettings> settings,IConfiguration configuration)
    {
        string accessKey = configuration["DigitalOceanSpaces:AccessKeyId"]!;
        string secretKey = configuration["DigitalOceanSpaces:SecretAccessKey"]!;

        AmazonS3Config config = new AmazonS3Config();
        config.ServiceURL = "https://fra1.digitaloceanspaces.com";

        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            config
        );
        
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.");
            }

            using var fileStream = file.OpenReadStream();

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var fileTransferUtility = new TransferUtility(_s3Client);

            await fileTransferUtility.UploadAsync(fileStream, _bucketName, fileName);

            return fileName;
        }
        catch (Exception ex)
        {
            throw new Exception("Error uploading file to DigitalOcean Spaces.", ex);
        }
    }
}

public class DigitalOceanSpacesSettings
{
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? BucketName { get; set; } 
    public string? Region { get; set; }
    public string? EndpointUrl { get; set; }
}
