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
        public Func<WeChatServerCompletedContext, Task> OnWeChatServerCompleted { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// 当微信服务端验证通过并且返回有效的OpenId之后，将执行创建AuthenticationTicket的事件。
        /// 一般情况下，您无需使用该方法。因为AuthenticationTicket是供创建Cookies使用，而使用小程序时，您无法使用Cookies。
        /// </summary>
        public Func<WeChatCreatingTicketContext, Task> OnCreatingTicket { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// 当调用微信服务端进行验证完成后将调用该方法.
        /// </summary>
        public virtual Task WeChatServerCompleted(WeChatServerCompletedContext context) => OnWeChatServerCompleted(context);

        /// <summary>
        /// 当微信服务端验证通过并且返回有效的OpenId之后，将调用该方法。
        /// </summary>
        public virtual Task CreatingTicket(WeChatCreatingTicketContext context) => OnCreatingTicket(context);
    }
}
