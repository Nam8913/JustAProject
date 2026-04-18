using System.Collections.Generic;
using UnityEngine;

public class ContentHolder<T>  where T : class
{
    public Dictionary<string,T> contentDic = new Dictionary<string, T>();

    public void AddContent(string name, T content , bool overwriteIfExists = true)
    {
        if(contentDic.ContainsKey(name))
        {
            Debug.LogWarning($"Content with name {name} already exists. {(overwriteIfExists ? "Overwriting it." : "Not adding the new content.")}");
        }
        contentDic[name] = content;
    }

    public T GetContentByName(string name)
    {
        if(contentDic.TryGetValue(name, out T content))
        {
            return content;
        }
        else
        {
            Debug.LogError($"Content with name {name} of type {typeof(T)} not found.");
            return null;
        }
    }

    public bool TryGetContent(string name, out T content)
    {
        return contentDic.TryGetValue(name, out content);
    }
}
