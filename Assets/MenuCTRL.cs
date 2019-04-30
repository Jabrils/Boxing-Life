using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuCTRL : MonoBehaviour
{
    public static Texture2D main;
    public GameObject g;
    public Button[] button;
    public Color[] c = new Color[16];

    // Start is called before the first frame update
    void Start()
    {
        main = new Texture2D(16, 1);

        for (int i = 0; i < button.Length; i++)
        {
            button[i].onClick.AddListener(() =>
            {
                for (int j = 0; j < button.Length; j++)
                {
                    c[j] = new Color(Random.value, Random.value, Random.value);
                    button[j].GetComponent<Image>().color = c[j];
                    main = CTRL.SetNewBoxerTexture(c);
                }
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTexture();
        // 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    public void UpdateTexture()
    {
        foreach (SkinnedMeshRenderer smr in g.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.material.mainTexture = main;
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("game");
    }

    public void Socials(int which)
    {
        if (which == 0)
        {
            Application.OpenURL("https://twitter.com/jabrils_");
        }
        else if (which == 1)
        {
            Application.OpenURL("https://t.co/g4v1MxrXCG");
        }
        else if (which == 2)
        {
            Application.OpenURL("https://www.instagram.com/jabrils_/");
        }
    }
}
