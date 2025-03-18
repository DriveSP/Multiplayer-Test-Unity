using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    public GameObject droppedItemPrefab;
    public List<Loot> lootsList = new List<Loot>();
    void Start()
    {
        
    }

    Loot GetDroppedItem()
    {
        int randomNumber = Random.Range(1, 101);
        List<Loot> possibleItems = new List<Loot>();
        foreach (Loot item in lootsList)
        {
            if (randomNumber <= item.dropChance)
            {
                possibleItems.Add(item);
            }
        }
        if(possibleItems.Count > 0)
        {
            Loot droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
            return droppedItem;
        }
        Debug.Log("No loot dropped");
        return null;
    }

    public void InstantiateLoot(Vector3 spawnPosition)
    {
        Loot droppedItem = GetDroppedItem();
    }
}
