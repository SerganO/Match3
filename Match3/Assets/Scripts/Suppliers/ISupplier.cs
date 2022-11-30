using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISupplier<T>
{
    T GetObjectForID(string objectId);
    T GetObjectForID(string objectId, params string[] subfolders);
}
