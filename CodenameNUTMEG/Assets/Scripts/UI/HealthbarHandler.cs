using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarHandler : MonoBehaviour {

    [SerializeField] private PlayerHealthHandler targetHealthHandler;
    [SerializeField] private bool enableZeldaHearts;

    [Header("UI Prefabs")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject halfHeartPrefab;

    List<GameObject> hearts = new List<GameObject>();
    GameObject halfHeart;

    private readonly int heartSpriteSize = 45;

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

        halfHeart = Instantiate(halfHeartPrefab);
        halfHeart.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
        halfHeart.SetActive(false);

        //We cannot guarantee start() call order, so activate hearts based on InitHealth rather than CurrentHealth at this time.
        UpdateHeartDisplay(targetHealthHandler.GetInitHealth());

        targetHealthHandler.OnHealthChangedEvent += UpdateHeartDisplay;
    }

    public void UpdateHeartDisplay(int numHealthToDisplay)
    {
        
        int numHearts = enableZeldaHearts ? numHealthToDisplay / 2 : numHealthToDisplay;
        bool showHalfHeart = enableZeldaHearts ? (numHealthToDisplay % 2 > 0) : false;

        //empty heart list.
        for(int i = hearts.Count; i > 0; i--)
        {
            hearts[i - 1].SetActive(false);
        }

        //fill heart list.
        for (int i = 0; i < numHearts; i++)
        {
            hearts[i].SetActive(true);
        }

        if (showHalfHeart && enableZeldaHearts)
        {
            halfHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((heartSpriteSize * numHearts) - heartSpriteSize, 0);
            halfHeart.SetActive(true);
        }
        else
        {
            halfHeart.SetActive(false);
        }
    }

    

}
