using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentLoader : MonoBehaviour
{
  public string EnvironmentName;
  public Vector3 environmentOffset;
  public Vector3 environmentPosition;
  // Start is called before the first frame update
  void Start()
  {
    //SceneManager.LoadSceneAsync(EnvironmentName, LoadSceneMode.Additive);
    StartCoroutine(LoadAndTranslate());
  }

  // Update is called once per frame
  void Update()
  {

  }
  IEnumerator LoadAndTranslate()
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(EnvironmentName, LoadSceneMode.Additive);
    while (!asyncLoad.isDone)
    {
      yield return null;
    }
    List<GameObject> rootObjects = new List<GameObject>();
    Scene scene = SceneManager.GetSceneByName(EnvironmentName);
    SceneManager.SetActiveScene(scene);
    scene.GetRootGameObjects(rootObjects);

    // iterate root objects and do something
    foreach (GameObject obj in rootObjects)
    {
      // obj.transform.Translate(environmentOffset);
      obj.transform.position = environmentPosition;
    }
  }
}
