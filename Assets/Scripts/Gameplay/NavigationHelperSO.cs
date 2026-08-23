using Lechuza.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Navigation Helper", menuName = "Gameplay/Navigation Helper")]
public class NavigationHelperSO : ScriptableObject
{
    public void GoToScreen(string nodeId)
    {
        if (Services.TryGet<ScreenService>(out var screens))
            screens.GoTo(nodeId);
    }

    public void GoToScreen(ScreenNodeSO screen)
    {
        if (Services.TryGet<ScreenService>(out var screens))
            screens.GoTo(screen);
    }

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    public void LoadScene(int buildIndex) => SceneManager.LoadScene(buildIndex);

    public void ReloadActiveScene() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
