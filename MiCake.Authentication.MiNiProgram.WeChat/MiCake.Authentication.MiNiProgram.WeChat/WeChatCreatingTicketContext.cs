using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    public class WeChatCreatingTicketContext : ResultContext<WeChatMiniProgramOptions>
    {
        public WeChatCreatingTicketContext(
            HttpContext context,
            AuthenticationScheme scheme,
            WeChatMiniProgramOptions options,
            WeChatTokenResponse weChatTokenResponse,
            ClaimsPrincipal claimsPrincipal
            ) : base(context, scheme, options)
        {
            WeChatTokenResponse = weChatTokenResponse;
            Principal = claimsPrincipal;
        }

        /// <summary>
        /// 微信服务器所返回的登录信息，包含openid和session_key。
        /// </summary>
        public WeChatTokenResponse WeChatTokenResponse { get; set; }
    }
}
