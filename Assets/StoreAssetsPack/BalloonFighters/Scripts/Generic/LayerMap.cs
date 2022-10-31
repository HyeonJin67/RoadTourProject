using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMap
{
    public static int Item  = 1 << LayerMask.NameToLayer("Item");
    public static int Body  = 1 << LayerMask.NameToLayer("Body");
}
