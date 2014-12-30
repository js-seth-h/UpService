using Cinchoo.Core;
using Cinchoo.Core.Configuration;
using Cinchoo.Core.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UpService
{ 
    public class CDATAElement : ConfigurationElement
    {
        protected override void DeserializeElement(XmlReader reader, bool s)
        {
            Value = reader.ReadElementContentAs(typeof(string), null) as string;
        } 
        public string Value { get; private set; } 
    }
     
    public class UpServiceSection : ConfigurationSection
    {
        [ConfigurationProperty("ServiceName", IsRequired=true)]
        public CDATAElement ServiceNameEl
        {
            get { return (this["ServiceName"] as CDATAElement); }
        }
        public string ServiceName { get { return ServiceNameEl.Value; } }

        [ConfigurationProperty("UserName", IsRequired = true)]
        public CDATAElement UserNameEl
        {
            get { return (this["UserName"] as CDATAElement); }
        }
        public string UserName { get { return UserNameEl.Value; } }

        [ConfigurationProperty("Password")]
        public CDATAElement PasswordEl
        {
            get { return (this["Password"] as CDATAElement); }
        }
        public string Password { get { return PasswordEl.Value; } }

        [ConfigurationProperty("StartScript")]
        public CDATAElement StartScriptEl
        {
            get { return (this["StartScript"] as CDATAElement); }
        }
        public string StartScript { get { return StartScriptEl.Value; } }

        [ConfigurationProperty("StopScript")]
        public CDATAElement StopScriptEl
        {
            get { return (this["StopScript"] as CDATAElement); }
        }
        public string StopScript { get { return StopScriptEl.Value; } }

    }
}
