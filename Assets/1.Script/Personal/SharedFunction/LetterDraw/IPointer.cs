using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointer
{
    (int, Vector2, bool) NextLine();
    void Resurrect();
}
