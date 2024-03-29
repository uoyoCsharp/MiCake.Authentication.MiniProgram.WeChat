﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    /// <summary>
    /// Options for the WeChat MiniProgram authentication handler.
    /// </summary>
    public class WeChatMiniProgramOptions : RemoteAuthenticationOptions
    {
        /// <summary>
        /// 小程序 appId
        /// 【请注意该信息的安全性,避免下发至客户端】
        /// </summary>
        public string WeChatAppId { get; set; } = string.Empty;

        /// <summary>
        /// 小程序 appSecret
        /// 【请注意该信息的安全性,避免下发至客户端】
        /// </summary>
        public string WeChatSecret { get; set; } = string.Empty;

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
        /// 是否要保存微信服务端所返回的OpenId和SessionKey到缓存中.
        /// 
        /// <para>
        /// 当该值为true时，从DI容器中获取<see cref="IWeChatSessionInfoStore"/>服务来将微信服务端返回的结果保存到缓存中.
        /// 默认的保存将数据保存在内存之中。
        /// <para>
        ///     默认值： true
        /// </para>
        /// </para>
        /// </summary>
        public bool SaveSessionToCache { get; set; } = true;

        /// <summary>
        /// 缓存滑动过期的时间.
        /// 默认值为：30分钟.
        /// 
        /// <para>
        ///     该值只有当 <see cref="SaveSessionToCache"/> = true的时候才有意义.
        /// </para>
        /// </summary>
        public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// 缓存的key生成规则.
        /// 
        /// <para>
        ///     该值只有当 <see cref="SaveSessionToCache"/> = true的时候才有意义.
        ///     若不指定，默认将采用 'prefix + guid' 的方式生成.
        /// </para>
        /// </summary>
        public Func<WeChatSessionInfo, string>? CacheKeyGenerationRule { get; set; }

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
            Events = new WeChatEvents();
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
