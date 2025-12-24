using Microsoft.AspNetCore.Mvc;

namespace Aneiang.Pa.ClientDemo.Controllers
{
    /// <summary>
    /// 测试控制器 - 用于验证其他控制器不受授权影响
    /// </summary>
    [ApiController]
    [Route("api/test")]
    [Produces("application/json")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// 测试接口 - 不需要授权
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "测试接口正常工作，无需授权", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// 测试接口 - 不需要授权
        /// </summary>
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok(new { message = "Hello from TestController", timestamp = DateTime.UtcNow });
        }
    }
}

