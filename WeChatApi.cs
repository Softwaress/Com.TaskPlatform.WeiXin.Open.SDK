using System;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities.Response;
using Com.TaskPlatform.WeiXin.Open.SDK.HttpUtility;
/*
 *创建日期 2019-10-15
 *创建人 wulincheng
 *作用：
 */

namespace Com.TaskPlatform.WeiXin.Open.SDK.Api
{
    /// <summary>
    /// 个人登录网站
    /// </summary>
    public class WeChatApi
    {

        /// <summary>
        /// 通过code获取access_token||用户唯一标识openID
        /// </summary>
        /// <param name="code">扫二维码跳转时带的code</param>
        /// <param name="url">获取access_token的url</param>
        public static OutWechatSuccessInfo getAccess_TokenApi(string code)
        {
            string httpUrl =
                $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={Config.ServerAppID}&secret={Config.ServerAppSecret}&code={code}&grant_type=authorization_code";
            return CommonJsonSend.Send<OutWechatSuccessInfo>(null, httpUrl, null, CommonJsonSendType.GET, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 网站登录平台刷新token
        /// </summary>
        public static WxJsonResult Refresh_tokenApi(string token)
        {
            string httpUrl =
                $"https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={Config.ServerAppID}&grant_type=refresh_token&refresh_token={token}";
            return CommonJsonSend.Send(null, httpUrl, null, CommonJsonSendType.GET, timeOut: Config.TIME_OUT);

        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="accessToken">getAccess_Token接口返回的</param>
        /// <param name="openId">用户个人的openid</param>
        /// <returns></returns>
        public static OutWeChatUserInfo GetWeChatUserInfoApi(string accessToken, string openId)
        {

            string WeChatUserInfoUrl =
                $"https://api.weixin.qq.com/sns/userinfo?access_token={accessToken}&openid={openId}";
            return CommonJsonSend.Send<OutWeChatUserInfo>(null, WeChatUserInfoUrl, null, CommonJsonSendType.GET, timeOut: Config.TIME_OUT);

        }

        /// <summary>
        /// 检验token是否有效
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        public static bool CheckToken(string accessToken, string openId)
        {
            try
            {
                string token = $"https://api.weixin.qq.com/sns/auth?access_token={accessToken}&openid={openId}";
                var data = CommonJsonSend.Send(null, token, null, CommonJsonSendType.GET, timeOut: Config.TIME_OUT);
                return !Equals(data.errcode, "0");
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
