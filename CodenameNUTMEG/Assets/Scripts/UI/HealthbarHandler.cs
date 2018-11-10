using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarHandler : MonoBehaviour {

    [SerializeField] PlayerHealthHandler targetHealthHandler;
    [SerializeField] GameObject heartPrefab;

    List<GameObject> hearts = new List<GameObject>();

    private readonly int heartSpriteSize = 35;

    private void Start()
    {
        //cache a heart for each available health point.
        for(int i = 0; i < targetHealthHandler.GetMaxHealth(); i++)
        {
            GameObject newHeart = Instantiate(heartPrefab);

            newHeart.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
            newHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((heartSpriteSize * hearts.Count) - heartSpriteSize, 0);

            //cached hearts are not active, but are activated when needed.
            newHeart.SetActive(false);

            hearts.Add(newHeart);
        }

        //We cannot guarantee start() call order, so activate hearts based on InitHealth rather than CurrentHealth at this time.
        for (int i = 0; i < targetHealthHandler.GetInitHealth(); i++)
        {
            hearts[i].SetActive(true);
        }

        targetHealthHandler.OnHealthChangedEvent += UpdateHeartDisplay;
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
