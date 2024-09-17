using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedshiftDataGeneratator.ViewModels;
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
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Importing keybd_event to simulate keypress
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000; // Key down flag
        private const int KEYEVENTF_KEYUP = 0x0002;   // Key up flag
        private const byte VK_MENU = 0x12;            // Alt key virtual key code
        private const byte VK_TAB = 0x09;             // Tab key virtual key code

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

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            var imageBytes = stream.ToArray();

            var base64String = Convert.ToBase64String(imageBytes);

            return Ok(base64String);
        }


        [HttpPut("putredshiftsched")]
        public IActionResult PutRedshiftSched(CoordinateVM coords)
        {
            try
            {
                bool result = SetCursorPos(coords.x, coords.y);

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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("ClickButton")]
        public IActionResult ClickButton()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);  
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);    

            return Ok("Button clicked successfully.");
        }

        [HttpPut("MoveAndClick")]
        public IActionResult MoveAndClick([FromBody] CoordinateVM coords)
        {
            try
            {
                bool result = SetCursorPos(coords.x, coords.y);

                if (!result)
                {
                    return StatusCode(500, "Failed to move mouse pointer.");
                }

                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                return Ok("Mouse pointer moved and button clicked successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Keypress")]
        public async Task<IActionResult> Keypress(string keypress)
        {
            try
            {
                foreach (char key in keypress)
                {
                    // Simulate key down and key up for each character
                    keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, 0);
                    keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);

                    // Wait for 3 seconds before the next key press
                    await Task.Delay(3000);
                }

                return Ok("All key presses simulated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("AltTab")]
        public async Task<IActionResult> AltTab(int minutes)
        {
            try
            {
                // Convert minutes to seconds for the total duration
                int durationInSeconds = minutes * 60;
                int intervalInMilliseconds = 3000; // 3 seconds between each Alt+Tab
                int totalIterations = durationInSeconds / (intervalInMilliseconds / 1000); // Calculate how many times to repeat Alt+Tab

                for (int i = 0; i < totalIterations; i++)
                {
                    // Simulate Alt down
                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYDOWN, 0);

                    // Simulate Tab press
                    keybd_event(VK_TAB, 0, KEYEVENTF_KEYDOWN, 0);
                    keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, 0);

                    // Release Alt after Tab press
                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);

                    // Wait for 3 seconds before the next Alt+Tab
                    await Task.Delay(intervalInMilliseconds);
                }

                return Ok($"Alt+Tab simulated successfully every 3 seconds for {minutes} minute(s).");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
