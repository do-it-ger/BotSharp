/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc
    
    文件名：CustomMessageContext.cs
    文件功能描述：微信消息上下文
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Senparc.NeuChar;
using Senparc.NeuChar.Context;
using Senparc.NeuChar.Entities;

namespace BotSharp.Channel.Weixin.Models
{
    public class CustomMessageContext : MessageContext<IRequestMessageBase, IResponseMessageBase>
    {
        public override IRequestMessageBase GetRequestEntityMappingResult(RequestMsgType requestMsgType, XDocument doc)
        {
            throw new NotImplementedException();
        }

        public override IResponseMessageBase GetResponseEntityMappingResult(ResponseMsgType responseMsgType, XDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}
