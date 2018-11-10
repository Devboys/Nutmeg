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
        //cache a heart for each available health point.
        for(int i = 0; i < playerHealthScript.GetMaxHealth(); i++)
        {
            GameObject newHeart = Instantiate(heartPrefab);

            newHeart.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());

            newHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((heartSpriteSize * hearts.Count) - heartSpriteSize, 0);

            //cached hearts are not active, but are activated when needed.
            newHeart.SetActive(false);

            hearts.Add(newHeart);

        }

        //enable enough hearts to indicate initHealth
        for (int i = 0; i < playerHealthScript.GetInitHealth(); i++)
        {
            hearts[i].SetActive(true);
        }


        playerHealthScript.onHealthChangedEvent += UpdateHeartDisplay;
    }

    public void UpdateHeartDisplay(int totalNumHearts)
    {
        //empty heart list.
        for(int i = hearts.Count; i > 0; i--)
        {
            hearts[i - 1].SetActive(false);
        }

        //fill heart list.
        for (int i = 0; i < totalNumHearts; i++)
        {
            hearts[i].SetActive(true);
        }
    }

    

}
