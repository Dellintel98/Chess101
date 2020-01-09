using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadDesiredScene(string desiredScene) 
    {
        //int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int desiredSceneIndex;

        switch (desiredScene) 
        {
            case "ChessMenu":
                desiredSceneIndex = 1;
                break;
            case "ChessGame":
                desiredSceneIndex = 2;
                break;
            case "GoMenu":
                desiredSceneIndex = 3;
                break;
            case "GoGame":
                desiredSceneIndex = 4;
                break;
            default:
                desiredSceneIndex = 0;
                break;
        }

        SceneManager.LoadScene(desiredSceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
