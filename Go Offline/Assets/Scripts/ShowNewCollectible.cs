using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class ShowNewCollectible : MonoBehaviour
{
    public static ShowNewCollectible instance = null;

    [SerializeField] private CollectibleVisualManager prefabVisual;
    [SerializeField] private float showDuration;
    float animTimer;

    private void Awake()
    {
        //Initiate Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void GotNewCollectible(CollectibleUnit newUnit)
    {
        animTimer = showDuration;
        prefabVisual.gameObject.SetActive(true);
        prefabVisual.DisplayNewCollectibleUnit(newUnit);
    }

    public void LeveledUpCollectible(CollectibleUnit leveledUnit)
    {

    }

    private void Update()
    {
        if (!prefabVisual.gameObject.activeInHierarchy)
        {
            return;
        }

        animTimer -= Time.deltaTime;
        if(animTimer <= 0)
        {
            prefabVisual.gameObject.SetActive(false);
        }
    }
}
