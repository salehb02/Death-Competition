using UnityEditor;

public class EditorWindows : Editor
{
    [MenuItem("Death Match/Game Manager", false, -68)]
    public static void OpenGameManager()
    {
        Selection.activeObject = GameManager.Instance;
    }
}