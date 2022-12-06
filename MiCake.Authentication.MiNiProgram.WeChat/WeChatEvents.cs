using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    /// <summary>
    /// 微信小程序身份验证过程的生命周期事件。
    /// </summary>
    public class WeChatEvents : RemoteAuthenticationEvents
    {
        /// <summary>
        /// 当调用微信服务端进行验证完成后触发的事件.
        /// 可以通过注册该方法进行获取系统用户信息并且颁发jwtToken等操作.
        /// </summary>
        public Func<WeChatServerCompletedContext, Task>? OnWeChatServerCompleted { get; set; }
    }
}
