using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MVC.Models
{
    [DataContract]
    public class Response
    {
        [DataMember]
        internal string response;

        [DataMember]
        internal string state;

        [DataMember]
        internal string sub_state;

        [DataMember]
        internal string chat_type;
    }
}