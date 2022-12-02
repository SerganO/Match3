using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaseResourcesSuppliers
{
    public static ResourcesSupplier<Sprite> SpriteSupplier = new ResourcesSupplier<Sprite>("Sprites");
    public static ResourcesSupplier<AudioClip> AudioSupplier = new ResourcesSupplier<AudioClip>("Audios");
    public static ResourcesSupplier<GameObject> PrefabsSupplier = new ResourcesSupplier<GameObject>("Prefabs");
}
