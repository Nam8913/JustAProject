using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
public static class TypeUtils
{
    public static List<Assembly> GetAllAssemblies()
    {
        return new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
    }

    public static List<Type> GetAllTypes() => typesCached.Values.ToList();
    public static Type TryGetType(string typeName)
    {
        if(typesCached.TryGetValue(typeName, out Type type))
        {
            return type;
        }
        return null;
    }
    public static void LoadTypesCachedFromAssembly(Assembly assembly)
    {
        Type[] types = assembly.GetTypes();
        foreach(var type in types)
        {
            if(!typesCached.ContainsKey(type.FullName))
            {
                typesCached.Add(type.FullName, type);
            }
        }
        
    }

    public static bool IsClass(this Type type)
    {
        return type.IsClass;
    }

    public static bool IsStruct(this Type type)
    {
        return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
    }
    
    public static bool IsAbstract(this Type type)
    {
        return type.IsAbstract;
    }

    public static bool IsInterface(this Type type)
    {
        return type.IsInterface;
    }

    // public static bool IsList(this Type type)
    // {
    //     return typeof(System.Collections.IList).IsAssignableFrom(type);
    // }

    // public static bool IsDictionary(this Type type)
    // {
    //     return typeof(System.Collections.IDictionary).IsAssignableFrom(type);
    // }

    public static bool IsList(this Type type)
    {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return true;
        }
        return false;
    }

    public static bool IsDictionary(this Type type)
    {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return true;
        }
        return false;
    }

    public static bool IsHashSet(this Type type)
    {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            return true;
        }
        return false;
    }

    public static bool IsCollection(this Type type)
    {
        if (type == typeof(string))
        return false;

        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private static Dictionary<string,Type> typesCached = new Dictionary<string,Type>();
    // private static Dictionary<Type, List<Type>> cachedSubClasses = new Dictionary<Type, List<Type>>();
    // private static Dictionary<Type, List<Type>> cachedSubClassesNonAbstract = new Dictionary<Type, List<Type>>();
    // private static Dictionary<Type, List<Type>> cachedTypesWithAttribute = new Dictionary<Type, List<Type>>();
}
