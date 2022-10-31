using System;
using UnityEngine;

public static class DirectionFlag
{
    [Flags]
    public enum Direction
    {
        Up = 1,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = int.MaxValue
    };

    public static Vector2 GetDirection(Direction dir)
    {
        Vector2 dirVec = new Vector2(0, 0);
        if (dir.HasFlag(Direction.Up)) { dirVec += Vector2.up; }
        if (dir.HasFlag(Direction.Down)) { dirVec += Vector2.down; }
        if (dir.HasFlag(Direction.Right)) { dirVec += Vector2.right; }
        if (dir.HasFlag(Direction.Left)) { dirVec += Vector2.left; }
        return dirVec;
    }
}
