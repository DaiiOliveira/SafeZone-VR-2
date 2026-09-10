using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    // Método simples para carregar uma fase pelo nome exato
    public void OpenSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Método dinâmico caso prefira carregar pelo número da fase
    public void OpenSceneByNumber(int levelNumber)
    {
        // Exemplo: se suas cenas se chamarem "Level1", "Level2", etc.
        SceneManager.LoadScene("Level " + levelNumber);
    }
}