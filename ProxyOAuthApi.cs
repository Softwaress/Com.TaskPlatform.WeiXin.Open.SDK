using Com.TaskPlatform.WeiXin.Open.SDK.HttpUtility;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities.Response;
/*
 *创建日期 2019-10-15
 *创建人 wulincheng
 *作用：
 */
namespace Com.TaskPlatform.WeiXin.Open.SDK.Api
{
    /// <summary>
    /// 代公众号发起网页授权API
    /// </summary>
    public  class ProxyOAuthApi
    {
        /// <summary>
        /// 获取授权连接
        /// </summary>
        /// <param name="appId">公众号的appid</param>
        /// <param name="redirectUrl">重定向地址，需要urlencode，这里填写的应是服务开发方的回调地址</param>
        /// <param name="scope">授权作用域，拥有多个作用域用逗号（,）分隔</param>
        /// <param name="state">重定向后会带上state参数，开发者可以填写任意参数值，最多128字节</param>
        /// <param name="componentAppid">服务方的appid，在申请创建公众号服务成功后，可在公众号服务详情页找到</param>
        /// <param name="responseType">默认为填code</param>
        /// <returns>URL</returns>
        public static string GetAuthorizeUrl(string appId, string redirectUrl,  OAuthScope scope,string state, string componentAppid,string responseType = "code")
        {
            var url =
                $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appId}&redirect_uri={redirectUrl.UrlEncode()}&response_type={responseType}&scope={scope}&state={state}&component_appid={componentAppid}#wechat_redirect";
            return url;
        }
        /// <summary>
        /// 通过code换取access_token
        /// </summary>
        /// <param name="appId">公众号的appid</param>
        /// <param name="code">填写第一步获取的code参数</param>
        /// <param name="componentAppid">服务开发方的appid</param>
        /// <param name="componentAccessToken">服务开发方的access_token</param>
        /// <returns></returns>
        public static string Access_token(string appId, string code,string componentAppid,string componentAccessToken)
        {
            var url =
                $"https://api.weixin.qq.com/sns/oauth2/component/access_token?appid={appId}&code={code}&grant_type=authorization_code&component_appid={componentAppid}&component_access_token={componentAccessToken}";
            return url;
        }
        /// <summary>
        /// 通过code换取access_token
        /// </summary>
        /// <param name="appId">公众号的appid</param>
        /// <param name="code">填写第一步获取的code参数</param>
        /// <param name="componentAppId">服务开发方的appid</param>
        /// <param name="componentAccessToken">服务开发方的access_token</param>
        /// <param name="grantType">填authorization_code</param>
        /// <returns></returns>
        public static ResponseOAuthOpenAccessToken GetOpenAccessToken(string appId, string code, string componentAppId, string componentAccessToken, string grantType = "authorization_code")
        {
            var url =
                $"https://api.weixin.qq.com/sns/oauth2/component/access_token?appid={appId}&code={code}&grant_type={grantType}&component_appid={componentAppId}&component_access_token={componentAccessToken}";

            return Get.GetJson<ResponseOAuthOpenAccessToken>(url);
        }
        /// <summary>
        /// 刷新access_token
        /// 由于access_token拥有较短的有效期，当access_token超时后，可以使用refresh_token进行刷新，refresh_token拥有较长的有效期（30天），当refresh_token失效的后，需要用户重新授权。
        /// </summary>
        /// <param name="appId">公众号的appid</param>
        /// <param name="refreshToken">填写通过access_token获取到的refresh_token参数</param>
        /// <param name="componentAppId">服务开发商的appid</param>
        /// <param name="componentAccessToken">服务开发方的access_token</param>
        /// <param name="grantType">填refresh_token</param>
        /// <returns></returns>
        public static ResponseOAuthOpenAccessToken RefreshOpenToken(string appId, string refreshToken, string componentAppId, string componentAccessToken, string grantType = "refresh_token")
        {
            var url =
                $"https://api.weixin.qq.com/sns/oauth2/component/refresh_token?appid={appId}&grant_type={grantType}&component_appid={componentAppId}&component_access_token={componentAccessToken}&refresh_token={refreshToken}";

            return Get.GetJson<ResponseOAuthOpenAccessToken>(url);
        }

        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <param name="accessToken">调用接口凭证</param>
        /// <param name="openId">普通用户的标识，对当前公众号唯一</param>
        /// <param name="lang">返回国家地区语言版本，zh_CN 简体，zh_TW 繁体，en 英语</param>
        /// <returns></returns>
        public static ResponseOAuthUserInfo GetUserInfo(string accessToken, string openId, Language lang = Language.zh_CN)
        {
            var url = $"https://api.weixin.qq.com/sns/userinfo?access_token={accessToken}&openid={openId}&lang={lang}";
            return CommonJsonSend.Send<ResponseOAuthUserInfo>(null, url, null, CommonJsonSendType.GET);
        }

        /// <summary>
        /// 针对单个公众号的清零 quota
        /// </summary>
        /// <param name="accessToken">调用接口凭据</param>
        /// <param name="appid">公众号的 APPID</param>
        /// <returns></returns>
        public static WxJsonResult Person_Clear_quota(string accessToken, string appid)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/clear_quota?access_token={accessToken}";

            var data = new
            {
                appid = appid,
            };
            return CommonJsonSend.Send(accessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 第三方平台对其所有 API 调用次数清零（只与第三方平台相关，与公众号无关，接口如 api_component_token）
        /// </summary>
        /// <param name="componentAccessToken">第三方平台component_access_token</param>
        /// <param name="appid">第三方平台 APPID</param>
        /// <returns></returns>
        public static WxJsonResult Third_Party_Clear_quota(string componentAccessToken)
        {

            var url = $"https://api.weixin.qq.com/cgi-bin/component/clear_quota?component_access_token=={componentAccessToken}";

            var data = new
            {
                component_appid = Config.ServerAppID,
            };
            return CommonJsonSend.Send(componentAccessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);
        }
    }
}
