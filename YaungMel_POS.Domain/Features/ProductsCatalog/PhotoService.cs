using CloudinaryDotNet;
using CloudinaryDotNet.Actions;


namespace YaungMel_POS.Domain.Features.ProductsCatalog
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ImageUploadResult> UploadPhotoAsync(Stream photoStream, string fileName)
        {
            try
            {
                if (photoStream == null)
                {
                    return new ImageUploadResult
                    {
                        Error = new Error { Message = "Photo stream is required." }
                    };
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return new ImageUploadResult
                    {
                        Error = new Error { Message = "File name is required for photo upload." }
                    };
                }

                long maxFileSize = 5 * 1024 * 1024; // 5 Megabytes
                
                if (photoStream.CanSeek && photoStream.Length > maxFileSize)
                { 
                    return new ImageUploadResult
                    {
                            Error = new Error { Message = "File size exceeds the 5MB limit. Upload rejected." }
                    };
                }
                

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, photoStream),
                    //Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "yaungmel_pos_product_photos"
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Photo upload error: {ex}");
                return new ImageUploadResult
                {
                    Error = new Error { Message = $"Internal error during photo upload: {ex.Message}" }
                };
            }
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result;
        }
    }
}
