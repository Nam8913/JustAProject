#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
namespace ModContent.XmlExtendedAttribute
{
    using System.Xml;
    using UnityEngine;

    public class GameObjectTarget : IXmlExtendedAttribute
    {
        public string keyAttribute => "GameObjectTarget";

        public bool condition(XmlNode node)
        {
            return true;
        }

        public void SolveAtt(XmlNode node, string valueAtt, XmlConverterSettings settings)
        {
            string typeOfGameObjectTarget;
            string Id;

            string[] split = valueAtt.Split(':');
            if(split.Length != 2)
            {
                Debug.LogError($"Invalid format for GameObjectTarget attribute value: {valueAtt}. Expected format is 'Type:Id'.");
                return;
            }

            typeOfGameObjectTarget = split[0].Trim();
            Id = split[1].Trim();

            #if DEBUG_LOG_FLAG && false
            Debug.Log($"Parsed GameObjectTarget attribute with Type: {typeOfGameObjectTarget} and Id: {Id}.");
            #endif
            ThingHandler.AddThingMappingById(Id, typeOfGameObjectTarget);
        }
    }
}