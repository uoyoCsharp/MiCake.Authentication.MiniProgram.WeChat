using MiCake.Authentication.MiniProgram.WeChat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WeChatAuthentication.Sample.Services;

namespace WeChatAuthentication.Sample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LoginController : ControllerBase
    {
        private readonly IWeChatSessionInfoStore _weChatSessionStore;
        private readonly AssociateWeChatUser _associateWeChatUser;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IWeChatSessionInfoStore weChatSessionStore, AssociateWeChatUser associateWeChatUser, ILogger<LoginController> logger)
        {
            _weChatSessionStore = weChatSessionStore;
            _associateWeChatUser = associateWeChatUser;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> CreateToken(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"key 不能为空");

            var weChatSession = await _weChatSessionStore.GetAndRemoveSession(key);
            _logger.LogInformation(message: weChatSession?.OpenId);

            return _associateWeChatUser.GetUserToken(weChatSession.OpenId);
        }
    }
}
