using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    /// <summary>
    /// Options for the WeChat MiniProgram authentication handler.
    /// </summary>
    public class WeChatMiniProgramOptions : RemoteAuthenticationOptions
    {
        /// <summary>
        /// 小程序 appId
        /// 【请注意该信息的安全性,不要下发至客户端】
        /// </summary>
        public string WeChatAppId { get; set; }

        /// <summary>
        /// 小程序 appSecret
        /// 【请注意该信息的安全性,不要下发至客户端】
        /// </summary>
        public string WeChatSecret { get; set; }

        /// <summary>
        /// 授权类型，该值为:authorization_code.
        /// </summary>
        public string WeChatGrantTtype { get; } = "authorization_code";

        /// <summary>
        /// 登录url中,携带小程序客户端获取到code的参数名.
        /// 默认为:"code". 
        /// 
        /// <para>
        ///     该值需要配合CallbackPath参数使用.假如CallbackPath为"signin-wechat"
        ///     则"https://yourhostaddress/signin-wechat?code=xxx"为验证地址,而"xxx"则会被传递至微信服务器进行验证.
        /// </para>
        /// </summary>
        public string WeChatJsCodeQueryString { get; set; } = "code";

        /// <summary>
        /// Gets or sets the <see cref="WeChatEvents"/> used to handle authentication events.
        /// </summary>
        public new WeChatEvents Events
        {
            get => (WeChatEvents)base.Events;
            set => base.Events = value;
        }

        public WeChatMiniProgramOptions()
        {
            CallbackPath = new PathString("/signin-wechat");
            BackchannelTimeout = TimeSpan.FromSeconds(60);
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(WeChatAppId))
                throw new ArgumentException($"WeChatAppId 不能为空.");

            if (string.IsNullOrEmpty(WeChatSecret))
                throw new ArgumentException($"WeChatSecret 不能为空.");
        }
    }
}
