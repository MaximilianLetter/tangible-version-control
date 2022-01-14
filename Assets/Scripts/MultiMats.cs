using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MultiMats
{
    public static Material[] BuildMaterials(Material mat, int matCount)
    {
        List<Material> matList = new List<Material>();

        for (int i = 0; i < matCount; i++)
        {
            matList.Add(mat);
        }

        return matList.ToArray();
    }
}
