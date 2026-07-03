using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [SerializeField] private List<UIWindow> allWindows; // gán sẵn trong Inspector
    private Dictionary<string, UIWindow> windowGroupDict = new Dictionary<string, UIWindow>();
    private HashSet<UIWindow> openWindows = new HashSet<UIWindow>();    
    public UIWindow focusedWindow;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenWindow(string groupName, UIWindow window, bool forceOpen = false)
    {
        if (!windowGroupDict.ContainsKey(groupName))
        {
            OpenWin(groupName, window, true);
            focusedWindow = window;
        }
        else
        {
            if(forceOpen)
            {
                UIWindow oldWindow = windowGroupDict[groupName];
                Debug.LogWarning($"Window group {groupName} đã tồn tại. Window cũ {oldWindow.name} sẽ bị đóng và window mới {window.name} sẽ được mở.");
                oldWindow.Hide();
                OpenWin(groupName, window, false);
            }
            else
            {
                Debug.LogWarning($"Window group {groupName} đã tồn tại. Không thể đăng ký window mới.");
            }
        }
    }

    public void OpenWindow<T>(string groupName, bool forceOpen = false)
    {
        UIWindow window = allWindows.Find(w => w is T);
        if (window == null) { Debug.LogError($"Không tìm thấy window {typeof(T)}"); return; }
        OpenWindow(groupName, window, forceOpen);
    }

    public void OpenChildWindow(UIWindow parent, UIWindow childWindow)
    {
        if (parent != null)
        {
            parent.RegisterChild(childWindow);
            childWindow.Show();
            focusedWindow = childWindow; // Cập nhật focusedWindow thành childWindow
        }
        else
        {
            Debug.LogWarning("Không có window nào đang được focus để mở child window.");
        }
    }

    public void OpenChildWindow(UIWindow childWindow)
    {
        if (focusedWindow != null)
        {
            focusedWindow.RegisterChild(childWindow);
            childWindow.Show();
            focusedWindow = childWindow; // Cập nhật focusedWindow thành childWindow
        }
        else
        {
            Debug.LogWarning("Không có window nào đang được focus để mở child window.");
        }

        // Thêm child window vào danh sách openWindows
        openWindows.Add(childWindow);
    }

    public void CloseWindow(string groupName)
    {
        if (windowGroupDict.ContainsKey(groupName))
        {
            CloseWin(groupName);
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy window group {groupName} để đóng.");
        }
    }

    public void CloseWindow(UIWindow window)
    {
        if(!openWindows.Contains(window)) return; // Window không mở, không cần đóng
        openWindows.Remove(window);
        
        // Trong trường hợp window có groupName và là window chính của group
        for(int i = windowGroupDict.Count - 1; i >= 0; i--)
        {
            var kvp = windowGroupDict.ElementAt(i); // Duyệt ngược để tránh lỗi khi xóa phần tử trong quá trình duyệt
            if(kvp.Value == window)
            {
                CloseWin(kvp.Key); // Đóng window và xóa khỏi dictionary
                return;
            }
        }

        // Nếu không tìm thấy trong windowGroupDict, tức window ở đây là child window hoặc không có groupName, chỉ cần ẩn nó
        // Và tất nhiên openWindows đã loại bỏ nó ở trên nên không cần xóa khỏi openWindows nữa
        window.Hide();
        if(focusedWindow == window)
        {
            focusedWindow = null; // Reset focusedWindow nếu nó là window đang được focus
        }
    }

    public void CloseAllWindows()
    {
        foreach(var window in windowGroupDict.Values)
        {
            CloseWin(window.name);
        }
        windowGroupDict.Clear();
    }

    void OpenWin(string groupName, UIWindow window , bool addNew = false)
    {
        if(addNew)
        {
            windowGroupDict.Add(groupName, window);
        }else
        {
            windowGroupDict[groupName] = window;
        }
        openWindows.Add(window);
        window.Show();
    }
    
    void CloseWin(string groupName)
    {
        UIWindow window = windowGroupDict[groupName];

        if (focusedWindow == window)
        {
            focusedWindow = null;
        }
        window.Hide();
        openWindows.Remove(window);
        windowGroupDict.Remove(groupName);
    }

    public bool IsWindowOpen(string groupName)
    {
        return windowGroupDict.ContainsKey(groupName);
    }

    public bool IsWindowOpen(UIWindow window)
    {
        return openWindows.Contains(window);
    }

    public bool IsWindowFocused(UIWindow window)
    {
        return focusedWindow == window;
    }

    public bool IsAnyWindowOpen()
    {
        return openWindows.Count > 0;
    }

    public UIWindow GetWindowOfType<T>() where T : UIWindow
    {
        return allWindows.Find(w => w is T);
    }

    public List<string> GetOpenWindowGroupNames()
    {
        return windowGroupDict.Keys.ToList();
    }
}
