using System.Threading;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    /// <summary>
    /// 保存微信服务端所返回的Sessionkey等信息到缓存的操作接口.
    /// </summary>
    public interface IWeChatSessionInfoStore
    {
        /// <summary>
        /// 保存<see cref="WeChatSessionInfo"/>,并且返回所关联的Key。
        /// </summary>
        /// <param name="sessionInfo"><see cref="WeChatSessionInfo"/></param>
        /// <param name="currentOption">当前的微信验证配置信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns>与该seesionInfo所关联的Key信息</returns>
        Task<string> Store(WeChatSessionInfo sessionInfo, WeChatMiniProgramOptions currentOption, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据Key来移除缓存中的结果。
        /// </summary>
        /// <param name="key"></param>
        Task Remove(string key);

        /// <summary>
        /// 根据Key来获取对应的<see cref="WeChatSessionInfo"/>。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        Task<WeChatSessionInfo?> GetSession(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据Key来获取对应的<see cref="WeChatSessionInfo"/>，并且在之后移除该缓存。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<WeChatSessionInfo?> GetAndRemoveSession(string key, CancellationToken cancellationToken = default);
    }
}
