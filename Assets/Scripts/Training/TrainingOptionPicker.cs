using System.Collections.Generic;
using UnityEngine;

public static class TrainingOptionPicker
{
    public static List<TrainingDefinition> PickRandomTrainings(int count)
    {
        List<TrainingDefinition> pool = TrainingCatalog.GetAllTrainings();
        List<TrainingDefinition> selected = new List<TrainingDefinition>();

        while (selected.Count < count && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return selected;
    }
}
