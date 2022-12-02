using MiCake.Authentication.MiniProgram.WeChat;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WeChatMiniProgramExtensions
    {
        public static AuthenticationBuilder AddWeChatMiniProgram(this AuthenticationBuilder builder, Action<WeChatMiniProgramOptions> configureOptions)
            => builder.AddWeChatMiniProgram(WeChatMiniProgramAuthConstants.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddWeChatMiniProgram(this AuthenticationBuilder builder, string authenticationScheme, Action<WeChatMiniProgramOptions> configureOptions)
            => builder.AddWeChatMiniProgram(authenticationScheme, WeChatMiniProgramAuthConstants.DisplayName, configureOptions);

        public static AuthenticationBuilder AddWeChatMiniProgram(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeChatMiniProgramOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WeChatMiniProgramOptions>, WeChatMiniProgramPostConfigureOptions>());
            builder.Services.TryAddSingleton<IWeChatSessionInfoStore, DefaultSessionInfoStore>();
            return builder.AddRemoteScheme<WeChatMiniProgramOptions, WeChatMiniProgramHandler>(authenticationScheme, displayName, configureOptions); ;
        }
    }
}
