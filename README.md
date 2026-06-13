# Secure File Upload Demo (.NET 10)
A production-ready ASP.NET Core Web API demonstrating a secure file upload pipeline. The project uses Cloudinary as a cloud storage provider to ensure uploaded files never touch the local application server's file system.
## Security Layers Implemented
1. **File Size Validation**: Restricts uploads to a maximum file size (default: 5MB) to prevent Denial of Service (DoS) attacks.
2. **Extension Whitelisting**: Validates file extensions against a set of allowed types (`.jpg`, `.jpeg`, `.png`, `.webp`).
3. **Magic Bytes (File Signature) Verification**: Inspects the initial bytes of the file stream to verify its actual content type, preventing extension-spoofing attacks (e.g., renaming a `.php` shell to `.jpg`).
4. **Safe Filename Generation**: Generates a unique GUID-based filename, completely discarding the original client-supplied filename to prevent Path Traversal attacks (e.g., payloads like `../../appsettings.json`).
5. **Direct Cloud Upload**: Streams files directly to Cloudinary. Since files are not written to the local web server, there is no threat of local execution.
## Getting Started
### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A [Cloudinary Account](https://cloudinary.com) (free tier is sufficient)
### Configuration
Configure your Cloudinary credentials in `appsettings.json`:
```json
{
  "CloudinarySettings": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  },
  "UploadSettings": {
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".webp" ],
    "MaxFileSizeInMB": 5
  }
}
```
### Running the Application
Restore dependencies and run the API:
```bash
dotnet run
```
### Testing the Endpoint
Make a `POST` request to `/api/upload/profile-picture` using a tool like Postman, curl, or HTTP files. The payload must be `multipart/form-data` with a key named `file`.
Example `curl` command:
```bash
curl -X POST -F "file=@/path/to/your/image.jpg" https://localhost:7001/api/upload/profile-picture
```
