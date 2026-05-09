namespace ModContent.XmlExtendedAttribute
{
    using System.Xml;
    public interface IXmlExtendedAttribute
    {
        public void SolveAtt(XmlNode node,string valueAtt,XmlConverterSettings settings);
        
        public bool condition(XmlNode node);

        public string keyAttribute { get; }
    }
}
