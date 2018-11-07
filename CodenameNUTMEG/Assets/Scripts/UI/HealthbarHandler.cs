using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarHandler : MonoBehaviour {

    [SerializeField] PlayerHealthHandler playerHealthScript;
    [SerializeField] GameObject heartPrefab;

    List<GameObject> hearts = new List<GameObject>();

    private readonly int heartSpriteSize = 35;

    private void Start()
    {
        for(int i = 0; i < playerHealthScript.GetInitHealth(); i++)
        {
            GameObject newHeart = Instantiate(heartPrefab);

            newHeart.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());

            newHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((heartSpriteSize * hearts.Count) - heartSpriteSize, 0);

            hearts.Add(newHeart);
        }

        playerHealthScript.onHealthChangedEvent += UpdateHeartDisplay;
    }

    public void UpdateHeartDisplay(int totalNumHearts)
    {
        //empty heart list.
        for(int i = hearts.Count; i > 0; i--)
        {
            Destroy(hearts[i - 1]);
            hearts.RemoveAt(i - 1);
        }

        //fill heart list.
        for (int i = 0; i < totalNumHearts; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab);

            newHeart.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());

            newHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((heartSpriteSize * hearts.Count) - heartSpriteSize, 0);

            hearts.Add(newHeart);
        }
    }

    

}
