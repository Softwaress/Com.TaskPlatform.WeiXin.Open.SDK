# Com.TaskPlatform.WeiXin.Open.SDK
C#、ASP .NET MVC WebApi 微信登录个人网站、第三方平台接口说明实现(获取验证票据、获取令牌、获取预授权码等)、代公众号实现业务
使用SQLite进行保存数据保存、Quartz进行定时获取token、Code
以下是webApi使用例示如下(部分)：
 /*
         *当没有必生http请求时使用System.Web.HttpContext.Current.Server.MapPath()
         * 会出现未将对象引用设置到对象的实例的错误的解决方法:
         *使用System.AppDomain.CurrentDomain.BaseDirectory
         */
        private static SqLiteHelper openDb = new SqLiteHelper(@"data source=" + System.Web.HttpContext.Current.Server.MapPath(@"/DBSqlite/wxdb.db") + "");
        private static bool DefineExecution = true;//定义执行
        private static bool isAuthCode = true;
        /// <summary>
        /// 获取验票据
        /// </summary>
        /// <param name="req"></param>
        /// <param name="postModel"></param>
        /// <returns></returns>
        [System.Web.Http.Route("AuthReceive")]
        [HttpPost]
        public HttpResponseMessage AuthReceive()
        {
            try
            {

                PostModel postModel = new PostModel();
                var request = HttpContext.Current.Request;
                postModel.Timestamp = request.QueryString["timestamp"].ToString();
                postModel.Msg_Signature = request.QueryString["msg_signature"].ToString();
                postModel.Nonce = request.QueryString["Nonce"].ToString();
                //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
                //HttpRequestBase request = context.Request;//定义传统request对象    
                var msg = InterfaceApi.Component_verify_ticket(postModel, request.InputStream);
                if (msg.InfoType == ThirdPartyInfo.component_verify_ticket.ToString())
                {
                    Logger.Debug("msg.ComponentVerifyTicket:" + msg.ComponentVerifyTicket);

                    var sql =
                        $@"INSERT INTO WxComponentVerifyTicket (AppId,AuthorizerAppid,ComponentVerifyTicket,CreateTime,InfoType) VALUES ('{msg.AppId}','{msg.AuthorizerAppid}','{msg.ComponentVerifyTicket}',{msg.CreateTime},'{msg.InfoType}');";
                    var retData = openDb.ExecuteNonQuery(sql);
                    if (retData >= 1)
                    {
                        if (DefineExecution)//等于空立刻执行获取Token
                        {
                            Logger.Debug("执行获取token方法" + msg.ComponentVerifyTicket);
                            DefineExecution = false;
                            ChangeTokensJob.token();
                        }
                    }


                }
                else if (msg.InfoType == ThirdPartyInfo.unauthorized.ToString())
                {
                    //取消事件 todo
                }

                return new HttpResponseMessage()
                {
                    Content = new StringContent("success", Encoding.GetEncoding("UTF-8"),
                        "application/x-www-form-urlencoded")
                };
            }
            catch (Exception ex)
            {
                Logger.Debug(@"获取ComponentVerifyTicket出错:" + ex);
                //return Content("success");
                return new HttpResponseMessage()
                {
                    Content = new StringContent("success", Encoding.GetEncoding("UTF-8"),
                        "application/x-www-form-urlencoded")
                };
            }
        }

        /// <summary>
        /// 使授权码获取授权后的信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [System.Web.Http.Route("ObtainAuthorizationInformation")]
        [HttpPost]
        public HttpResponseMessage ObtainAuthorizationInformation([FromBody]Dictionary<string,string> req)
        {
            var re = new ApiResponseObject()
            {
                ErrorCode = (int)ErrorCodeEnum.Normal,
                ErrorMsg = "系统出错了",
                success = true
            };
            try
            {
                if (!string.IsNullOrWhiteSpace(req["authCode"]))
                {
                    var sqlToken = string.Format(@"select * from WxComponentAccessToken Order by ID Desc  limit 0,1");
                    var model = openDb.ExecuteModel<WxComponentAccessToken>(sqlToken);
                    var data= InterfaceApi.Query_auth(model.ComponentAccessToken, req["authCode"]);
                    re.Data = data.authorization_info;
                    re.ErrorMsg = "";
                }
                else
                {
                    re.ErrorMsg = "缺少auth_code";
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            return new HttpResponseMessage()
            {
                Content = new StringContent(JsonConvert.SerializeObject(re), Encoding.UTF8,
                    "application/json"),
            };

        }


        public class ChangeTokensJob
        {
            public static int ExpiresIn => Convert.ToInt32(ConfigurationManager.AppSettings["TokenDate"]);

            public static void token()
            {
                Logger.Debug(@"添加定时器并执行，每隔多少秒执行一次:" + ExpiresIn);
                new QJob("systoken1", "token1", "token", "group").Handle(GetComponent_Token).Start(DateTime.Now, ExpiresIn == 0 ? 7000 : ExpiresIn, 0);
                Logger.Debug(@"添加定时器完成");
            }

            /// <summary>
            /// 获取令牌
            /// </summary>
            private static void GetComponent_Token()
            {
                var re = new ApiResponseObject()
                {
                    ErrorCode = (int)ErrorCodeEnum.Normal,
                    ErrorMsg = "系统出错了",
                    success = false
                };
                try
                {
                    var sql = string.Format(@"select * from WxComponentVerifyTicket Order by ID Desc  limit 0,1");
                    var model = openDb.ExecuteModel<WxComponentVerifyTicket>(sql);
                    var data = InterfaceApi.Component_token(model.ComponentVerifyTicket);
                    Logger.Debug("获取Component_token成功:" + data.component_access_token + "---------" + data.expires_in);
                    var sqlInser =
                        $@"INSERT INTO WxComponentAccessToken (ComponentAccessToken,ExpiresIn) VALUES ('{data.component_access_token}',{data.expires_in});";
                    var retData = openDb.ExecuteNonQuery(sqlInser);
                    if (retData >= 1)
                    {
                        Logger.Debug("Component_token插入成功");
                    }
                    Logger.Debug("去获取Code");
                    if (isAuthCode)
                    {
                        isAuthCode = false;
                        new QJob("sysCode1", "Code1", "Code", "CodeGroup").Handle(GetAuthCode).Start(DateTime.Now, 1700, 0);
                    }


                }
                catch (Exception ex)
                {
                    Logger.Debug(@"获取getComponent_token出错:" + ex);

                }
                //return new HttpResponseMessage()
                //{
                //    Content = new StringContent(JsonConvert.SerializeObject(re), Encoding.UTF8,
                //        "application/json"),
                //};
            }

            /// <summary>
            /// 获取Code  Code有效期十分钟
            /// </summary>
            public static void GetAuthCode()
            {
                var sqlToken = string.Format(@"select * from WxComponentAccessToken Order by ID Desc  limit 0,1");
                var model = openDb.ExecuteModel<WxComponentAccessToken>(sqlToken);
                var code = InterfaceApi.Create_preauthcode(model.ComponentAccessToken).pre_auth_code;
                Logger.Debug("成功获取code:" + code);
                QrCode(code);
            }

            public void ChangeTokensPeriodically()
            {

            }

            public static void QrCode(string preAuthCode)
            {

                try
                {

  
                    var data = InterfaceApi.GenerateQrCode(preAuthCode);
                    Logger.Debug("image:" + data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Logger.Debug("生成二维码出错了:" + e);
                    throw e;
                }
            }

        }
 
