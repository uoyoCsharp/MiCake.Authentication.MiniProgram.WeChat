using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
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

            if (string.IsNullOrWhiteSpace(jsCode))
            {
                return HandleRequestResult.Fail("没有找到客户端所提供的JsCode供微信服务器进行验证。");
            }

            using var tokens = await ExchangeCodeAsync(jsCode!);

            if (tokens.Error != null)
            {
                return HandleRequestResult.Fail(tokens.Error);
            }

            var completedContext = new WeChatServerCompletedContext(Context, Scheme, Options, tokens);
            var completedTask = Options.Events?.OnWeChatServerCompleted?.Invoke(completedContext);
            if (completedTask is not null) { await completedTask; }

            if (string.IsNullOrEmpty(tokens.OpenId) || string.IsNullOrEmpty(tokens.SessionKey))
            {
                var failMsg = "没有接收到微信服务器所返回的OpenID和SessionKey。";
                failMsg += string.IsNullOrWhiteSpace(tokens.ErrMsg) ? "" : $"微信服务端错误信息：{tokens.ErrMsg}";

                return HandleRequestResult.Fail(failMsg);
            }

            if (Options.CustomLoginState == null)
            {
                Logger.LogWarning("当前没有提供微信小程序自定义登录态的逻辑。");
            }
            else
            {
                string? sessionCacheKey = null;

                if (Options.SaveSessionToCache)
                {
                    var sessionStore = Context.RequestServices.GetService<IWeChatSessionInfoStore>();
                    if (sessionStore != null)
                        sessionCacheKey = await sessionStore.Store(new WeChatSessionInfo(tokens.OpenId, tokens.SessionKey), Options);
                }

                try
                {
                    var customLoginStateContext = new CustomLoginStateContext(Context, Scheme, Options, tokens, sessionCacheKey);
                    var customStateAction = Options.CustomLoginState?.Invoke(customLoginStateContext);
                    if (customStateAction is not null) { await customStateAction; }
                }
                catch (Exception ex)
                {
                    return HandleRequestResult.Fail(ex);
                }
            }

            return HandleRequestResult.Handle();
        }

        protected virtual async Task<WeChatTokenResponse> ExchangeCodeAsync(string clientJsCode)
        {
            var queryStringBuilder = new StringBuilder("?");
            queryStringBuilder.Append("appid=" + Options.WeChatAppId);
            queryStringBuilder.Append('&');
            queryStringBuilder.Append("secret=" + Options.WeChatSecret);
            queryStringBuilder.Append('&');
            queryStringBuilder.Append("js_code=" + clientJsCode);
            queryStringBuilder.Append('&');
            queryStringBuilder.Append("grant_type=" + Options.WeChatGrantTtype);

            var requestURL = WeChatMiniProgramAuthConstants.AuthorizationEndpoint + queryStringBuilder.ToString();

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
