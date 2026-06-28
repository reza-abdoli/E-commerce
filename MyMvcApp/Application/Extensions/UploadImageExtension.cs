using Application.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Extensions
{
    public static class UploadImageExtension
    {
        public static bool AddImageToServer(this IFormFile image, string fileName, string originalPath, int? width, int? height, string thumbPath = null, string deleteFileName = null)
        {
            if (!image.IsImage()) return false;
            if (!Directory.Exists(originalPath))
                Directory.CreateDirectory(originalPath);

            if (!string.IsNullOrEmpty(deleteFileName))
            {
                if (File.Exists(originalPath + deleteFileName))
                    File.Delete(originalPath + deleteFileName);

                if (!string.IsNullOrEmpty(thumbPath))
                {
                    if (File.Exists(thumbPath + deleteFileName))
                        File.Delete(thumbPath + deleteFileName);
                }
            }

            var originPath = originalPath + fileName;

            using (var stream = new FileStream(originPath, FileMode.Create))
            {
                if (!Directory.Exists(originPath)) image.CopyTo(stream);
            }


            if (!string.IsNullOrEmpty(thumbPath))
            {
                if (!Directory.Exists(thumbPath))
                    Directory.CreateDirectory(thumbPath);

                var reSizer = new ImageOptimizer();

                if (width != null && height != null)
                    reSizer.ImageResizer(originalPath + fileName, thumbPath + fileName, width, height);
            }

            return true;

        }

        public static void DeleteImage(this string imageName, string originPath, string? thumbPath)
        {
            if (string.IsNullOrEmpty(imageName)) return;
            if (File.Exists(originPath + imageName))
                File.Delete(originPath + imageName);
            if (string.IsNullOrEmpty(thumbPath)) return;
            if (File.Exists(thumbPath + imageName))
                File.Delete(thumbPath + imageName);
        }
    }
}
