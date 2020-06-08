using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    /// <summary>
    /// Default <see cref="WeChatEvents"/> implementation.
    /// </summary>
    public class WeChatEvents : RemoteAuthenticationEvents
    {
        /// <summary>
        /// 当调用微信服务端进行验证完成后触发的事件.
        /// </summary>
        public Func<WeChatServerCompletedContext, Task> OnWeChatServerCompleted { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// 当调用微信服务端进行验证完成后将调用该方法.
        /// </summary>
        public virtual Task WeChatServerCompleted(WeChatServerCompletedContext context) => OnWeChatServerCompleted(context);
    }
}
