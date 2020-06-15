using MiCake.Authentication.MiniProgram.WeChat;
using Microsoft.AspNetCore.Mvc;
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

        public LoginController(IWeChatSessionInfoStore weChatSessionStore, AssociateWeChatUser associateWeChatUser)
        {
            _weChatSessionStore = weChatSessionStore;
            _associateWeChatUser = associateWeChatUser;
        }

        [HttpGet]
        public async Task<string> CreateToken(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"key 不能为空");

            var weChatSession =await _weChatSessionStore.GetSessionInfo(key);
            return _associateWeChatUser.GetUserToken(weChatSession.OpenId);
        }
    }
}
