using UnityEngine;

namespace ModContent.XmlConverter
{
    public abstract class XmlConverter
    {
        public abstract string toXML(object value);
        public abstract object fromXML(string xml);
        public abstract bool CanConvert(System.Type type);

        public virtual bool canWrite => true;
        public virtual bool canRead => true;
    }
}
