using System.IO;
using System.Xml;
using System.Xml.Linq;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities.Response;
using Com.TaskPlatform.WeiXin.Open.SDK.HttpUtility;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities;
using Com.TaskPlatform.WeiXin.Open.SDK.Entities.Request;
using Com.TaskPlatform.WeiXin.Open.SDK.Helpers;
using Com.TaskPlatform.WeiXin.Open.SDK.Tencent;

/*
 *创建日期 2019-10-15
 *创建人 wulincheng
 *作用：
 */
namespace Com.TaskPlatform.WeiXin.Open.SDK.Api
{
    /// <summary>
    /// 微信第三方平台调用接口
    /// </summary>
    public class InterfaceApi
    {
        /// <summary>
        /// 接收component_verify_ticket协议
        /// </summary>
        /// <param name="postModel"></param>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static ResponseAuthEventReceiveMsg Component_verify_ticket(PostModel postModel, Stream inputStream)
        {
            if (inputStream == null) return null;
            inputStream.Seek(0, SeekOrigin.Begin);//强制调整指针位置
            using (XmlReader xr = XmlReader.Create(inputStream))
            {
                var postDataDocument = XDocument.Load(xr);

                var result = Init(postDataDocument, postModel);

                var resultMessage = new ResponseAuthEventReceiveMsg();
                //xml to model
                EntityHelper.FillEntityWithXml(resultMessage, result);
                return resultMessage;
            }

        }

        private static XDocument Init(XDocument postDataDocument, PostModel _postModel)
        {
            //进行加密判断并处理

            var postDataStr = postDataDocument.ToString();
            XDocument decryptDoc = postDataDocument;
            if (postDataDocument.Root == null || (_postModel == null ||
                                                  postDataDocument.Root.Element("Encrypt") == null ||
                                                  string.IsNullOrEmpty(postDataDocument.Root.Element("Encrypt")?.Value))
            ) return decryptDoc;
            //使用了加密

            string sToken = Config.ServerToken;
            string sAppID = Config.ServerAppID;
            string sEncodingAESKey = Config.ServerEncodingAESKey;

            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(sToken, sEncodingAESKey, sAppID);
            string msgXml = null;

            var result = wxcpt.DecryptMsg(_postModel.Msg_Signature, _postModel.Timestamp, _postModel.Nonce, postDataStr, ref msgXml);

            //判断result类型
            if (result != 0)
            {
                //验证没有通过，取消执行

                return null;
            }

            decryptDoc = XDocument.Parse(msgXml);//完成解密
            return decryptDoc;
        }

        /// <summary>
        /// 获取第三方平台access_token
        /// </summary>
        /// <param name="component_appid"></param>
        /// <param name="component_appsecret"></param>
        /// <param name="componentVerifyTicket"></param>
        /// <returns></returns>
        public static ResponseComponentToken Component_token(string componentVerifyTicket)
        {
            var urlFormat = "https://api.weixin.qq.com/cgi-bin/component/api_component_token";
            object data = null;
            data = new
            {
                component_appid = Config.ServerAppID,
                component_appsecret = Config.ServerAppSecret,
                component_verify_ticket = componentVerifyTicket
            };
            return CommonJsonSend.Send<ResponseComponentToken>("", urlFormat, data, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 用于获取预授权码。预授权码用于公众号授权时的第三方平台方安全验证
        /// </summary>
        /// <param name="component_verify_ticket"></param>
        /// <param name="componentAccessToken"></param>
        /// <returns></returns>
        public static ResponseCreatePreauthCode Create_preauthcode(string componentAccessToken)
        {
            var urlFormat = "https://api.weixin.qq.com/cgi-bin/component/api_create_preauthcode?component_access_token={0}";
            object data = null;
            data = new
            {
                component_appid = Config.ServerAppID,
            };
            return CommonJsonSend.Send<ResponseCreatePreauthCode>(componentAccessToken, urlFormat, data, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 使用授权码换取公众号的授权信息
        /// </summary>
        /// <param name="componentAccessToken"></param>
        /// <param name="authCodeValue"></param>
        /// <returns></returns>
        public static PublicWechatAuthorizerInfo Query_auth(string componentAccessToken, string authCodeValue)
        {
            var urlFormat = "https://api.weixin.qq.com/cgi-bin/component/api_query_auth?component_access_token={0}";
            object data = null;
            data = new
            {
                component_appid = Config.ServerAppID,
                authorization_code = authCodeValue
            };
            return CommonJsonSend.Send<PublicWechatAuthorizerInfo>(componentAccessToken, urlFormat, data, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 获取授权方的账户信息
        /// </summary>
        /// <param name="componentAccessToken">第三方平台access_token</param>
        /// <param name="authorizerAppid">授权方appid</param>
        /// <returns></returns>
        public static PublicAuthorizerUserInfo Get_Authorizer_Info(string componentAccessToken, string authorizerAppid)
        {
            var urlFormat = "https://api.weixin.qq.com/cgi-bin/component/api_get_authorizer_info?component_access_token={0}";
            object data = null;
            data = new
            {
                component_appid = Config.ServerAppID,
                authorizer_appid = authorizerAppid
            };
            return CommonJsonSend.Send<PublicAuthorizerUserInfo>(componentAccessToken, urlFormat, data, timeOut: Config.TIME_OUT);
        }


        /// <summary>
        /// 获取（刷新）授权公众号的令牌
        /// </summary>
        /// <param name="componentAccessToken">第三方平台appid</param>
        /// <param name="authorizerAppId">授权方appid</param>
        /// <param name="authorizerRefreshToken">授权方的刷新令牌，刷新令牌主要用于公众号第三方平台获取和刷新已授权用户的access_token，只会在授权时刻提供，请妥善保存。 一旦丢失，只能让用户重新授权，才能再次拿到新的刷新令牌</param>
        /// <returns></returns>
        public static ResponseAuthorizerToken Refresh_Authorizer_Token(string componentAccessToken, string authorizerAppId, string authorizerRefreshToken)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/component/api_authorizer_token?component_access_token={componentAccessToken}";
            var data = new
            {
                component_appid = Config.ServerAppID,
                authorizer_appid = authorizerAppId,
                authorizer_refresh_token = authorizerRefreshToken
            };
            return CommonJsonSend.Send<ResponseAuthorizerToken>(componentAccessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 获取授权方的选项设置信息
        /// </summary>
        /// <param name="componentAccessToken">第三方平台appid</param>
        /// <param name="authorizerAppId">授权方appid</param>
        /// <param name="optionName">选项值</param>
        /// <returns></returns>
        public static ResponseAuthorizerOption GetAuthorizerOption(string componentAccessToken, string authorizerAppId, OptionName optionName)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/component/ api_get_authorizer_option?component_access_token={componentAccessToken}";

            var data = new
            {
                component_appid = Config.ServerAppID,
                authorizer_appid = authorizerAppId,
                option_name = optionName
            };

            return CommonJsonSend.Send<ResponseAuthorizerOption>(componentAccessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 设置授权方的选项信息
        /// </summary>
        /// <param name="componentAccessToken">服务开发方的access_token</param>
        /// <param name="authorizerAppId">授权公众号appid</param>
        /// <param name="optionName">选项名称</param>
        /// <param name="optionValue">设置的选项值</param>
        /// <returns></returns>
        public static WxJsonResult SetAuthorizerOption(string componentAccessToken, string authorizerAppId, OptionName optionName, int optionValue)
        {
            var url =
                string.Format(
                    "https://api.weixin.qq.com/cgi-bin/component/ api_set_authorizer_option?component_access_token={0}",
                    componentAccessToken);

            var data = new
            {
                component_appid = Config.ServerAppID,
                authorizer_appid = authorizerAppId,
                option_name = optionName,
                option_value = optionValue
            };

            return CommonJsonSend.Send<WxJsonResult>(componentAccessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);
        }

        /// <summary>
        /// 生成手机端二维码
        /// </summary>
        /// <param name="preAuthCode"></param>
        /// <param name="authType">1公众号、2小程序、3公众号，小程序</param>
        /// <returns></returns>
        public static string GenerateQrCode(string preAuthCode, int authType = 1)
        {
            var url =
                $@"https://mp.weixin.qq.com/safe/bindcomponent?action=bindcomponent&auth_type=3&no_scan=1&component_appid={Config.ServerAppID}&pre_auth_code={preAuthCode}&redirect_uri={Config.wxLoginVerifyUrl}&auth_type={authType}#wechat_redirect";
            return QrCoderHelper.CreateQrCodeToFile(url, "wxCode");
        }
        /// <summary>
        /// 授权变更通知推送
        /// </summary>
        /// <param name="postModel"></param>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static ResponseAuthorization Authorization_Notification(PostModel postModel, Stream inputStream)
        {
            if (inputStream == null) return null;
            inputStream.Seek(0, SeekOrigin.Begin);//强制调整指针位置
            using (XmlReader xr = XmlReader.Create(inputStream))
            {
                var postDataDocument = XDocument.Load(xr);

                var result = Init(postDataDocument, postModel);

                var resultMessage = new ResponseAuthorization();
                //xml to model
                EntityHelper.FillEntityWithXml(resultMessage, result);
                return resultMessage;
            }
        }
        /// <summary>
        /// 拉取所有已授权的帐号信息
        /// </summary>
        /// <param name="componentAccessToken">令牌</param>
        /// <param name="offset">偏移位置/起始位置 默认值为零</param>
        /// <param name="count">拉取数量，最大为 500。默认拉取最大值</param>
        /// <returns></returns>
        public static ResponseAuthorizerList GetAuthorizerList(string componentAccessToken, int offset = 0, int count = 500)
        {
            var url = $" https://api.weixin.qq.com/cgi-bin/component/api_get_authorizer_list?component_access_token={componentAccessToken}";

            var data = new
            {
                component_appid = Config.ServerAppID,
                offset = offset,
                count = count
            };

            return CommonJsonSend.Send<ResponseAuthorizerList>(componentAccessToken, url, data, CommonJsonSendType.POST, timeOut: Config.TIME_OUT);

        }

    }
}
