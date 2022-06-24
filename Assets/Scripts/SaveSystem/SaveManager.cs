using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour {
    public string[] saveFiles;

    public TMP_InputField saveName;
    public GameObject loadButtonPrefab;
    public Transform loadArea;

    public void OnSave() {
        SerializationManager.Save(saveName.text, SaveData.current);
    }

    public void GetLoadFiles() {
        if (!Directory.Exists(Application.persistentDataPath + $"/{SerializationManager.SAVESFOLDERNAME}/")) {
            Directory.CreateDirectory(Application.persistentDataPath + $"/{SerializationManager.SAVESFOLDERNAME}/");
        }

        saveFiles = Directory.GetFiles(Application.persistentDataPath + $"/{SerializationManager.SAVESFOLDERNAME}/");
    }

    public void ShowLoadScreen() {
        GetLoadFiles();

        foreach (Transform button in loadArea) {
            Destroy(button.gameObject);
        }

        for (int i = 0; i < saveFiles.Length; i++) {
            GameObject buttonObj = Instantiate(loadButtonPrefab);
            buttonObj.transform.SetParent(loadArea.transform, false);

            var index = i;
            buttonObj.GetComponent<Button>().onClick.AddListener(() => {
                // OnLoad(saveFiles[index])
            });
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = saveFiles[index].Replace(Application.persistentDataPath + $"/{SerializationManager.SAVESFOLDERNAME}/", "");
        }
    }
}
