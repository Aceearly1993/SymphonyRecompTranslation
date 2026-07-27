using System.Collections.Generic;
using RecompOne.Runtime.Memory;

namespace Sotn;

public enum PlayableCharacter { Alucard = 0, Richter = 1 }

public static class Game
{
    public const uint EntitiesAddr = 0x800733D8u;
    public const uint StatusAddr = 0x80097964u;
    public const uint PlayerStateAddr = 0x80072BD0u;
    public const uint PlayableCharacterAddr = 0x8003C9A0u;
    public const uint CurrentEntityAddr = 0x8006C3B8u;
    public const int EntityCount = 256;

    internal static IMemory M => RecompOne.Runtime.Runtime.Mem;

    public static bool Available => RecompOne.Runtime.Runtime.Mem != null; //game is only available when mem is instantiated, adding this so no one tries to do stuff before it can

    public static PlayableCharacter Character => (PlayableCharacter)(int)M.ReadU32(PlayableCharacterAddr);

    public static Entity CurrentEntity => new(M.ReadU32(CurrentEntityAddr));
}

public static class Entities
{
    public const int Count = Game.EntityCount;
    public const int StageStart = 64;
    public const int ServantStart = 0xD0;

    public static Entity At(int slot) => new(Game.EntitiesAddr + (uint)(slot * Entity.Stride));

    public static Entity Player => At(0);
    public static Entity Current => Game.CurrentEntity;

    public static Entity GetFree(int start, int end) => GameApi.GetFreeEntity(start, end);
    public static Entity GetFreeStage() => GameApi.GetFreeEntity(StageStart, Count);

    public static IEnumerable<Entity> All()
    {
        for (int i = 0; i < Count; i++)
        {
            var e = At(i);
            if (e.IsAlive) yield return e;
        }
    }

    public static IEnumerable<Entity> Enemies()
    {
        for (int i = 0; i < Count; i++)
        {
            var e = At(i);
            if (e.IsEnemy) yield return e;
        }
    }
}
