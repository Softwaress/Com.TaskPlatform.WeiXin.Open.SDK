using System.Configuration;

namespace Com.TaskPlatform.WeiXin.Open.SDK
{
    /// <summary>
    /// 全局设置(对应启动项的web.config)
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 请求超时设置（以毫秒为单位），默认为10秒。
        /// 说明：此处常量专为提供给方法的参数的默认值，不是方法内所有请求的默认超时时间。
        /// </summary>
        public const int TIME_OUT = 10000;

        #region 服务号相关配置
        public static string ServerToken => ConfigurationManager.AppSettings["ServerToken"];

        public static string ServerAppID => ConfigurationManager.AppSettings["ServerAppID"];

        public static string ServerAppSecret => ConfigurationManager.AppSettings["ServerAppSecret"];

        public static string ServerEncodingAESKey => ConfigurationManager.AppSettings["ServerEncodingAESKey"];

        /// <summary>
        /// 服务号授权回调的URL
        /// </summary>
        public static string ServeAuthRedirectUri => ConfigurationManager.AppSettings["ServeAuthRedirectUri"];

        #endregion

        /// <summary>
        /// 个人号授权回调的URL
        /// </summary>
        public static string AuthRedirectUri => ConfigurationManager.AppSettings["AuthRedirectUri"];

        #region 微信开放平台移动应用配置

        //public static string wxOpenAppid => ConfigurationManager.AppSettings["wxOpenAppid"];

        //public static string wxOpenAppSecret => ConfigurationManager.AppSettings["wxOpenAppSecret"];

        public static string wxLoginVerifyUrl => ConfigurationManager.AppSettings["wxLoginVerifyUrl"];

        #endregion

    }
}
