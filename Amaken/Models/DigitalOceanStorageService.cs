using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Amazon.S3.Model;

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

    public async Task<string> UploadImageAsync(Stream stream, string contentType, string fileName)
    {
        try
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream is null or empty.");
            }

            var fileTransferUtility = new TransferUtility(_s3Client);

            await fileTransferUtility.UploadAsync(stream, _bucketName, fileName);

            var aclRequest = new PutACLRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutACLAsync(aclRequest);

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
