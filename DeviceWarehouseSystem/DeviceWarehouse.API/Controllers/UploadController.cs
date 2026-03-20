using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace DeviceWarehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "没有上传文件" });
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                var contentType = file.ContentType;

                return Ok(new { 
                    success = true, 
                    data = new { 
                        imageData = System.Convert.ToBase64String(imageData),
                        contentType = contentType
                    } 
                });
            }
        }
    }
}