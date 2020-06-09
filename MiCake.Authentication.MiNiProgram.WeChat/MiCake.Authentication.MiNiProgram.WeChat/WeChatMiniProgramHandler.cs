using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    public class WeChatMiniProgramHandler : RemoteAuthenticationHandler<WeChatMiniProgramOptions>
    {
        public WeChatMiniProgramHandler(
            IOptionsMonitor<WeChatMiniProgramOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var query = Request.Query;
            var jsCode = query[Options.WeChatJsCodeQueryString];

            if (string.IsNullOrEmpty(jsCode))
            {
                return HandleRequestResult.Fail("没有找到客户端所提供的JsCode供微信服务器进行验证。");
            }

            using var tokens = await ExchangeCodeAsync(jsCode);

            if (tokens.Error != null)
            {
                return HandleRequestResult.Fail(tokens.Error);
            }

            var completedContext = new WeChatServerCompletedContext(Context, Scheme, Options, tokens.OpenId, tokens.SessionKey, tokens.UnionId, tokens.ErrCode, tokens.ErrMsg);
            await Options.Events?.OnWeChatServerCompleted(completedContext);

            if (string.IsNullOrEmpty(tokens.OpenId) || string.IsNullOrEmpty(tokens.SessionKey))
            {
                return HandleRequestResult.Fail("没有接收到微信服务器所返回的OpenID和SessionKey。");
            }
            return HandleRequestResult.Handle(); ;
        }

        protected virtual async Task<WeChatTokenResponse> ExchangeCodeAsync(string clientJsCode)
        {
            var queryStringBuilder = new StringBuilder("?");
            queryStringBuilder.Append("appid=" + Options.WeChatAppId);
            queryStringBuilder.Append("&");
            queryStringBuilder.Append("secret=" + Options.WeChatSecret);
            queryStringBuilder.Append("&");
            queryStringBuilder.Append("js_code=" + clientJsCode);
            queryStringBuilder.Append("&");
            queryStringBuilder.Append("grant_type=" + Options.WeChatGrantTtype);

            var requestURL = WeChatMiniProgramDefault.AuthorizationEndpoint + queryStringBuilder.ToString();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestURL);
            var response = await Options.Backchannel.SendAsync(requestMessage, Context.RequestAborted);

            if (response.IsSuccessStatusCode)
            {
                var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return WeChatTokenResponse.Success(payload);
            }
            else
            {
                var error = "请求微信服务端交换Token失败，请检查网络环境是否正常。";
                return WeChatTokenResponse.Failed(new Exception(error));
            }
        }
    }
}
