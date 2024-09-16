using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RedshiftDataGeneratator.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        public DataController()
        {
                
        }


        [HttpGet("getredshiftdata")]
        public IActionResult GetRedshiftData()
        {
            using var bitmap = new Bitmap(1920, 1080);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
            }

            // Convert the bitmap to a byte array
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            var imageBytes = stream.ToArray();

            // Convert the byte array to a Base64 string
            var base64String = Convert.ToBase64String(imageBytes);

            // Return the Base64 string as a response
            return Ok(base64String);
        }


        [HttpPut("MoveMousePointer")]
        public IActionResult MoveMousePointer(int x, int y)
        {
            try
            {
                // Move the mouse pointer to the specified coordinates
                bool result = SetCursorPos(x, y);

                if (result)
                {
                    return Ok("Mouse pointer moved successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to move mouse pointer.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and return a meaningful error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
