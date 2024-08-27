namespace Demo.PL.Helper
{
    public class FileSettings
    {
        public static async Task<(string fileName, string imagePath)> AddOrUpdateFile(IFormFile file, string folderPath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var imagesFolderPath = Path.Combine(path, folderPath);

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var fileName = $"{Guid.NewGuid().ToString()}{file.FileName}";
            var imagePath = Path.Combine(imagesFolderPath, fileName); 

            await using (var fs = new FileStream(imagePath, FileMode.Create))
               await file.CopyToAsync(fs);

            var APath = Path.Combine("wwwroot", "Images",folderPath,fileName);
            return (fileName, APath);

        }

        public static  void DeleteFile(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                
            }
        }
    }
}
