# Quickstart: Cloudflare Storage Integration

## Environment Variables

Set the following environment variables in your hosting platform (Azure, somee.com, etc):

### Required Configuration
- `Cloudflare:AccessKey` - Cloudflare R2 access key (S3-compatible API key)
- `Cloudflare:SecretKey` - Cloudflare R2 secret key (S3-compatible secret key)
- `Cloudflare:Bucket` - Cloudflare R2 bucket name for storing photos and thumbnails
- `Cloudflare:AccountId` - Cloudflare account ID (found in Cloudflare dashboard)

### Optional Configuration
- `Cloudflare:CdnDomain` - Custom CDN domain for faster content delivery (optional, will use direct R2 URLs if not set)

### Example Configuration

#### Azure App Service
1. Go to Configuration → Application settings
2. Add each setting as a new application setting:
   - Name: `Cloudflare:AccessKey`, Value: `your-access-key`
   - Name: `Cloudflare:SecretKey`, Value: `your-secret-key`
   - Name: `Cloudflare:Bucket`, Value: `photogallery-bucket`
   - Name: `Cloudflare:AccountId`, Value: `your-account-id`
   - Name: `Cloudflare:CdnDomain`, Value: `cdn.yourdomain.com` (optional)

#### Local Development (appsettings.json)

The Cloudflare configuration section is already included in the appsettings.json files. Simply update the values:

**HttpApi.Host/appsettings.Development.json** and **PublicWeb/appsettings.Development.json**:
```json
{
  "Cloudflare": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key", 
    "Bucket": "photogallery-dev",
    "AccountId": "your-account-id",
    "CdnDomain": "cdn-dev.yourdomain.com"
  }
}
```

⚠️ **Security Note**: The Development files contain placeholder values. Update them with your actual credentials for local development, but never commit real credentials to source control. Use environment variables or secure configuration for production.

## Getting Cloudflare R2 Credentials

1. **Create Cloudflare R2 Bucket**:
   - Log into Cloudflare dashboard
   - Go to R2 Object Storage
   - Create a new bucket (e.g., "photogallery-bucket")

2. **Generate API Tokens**:
   - Go to "Manage R2 API tokens"
   - Create token with "Object Read & Write" permissions
   - Copy the Access Key ID and Secret Access Key

3. **Find Account ID**:
   - Account ID is visible in the right sidebar of your Cloudflare dashboard

## Migration Steps

1. **Backup existing photos** (if migrating from local storage):
   ```bash
   # Example backup command
   tar -czf photos-backup.tar.gz wwwroot/uploads/photos/
   ```

2. **Configure Cloudflare credentials** as environment variables.

3. **Deploy updated application** with new PhotoStorageService.

4. **Verify integration**:
   - Test photo upload functionality
   - Check that photos are accessible via CDN URLs
   - Confirm thumbnails are generated correctly

5. **Clean up local files** (after successful migration):
   - Remove wwwroot/uploads/photos/ directory
   - Update any hardcoded local file references

## Troubleshooting

### Common Issues

**Connection Errors**: 
- Verify AccessKey, SecretKey, and AccountId are correct
- Check bucket exists and is in the correct account

**Upload Failures**:
- Ensure bucket permissions allow writes
- Verify file size limits (10MB default)
- Check network connectivity to Cloudflare

**CDN Issues**:
- Verify CdnDomain is configured correctly
- Check DNS settings for custom domain
- Allow time for CDN propagation (up to 24 hours)

### Health Check
```bash
# Test Cloudflare R2 connectivity
curl -X GET "https://{account-id}.r2.cloudflarestorage.com/{bucket-name}/"
```

Refer to your platform's documentation for setting environment variables securely.
