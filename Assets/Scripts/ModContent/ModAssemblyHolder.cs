using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

public class ModAssemblyHolder
{
    public ModAssemblyHolder(){}

    public void LoadAssembly(string assemblyFolderPath)
    {
        if(!Directory.Exists(assemblyFolderPath))
        {
            Debug.LogWarning($"Assembly folder not found at path: {assemblyFolderPath}");
            return;
        }
        string[] assemblyFiles = Directory.GetFiles(assemblyFolderPath, "*.dll", SearchOption.AllDirectories);
        foreach(string assemblyFile in assemblyFiles)
        {
            FileInfo fileInfo = new FileInfo(assemblyFile);
            Assembly asm = null;
            try
            {
                byte[] rawDataAsm = File.ReadAllBytes(fileInfo.FullName);
                FileInfo fileInfo2 = new FileInfo(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(fileInfo.FullName)) + ".pdb");
                if(fileInfo2.Exists)
                {
                    byte[] rawDataPdb = File.ReadAllBytes(fileInfo2.FullName);
                    asm = Assembly.Load(rawDataAsm, rawDataPdb);
                }
                else
                {
                    asm = Assembly.Load(rawDataAsm);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load assembly from file: {assemblyFile}\nException: {ex} \n{ex.StackTrace}");
                continue;
            }

            if(asm != null && IsAssemblyUsable(asm))
            {
                loadedAssemblies.Add(asm);
                TypeUtils.LoadTypesCachedFromAssembly(asm);
                Debug.Log($"Loaded assembly: {asm.GetName().Name} from file: {assemblyFile}");
            }
            else
            {
                Debug.LogError($"Assembly {asm?.GetName()?.Name} is not usable and will be skipped.");
            }
        }
    }

    public bool IsAssemblyUsable(Assembly asm)
    {
        try
        {
            asm.GetTypes();
        }catch(ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Concat(new object[]
            {
                "ReflectionTypeLoadException getting types from assembly: ",
                asm?.GetName()?.Name,
                "\nLoaderExceptions:\n",
                ex
            }));
            sb.AppendLine();
            sb.AppendLine("LoaderExceptions:");
            if(ex.LoaderExceptions != null)
            {
                foreach(Exception loaderEx in ex.LoaderExceptions)
                {
                    sb.AppendLine(" => " + loaderEx.ToString());
                }
            }
            Debug.LogError(sb.ToString());
            return false;
        }catch(Exception ex)
        {
            Debug.LogError($"Exception getting types from assembly: {asm.GetName().Name}\n{ex}");
            return false;
        }
        return true;
    }

    public List<Assembly> loadedAssemblies = new List<Assembly>();
}
