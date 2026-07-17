// sotn.io Randomizer Compatibility Patches by: MottZilla
// These patches allow various aspects of the randomizer to function as intended. Because the game is recompiled from the
// original unmodified game all code changes made by the randomizer are missing. Here we patch various functions to read from
// the patched overlay data to match the behavior that is intended. Complex presets would need large amounts of code re-implemented.
// Currently we are just doing the basics for simple presets.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Memory;

namespace Recompiled;

public enum PresetId : byte { None, Lycanthrope, Nimble, NimbleLite, Expedition, Warlock }

public static partial class RandoPatch
{ 
    static bool _initialized;

    static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
    }

    // Helper Function for comparing in game memory preset string to given parameter string
    public static bool PresetNameIs(CpuContext c, IMemory m,string CheckString)
    {
        UInt32 PresetBaseOffset = 0x801A78E5;
        byte StringIndex = 0;
        byte ReadIndex = 0;
        byte ReadByte = 0;

        // Console.WriteLine("Comparing Preset Name to: " + CheckString);

        while (true)
        {
            // Console.WriteLine("Preset Name Checking Position" + ReadIndex);
            ReadByte = m.ReadU8(PresetBaseOffset + ReadIndex);  // Read a byte from preset string in game memory
            ReadIndex++;

            if (ReadByte == 0)
                break;

            // Console.WriteLine("Preset Name ReadByte equals " + $"Hex: {ReadByte:X}" + "CheckString Byte equals " + $"Hex: {(int)CheckString[StringIndex]:X}");

            if (ReadByte == 0x81) // Detecting hyphens
            {
                if(m.ReadU8(PresetBaseOffset + ReadIndex) == 0x7C)
                {
                    ReadByte = 0x2D;
                    ReadIndex++;
                }
            }

            if (ReadByte != CheckString[StringIndex])
            {
                // Console.WriteLine("Failed Preset Match on position:" + ReadIndex);
                return false;
            }
            if (CheckString[StringIndex] == 0)
                break;

            StringIndex++;
        }

        // Console.WriteLine("Found Preset Name Match for: " + CheckString);
        return true;
    }

    public static bool PreHandleGravityBootsMP(CpuContext c, IMemory m)
    {
        byte CUR_PRESET = m.ReadU8(0x8000C000);

        if (CUR_PRESET == (byte)PresetId.NimbleLite)    // Gravity Boots free to use in this preset
        {
            c.V0 = 0;       // Set Return Value to 0
            return false;   // Do not Execute HandleGravityBootsMP
        }

        return true;        // Execute HandleGravityBootsMP normally.
    }

    // Handles various presets discounted transformation MP costs
    public static void PreHandleTransformationMP(CpuContext c, IMemory m)
    {
        byte CUR_PRESET = m.ReadU8(0x8000C000);
        UInt32 g_GameTimer = m.ReadU32(0x8003c8c4);
        UInt32 CUR_MP = m.ReadU32(0x80097BB0);

        if (CUR_PRESET == (byte)PresetId.Lycanthrope)
        {
            if (c.A0 == 2 && c.A1 == 1 && g_GameTimer % 120 == 0)    // A0 == WOLF, A1 = Reduce MP
            {
                CUR_MP++;   // Offset MP about to be consumed
                m.WriteU32(0x80097BB0,CUR_MP);
            }
        }
        if (CUR_PRESET == (byte)PresetId.Warlock)
        {
            if (m.ReadU8(0x8009796C) > 1 && c.A0 == 1 && c.A1 == 1 && g_GameTimer % 30 == 0)    // Power of Mist Active A0 == MIST, A1 = Reduce MP
            {
                CUR_MP+= 2;   // Offset MP about to be consumed
                m.WriteU32(0x80097BB0,CUR_MP);
            }
            else
            {
                if (m.ReadU8(0x8009796C) < 2 && c.A0 == 1 && c.A1 == 1 && g_GameTimer % 8 == 0)    // Power of Mist not Active A0 == MIST, A1 = Reduce MP
                {
                    CUR_MP+=10;   // Offset MP about to be consumed
                    m.WriteU32(0x80097BB0, CUR_MP);
                }
            }
        }
    }

    // Gives starting relics for various presets.
    public static void SetupStartingRelics(CpuContext c, IMemory m)
    {
        byte CUR_PRESET;

        CUR_PRESET = m.ReadU8(0x8000C000);

        if (CUR_PRESET == (byte)PresetId.Lycanthrope)
        {
            m.WriteU32(0x80097964 + 4, 0x00030303); // Soul of Wolf, Power of Wolf, Skill of Wolf
        }
        if (CUR_PRESET == (byte)PresetId.Nimble || CUR_PRESET == (byte)PresetId.NimbleLite || CUR_PRESET == (byte)PresetId.Expedition)
        {
            m.WriteU8(0x80097964, 0x03);    // Soul of Bat
            m.WriteU16(0x80097970, 0x0303); // Gravity Boots & Leap Stone
        }
        if (CUR_PRESET == (byte)PresetId.Warlock)
        {
            m.WriteU8(0x80097964 + 7, 0x03);    // Form of Mist
        }
    }

    // Detects randomizer preset by reading game memory and sets an identifier in game memory to be used by other functions.
    public static void DetectPreset(CpuContext c, IMemory m)
    {
        byte CUR_PRESET = m.ReadU8(0x8000C000);

        if (CUR_PRESET != 0)
            return;

        // Lycanthrope
        if (PresetNameIs(c, m, "lycanthrope"))
        {
            m.WriteU8(0x8000C000, (byte)PresetId.Lycanthrope);
        }
        // Nimble
        if (PresetNameIs(c, m, "nimble"))
        {
            m.WriteU8(0x8000C000, (byte)PresetId.Nimble);
        }
        // Nimble-Lite
        if(PresetNameIs(c,m,"nimble-lite"))
        {
            m.WriteU8(0x8000C000, (byte)PresetId.NimbleLite);
        }
        // Warlock
        if (PresetNameIs(c, m, "warlock"))
        {
            m.WriteU8(0x8000C000, (byte)PresetId.Warlock);
        }
        // Expedition
        if (PresetNameIs(c, m, "expedition"))
        {
            m.WriteU8(0x8000C000, (byte)PresetId.Expedition);
        }
    }

    public static void InitStatsAndGear(CpuContext c, IMemory m)
    {
        // Modified to read Overlay to get updated Starting Gear and updated Prologue Reward Items.
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x38D0u));
        c.SP = c.SP - 0x18u;
        m.WriteU32((c.SP + 0x14u), c.RA);
        if (c.V0 == 0u)
        {
            m.WriteU32((c.SP + 0x10u), c.S0);
            goto L800FF7E8;
        }
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.RA = 0x800FF7D8u;
        SoTN.func_800F53A4(c, m);
        c.RA = 0x800FF7E0u;
        SoTN.UpdateCapePalette(c, m);
        goto L8010073C;
    L800FF7E8:;
        c.V0 = 0u | 0x0001u;
        if (c.A0 != c.V0)
        {
            c.S0 = 0u | 0x07FFu;
            goto L800FF9D8;
        }
        c.S0 = 0u | 0x07FFu;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C00u;
        c.V0 = m.ReadU32(c.A0);
        //c.V1 = 0u | 0x007Bu;              // 7B = Alucard Sword
        c.V1 = m.ReadU16(0x800FF800);       // Read Right Hand Starting Weapon Value from Overlay
        if (c.V0 != c.V1)                   // If Right Hand Doesn't have (default) Alucard Sword...
        {
            goto L800FF814;
        }
        m.WriteU32(c.A0, 0u);
        goto L800FF85C;
    L800FF814:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7C04u));
        if (c.V0 != c.V1)
        {
            goto L800FF838;
        }
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C04u), 0u);
        goto L800FF854;
    L800FF838:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x7A05u));
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF83C)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF854;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x7A05u), (byte)c.V0);
        m.WriteU8((c.At + m.ReadU16(0x800FF850)), (byte)c.V0);
    L800FF854:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C00u;
    L800FF85C:;
        c.V0 = m.ReadU32(c.A0);
        //c.V1 = 0u | 0x0010u;              // 10 = Alucard Shield
        c.V1 = m.ReadU16(0x800FF860);       // Read Left Hand Starting Weapon Value from Overlay
        if (c.V0 != c.V1)
        {
            goto L800FF874;
        }
        m.WriteU32(c.A0, 0u);
        goto L800FF8B4;
    L800FF874:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7C04u));
        if (c.V0 != c.V1)
        {
            goto L800FF898;
        }
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C04u), 0u);
        goto L800FF8B4;
    L800FF898:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x799Au));                // Read Inventory Count of Alucard Shields
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF89C)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF8B4;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x799Au), (byte)c.V0);          // Write back new Count
        m.WriteU8((c.At + m.ReadU16(0x800FF8B0)), (byte)c.V0);
    L800FF8B4:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C08u;
        c.V1 = m.ReadU32(c.A0);
        //c.V0 = 0u | 0x002Du;              // 2D = Dragon Helm
        c.V0 = m.ReadU16(0x800FF8C0);       // Read Starting Helm from Overlay
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x001Au;
            goto L800FF8D4;
        }
        c.V0 = 0u | 0x001Au;
        m.WriteU32(c.A0, c.V0);
        goto L800FF8F0;
    L800FF8D4:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x7A60u));
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF8D8)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF8F0;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x7A60u), (byte)c.V0);
        m.WriteU8((c.At + m.ReadU16(0x800FF8EC)), (byte)c.V0);
    L800FF8F0:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C0Cu;
        c.V1 = m.ReadU32(c.A0);
        //c.V0 = 0u | 0x000Fu;              // 0F = Alucard Mail
        c.V0 = m.ReadU16(0x800FF8FC);       // Read Starting Body Armor Value from Overlay
        if (c.V1 != c.V0)
        {
            goto L800FF910;
        }
        m.WriteU32(c.A0, 0u);
        goto L800FF92C;
    L800FF910:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x7A42u));
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF914)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF92C;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x7A42u), (byte)c.V0);
        m.WriteU8((c.At + m.ReadU16(0x800FF928)), (byte)c.V0);
    L800FF92C:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C10u;
        c.V1 = m.ReadU32(c.A0);
        //c.V0 = 0u | 0x0038u;              // 38 = Twilight Cloak
        c.V0 = m.ReadU16(0x800FF938);       // Read Starting Cape Value from Overlay
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x0030u;
            goto L800FF954;
        }
        c.V0 = 0u | 0x0030u;
        m.WriteU32(c.A0, c.V0);
        c.RA = 0x800FF94Cu;
        SoTN.UpdateCapePalette(c, m);
        goto L800FF970;
    L800FF954:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x7A6Bu));    // Item Count
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF958)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF970;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x7A6Bu), (byte)c.V0);
        m.WriteU8((c.At + m.ReadU16(0x800FF96C)), (byte)c.V0);
    L800FF970:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7C14u;
        c.V0 = m.ReadU32(c.A0);
        //c.V1 = 0u | 0x004Eu;              // 4E = Necklace of J
        c.V1 = m.ReadU16(0x800FF97C);       // Read Starting Acc1 Value from Overlay
        if (c.V0 != c.V1)
        {
            c.V0 = 0u | 0x0039u;
            goto L800FF990;
        }
        c.V0 = 0u | 0x0039u;
        m.WriteU32(c.A0, c.V0);
        goto L80100734;
    L800FF990:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7C18u));
        if (c.V0 != c.V1)
        {
            c.V0 = 0u | 0x0039u;
            goto L800FF9B4;
        }
        c.V0 = 0u | 0x0039u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C18u), c.V0);
        goto L80100734;
    L800FF9B4:;
        c.V0 = 0x80090000u;
        //c.V0 = m.ReadU8((c.V0 + 0x7A81u));      // Read Necklace of J Item Count
        c.V0 = m.ReadU8((c.V0 + m.ReadU16(0x800FF9B8)));
        if (c.V0 == 0u)
        {
            c.V0 = c.V0 - 0x1u;
            goto L80100734;
        }
        c.V0 = c.V0 - 0x1u;
        c.At = 0x80090000u;
        //m.WriteU8((c.At + 0x7A81u), (byte)c.V0);  // Update item Count
        m.WriteU8((c.At + m.ReadU16(0x800FF9CC)), (byte)c.V0);
        goto L80100734;
    L800FF9D8:;
        c.V0 = 0x80070000u;
        c.V0 = c.V0 - 0x3C8Du;
    L800FF9E0:;
        m.WriteU8(c.V0, (byte)0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FF9E0;
        }
        c.V0 = c.V0 - 0x1u;
        c.S0 = 0u | 0x0003u;
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x7BF8u;
        c.V1 = c.V0 - 0x24u;
        c.At = 0x80040000u;
        m.WriteU32((c.At - 0x38A0u), 0u);
        m.WriteU32(c.V0, 0u);
    L800FFA0C:;
        m.WriteU32(c.V1, 0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V1 = c.V1 - 0x4u;
            goto L800FFA0C;
        }
        c.V1 = c.V1 - 0x4u;
        c.S0 = 0u + 0u;
        c.A0 = 0u | 0x0001u;
        c.V1 = 0u + 0u;
        c.V0 = 0u | 0x0001u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BECu), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BE8u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BF4u), 0u);
    L800FFA44:;
        c.At = 0x80090000u;
        c.At = c.At + c.V1;
        m.WriteU32((c.At + 0x7C44u), c.A0);
        c.At = 0x80090000u;
        c.At = c.At + c.V1;
        m.WriteU32((c.At + 0x7C48u), 0u);
        c.At = 0x80090000u;
        c.At = c.At + c.V1;
        m.WriteU32((c.At + 0x7C4Cu), 0u);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 7 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V1 = c.V1 + 0xCu;
            goto L800FFA44;
        }
        c.V1 = c.V1 + 0xCu;
        c.S0 = 0u + 0u;
    L800FFA7C:;
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        m.WriteU8((c.At + 0x798Au), (byte)0u);
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        m.WriteU8((c.At + 0x7A8Du), (byte)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 169 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L800FFA7C;
        }
        c.S0 = 0u + 0u;
    L800FFAA8:;
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        m.WriteU8((c.At + 0x7A33u), (byte)0u);
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        m.WriteU8((c.At + 0x7B36u), (byte)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 90 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L800FFAA8;
        }
        c.V0 = 0u | 0x0001u;
        c.S0 = 0u | 0x0007u;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x798Au;
        c.A0 = c.V1 - 0x1u;
        m.WriteU8(c.V1, (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7A4Du), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7A33u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7A63u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7A6Cu), (byte)c.V0);
    L800FFB04:;
        m.WriteU8(c.A0, (byte)0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.A0 = c.A0 - 0x1u;
            goto L800FFB04;
        }
        c.A0 = c.A0 - 0x1u;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7B9Cu;
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x74A0u));
        c.V0 = 0u | 0x001Fu;
        if (c.V1 == c.V0)
        {
            m.WriteU32(c.A0, 0u);
            goto L800FFB44;
        }
        m.WriteU32(c.A0, 0u);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3660u));
        if (c.V0 == 0u)
        {
            goto L800FFD60;
        }
    L800FFB44:;
        c.V1 = 0u | 0x0001u;
        c.S0 = 0u | 0x001Du;
        c.V0 = c.A0 - 0x21Bu;
    L800FFB50:;
        m.WriteU8(c.V0, (byte)c.V1);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V0 = c.V0 - 0x1u;
            goto L800FFB50;
        }
        c.V0 = c.V0 - 0x1u;
        c.S0 = 0u | 0x001Fu;
        c.A1 = 0x80040000u;
        c.A1 = c.A1 - 0x355Cu;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x796Eu;
        c.V0 = m.ReadU8(c.A0);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU8((c.V1 + 0x796Fu));
        c.V0 = c.V0 | 0x0002u;
        m.WriteU8(c.A0, (byte)c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU8((c.V0 + 0x7973u));
        c.V1 = c.V1 | 0x0002u;
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x796Fu), (byte)c.V1);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU8((c.V1 + 0x7974u));
        c.V0 = c.V0 | 0x0002u;
        c.V1 = c.V1 | 0x0002u;
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7973u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7974u), (byte)c.V1);
    L800FFBBC:;
        m.WriteU32(c.A1, 0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.A1 = c.A1 - 0x4u;
            goto L800FFBBC;
        }
        c.A1 = c.A1 - 0x4u;
        c.S0 = 0x80090000u;
        c.S0 = c.S0 + 0x7BFCu;
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x74A0u));
        c.V0 = 0u | 0x001Fu;
        c.At = 0x80040000u;
        m.WriteU32((c.At - 0x3500u), 0u);
        c.At = 0x80040000u;
        m.WriteU32((c.At - 0x34FCu), 0u);
        if (c.V1 == c.V0)
        {
            m.WriteU32(c.S0, 0u);
            goto L800FFC3C;
        }
        m.WriteU32(c.S0, 0u);
        c.V0 = 0u | 0x0041u;
        if (c.V1 == c.V0)
        {
            goto L800FFC3C;
        }
        c.RA = 0x800FFC0Cu;
        SoTN.rand(c, m);
        c.V1 = 0x38E30000u;
        c.V1 = c.V1 | 0x8E39u;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = (uint)((int)c.V0 >> 31);
        c.A0 = c.HI;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.A0 = c.A0 - c.V1;
        c.V1 = c.A0 << 3;
        c.V1 = c.V1 + c.A0;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 + 0x1u;
        m.WriteU32(c.S0, c.V0);
    L800FFC3C:;
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x74A0u));
        c.V0 = 0u | 0x0032u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA4u), c.V0);
        c.V0 = 0u | 0x001Eu;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA8u), c.V0);
        c.V0 = 0u | 0x0063u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.V0 = 0u | 0x0014u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB0u), c.V0);
        c.V0 = 0u | 0x000Au;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V0);
        c.V0 = 0u | 0x001Au;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C08u), c.V0);
        c.V0 = 0u | 0x0030u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C10u), c.V0);
        c.V0 = 0u | 0x0039u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C14u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C18u), c.V0);
        c.V0 = 0u | 0x0041u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BF0u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C00u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C04u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C0Cu), 0u);
        if (c.V1 != c.V0)
        {
            c.A0 = 0u | 0x001Au;
            goto L800FFD38;
        }
        c.A0 = 0u | 0x001Au;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x800FFD08u;
        SoTN.TimeAttackController(c, m);
        c.A0 = 0u | 0x0009u;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x800FFD14u;
        SoTN.TimeAttackController(c, m);
        c.A0 = 0u | 0x0004u;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x800FFD20u;
        SoTN.TimeAttackController(c, m);
        c.A0 = 0u | 0x000Eu;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x800FFD2Cu;
        SoTN.TimeAttackController(c, m);
        c.A0 = 0u | 0x000Cu;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x800FFD38u;
        SoTN.TimeAttackController(c, m);
    L800FFD38:;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C30u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C34u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C38u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C3Cu), 0u);
        goto L80100734;
    L800FFD60:;
        c.V0 = 0u | 0x0041u;
        if (c.V1 != c.V0)
        {
            c.S0 = 0u | 0x001Fu;
            goto L8010031C;
        }
        c.S0 = 0u | 0x001Fu;
        c.S0 = 0u | 0x001Du;
        c.V1 = c.A0 - 0x21Bu;
        c.V0 = 0u | 0x0006u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BF0u), 0u);
    L800FFDA0:;
        m.WriteU8(c.V1, (byte)0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V1 = c.V1 - 0x1u;
            goto L800FFDA0;
        }
        c.V1 = c.V1 - 0x1u;
        c.V0 = 0x80140000u;
        c.V0 = m.ReadU32((c.V0 - 0x6804u));
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x009Fu;
            goto L800FFDD4;
        }
        //c.A0 = 0u | 0x009Fu;                  // 9F = Potion
        c.A0 = 0u | m.ReadU16(0x800FFDC0);      // Read Reward Id from Overlay Data
        c.A1 = 0u + 0u;
        c.RA = 0x800FFDCCu;
        SoTN.AddToInventory(c, m);
        c.S0 = 0u | 0x0003u;
        goto L800FFE94;
    L800FFDD4:;
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x7BA0u));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BA4u));
        if (c.A0 != c.V1)
        {
            c.V0 = c.V1 >> 31;
            goto L800FFE48;
        }
        c.V0 = c.V1 >> 31;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BB8u));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BBCu));
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BC0u));
        c.V1 = c.V1 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V1);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BC4u));
        c.V0 = c.V0 + 0x1u;
        c.V1 = c.V1 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V1);
        c.S0 = 0u + 0u;
        goto L800FFE94;
    L800FFE48:;
        c.V0 = c.V1 + c.V0;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.V0 = (int)c.A0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S0 = 0u | 0x0002u;
            goto L800FFE7C;
        }
        c.S0 = 0u | 0x0002u;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BB8u));
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
        c.S0 = 0u | 0x0001u;
        goto L800FFE94;
    L800FFE7C:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BBCu));
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V0);
    L800FFE94:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BA8u));
        if (c.V0 != 0u)
        {
            c.V0 = (int)c.S0 < 3 ? 1u : 0u;
            goto L800FFEB8;
        }
        c.V0 = (int)c.S0 < 3 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x008Eu;
            goto L800FFEB8;
        }
        //c.A0 = 0u | 0x008Eu;              // 8E = Heart Refresh
        c.A0 = 0u | m.ReadU16(0x800FFEAC);  // Read Reward ID from Overlay Data
        c.A1 = 0u + 0u;
        c.RA = 0x800FFEB8u;
        SoTN.AddToInventory(c, m);
    L800FFEB8:;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BA4u;
        c.V0 = 0u | 0x0046u;
        if (c.S0 != 0u)
        {
            m.WriteU32(c.V1, c.V0);
            goto L800FFED4;
        }
        m.WriteU32(c.V1, c.V0);
        c.V0 = 0u | 0x004Bu;
        m.WriteU32(c.V1, c.V0);
    L800FFED4:;
        c.V0 = 0u | 0x000Au;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA8u), c.V0);
        c.V0 = 0u | 0x0032u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.V0 = 0x80140000u;
        c.V0 = m.ReadU32((c.V0 - 0x6FF8u));
        c.V1 = 0u | 0x0014u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.V1);
        c.V0 = (int)c.V0 < 41 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x0047u;
            goto L800FFF34;
        }
        //c.A0 = 0u | 0x0047u;              // 47 = Neutron Bomb
        c.A0 = 0u | m.ReadU16(0x800FFF08);  // Read Reward ID from Overlay Data
        c.A1 = 0u + 0u;
        c.RA = 0x800FFF14u;
        SoTN.AddToInventory(c, m);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BC0u));
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        goto L800FFF4C;
    L800FFF34:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BB8u));
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
    L800FFF4C:;
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BFCu));
        c.V0 = 0u | 0x0004u;
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x0003u;
            goto L800FFF9C;
        }
        c.V0 = 0u | 0x0003u;
        c.V0 = (int)c.S0 < 3 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u + 0u;
            goto L80100084;
        }
        c.A0 = 0u + 0u;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BACu));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BB4u));
        c.V0 = c.V0 + 0x5u;
        c.V1 = c.V1 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.V1);
        goto L80100080;
    L800FFF9C:;
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x0001u;
            goto L800FFFE0;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u + 0u;
            goto L80100084;
        }
        c.A0 = 0u + 0u;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BACu));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BC0u));
        c.V0 = c.V0 + 0x5u;
        c.V1 = c.V1 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V1);
        goto L80100080;
    L800FFFE0:;
        if (c.S0 == c.V0)
        {
            c.V0 = (int)c.S0 < 2 ? 1u : 0u;
            goto L80100050;
        }
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L80100000;
        }
        if (c.S0 == 0u)
        {
            c.A0 = 0u + 0u;
            goto L80100014;
        }
        c.A0 = 0u + 0u;
        goto L80100084;
    L80100000:;
        c.V0 = 0u | 0x0002u;
        if (c.S0 == c.V0)
        {
            c.A0 = 0u + 0u;
            goto L80100068;
        }
        c.A0 = 0u + 0u;
        goto L80100084;
    L80100014:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BC4u));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BBCu));
        c.V0 = c.V0 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BC0u));
        c.V1 = c.V1 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V1);
        c.V0 = c.V0 + 0x1u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
    L80100050:;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BA4u;
        c.V0 = m.ReadU32(c.V1);
        c.V0 = c.V0 + 0x5u;
        m.WriteU32(c.V1, c.V0);
    L80100068:;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BB8u;
        c.V0 = m.ReadU32(c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU32(c.V1, c.V0);
    L80100080:;
        c.A0 = 0u + 0u;
    L80100084:;
        c.A1 = 0u + 0u;
        c.RA = 0x8010008Cu;
        SoTN.TimeAttackController(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 101 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = (int)c.V1 < 201 ? 1u : 0u;
            goto L80100134;
        }
        c.V0 = (int)c.V1 < 201 ? 1u : 0u;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BA4u;
        c.V0 = m.ReadU32(c.V1);
        c.V0 = c.V0 + 0x5u;
        m.WriteU32(c.V1, c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BB4u));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BACu));
        c.V0 = c.V0 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BB8u));
        c.V1 = c.V1 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V1);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BBCu));
        c.V0 = c.V0 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x7BC0u));
        c.V1 = c.V1 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V1);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BC4u));
        c.V0 = c.V0 + 0x5u;
        c.V1 = c.V1 + 0x5u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V1);
        c.S0 = 0u + 0u;
        goto L80100190;
    L80100134:;
        if (c.V0 == 0u)
        {
            goto L80100150;
        }
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BC4u;
        c.V0 = m.ReadU32(c.V1);
        c.V0 = c.V0 + 0x2u;
        goto L80100188;
    L80100150:;
        c.V0 = (int)c.V1 < 301 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = (int)c.V1 < 1000 ? 1u : 0u;
            goto L8010016C;
        }
        c.V0 = (int)c.V1 < 1000 ? 1u : 0u;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BC4u;
        goto L8010017C;
    L8010016C:;
        if (c.V0 != 0u)
        {
            c.S0 = 0u + 0u;
            goto L80100190;
        }
        c.S0 = 0u + 0u;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x7BBCu;
    L8010017C:;
        c.V0 = m.ReadU32(c.V1);
        c.V0 = c.V0 + 0x1u;
    L80100188:;
        m.WriteU32(c.V1, c.V0);
        c.S0 = 0u + 0u;
    L80100190:;
        c.A1 = 0x800A0000u;
        c.A1 = m.ReadU32((c.A1 + 0x300Cu));
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7BA4u));
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x7BB4u));

        // Preset Setup
        SetupStartingRelics(c, m);

        // Starting Equipment Setup

        //c.V0 = 0u | 0x007Bu;              // 7B = Alucard Sword
        c.V0 = m.ReadU16(0x801001A8);       // Read Right Hand Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C00u), c.V0);
        //c.V0 = 0u | 0x0010u;              // 10 = Alucard Shield
        c.V0 = m.ReadU16(0x801001B4);       // Read Left Hand Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C04u), c.V0);
        //c.V0 = 0u | 0x002Du;              // 2D = Dragon Helm
        c.V0 = m.ReadU16(0x801001C0);       // Read Head Item Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C08u), c.V0);
        //c.V0 = 0u | 0x000Fu;              // 0F = Alucard Mail
        c.V0 = m.ReadU16(0x801001CC);       // Read Body Armor Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C0Cu), c.V0);
        //c.V0 = 0u | 0x0038u;              // 38 = Twilight Cape
        c.V0 = m.ReadU16(0x801001D8);       // Read Cape Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C10u), c.V0);
        //c.V0 = 0u | 0x004Eu;              // 4E = Necklace of J
        c.V0 = m.ReadU16(0x801001E4);       // Read Acc1 Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C14u), c.V0);
        //c.V0 = 0u | 0x0039u;              // 39 = No Accessory
        c.V0 = m.ReadU16(0x801001F0);       // Read Acc2 Value from Overlay
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BFCu), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C18u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA0u), c.V1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB0u), c.A0);
    L80100214:;
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        c.V1 = m.ReadU8((c.At + 0x7B90u));
        c.V0 = m.ReadU8(c.A1);
        if (c.V1 != c.V0)
        {
            c.A1 = c.A1 + 0x1u;
            goto L80100240;
        }
        c.A1 = c.A1 + 0x1u;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 8 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L80100214;
        }
    L80100240:;
        c.V0 = 0u | 0x0008u;
        if (c.S0 != c.V0)
        {
            c.V1 = 0u | 0x0001u;
            goto L801002B4;
        }
        c.V1 = 0u | 0x0001u;
        c.V0 = 0u | 0x0063u;
        c.A0 = 0u | 0x0019u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V0);
        c.V0 = 0u | 0x0005u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA8u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.V0 = 0u | 0x0046u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB8u), c.V1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA4u), c.A0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.V1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA0u), c.A0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB0u), c.V1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C18u), c.V0);
    L801002B4:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x4220u));
        if (c.V0 == 0u)
        {
            goto L80100734;
        }
        c.A1 = 0x800A0000u;
        c.A1 = m.ReadU32((c.A1 + 0x3010u));
        c.S0 = 0u + 0u;
    L801002D4:;
        c.At = 0x80090000u;
        c.At = c.At + c.S0;
        c.V1 = m.ReadU8((c.At + 0x7B90u));
        c.V0 = m.ReadU8(c.A1);
        if (c.V1 != c.V0)
        {
            c.A1 = c.A1 + 0x1u;
            goto L80100300;
        }
        c.A1 = c.A1 + 0x1u;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 8 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801002D4;
        }
    L80100300:;
        c.V0 = 0u | 0x0008u;
        if (c.S0 != c.V0)
        {
            c.A0 = 0u | 0x0019u;
            goto L80100734;
        }
        c.A0 = 0u | 0x0019u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100314u;
        SoTN.AddToInventory(c, m);
        goto L80100734;
    L8010031C:;
        c.V0 = 0x80040000u;
        c.V0 = c.V0 - 0x355Cu;
    L80100324:;
        m.WriteU32(c.V0, 0u);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V0 = c.V0 - 0x4u;
            goto L80100324;
        }
        c.V0 = c.V0 - 0x4u;
        c.V1 = 0x00070000u;
        c.V1 = c.V1 | 0xA120u;
        c.A1 = 0x80090000u;
        c.A1 = c.A1 + 0x7BB8u;
        c.V0 = 0u | 0x0006u;
        m.WriteU32(c.A1, c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BBCu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC0u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BC4u), c.V0);
        c.V0 = 0u | 0x0046u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA4u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA0u), c.V0);
        c.V0 = 0u | 0x000Au;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA8u), c.V0);
        c.V0 = 0u | 0x0032u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BF0u), c.V1);
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x74A0u));
        c.A0 = 0u | 0x0014u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.V0 = 0u | 0x04D2u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BA8u), c.V0);
        c.V0 = 0u | 0x07D0u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BACu), c.V0);
        c.V0 = 0u | 0x2AF8u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB4u), c.A0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BB0u), c.A0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BECu), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BE8u), c.A0);
        c.V1 = c.V1 & 0x0020u;
        if (c.V1 == 0u)
        {
            c.A3 = 0u | 0x0003u;
            goto L801003FC;
        }
        c.A3 = 0u | 0x0003u;
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0xADB0u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BECu), c.V0);
    L801003FC:;
        c.A2 = 0u | 0x0001u;
        c.V1 = c.A1 - 0x254u;
        c.A0 = 0u + 0u;
        c.A1 = c.A1 - 0x236u;
    L8010040C:;
        m.WriteU8(c.V1, (byte)c.A3);
        c.At = 0x800B0000u;
        c.At = c.At + c.A0;
        c.V0 = m.ReadU32((c.At - 0x78D4u));
        if (c.V0 == 0u)
        {
            c.A0 = c.A0 + 0x10u;
            goto L8010042C;
        }
        c.A0 = c.A0 + 0x10u;
        m.WriteU8(c.V1, (byte)c.A2);
    L8010042C:;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < (int)c.A1 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L8010040C;
        }
        c.V1 = 0u | 0x0032u;
        c.S0 = 0u | 0x00A8u;
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x7A32u;
    L8010044C:;
        m.WriteU8(c.V0, (byte)c.V1);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V0 = c.V0 - 0x1u;
            goto L8010044C;
        }
        c.V0 = c.V0 - 0x1u;
        c.V1 = 0u | 0x0001u;
        c.S0 = 0u | 0x0059u;
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x7A8Cu;
    L8010046C:;
        m.WriteU8(c.V0, (byte)c.V1);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.V0 = c.V0 - 0x1u;
            goto L8010046C;
        }
        c.V0 = c.V0 - 0x1u;
        c.A0 = 0u | 0x006Fu;
        c.V0 = 0u | 0x0013u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C00u), c.V0);
        c.V0 = 0u | 0x0005u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C04u), c.V0);
        c.V0 = 0u | 0x001Au;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C08u), c.V0);
        c.V0 = 0u | 0x0002u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C0Cu), c.V0);
        c.V0 = 0u | 0x0030u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C10u), c.V0);
        c.V0 = 0u | 0x0039u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C14u), c.V0);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C18u), c.V0);
        c.V0 = 0u | 0x0003u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C30u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C34u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C38u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7C3Cu), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7BFCu), 0u);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x796Eu), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x796Fu), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7973u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7964u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7965u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7968u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7969u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x796Au), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x796Bu), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7970u), (byte)c.V0);
        c.At = 0x80090000u;
        m.WriteU8((c.At + 0x7971u), (byte)c.V0);
        c.A1 = 0u + 0u;
        c.RA = 0x8010055Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0070u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100568u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0071u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100574u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0062u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100580u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0080u;
        c.A1 = 0u + 0u;
        c.RA = 0x8010058Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0064u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100598u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0006u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005A4u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0007u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005B0u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0012u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005BCu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0017u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005C8u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0055u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005D4u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0058u;
        c.A1 = 0u + 0u;
        c.RA = 0x801005E0u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0001u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x801005ECu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0003u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x801005F8u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0004u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100604u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0005u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100610u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0006u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x8010061Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0007u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100628u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x000Au;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100634u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x000Du;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x80100640u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x001Fu;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x8010064Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0021u;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x80100658u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0023u;
        c.A1 = 0u | 0x0001u;
        c.RA = 0x80100664u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0031u;
        c.A1 = 0u | 0x0003u;
        c.RA = 0x80100670u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0033u;
        c.A1 = 0u | 0x0003u;
        c.RA = 0x8010067Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0035u;
        c.A1 = 0u | 0x0003u;
        c.RA = 0x80100688u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0032u;
        c.A1 = 0u | 0x0003u;
        c.RA = 0x80100694u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0052u;
        c.A1 = 0u | 0x0004u;
        c.RA = 0x801006A0u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x004Fu;
        c.A1 = 0u | 0x0004u;
        c.RA = 0x801006ACu;
        SoTN.AddToInventory(c, m);
        c.S0 = 0u + 0u;
        c.A0 = 0u | 0x009Fu;
    L801006B4:;
        c.A1 = 0u + 0u;
        c.RA = 0x801006BCu;
        SoTN.AddToInventory(c, m);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 80 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x009Fu;
            goto L801006B4;
        }
        c.A0 = 0u | 0x009Fu;
        c.S0 = 0u + 0u;
        c.A0 = 0u | 0x0019u;
    L801006D4:;
        c.A1 = 0u + 0u;
        c.RA = 0x801006DCu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0045u;
        c.A1 = 0u + 0u;
        c.RA = 0x801006E8u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0043u;
        c.A1 = 0u + 0u;
        c.RA = 0x801006F4u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0090u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100700u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0051u;
        c.A1 = 0u + 0u;
        c.RA = 0x8010070Cu;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0052u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100718u;
        SoTN.AddToInventory(c, m);
        c.A0 = 0u | 0x0049u;
        c.A1 = 0u + 0u;
        c.RA = 0x80100724u;
        SoTN.AddToInventory(c, m);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 10 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x0019u;
            goto L801006D4;
        }
        c.A0 = 0u | 0x0019u;
    L80100734:;
        c.RA = 0x8010073Cu;
        SoTN.func_800F53A4(c, m);
    L8010073C:;
        c.RA = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }

    // Darkwing Bat
    public static void func_801AC7CC_rnz1(CpuContext c, IMemory m)
    {
        // Dark Wing Patch to allow Relic that shows up after he's dead to be changed.
        // We may also need to patch the "Blue Swirl" so that an Item can show up?
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x20u), c.RA);
        m.WriteU32((c.SP + 0x18u), c.S2);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V1 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801ACB8C;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801A0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x6090u));
        switch (c.V0)
        {
            case 0x801AC814u: goto L801AC814;
            case 0x801AC8ECu: goto L801AC8EC;
            case 0x801AC940u: goto L801AC940;
            case 0x801AC9C8u: goto L801AC9C8;
            case 0x801ACA1Cu: goto L801ACA1C;
            case 0x801ACA68u: goto L801ACA68;
            case 0x801ACAC0u: goto L801ACAC0;
            case 0x801ACB40u: goto L801ACB40;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801AC814:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xB40u;
        c.RA = 0x801AC824u;
        SoTN.func_801B0FC8(c, m);
        c.A0 = 0u | 0x0014u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x37C0u));
        c.A1 = 0u + 0u;
        c.RA = 0x801AC83Cu;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x000Bu;
            goto L801AC874;
        }
        c.V0 = 0u | 0x000Bu;
        m.WriteU16((c.S3 + 0x26u), (ushort)c.V0);
        c.V0 = 0x801B0000u;
        //c.V0 = c.V0 + 0x2F84u;            // Entity Func Pointer Lower 16-bits
        c.V0 = c.V0 + m.ReadU16(0x801AC84C);
        m.WriteU32((c.S3 + 0x28u), c.V0);
        c.V0 = 0u | 0x0010u;
        m.WriteU8((c.S3 + 0x6Du), (byte)c.V0);
        //c.V0 = 0u | 0x001Cu;	            // Ring of Vlad Relic
        c.V0 = 0u | m.ReadU16(0x801AC85C);  // Read updated Relic Id / Sub-type
        m.WriteU16((c.S3 + 0x52u), (ushort)0u);
        m.WriteU16((c.S3 + 0x50u), (ushort)0u);
        m.WriteU16((c.S3 + 0x30u), (ushort)c.V0);
        m.WriteU16((c.S3 + 0x2Cu), (ushort)0u);
        goto L801ACB8C;
    L801AC874:;
        c.S0 = 0x80070000u;
        c.S0 = c.S0 + 0x6DDCu;
        c.A0 = 0u | 0x002Fu;
        c.A1 = c.S0 + 0u;
        c.RA = 0x801AC888u;
        SoTN.func_801AF518(c, m);
        c.A1 = c.S0 + 0xBCu;
        c.S2 = 0x80070000u;
        c.S2 = c.S2 + 0x308Eu;
        c.V0 = m.ReadU16(c.S2);
        c.S1 = 0u | 0x0080u;
        c.V0 = c.S1 - c.V0;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6DDEu), (ushort)c.V0);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x3092u));
        c.S0 = 0u | 0x0078u;
        c.V0 = c.S0 - c.V0;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6DE2u), (ushort)c.V0);
        c.A0 = 0u | 0x002Eu;
        c.RA = 0x801AC8C8u;
        SoTN.func_801AF518(c, m);
        c.V0 = m.ReadU16(c.S2);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        c.S1 = c.S1 - c.V0;
        c.S0 = c.S0 - c.V1;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6E9Au), (ushort)c.S1);
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6E9Eu), (ushort)c.S0);
    L801AC8EC:;
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DAu));
        c.V1 = 0x80070000u;
        c.V1 = (uint)(short)m.ReadU16((c.V1 + 0x308Eu));
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 - 0x31u;
        c.V0 = c.V0 < 0x0000009Fu ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x0014u;
            goto L801ACB8C;
        }
        c.A0 = 0u | 0x0014u;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x1308u));
        c.V1 = 0x80040000u;
        c.V1 = m.ReadU32((c.V1 - 0x37C0u));
        c.V0 = c.V0 | 0x0001u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x1308u), c.V0);
        c.A1 = 0u | 0x0002u;
        c.RA = 0x801AC938u;
        Dispatcher.Call(c, m, c.V1);
        goto L801ACB7C;
    L801AC940:;
        c.A0 = 0u | 0x0034u;
        c.A1 = c.S3 + 0xBCu;
        c.RA = 0x801AC94Cu;
        SoTN.func_801AF518(c, m);
        c.A0 = 0u | 0x0034u;
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x308Eu;
        c.V0 = 0xFFFFFFF8u;
        c.V1 = m.ReadU16(c.S1);
        c.A1 = c.S3 + 0x178u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S3 + 0xBEu), (ushort)c.V0);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x3092u));
        c.S0 = 0u | 0x0080u;
        m.WriteU16((c.S3 + 0xECu), (ushort)0u);
        c.V0 = c.S0 - c.V0;
        m.WriteU16((c.S3 + 0xC2u), (ushort)c.V0);
        c.RA = 0x801AC988u;
        SoTN.func_801AF518(c, m);
        c.V0 = 0u | 0x0001u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x1304u), c.V0);
        c.V0 = 0u | 0x0108u;
        c.A0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V1 = m.ReadU16(c.S1);
        c.A0 = c.A0 + 0x1u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S3 + 0x17Au), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S3 + 0x1A8u), (ushort)c.V0);
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.A0);
        c.S0 = c.S0 - c.V1;
        m.WriteU16((c.S3 + 0x17Eu), (ushort)c.S0);
    L801AC9C8:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3808u));
        c.RA = 0x801AC9DCu;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801AC9FC;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0090u;
        c.RA = 0x801AC9F8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0u | 0x0001u;
    L801AC9FC:;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V1 = 0u | 0x031Du;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V1);
        c.V0 = c.V0 + 0x1u;
        goto L801ACB88;
    L801ACA1C:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3808u));
        c.RA = 0x801ACA30u;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 != 0u)
        {
            goto L801ACA68;
        }
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x7910u));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), 0u);
        c.RA = 0x801ACA58u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.V0);
    L801ACA68:;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x1308u));
        c.V0 = c.V0 & 0x0002u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x0014u;
            goto L801ACB8C;
        }
        c.A0 = 0u | 0x0014u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x37C0u));
        c.A1 = 0u | 0x0001u;
        c.RA = 0x801ACA94u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0090u;
        c.RA = 0x801ACAA8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V1 = 0u | 0x0338u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V1);
        c.V0 = c.V0 + 0x1u;
        goto L801ACB88;
    L801ACAC0:;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x1308u));
        c.V0 = c.V0 & 0x0004u;
        if (c.V0 == 0u)
        {
            goto L801ACB8C;
        }
        c.A0 = 0x80080000u;
        c.A0 = c.A0 - 0x56A8u;
        c.A1 = c.A0 + 0x1780u;
        c.RA = 0x801ACAE8u;
        SoTN.func_801B0B28(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0035u;
            goto L801ACB8C;
        }
        c.A0 = 0u | 0x0035u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801ACB00u;
        SoTN.func_801AF58C(c, m);
        c.V0 = 0u | 0x0080u;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        c.V0 = 0u | 0x0014u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = 0u | 0x0001u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V1 = 0u | 0x0338u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x1304u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V1);
        c.V0 = c.V0 + 0x1u;
        goto L801ACB88;
    L801ACB40:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3808u));
        c.RA = 0x801ACB54u;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 != 0u)
        {
            goto L801ACB8C;
        }
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x7910u));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), 0u);
        c.RA = 0x801ACB7Cu;
        Dispatcher.Call(c, m, c.V0);
    L801ACB7C:;
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
    L801ACB88:;
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.V0);
    L801ACB8C:;
        c.RA = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Holy Glasses Location
    public static void EntityPlatform(CpuContext c, IMemory m)
    {
        // This is for Patching Holy Glasses to be another Item or No Item
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A1 = 0u | 0x0020u;
        c.A2 = 0u | 0x0011u;
        m.WriteU32((c.SP + 0x20u), c.RA);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x18u), c.S2);
        m.WriteU32((c.SP + 0x14u), c.S1);
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.A3 = 0u | 0x0004u;
        c.V0 = c.V0 - 0x8u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        c.RA = 0x8018F994u;
        SoTN.GetPlayerCollisionWith_cen(c, m);
        c.S3 = 0x80070000u;
        c.S3 = c.S3 + 0x3084u;
        c.S2 = 0x80070000u;
        c.S2 = c.S2 + 0x33D8u;
        c.A1 = c.V0 + 0u;
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x33DAu));
        c.A0 = 0x80070000u;
        c.A0 = m.ReadU16((c.A0 + 0x308Eu));
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.V1 = c.V1 + c.A0;
        c.S1 = c.V1 + 0u;
        c.A0 = 0x80070000u;
        c.A0 = m.ReadU16((c.A0 + 0x3092u));
        c.V1 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + c.A0;
        c.A0 = c.V0 + 0u;
        c.V0 = c.V1 < 0x0000000Au ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L8018FFE8;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x80190000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At - 0x2B50u));
        switch (c.V0)
        {
            case 0x8018F9FCu: goto L8018F9FC;
            case 0x8018FAF4u: goto L8018FAF4;
            case 0x8018FBF4u: goto L8018FBF4;
            case 0x8018FCBCu: goto L8018FCBC;
            case 0x8018FD9Cu: goto L8018FD9C;
            case 0x8018FE60u: goto L8018FE60;
            case 0x8018FEC8u: goto L8018FEC8;
            case 0x8018FF0Cu: goto L8018FF0C;
            case 0x8018FFC0u: goto L8018FFC0;
            case 0x8018FFE8u: goto L8018FFE8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L8018F9FC:;
        c.A0 = 0u | 0x0004u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0001u;
        c.RA = 0x8018FA14u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.S1 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.S1 == c.V0)
        {
            goto L8018FFE8;
        }
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x434u;
        c.RA = 0x8018FA38u;
        SoTN.InitializeEntity_cen(c, m);
        c.V0 = 0xFFFF8002u;
        c.V1 = 0u | 0x0009u;
        m.WriteU16((c.S0 + 0x54u), (ushort)c.V0);
        c.V0 = 0u | 0x0080u;
        m.WriteU16((c.S0 + 0x56u), (ushort)c.V1);
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x413Cu));
        if (c.V0 == 0u)
        {
            goto L8018FA68;
        }
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V1);
    L8018FA68:;
        c.A0 = 0u + 0u;
        c.RA = 0x8018FA70u;
        SoTN.func_8018F8EC(c, m);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A1 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S0 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S0 + 0x64u), c.S1);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S0 + 0x34u), c.V0);
        c.V0 = 0u | 0x000Fu;
        c.V1 = 0u | 0x0002u;
        m.WriteU16((c.A1 + 0x1Au), (ushort)c.V0);
        c.V0 = 0u | 0x00A0u;
        m.WriteU8((c.A1 + 0x24u), (byte)c.V0);
        m.WriteU8((c.A1 + 0xCu), (byte)c.V0);
        c.V0 = 0u | 0x00B0u;
        m.WriteU8((c.A1 + 0x30u), (byte)c.V0);
        m.WriteU8((c.A1 + 0x18u), (byte)c.V0);
        c.V0 = 0u | 0x00A1u;
        m.WriteU8((c.A1 + 0x19u), (byte)c.V0);
        m.WriteU8((c.A1 + 0xDu), (byte)c.V0);
        c.V0 = 0u | 0x00A7u;
        m.WriteU8((c.A1 + 0x31u), (byte)c.V0);
        m.WriteU8((c.A1 + 0x25u), (byte)c.V0);
        c.V0 = 0u | 0x007Fu;
        m.WriteU16((c.A1 + 0xEu), (ushort)c.V1);
        m.WriteU16((c.A1 + 0x26u), (ushort)c.V0);
        m.WriteU16((c.A1 + 0x32u), (ushort)c.V1);
        goto L8018FFE8;
    L8018FAF4:;
        c.RA = 0x8018FAFCu;
        SoTN.GetDistanceToPlayerX_cen(c, m);
        c.V0 = (int)c.V0 < 32 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L8018FFE8;
        }
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x6u));
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x6u));
        c.V0 = c.V0 - c.V1;
        c.V0 = (int)c.V0 < 80 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L8018FFE8;
        }
        c.V0 = 0u | 0x0001u;
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x2F2Cu));
        c.At = 0x80040000u;
        m.WriteU32((c.At - 0x3748u), 0u);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7400u), c.V0);
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0008u;
            goto L8018FB58;
        }
        c.V0 = 0u | 0x0008u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FBDC;
    L8018FB58:;
        c.V0 = c.V1 & 0x0002u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0004u;
            goto L8018FB74;
        }
        c.V0 = 0u | 0x0004u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FBDC;
    L8018FB74:;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0002u;
            goto L8018FB90;
        }
        c.V0 = 0u | 0x0002u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FBDC;
    L8018FB90:;
        c.V0 = c.S1 << 16;
        c.V1 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V1 < 385 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = (int)c.V1 < 384 ? 1u : 0u;
            goto L8018FBB8;
        }
        c.V0 = (int)c.V1 < 384 ? 1u : 0u;
        c.V0 = 0u | 0x8000u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FBDC;
    L8018FBB8:;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x2000u;
            goto L8018FBD0;
        }
        c.V0 = 0u | 0x2000u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FBDC;
    L8018FBD0:;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), 0u);
        c.V0 = 0u | 0x0001u;
    L8018FBDC:;
        c.At = 0x80070000u;
        m.WriteU8((c.At + 0x3510u), (byte)0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.V0);
        goto L8018FFD8;
    L8018FBF4:;
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x2F2Cu));
        c.A0 = 0x80070000u;
        c.A0 = c.A0 + 0x2EF4u;
        c.V0 = c.V1 & 0x0007u;
        if (c.V0 == 0u)
        {
            m.WriteU32(c.A0, 0u);
            goto L8018FC60;
        }
        m.WriteU32(c.A0, 0u);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3668u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 & 0x0001u;
            goto L8018FD88;
        }
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0008u;
            goto L8018FC38;
        }
        c.V0 = 0u | 0x0008u;
        m.WriteU32(c.A0, c.V0);
        goto L8018FD88;
    L8018FC38:;
        c.V0 = c.V1 & 0x0002u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0004u;
            goto L8018FC4C;
        }
        c.V0 = 0u | 0x0004u;
        m.WriteU32(c.A0, c.V0);
        goto L8018FD88;
    L8018FC4C:;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0002u;
            goto L8018FD88;
        }
        c.V0 = 0u | 0x0002u;
        m.WriteU32(c.A0, c.V0);
        goto L8018FD88;
    L8018FC60:;
        c.V0 = c.A1 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            c.V0 = c.S1 << 16;
            goto L8018FC84;
        }
        c.V0 = c.S1 << 16;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x2F20u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = c.S1 << 16;
            goto L8018FD88;
        }
        c.V0 = c.S1 << 16;
    L8018FC84:;
        c.V1 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V1 < 385 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = (int)c.V1 < 384 ? 1u : 0u;
            goto L8018FC9C;
        }
        c.V0 = (int)c.V1 < 384 ? 1u : 0u;
        c.V0 = 0u | 0x8000u;
        goto L8018FCA4;
    L8018FC9C:;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x2000u;
            goto L8018FCA8;
        }
        c.V0 = 0u | 0x2000u;
    L8018FCA4:;
        m.WriteU32(c.A0, c.V0);
    L8018FCA8:;
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        goto L8018FD88;
    L8018FCBC:;
        c.A0 = 0x80070000u;
        c.A0 = c.A0 + 0x2EF4u;
        c.V1 = m.ReadU32(c.A0);
        c.V0 = 0u | 0x8000u;
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x2000u;
            goto L8018FCF0;
        }
        c.V0 = 0u | 0x2000u;
        c.V0 = c.S1 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 385 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L8018FD0C;
        }
        m.WriteU32(c.A0, 0u);
        goto L8018FD0C;
    L8018FCF0:;
        if (c.V1 != c.V0)
        {
            c.V0 = c.S1 << 16;
            goto L8018FD0C;
        }
        c.V0 = c.S1 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 384 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L8018FD0C;
        }
        m.WriteU32(c.A0, 0u);
    L8018FD0C:;
        c.V1 = 0x80070000u;
        c.V1 = c.V1 + 0x2EF4u;
        c.V0 = m.ReadU32(c.V1);
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L8018FD8C;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = 0u | 0x8000u;
        m.WriteU32(c.V1, c.V0);
        c.V1 = m.ReadU16((c.S3 + 0xAu));
        c.V0 = 0u | 0x0180u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S2 + 0x2u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x060Du;
        c.RA = 0x8018FD60u;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = 0u + 0u;
        c.V0 = 0x801A0000u;
        c.V0 = m.ReadU32((c.V0 - 0x2BDCu));
        c.V1 = (uint)(short)m.ReadU16((c.S3 + 0xEu));
        c.V0 = c.V0 | 0x0001u;
        c.V1 = c.V1 + 0x100u;
        c.At = 0x801A0000u;
        m.WriteU32((c.At - 0x2BDCu), c.V0);
        m.WriteU32((c.S3 + 0x48u), c.V1);
        c.RA = 0x8018FD88u;
        SoTN.func_8018F8EC(c, m);
    L8018FD88:;
        c.V0 = 0u | 0x0001u;
    L8018FD8C:;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.V0);
        goto L8018FFE8;
    L8018FD9C:;
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x2EF4u;
        c.V0 = 0u | 0x0001u;
        m.WriteU32(c.S1, 0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.V0);
        c.V1 = m.ReadU16((c.S3 + 0xAu));
        c.V0 = 0u | 0x0180u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S2 + 0x2u), (ushort)c.V0);
        c.V0 = c.A0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 497 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L8018FE08;
        }
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x748Eu;
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x6u));
        c.V1 = m.ReadU16(c.A0);
        c.V0 = c.V0 - 0x1u;
        c.V1 = c.V1 - 0x1u;
        m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
        m.WriteU16(c.A0, (ushort)c.V1);
        goto L8018FE50;
    L8018FE08:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x064Fu;
        c.RA = 0x8018FE1Cu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x14u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x8000u;
            goto L8018FE30;
        }
        c.V0 = 0u | 0x8000u;
        m.WriteU32(c.S1, c.V0);
    L8018FE30:;
        c.V1 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = 0x801A0000u;
        c.V0 = m.ReadU32((c.V0 - 0x2BDCu));
        c.V1 = c.V1 + 0x1u;
        c.V0 = c.V0 | 0x0004u;
        c.At = 0x801A0000u;
        m.WriteU32((c.At - 0x2BDCu), c.V0);
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V1);
    L8018FE50:;
        c.A0 = 0u | 0x0200u;
        c.RA = 0x8018FE58u;
        SoTN.func_8018F890(c, m);
        goto L8018FFE8;
    L8018FE60:;
        c.A0 = 0u | 0x0200u;
        c.RA = 0x8018FE68u;
        SoTN.func_8018F890(c, m);
        c.V0 = 0x801A0000u;
        c.V0 = m.ReadU32((c.V0 - 0x2BDCu));
        c.V1 = 0u | 0x0001u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), 0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.V1);
        c.V0 = c.V0 & 0x0008u;
        if (c.V0 == 0u)
        {
            goto L8018FFE8;
        }
        c.A1 = 0x80080000u;
        c.A1 = c.A1 - 0x3658u;
        c.A0 = 0u | 0x000Au;
        c.RA = 0x8018FEA0u;
        if(m.ReadU32(0x8018FE98) != 0x0C064D31) // added
        {
            goto L8018FFD8;
        }
        Dispatcher.Call(c, m, 0x801934C4u);
        //c.V0 = 0u | 0x00CBu;  // CB = Holy Glasses
        c.V0 = 0u | m.ReadU16(0x8018FEA0);  // Read Updated Item Id
        c.At = 0x80080000u;
        m.WriteU16((c.At - 0x3628u), (ushort)c.V0);
        c.V0 = 0u | 0x0005u;
        c.At = 0x80080000u;
        m.WriteU16((c.At - 0x362Cu), (ushort)c.V0);
        c.At = 0x80080000u;
        m.WriteU32((c.At - 0x3624u), 0u);
        goto L8018FFD8;
    L8018FEC8:;
        c.V0 = 0x801A0000u;
        c.V0 = m.ReadU32((c.V0 - 0x2BDCu));
        c.V0 = c.V0 & 0x0002u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L8018FFA8;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x060Du;
        c.RA = 0x8018FF04u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0u | 0x0001u;
        goto L8018FFA8;
    L8018FF0C:;
        c.V0 = c.A0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 592 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L8018FF50;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x748Au;
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x6u));
        c.V1 = m.ReadU16(c.A0);
        c.V0 = c.V0 + 0x1u;
        c.V1 = c.V1 + 0x1u;
        m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
        m.WriteU16(c.A0, (ushort)c.V1);
        goto L8018FF9C;
    L8018FF50:;
        c.A0 = 0x80090000u;
        c.A0 = c.A0 + 0x7400u;
        c.V1 = m.ReadU32(c.A0);
        c.At = 0x80040000u;
        m.WriteU32((c.At - 0x3748u), c.V0);
        if (c.V1 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L8018FF70;
        }
        c.V0 = 0u | 0x0001u;
        m.WriteU32(c.A0, 0u);
    L8018FF70:;
        c.At = 0x80070000u;
        m.WriteU8((c.At + 0x3510u), (byte)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x064Fu;
        c.RA = 0x8018FF9Cu;
        Dispatcher.Call(c, m, c.V0);
    L8018FF9C:;
        c.A0 = 0u | 0x0300u;
        c.RA = 0x8018FFA4u;
        SoTN.func_8018F890(c, m);
        c.V0 = 0u | 0x0001u;
    L8018FFA8:;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), 0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.V0);
        goto L8018FFE8;
    L8018FFC0:;
        c.A0 = 0u | 0x0300u;
        c.RA = 0x8018FFC8u;
        SoTN.func_8018F890(c, m);
        c.V1 = m.ReadU32((c.S3 + 0x48u));
        c.V0 = 0u | 0x0300u;
        if (c.V1 != c.V0)
        {
            goto L8018FFE8;
        }
    L8018FFD8:;
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
    L8018FFE8:;
        c.V1 = m.ReadU32((c.S0 + 0x64u));
        c.A0 = 0x80080000u;
        c.A0 = c.A0 + 0x6FECu;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.A1 = c.V0 + c.A0;
        c.V1 = m.ReadU16((c.S0 + 0x2u));
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.A0 = c.V1 - 0x8u;
        c.V1 = c.V1 + 0x8u;
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        m.WriteU16((c.A1 + 0x20u), (ushort)c.A0);
        m.WriteU16((c.A1 + 0x8u), (ushort)c.A0);
        m.WriteU16((c.A1 + 0x2Cu), (ushort)c.V1);
        m.WriteU16((c.A1 + 0x14u), (ushort)c.V1);
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.V0 = c.V0 + 0xFu;
        m.WriteU16((c.A1 + 0x16u), (ushort)c.V0);
        m.WriteU16((c.A1 + 0xAu), (ushort)c.V0);
        c.V1 = m.ReadU16((c.S3 + 0xEu));
        c.V0 = 0u | 0x0268u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.A1 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.A1 + 0x22u), (ushort)c.V0);
        c.RA = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Gold Ring Location
    public static void func_us_801C8248(CpuContext c, IMemory m)
    {
        // Gold Ring, Checks if it was changed to a Relic
        bool GR_is_Relic = false;
        if(m.ReadU32(0x801CC590) == 0x08077AED)
        {
            GR_is_Relic = true;
        }
        c.SP = c.SP - 0x20u;
        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        c.A0 = 0u | 0x0009u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x37C0u));
        c.A1 = 0u + 0u;
        m.WriteU32((c.SP + 0x1Cu), c.RA);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.RA = 0x801C8274u;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x000Cu;
            goto L801C829C;
        }
        c.A0 = 0u | 0x000Cu;
        if (GR_is_Relic)
        {
            c.A0 = 0x000Bu;
        }
        c.S0 = m.ReadU32(c.S2);
        c.S1 = m.ReadU32((c.S2 + 0x4u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x801C828Cu;
        SoTN.CreateEntityFromCurrentEntity_no4(c, m);
        c.V0 = 0u | 0x000Au;
        if (GR_is_Relic)
        {
            c.V0 = m.ReadU8(0x80184278);
        }
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V0);
        m.WriteU32(c.S2, c.S0);
        m.WriteU32((c.S2 + 0x4u), c.S1);
    L801C829C:;
        c.RA = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }

    // Pot Roast drop in Entrance
    // 0x801BA7CC in 0x41
    // 0x801B506C in 0x07
    public static void EntityMermanRockLeftSide_no3(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x24u), c.RA);
        m.WriteU32((c.SP + 0x20u), c.S4);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.V0 = 0u | 0x0001u;
        if (c.V1 == c.V0)
        {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801BA608;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801BA508;
        }
        if (c.V1 == 0u)
        {
            goto L801BA51C;
        }
        goto L801BA844;
    L801BA508:;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0)
        {
            goto L801BA804;
        }
        goto L801BA844;
    L801BA51C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xADCu;
        c.S1 = 0u + 0u;
        c.RA = 0x801BA52Cu;
        SoTN.InitializeEntity_no3(c, m);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x127Cu;
        c.A2 = 0u | 0x01F1u;
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V0);
        c.V0 = 0u | 0x0010u;
        m.WriteU8((c.S2 + 0x46u), (byte)c.V0);
        c.V0 = 0u | 0x0018u;
        m.WriteU8((c.S2 + 0x47u), (byte)c.V0);
    L801BA550:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x30D8u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x30D8u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801BA550;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x41E1u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.A2 = 0u | 0x01F1u;
            goto L801BA844;
        }
        c.A2 = 0u | 0x01F1u;
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x1264u;
        c.S1 = 0u + 0u;
    L801BA5B4:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801BA5B4;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V0);
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801BA844;
    L801BA608:;
        c.V0 = m.ReadU8((c.S2 + 0x48u));
        if (c.V0 == 0u)
        {
            c.A2 = 0u | 0x01F1u;
            goto L801BA790;
        }
        c.A2 = 0u | 0x01F1u;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S1 = 0u + 0u;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80180000u;
        c.V0 = c.V0 + 0x1258u;
        c.A1 = c.V1 + c.V0;
    L801BA638:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801BA638;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801BA68Cu;
        Dispatcher.Call(c, m, c.V0);
        c.S3 = 0x80080000u;
        c.S3 = c.S3 - 0x27A8u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801BA6A0u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S1 = 0u + 0u;
            goto L801BA6EC;
        }
        c.S1 = 0u + 0u;
        c.A0 = 0u | 0x0002u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BA6BCu;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.V0 = 0u | 0x0013u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = 0u | 0x00A9u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.A0 = m.ReadU16((c.S0 + 0x6u));
        c.V1 = c.V1 << 4;
        c.V0 = c.V0 + c.V1;
        c.A0 = c.A0 + 0x10u;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
        m.WriteU16((c.S0 + 0x6u), (ushort)c.A0);
    L801BA6EC:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S4 = c.S3 + 0u;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = 0x80180000u;
        c.V0 = c.V0 + 0x1344u;
        c.S3 = c.V1 + c.V0;
        c.A0 = c.S4 + 0u;
    L801BA70C:;
        c.A1 = c.S4 + 0x1780u;
        c.RA = 0x801BA714u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0027u;
            goto L801BA770;
        }
        c.A0 = 0u | 0x0027u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BA72Cu;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.V0 = m.ReadU8(c.S3);
        c.S3 = c.S3 + 0x1u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.RA = 0x801BA73Cu;
        SoTN.Random_no3(c, m);
        c.V0 = c.V0 << 8;
        c.V1 = 0xFFFF8000u;
        c.V1 = c.V1 - c.V0;
        m.WriteU32((c.S0 + 0x8u), c.V1);
        c.RA = 0x801BA750u;
        SoTN.Random_no3(c, m);
        c.V0 = 0u - c.V0;
        c.V1 = m.ReadU16((c.S0 + 0x6u));
        c.V0 = c.V0 << 8;
        m.WriteU32((c.S0 + 0xCu), c.V0);
        c.V0 = c.S1 << 4;
        c.V1 = c.V1 - 0x10u;
        c.V1 = c.V1 + c.V0;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V1);
    L801BA770:;
        c.S1 = c.S1 + 0x1u;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = c.S4 + 0u;
            goto L801BA70C;
        }
        c.A0 = c.S4 + 0u;
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
    L801BA790:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801BA844;
        }
        c.A0 = 0x80080000u;
        c.A0 = c.A0 - 0x56A8u;
        c.A1 = c.A0 + 0x1780u;
        c.RA = 0x801BA7B4u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x000Au;
            goto L801BA7D4;
        }
        c.A0 = 0u | 0x000Au;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BA7CCu;
        SoTN.CreateEntityFromEntity_no3(c, m);
        //c.V0 = 0u | 0x0043u;
        c.V0 = 0u | m.ReadU16(0x801BA7CC);      // Read Pot Roast Replacement ID
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
    L801BA7D4:;
        c.V1 = 0x80040000u;
        c.V1 = c.V1 - 0x41E1u;
        c.V0 = m.ReadU8(c.V1);
        c.V0 = c.V0 | 0x0001u;
        m.WriteU8(c.V1, (byte)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Cu));
        c.V1 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801BA844;
    L801BA804:;
        c.V0 = m.ReadU8((c.S2 + 0x48u));
        if (c.V0 == 0u)
        {
            goto L801BA844;
        }
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x2F2Cu));
        c.V0 = c.V0 & 0x0004u;
        if (c.V0 == 0u)
        {
            goto L801BA844;
        }
        c.V1 = 0x80040000u;
        c.V1 = c.V1 - 0x41E1u;
        c.V0 = m.ReadU8(c.V1);
        c.V0 = c.V0 | 0x0004u;
        m.WriteU8(c.V1, (byte)c.V0);
    L801BA844:;
        c.RA = m.ReadU32((c.SP + 0x24u));
        c.S4 = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Pot Roast in NP3 Entrance
    public static void EntityMermanRockLeftSide_np3(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x24u), c.RA);
        m.WriteU32((c.SP + 0x20u), c.S4);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.V0 = 0u | 0x0001u;
        if (c.V1 == c.V0)
        {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801B4EA8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801B4DA8;
        }
        if (c.V1 == 0u)
        {
            goto L801B4DBC;
        }
        goto L801B50E4;
    L801B4DA8:;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0)
        {
            goto L801B50A4;
        }
        goto L801B50E4;
    L801B4DBC:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xA6Cu;
        c.S1 = 0u + 0u;
        c.RA = 0x801B4DCCu;
        SoTN.InitializeEntity_np3(c, m);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x1144u;
        c.A2 = 0u | 0x01F1u;
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V0);
        c.V0 = 0u | 0x0010u;
        m.WriteU8((c.S2 + 0x46u), (byte)c.V0);
        c.V0 = 0u | 0x0018u;
        m.WriteU8((c.S2 + 0x47u), (byte)c.V0);
    L801B4DF0:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x30D8u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x30D8u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801B4DF0;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x41E1u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.A2 = 0u | 0x01F1u;
            goto L801B50E4;
        }
        c.A2 = 0u | 0x01F1u;
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x112Cu;
        c.S1 = 0u + 0u;
    L801B4E54:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801B4E54;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V0);
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801B50E4;
    L801B4EA8:;
        c.V0 = m.ReadU8((c.S2 + 0x48u));
        if (c.V0 == 0u)
        {
            c.A2 = 0u | 0x01F1u;
            goto L801B5030;
        }
        c.A2 = 0u | 0x01F1u;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S1 = 0u + 0u;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80180000u;
        c.V0 = c.V0 + 0x1120u;
        c.A1 = c.V1 + c.V0;
    L801B4ED8:;
        c.A0 = c.A2 << 1;
        c.A2 = c.A2 + 0x30u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16(c.A1);
        c.V0 = c.A0 + c.V0;
        m.WriteU16(c.V0, (ushort)c.V1);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x3084u));
        c.V1 = m.ReadU16((c.A1 + 0x6u));
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + c.V0;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
            goto L801B4ED8;
        }
        m.WriteU16((c.A0 + 0x2u), (ushort)c.V1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801B4F2Cu;
        Dispatcher.Call(c, m, c.V0);
        c.S3 = 0x80080000u;
        c.S3 = c.S3 - 0x27A8u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801B4F40u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S1 = 0u + 0u;
            goto L801B4F8C;
        }
        c.S1 = 0u + 0u;
        c.A0 = 0u | 0x0002u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B4F5Cu;
        SoTN.CreateEntityFromEntity_np3(c, m);
        c.V0 = 0u | 0x0013u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = 0u | 0x00A9u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.A0 = m.ReadU16((c.S0 + 0x6u));
        c.V1 = c.V1 << 4;
        c.V0 = c.V0 + c.V1;
        c.A0 = c.A0 + 0x10u;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
        m.WriteU16((c.S0 + 0x6u), (ushort)c.A0);
    L801B4F8C:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S4 = c.S3 + 0u;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = 0x80180000u;
        c.V0 = c.V0 + 0x120Cu;
        c.S3 = c.V1 + c.V0;
        c.A0 = c.S4 + 0u;
    L801B4FAC:;
        c.A1 = c.S4 + 0x1780u;
        c.RA = 0x801B4FB4u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0027u;
            goto L801B5010;
        }
        c.A0 = 0u | 0x0027u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B4FCCu;
        SoTN.CreateEntityFromEntity_np3(c, m);
        c.V0 = m.ReadU8(c.S3);
        c.S3 = c.S3 + 0x1u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.RA = 0x801B4FDCu;
        Dispatcher.Call(c, m, 0x801B90BCu);
        c.V0 = c.V0 << 8;
        c.V1 = 0xFFFF8000u;
        c.V1 = c.V1 - c.V0;
        m.WriteU32((c.S0 + 0x8u), c.V1);
        c.RA = 0x801B4FF0u;
        Dispatcher.Call(c, m, 0x801B90BCu);
        c.V0 = 0u - c.V0;
        c.V1 = m.ReadU16((c.S0 + 0x6u));
        c.V0 = c.V0 << 8;
        m.WriteU32((c.S0 + 0xCu), c.V0);
        c.V0 = c.S1 << 4;
        c.V1 = c.V1 - 0x10u;
        c.V1 = c.V1 + c.V0;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V1);
    L801B5010:;
        c.S1 = c.S1 + 0x1u;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = c.S4 + 0u;
            goto L801B4FAC;
        }
        c.A0 = c.S4 + 0u;
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
    L801B5030:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801B50E4;
        }
        c.A0 = 0x80080000u;
        c.A0 = c.A0 - 0x56A8u;
        c.A1 = c.A0 + 0x1780u;
        c.RA = 0x801B5054u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x000Au;
            goto L801B5074;
        }
        c.A0 = 0u | 0x000Au;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B506Cu;
        SoTN.CreateEntityFromEntity_np3(c, m);
        //c.V0 = 0u | 0x0043u;
        c.V0 = 0u | m.ReadU16(0x801B506C);
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
    L801B5074:;
        c.V1 = 0x80040000u;
        c.V1 = c.V1 - 0x41E1u;
        c.V0 = m.ReadU8(c.V1);
        c.V0 = c.V0 | 0x0001u;
        m.WriteU8(c.V1, (byte)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Cu));
        c.V1 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x3Cu), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801B50E4;
    L801B50A4:;
        c.V0 = m.ReadU8((c.S2 + 0x48u));
        if (c.V0 == 0u)
        {
            goto L801B50E4;
        }
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x2F2Cu));
        c.V0 = c.V0 & 0x0004u;
        if (c.V0 == 0u)
        {
            goto L801B50E4;
        }
        c.V1 = 0x80040000u;
        c.V1 = c.V1 - 0x41E1u;
        c.V0 = m.ReadU8(c.V1);
        c.V0 = c.V0 | 0x0004u;
        m.WriteU8(c.V1, (byte)c.V0);
    L801B50E4:;
        c.RA = m.ReadU32((c.SP + 0x24u));
        c.S4 = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Turkey Drop in Entrance
    public static void EntityStairwayPiece_no3(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        m.WriteU32((c.SP + 0x44u), c.S1);
        c.S1 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x54u), c.RA);
        m.WriteU32((c.SP + 0x50u), c.S4);
        m.WriteU32((c.SP + 0x4Cu), c.S3);
        m.WriteU32((c.SP + 0x48u), c.S2);
        m.WriteU32((c.SP + 0x40u), c.S0);
        c.V1 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801BB398;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801B0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x7400u));
        switch (c.V0)
        {
            case 0x801BAF3Cu: goto L801BAF3C;
            case 0x801BAFF8u: goto L801BAFF8;
            case 0x801BB03Cu: goto L801BB03C;
            case 0x801BB240u: goto L801BB240;
            case 0x801BB2D8u: goto L801BB2D8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801BAF3C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xADCu;
        c.RA = 0x801BAF4Cu;
        SoTN.InitializeEntity_no3(c, m);
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.S1 + 0x46u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x47u), (byte)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x308Eu));
        c.V0 = 0u | 0x0598u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S1 + 0x2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S1 + 0x3Eu), (ushort)c.V0);
        c.V0 = 0u | 0x00C8u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S1 + 0x6u), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x41DCu));
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x03EEu;
            goto L801BAFCC;
        }
        c.V0 = 0u | 0x03EEu;
        m.WriteU16((c.S1 + 0x3Cu), (ushort)0u);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03D2u;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        c.V0 = 0u | 0x0020u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801BB398;
    L801BAFCC:;
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S1 + 0x3Cu), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x0408u;
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x040Du;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        goto L801BB398;
    L801BAFF8:;
        c.V0 = m.ReadU8((c.S1 + 0x48u));
        if (c.V0 == 0u)
        {
            goto L801BB01C;
        }
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x064Bu;
        c.RA = 0x801BB01Cu;
        Dispatcher.Call(c, m, c.V0);
    L801BB01C:;
        c.V0 = m.ReadU32((c.S1 + 0x34u));
        c.V0 = c.V0 & 0x0100u;
        if (c.V0 == 0u)
        {
            goto L801BB398;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        goto L801BB2D0;
    L801BB03C:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801BB050u;
        Dispatcher.Call(c, m, c.V0);
        c.S2 = 0x80080000u;
        c.S2 = c.S2 - 0x56A8u;
        c.A0 = c.S2 + 0u;
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03EEu;
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03D2u;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        c.V0 = 0u | 0x0001u;
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x41DCu), (byte)c.V0);
        c.A1 = c.S2 + 0x1780u;
        c.RA = 0x801BB090u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x000Au;
            goto L801BB0B0;
        }
        c.A0 = 0u | 0x000Au;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BB0A8u;
        SoTN.CreateEntityFromEntity_no3(c, m);
        //c.V0 = 0u | 0x0045u;
        c.V0 = 0u | m.ReadU16(0x801BB0A8);
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
    L801BB0B0:;
        c.A0 = c.S2 + 0x2F00u;
        c.A1 = c.S2 + 0x4680u;
        c.RA = 0x801BB0BCu;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0006u;
            goto L801BB100;
        }
        c.A0 = 0u | 0x0006u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BB0D4u;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        c.V1 = m.ReadU16((c.S0 + 0x6u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.V1 = c.V1 + 0x8u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V1);
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
    L801BB100:;
        c.A0 = 0u | 0x0004u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0010u;
        c.RA = 0x801BB118u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A1 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A1 == c.V0)
        {
            c.V0 = c.A1 << 1;
            goto L801BB390;
        }
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.S0 = c.V0 + c.V1;
        c.A0 = c.S0 + 0u;
        c.V0 = m.ReadU32((c.S1 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S1 + 0x64u), c.A1);
        m.WriteU32((c.S1 + 0x7Cu), c.S0);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S1 + 0x34u), c.V0);
        c.RA = 0x801BB168u;
        SoTN.UnkPolyFunc2_no3(c, m);
        c.A0 = 0x80070000u;
        c.A0 = m.ReadU32((c.A0 + 0x3088u));
        c.V0 = m.ReadU32((c.A0 + 0x4u));
        c.V1 = m.ReadU8((c.V0 + 0x409u));
        c.V0 = m.ReadU32(c.A0);
        c.A0 = m.ReadU32((c.A0 + 0x8u));
        c.A1 = c.V1 << 4;
        c.A3 = c.A1 | 0x000Fu;
        c.V1 = c.V1 & 0x00F0u;
        c.A2 = c.V1 | 0x000Fu;
        c.V0 = m.ReadU8((c.V0 + 0x409u));
        c.A0 = m.ReadU8((c.A0 + 0x409u));
        m.WriteU8((c.S0 + 0x19u), (byte)c.V1);
        m.WriteU8((c.S0 + 0xDu), (byte)c.V1);
        c.V1 = m.ReadU32(c.S0);
        m.WriteU8((c.S0 + 0x24u), (byte)c.A1);
        m.WriteU8((c.S0 + 0xCu), (byte)c.A1);
        m.WriteU8((c.S0 + 0x30u), (byte)c.A3);
        m.WriteU8((c.S0 + 0x18u), (byte)c.A3);
        m.WriteU8((c.S0 + 0x31u), (byte)c.A2);
        m.WriteU8((c.S0 + 0x25u), (byte)c.A2);
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S0 + 0xEu), (ushort)c.A0);
        m.WriteU16((c.S0 + 0x1Au), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x2u));
        m.WriteU16((c.V1 + 0x14u), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU16((c.S1 + 0x6u));
        m.WriteU16((c.V1 + 0xAu), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S0);
        c.V1 = 0xFFFF0000u;
        m.WriteU32((c.V0 + 0xCu), c.V1);
        c.V0 = m.ReadU32(c.S0);
        m.WriteU32((c.V0 + 0x10u), c.V1);
        c.V0 = m.ReadU32(c.S0);
        c.V1 = 0u | 0x0010u;
        m.WriteU16((c.V0 + 0x1Cu), (ushort)c.V1);
        c.V0 = m.ReadU32(c.S0);
        m.WriteU16((c.V0 + 0x1Eu), (ushort)c.V1);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
    L801BB240:;
        c.S0 = m.ReadU32((c.S1 + 0x7Cu));
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU16((c.V1 + 0x1Au));
        c.V0 = c.V0 - 0x20u;
        m.WriteU16((c.V1 + 0x1Au), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU32((c.V1 + 0x10u));
        c.A0 = c.S0 + 0u;
        c.V0 = c.V0 + 0x2000u;
        m.WriteU32((c.V1 + 0x10u), c.V0);
        c.RA = 0x801BB27Cu;
        SoTN.UnkPrimHelper_no3(c, m);
        c.A2 = c.SP + 0x10u;
        c.V0 = m.ReadU32(c.S0);
        c.A3 = 0u + 0u;
        c.A0 = (uint)(short)m.ReadU16((c.V0 + 0x14u));
        c.S2 = m.ReadU16((c.V0 + 0xAu));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3844u));
        c.A1 = c.S2 + 0x8u;
        c.A1 = c.A1 << 16;
        c.A1 = (uint)((int)c.A1 >> 16);
        c.S0 = c.A0 + 0u;
        c.RA = 0x801BB2ACu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU32((c.SP + 0x10u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V1 = c.S2 - 0x4u;
            goto L801BB398;
        }
        c.V1 = c.S2 - 0x4u;
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        m.WriteU16((c.S1 + 0x2u), (ushort)c.S0);
        m.WriteU16((c.S1 + 0x6u), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
    L801BB2D0:;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801BB398;
    L801BB2D8:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801BB2ECu;
        Dispatcher.Call(c, m, c.V0);
        c.S3 = 0x80080000u;
        c.S3 = c.S3 - 0x27A8u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801BB300u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S2 = 0u + 0u;
            goto L801BB334;
        }
        c.S2 = 0u + 0u;
        c.A0 = 0u | 0x0002u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BB31Cu;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.V0 = 0u | 0x0011u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
    L801BB334:;
        c.S4 = 0u | 0x0003u;
        c.A0 = c.S3 + 0u;
    L801BB33C:;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801BB344u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S2 = c.S2 + 0x1u;
            goto L801BB384;
        }
        c.S2 = c.S2 + 0x1u;
        c.A0 = 0u | 0x005Du;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801BB360u;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.RA = 0x801BB368u;
        SoTN.Random_no3(c, m);
        c.V0 = c.V0 & 0x0003u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x30u));
        if (c.V0 != c.S4)
        {
            c.V0 = (int)c.S2 < 6 ? 1u : 0u;
            goto L801BB388;
        }
        c.V0 = (int)c.S2 < 6 ? 1u : 0u;
        m.WriteU16((c.S0 + 0x30u), (ushort)0u);
    L801BB384:;
        c.V0 = (int)c.S2 < 6 ? 1u : 0u;
    L801BB388:;
        if (c.V0 != 0u)
        {
            c.A0 = c.S3 + 0u;
            goto L801BB33C;
        }
        c.A0 = c.S3 + 0u;
    L801BB390:;
        c.A0 = c.S1 + 0u;
        c.RA = 0x801BB398u;
        SoTN.DestroyEntity_no3(c, m);
    L801BB398:;
        c.RA = m.ReadU32((c.SP + 0x54u));
        c.S4 = m.ReadU32((c.SP + 0x50u));
        c.S3 = m.ReadU32((c.SP + 0x4Cu));
        c.S2 = m.ReadU32((c.SP + 0x48u));
        c.S1 = m.ReadU32((c.SP + 0x44u));
        c.S0 = m.ReadU32((c.SP + 0x40u));
        c.SP = c.SP + 0x58u;
        return;
    }

    // Turkey Drop
    public static void EntityStairwayPiece_np3(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        m.WriteU32((c.SP + 0x44u), c.S1);
        c.S1 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x54u), c.RA);
        m.WriteU32((c.SP + 0x50u), c.S4);
        m.WriteU32((c.SP + 0x4Cu), c.S3);
        m.WriteU32((c.SP + 0x48u), c.S2);
        m.WriteU32((c.SP + 0x40u), c.S0);
        c.V1 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801B5C38;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801B0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x1EA8u));
        switch (c.V0)
        {
            case 0x801B57DCu: goto L801B57DC;
            case 0x801B5898u: goto L801B5898;
            case 0x801B58DCu: goto L801B58DC;
            case 0x801B5AE0u: goto L801B5AE0;
            case 0x801B5B78u: goto L801B5B78;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801B57DC:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xA6Cu;
        c.RA = 0x801B57ECu;
        SoTN.InitializeEntity_np3(c, m);
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.S1 + 0x46u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x47u), (byte)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x308Eu));
        c.V0 = 0u | 0x0598u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S1 + 0x2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S1 + 0x3Eu), (ushort)c.V0);
        c.V0 = 0u | 0x00C8u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S1 + 0x6u), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x41DCu));
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x03EEu;
            goto L801B586C;
        }
        c.V0 = 0u | 0x03EEu;
        m.WriteU16((c.S1 + 0x3Cu), (ushort)0u);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03D2u;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        c.V0 = 0u | 0x0020u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801B5C38;
    L801B586C:;
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S1 + 0x3Cu), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x0408u;
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x040Du;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        goto L801B5C38;
    L801B5898:;
        c.V0 = m.ReadU8((c.S1 + 0x48u));
        if (c.V0 == 0u)
        {
            goto L801B58BC;
        }
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x064Bu;
        c.RA = 0x801B58BCu;
        Dispatcher.Call(c, m, c.V0);
    L801B58BC:;
        c.V0 = m.ReadU32((c.S1 + 0x34u));
        c.V0 = c.V0 & 0x0100u;
        if (c.V0 == 0u)
        {
            goto L801B5C38;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        goto L801B5B70;
    L801B58DC:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801B58F0u;
        Dispatcher.Call(c, m, c.V0);
        c.S2 = 0x80080000u;
        c.S2 = c.S2 - 0x56A8u;
        c.A0 = c.S2 + 0u;
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03EEu;
        m.WriteU16((c.V1 + 0x9B2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU32((c.V1 + 0x3084u));
        c.V0 = 0u | 0x03D2u;
        m.WriteU16((c.V1 + 0xA72u), (ushort)c.V0);
        c.V0 = 0u | 0x0001u;
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x41DCu), (byte)c.V0);
        c.A1 = c.S2 + 0x1780u;
        c.RA = 0x801B5930u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x000Au;
            goto L801B5950;
        }
        c.A0 = 0u | 0x000Au;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B5948u;
        SoTN.CreateEntityFromEntity_np3(c, m);
        //c.V0 = 0u | 0x0045u;
        c.V0 = 0u | m.ReadU16(0x801B5948);
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
    L801B5950:;
        c.A0 = c.S2 + 0x2F00u;
        c.A1 = c.S2 + 0x4680u;
        c.RA = 0x801B595Cu;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0006u;
            goto L801B59A0;
        }
        c.A0 = 0u | 0x0006u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B5974u;
        SoTN.CreateEntityFromEntity_np3(c, m);
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        c.V1 = m.ReadU16((c.S0 + 0x6u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.V1 = c.V1 + 0x8u;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V1);
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
    L801B59A0:;
        c.A0 = 0u | 0x0004u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0010u;
        c.RA = 0x801B59B8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A1 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A1 == c.V0)
        {
            c.V0 = c.A1 << 1;
            goto L801B5C30;
        }
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.S0 = c.V0 + c.V1;
        c.A0 = c.S0 + 0u;
        c.V0 = m.ReadU32((c.S1 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S1 + 0x64u), c.A1);
        m.WriteU32((c.S1 + 0x7Cu), c.S0);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S1 + 0x34u), c.V0);
        c.RA = 0x801B5A08u;
        SoTN.UnkPolyFunc2_np3(c, m);
        c.A0 = 0x80070000u;
        c.A0 = m.ReadU32((c.A0 + 0x3088u));
        c.V0 = m.ReadU32((c.A0 + 0x4u));
        c.V1 = m.ReadU8((c.V0 + 0x409u));
        c.V0 = m.ReadU32(c.A0);
        c.A0 = m.ReadU32((c.A0 + 0x8u));
        c.A1 = c.V1 << 4;
        c.A3 = c.A1 | 0x000Fu;
        c.V1 = c.V1 & 0x00F0u;
        c.A2 = c.V1 | 0x000Fu;
        c.V0 = m.ReadU8((c.V0 + 0x409u));
        c.A0 = m.ReadU8((c.A0 + 0x409u));
        m.WriteU8((c.S0 + 0x19u), (byte)c.V1);
        m.WriteU8((c.S0 + 0xDu), (byte)c.V1);
        c.V1 = m.ReadU32(c.S0);
        m.WriteU8((c.S0 + 0x24u), (byte)c.A1);
        m.WriteU8((c.S0 + 0xCu), (byte)c.A1);
        m.WriteU8((c.S0 + 0x30u), (byte)c.A3);
        m.WriteU8((c.S0 + 0x18u), (byte)c.A3);
        m.WriteU8((c.S0 + 0x31u), (byte)c.A2);
        m.WriteU8((c.S0 + 0x25u), (byte)c.A2);
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S0 + 0xEu), (ushort)c.A0);
        m.WriteU16((c.S0 + 0x1Au), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x2u));
        m.WriteU16((c.V1 + 0x14u), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU16((c.S1 + 0x6u));
        m.WriteU16((c.V1 + 0xAu), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S0);
        c.V1 = 0xFFFF0000u;
        m.WriteU32((c.V0 + 0xCu), c.V1);
        c.V0 = m.ReadU32(c.S0);
        m.WriteU32((c.V0 + 0x10u), c.V1);
        c.V0 = m.ReadU32(c.S0);
        c.V1 = 0u | 0x0010u;
        m.WriteU16((c.V0 + 0x1Cu), (ushort)c.V1);
        c.V0 = m.ReadU32(c.S0);
        m.WriteU16((c.V0 + 0x1Eu), (ushort)c.V1);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
    L801B5AE0:;
        c.S0 = m.ReadU32((c.S1 + 0x7Cu));
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU16((c.V1 + 0x1Au));
        c.V0 = c.V0 - 0x20u;
        m.WriteU16((c.V1 + 0x1Au), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S0);
        c.V0 = m.ReadU32((c.V1 + 0x10u));
        c.A0 = c.S0 + 0u;
        c.V0 = c.V0 + 0x2000u;
        m.WriteU32((c.V1 + 0x10u), c.V0);
        c.RA = 0x801B5B1Cu;
        SoTN.UnkPrimHelper_np3(c, m);
        c.A2 = c.SP + 0x10u;
        c.V0 = m.ReadU32(c.S0);
        c.A3 = 0u + 0u;
        c.A0 = (uint)(short)m.ReadU16((c.V0 + 0x14u));
        c.S2 = m.ReadU16((c.V0 + 0xAu));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3844u));
        c.A1 = c.S2 + 0x8u;
        c.A1 = c.A1 << 16;
        c.A1 = (uint)((int)c.A1 >> 16);
        c.S0 = c.A0 + 0u;
        c.RA = 0x801B5B4Cu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU32((c.SP + 0x10u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V1 = c.S2 - 0x4u;
            goto L801B5C38;
        }
        c.V1 = c.S2 - 0x4u;
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        m.WriteU16((c.S1 + 0x2u), (ushort)c.S0);
        m.WriteU16((c.S1 + 0x6u), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
    L801B5B70:;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801B5C38;
    L801B5B78:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x0644u;
        c.RA = 0x801B5B8Cu;
        Dispatcher.Call(c, m, c.V0);
        c.S3 = 0x80080000u;
        c.S3 = c.S3 - 0x27A8u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801B5BA0u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S2 = 0u + 0u;
            goto L801B5BD4;
        }
        c.S2 = 0u + 0u;
        c.A0 = 0u | 0x0002u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B5BBCu;
        SoTN.CreateEntityFromEntity_np3(c, m);
        c.V0 = 0u | 0x0011u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x24u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x24u), (ushort)c.V0);
    L801B5BD4:;
        c.S4 = 0u | 0x0003u;
        c.A0 = c.S3 + 0u;
    L801B5BDC:;
        c.A1 = c.S3 + 0x1780u;
        c.RA = 0x801B5BE4u;
        SoTN.AllocEntity_np3(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.S2 = c.S2 + 0x1u;
            goto L801B5C24;
        }
        c.S2 = c.S2 + 0x1u;
        c.A0 = 0u | 0x004Cu;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801B5C00u;
        SoTN.CreateEntityFromEntity_np3(c, m);
        c.RA = 0x801B5C08u;
        Dispatcher.Call(c, m, 0x801B90BCu);
        c.V0 = c.V0 & 0x0003u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x30u));
        if (c.V0 != c.S4)
        {
            c.V0 = (int)c.S2 < 6 ? 1u : 0u;
            goto L801B5C28;
        }
        c.V0 = (int)c.S2 < 6 ? 1u : 0u;
        m.WriteU16((c.S0 + 0x30u), (ushort)0u);
    L801B5C24:;
        c.V0 = (int)c.S2 < 6 ? 1u : 0u;
    L801B5C28:;
        if (c.V0 != 0u)
        {
            c.A0 = c.S3 + 0u;
            goto L801B5BDC;
        }
        c.A0 = c.S3 + 0u;
    L801B5C30:;
        c.A0 = c.S1 + 0u;
        c.RA = 0x801B5C38u;
        SoTN.DestroyEntity_np3(c, m);
    L801B5C38:;
        c.RA = m.ReadU32((c.SP + 0x54u));
        c.S4 = m.ReadU32((c.SP + 0x50u));
        c.S3 = m.ReadU32((c.SP + 0x4Cu));
        c.S2 = m.ReadU32((c.SP + 0x48u));
        c.S1 = m.ReadU32((c.SP + 0x44u));
        c.S0 = m.ReadU32((c.SP + 0x40u));
        c.SP = c.SP + 0x58u;
        return;
    }

    // Bone Scimitar Drops
    public static void EntityBoneScimitar_no3(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x24u), c.RA);
        m.WriteU32((c.SP + 0x20u), c.S4);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x18u), c.S2);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V0 = m.ReadU32((c.S1 + 0x34u));
        c.V0 = c.V0 & 0x0100u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0007u;
            goto L801D5AE4;
        }
        c.V0 = 0u | 0x0007u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
    L801D5AE4:;
        c.V1 = m.ReadU16((c.S1 + 0x2Cu));
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801D6138;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801B0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x7784u));
        switch (c.V0)
        {
            case 0x801D5B10u: goto L801D5B10;
            case 0x801D5B8Cu: goto L801D5B8C;
            case 0x801D5BB8u: goto L801D5BB8;
            case 0x801D5C20u: goto L801D5C20;
            case 0x801D5C98u: goto L801D5C98;
            case 0x801D5D74u: goto L801D5D74;
            case 0x801D5E84u: goto L801D5E84;
            case 0x801D5FD0u: goto L801D5FD0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801D5B10:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xB78u;
        c.RA = 0x801D5B20u;
        SoTN.InitializeEntity_no3(c, m);
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        if (c.V0 == 0u)
        {
            c.A1 = 0x3FFF0000u;
            goto L801D5B78;
        }
        c.A1 = 0x3FFF0000u;
        c.A1 = c.A1 | 0xF3FFu;
        c.V0 = m.ReadU16((c.S1 + 0x16u));
        c.A0 = m.ReadU16((c.S1 + 0x30u));
        c.V1 = m.ReadU32((c.S1 + 0x34u));
        c.V0 = c.V0 + c.A0;
        c.V1 = c.V1 & c.A1;
        m.WriteU16((c.S1 + 0x16u), (ushort)c.V0);
        m.WriteU32((c.S1 + 0x34u), c.V1);
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x308Eu));
        c.A0 = (uint)(short)m.ReadU16((c.S1 + 0x2u));
        c.V1 = m.ReadU16((c.S1 + 0x30u));
        c.A1 = 0x80180000u;
        c.A1 = m.ReadU32((c.A1 + 0x3B50u));
        c.V0 = c.V0 + c.A0;
        c.V1 = c.V1 & c.A1;
        if (c.V1 != 0u)
        {
            m.WriteU32((c.S1 + 0x9Cu), c.V0);
            goto L801D6130;
        }
        m.WriteU32((c.S1 + 0x9Cu), c.V0);
    L801D5B78:;
        c.V0 = 0u | 0x0050u;
        m.WriteU8((c.S1 + 0x7Cu), (byte)c.V0);
        m.WriteU8((c.S1 + 0x80u), (byte)0u);
        m.WriteU8((c.S1 + 0x84u), (byte)0u);
        goto L801D6138;
    L801D5B8C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3C20u;
        c.RA = 0x801D5B9Cu;
        Dispatcher.Call(c, m, 0x801C5074u);
        if (c.V0 == 0u)
        {
            goto L801D6138;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Cu));
        c.V1 = m.ReadU16((c.S1 + 0x30u));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801D5D5C;
    L801D5BB8:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B54u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5BC8u;
        Dispatcher.Call(c, m, 0x801C4D94u);
        if (c.V0 != 0u)
        {
            goto L801D5BE4;
        }
        c.RA = 0x801D5BD8u;
        Dispatcher.Call(c, m, 0x801C4FD4u);
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        m.WriteU16((c.S1 + 0x14u), (ushort)c.V0);
    L801D5BE4:;
        c.V0 = m.ReadU8((c.S1 + 0x14u));
        m.WriteU8((c.S1 + 0x80u), (byte)c.V0);
        c.V0 = m.ReadU8((c.S1 + 0x80u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x8000u;
            goto L801D5C04;
        }
        c.V0 = 0u | 0x8000u;
        c.V0 = 0xFFFF8000u;
    L801D5C04:;
        m.WriteU32((c.S1 + 0x8u), c.V0);
        c.RA = 0x801D5C0Cu;
        Dispatcher.Call(c, m, 0x801C4F64u);
        c.V0 = (int)c.V0 < 76 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0003u;
            goto L801D5C88;
        }
        c.V0 = 0u | 0x0003u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        goto L801D5C88;
    L801D5C20:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B64u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5C30u;
        Dispatcher.Call(c, m, 0x801C4D94u);
        if (c.V0 != 0u)
        {
            goto L801D5C4C;
        }
        c.RA = 0x801D5C40u;
        Dispatcher.Call(c, m, 0x801C4FD4u);
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        m.WriteU16((c.S1 + 0x14u), (ushort)c.V0);
    L801D5C4C:;
        c.V0 = m.ReadU8((c.S1 + 0x14u));
        c.V0 = c.V0 ^ 0x0001u;
        m.WriteU8((c.S1 + 0x80u), (byte)c.V0);
        c.V0 = m.ReadU8((c.S1 + 0x80u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x8000u;
            goto L801D5C70;
        }
        c.V0 = 0u | 0x8000u;
        c.V0 = 0xFFFF8000u;
    L801D5C70:;
        m.WriteU32((c.S1 + 0x8u), c.V0);
        c.RA = 0x801D5C78u;
        Dispatcher.Call(c, m, 0x801C4F64u);
        c.V0 = (int)c.V0 < 93 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0002u;
            goto L801D5C88;
        }
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
    L801D5C88:;
        c.RA = 0x801D5C90u;
        SoTN.BoneScimitarAttackCheck_no3(c, m);
        goto L801D6138;
    L801D5C98:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B74u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5CA8u;
        Dispatcher.Call(c, m, 0x801C4D94u);
        c.S0 = c.V0 + 0u;
        c.V1 = (uint)(short)m.ReadU16((c.S1 + 0x56u));
        c.V0 = 0u | 0x000Cu;
        if (c.V1 != c.V0)
        {
            c.V0 = 0u | 0x0008u;
            goto L801D5CE0;
        }
        c.V0 = 0u | 0x0008u;
        c.V0 = 0u | 0x0014u;
        m.WriteU8((c.S1 + 0x46u), (byte)c.V0);
        c.V0 = 0u | 0x0011u;
        m.WriteU8((c.S1 + 0x47u), (byte)c.V0);
        c.V0 = 0xFFFFFFF5u;
        m.WriteU16((c.S1 + 0x10u), (ushort)c.V0);
        c.V0 = 0xFFFFFFF2u;
        m.WriteU16((c.S1 + 0x12u), (ushort)c.V0);
        goto L801D5CF8;
    L801D5CE0:;
        m.WriteU8((c.S1 + 0x46u), (byte)c.V0);
        c.V0 = 0u | 0x0012u;
        m.WriteU8((c.S1 + 0x47u), (byte)c.V0);
        c.V0 = 0xFFFFFFFFu;
        m.WriteU16((c.S1 + 0x10u), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x12u), (ushort)0u);
    L801D5CF8:;
        c.V1 = m.ReadU32((c.S1 + 0x50u));
        c.V0 = 0u | 0x0007u;
        if (c.V1 != c.V0)
        {
            c.V0 = c.S0 & 0x00FFu;
            goto L801D5D14;
        }
        c.V0 = c.S0 & 0x00FFu;
        c.A0 = 0u | 0x066Du;
        c.RA = 0x801D5D10u;
        SoTN.PlaySfxPositional_no3(c, m);
        c.V0 = c.S0 & 0x00FFu;
    L801D5D14:;
        if (c.V0 != 0u)
        {
            goto L801D6138;
        }
        c.A0 = 0u | 0x0003u;
        c.RA = 0x801D5D24u;
        SoTN.SetStep_no3(c, m);
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3C18u;
        c.V1 = m.ReadU8((c.S1 + 0x84u));
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        c.V1 = c.V1 + 0x1u;
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        m.WriteU8((c.S1 + 0x84u), (byte)c.V1);
        c.V1 = c.V1 & 0x0003u;
        c.V0 = c.V0 + c.V1;
        c.V0 = m.ReadU8(c.V0);
        c.V1 = m.ReadU16((c.S1 + 0x30u));
        m.WriteU8((c.S1 + 0x7Cu), (byte)c.V0);
    L801D5D5C:;
        if (c.V1 == 0u)
        {
            goto L801D6138;
        }
        c.A0 = 0u | 0x0006u;
        c.RA = 0x801D5D6Cu;
        SoTN.SetStep_no3(c, m);
        goto L801D6138;
    L801D5D74:;
        c.V1 = m.ReadU16((c.S1 + 0x2Eu));
        c.V0 = 0u | 0x0001u;
        if (c.V1 == c.V0)
        {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801D5E1C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801D5D9C;
        }
        if (c.V1 == 0u)
        {
            goto L801D5DB0;
        }
        goto L801D6138;
    L801D5D9C:;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0)
        {
            goto L801D5E5C;
        }
        goto L801D6138;
    L801D5DB0:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B90u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5DC0u;
        Dispatcher.Call(c, m, 0x801C4D94u);
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u)
        {
            goto L801D6138;
        }
        c.S0 = m.ReadU8((c.S1 + 0x80u));
        c.RA = 0x801D5DD8u;
        SoTN.Random_no3(c, m);
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 != 0u)
        {
            c.V0 = c.S0 & 0x00FFu;
            goto L801D5DEC;
        }
        c.V0 = c.S0 & 0x00FFu;
        c.S0 = c.S0 ^ 0x0001u;
        c.V0 = c.S0 & 0x00FFu;
    L801D5DEC:;
        if (c.V0 != 0u)
        {
            c.V0 = 0x00020000u;
            goto L801D5DF8;
        }
        c.V0 = 0x00020000u;
        c.V0 = 0xFFFE0000u;
    L801D5DF8:;
        m.WriteU32((c.S1 + 0x8u), c.V0);
        c.V1 = m.ReadU16((c.S1 + 0x2Eu));
        c.V0 = 0xFFFD0000u;
        m.WriteU32((c.S1 + 0xCu), c.V0);
        m.WriteU16((c.S1 + 0x50u), (ushort)0u);
        m.WriteU16((c.S1 + 0x52u), (ushort)0u);
        c.V1 = c.V1 + 0x1u;
        m.WriteU16((c.S1 + 0x2Eu), (ushort)c.V1);
        goto L801D6138;
    L801D5E1C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3C20u;
        c.RA = 0x801D5E2Cu;
        Dispatcher.Call(c, m, 0x801C5074u);
        if (c.V0 == 0u)
        {
            goto L801D5E44;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S1 + 0x2Eu), (ushort)c.V0);
    L801D5E44:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3C38u;
        c.A1 = 0u | 0x0002u;
        c.RA = 0x801D5E54u;
        Dispatcher.Call(c, m, 0x801C5BC0u);
        goto L801D6138;
    L801D5E5C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B9Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5E6Cu;
        Dispatcher.Call(c, m, 0x801C4D94u);
        if (c.V0 != 0u)
        {
            goto L801D6138;
        }
        c.A0 = 0u | 0x0003u;
        c.RA = 0x801D5E7Cu;
        SoTN.SetStep_no3(c, m);
        goto L801D6138;
    L801D5E84:;
        c.RA = 0x801D5E8Cu;
        Dispatcher.Call(c, m, 0x801C4FD4u);
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3C30u;
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        m.WriteU16((c.S1 + 0x14u), (ushort)c.V0);
        c.RA = 0x801D5EA4u;
        SoTN.UnkCollisionFunc2_no3(c, m);
        c.V0 = m.ReadU32((c.S1 + 0x8u));
        c.V1 = m.ReadU16((c.S1 + 0x14u));
        c.V0 = c.V0 >> 31;
        c.V0 = c.V0 ^ c.V1;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B64u;
        if (c.V0 == 0u)
        {
            goto L801D5ECC;
        }
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x3B54u;
    L801D5ECC:;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801D5ED4u;
        Dispatcher.Call(c, m, 0x801C4D94u);
        c.V1 = m.ReadU16((c.S1 + 0x2Eu));
        if (c.V1 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801D5EF4;
        }
        c.V0 = 0u | 0x0001u;
        if (c.V1 == c.V0)
        {
            c.V0 = 0xFFFF8000u;
            goto L801D5F34;
        }
        c.V0 = 0xFFFF8000u;
        goto L801D5F74;
    L801D5EF4:;
        c.V0 = 0u | 0x8000u;
        m.WriteU32((c.S1 + 0x8u), c.V0);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x308Eu));
        c.V1 = m.ReadU16((c.S1 + 0x2u));
        c.A0 = m.ReadU16((c.S1 + 0x9Cu));
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 33 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801D5F74;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801D5F70;
    L801D5F34:;
        m.WriteU32((c.S1 + 0x8u), c.V0);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x308Eu));
        c.V1 = m.ReadU16((c.S1 + 0x2u));
        c.A0 = m.ReadU16((c.S1 + 0x9Cu));
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < -32 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801D5F74;
        }
        c.V0 = m.ReadU16((c.S1 + 0x2Eu));
        c.V0 = c.V0 - 0x1u;
    L801D5F70:;
        m.WriteU16((c.S1 + 0x2Eu), (ushort)c.V0);
    L801D5F74:;
        c.V0 = m.ReadU8((c.S1 + 0x7Cu));
        if (c.V0 == 0u)
        {
            goto L801D5F98;
        }
        c.V0 = m.ReadU8((c.S1 + 0x7Cu));
        c.V0 = c.V0 - 0x1u;
        m.WriteU8((c.S1 + 0x7Cu), (byte)c.V0);
        goto L801D6138;
    L801D5F98:;
        c.RA = 0x801D5FA0u;
        Dispatcher.Call(c, m, 0x801C4F64u);
        c.V0 = (int)c.V0 < 48 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801D6138;
        }
        c.RA = 0x801D5FB4u;
        SoTN.GetDistanceToPlayerY_no3(c, m);
        c.V0 = (int)c.V0 < 32 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801D6138;
        }
        c.A0 = 0u | 0x0004u;
        c.RA = 0x801D5FC8u;
        SoTN.SetStep_no3(c, m);
        goto L801D6138;
    L801D5FD0:;
        c.A0 = 0u | 0x062Bu;
        c.S2 = 0u + 0u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.S3 = 0x80180000u;
        c.S3 = c.S3 + 0x3BF8u;
        c.S4 = 0u + 0u;
        c.RA = 0x801D5FF0u;
        Dispatcher.Call(c, m, c.V0);
    L801D5FF0:;
        c.A0 = 0x80080000u;
        c.A0 = c.A0 - 0x27A8u;
        c.A1 = c.A0 + 0x1780u;
        c.RA = 0x801D6000u;
        Dispatcher.Call(c, m, 0x801C54D4u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u)
        {
            c.A0 = 0u | 0x0047u;
            goto L801D60CC;
        }
        c.A0 = 0u | 0x0047u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x801D6014u;
        SoTN.CreateEntityFromCurrentEntity_no3(c, m);
        c.V0 = m.ReadU16((c.S1 + 0x14u));
        m.WriteU16((c.S0 + 0x30u), (ushort)c.S2);
        m.WriteU16((c.S0 + 0x14u), (ushort)c.V0);
        c.At = 0x80180000u;
        c.At = c.At + c.S2;
        c.V0 = m.ReadU8((c.At + 0x3BB8u));
        m.WriteU8((c.S0 + 0x88u), (byte)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x14u));
        if (c.V0 == 0u)
        {
            goto L801D6054;
        }
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.V1 = m.ReadU16(c.S3);
        c.V0 = c.V0 - c.V1;
        goto L801D6064;
    L801D6054:;
        c.V0 = m.ReadU16((c.S0 + 0x2u));
        c.V1 = m.ReadU16(c.S3);
        c.V0 = c.V0 + c.V1;
    L801D6064:;
        m.WriteU16((c.S0 + 0x2u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x6u));
        c.At = 0x80180000u;
        c.At = c.At + c.S4;
        c.V1 = m.ReadU16((c.At + 0x3C08u));
        c.S4 = c.S4 + 0x2u;
        c.V0 = c.V0 + c.V1;
        c.V1 = c.S2 << 2;
        m.WriteU16((c.S0 + 0x6u), (ushort)c.V0);
        c.At = 0x80180000u;
        c.At = c.At + c.V1;
        c.V0 = m.ReadU32((c.At + 0x3BC0u));
        c.S3 = c.S3 + 0x2u;
        m.WriteU32((c.S0 + 0x8u), c.V0);
        c.At = 0x80180000u;
        c.At = c.At + c.V1;
        c.V0 = m.ReadU32((c.At + 0x3BDCu));
        c.S2 = c.S2 + 0x1u;
        m.WriteU32((c.S0 + 0xCu), c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        c.V1 = m.ReadU16((c.S0 + 0x30u));
        c.V0 = c.V0 << 8;
        c.V1 = c.V1 | c.V0;
        c.V0 = (int)c.S2 < 7 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16((c.S0 + 0x30u), (ushort)c.V1);
            goto L801D5FF0;
        }
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V1);
    L801D60CC:;
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        if (c.V0 == 0u)
        {
            c.S0 = c.S1 + 0xBCu;
            goto L801D6130;
        }
        c.S0 = c.S1 + 0xBCu;
        c.A0 = 0u | 0x000Au;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x801D60ECu;
        SoTN.CreateEntityFromEntity_no3(c, m);
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u)
        {
            //c.V0 = 0u | 0x0013u;
            c.V0 = 0u | m.ReadU16(0x800A9984);
            c.V0 -= 0x80;   // added adjustment
            goto L801D6104;
        }
        c.V0 = 0u | 0x0013u;
        //c.V0 = 0u | 0x001Au;
        c.V0 = 0u | m.ReadU16(0x800A9982);
        c.V0 -= 0x80;   // added adjustment
    L801D6104:;
        m.WriteU16((c.S1 + 0xECu), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x30u));
        c.V1 = 0x80180000u;
        c.V1 = m.ReadU32((c.V1 + 0x3B50u));
        c.V0 = c.V0 | 0x8000u;
        m.WriteU16((c.S0 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S1 + 0x30u));
        c.V0 = c.V0 | c.V1;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x3B50u), c.V0);
    L801D6130:;
        c.A0 = c.S1 + 0u;
        c.RA = 0x801D6138u;
        SoTN.DestroyEntity_no3(c, m);
    L801D6138:;
        c.RA = m.ReadU32((c.SP + 0x24u));
        c.S4 = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Trio Relic/Item Drop
    public static void RBO0_EntityBoss(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x20u), c.RA);
        m.WriteU32((c.SP + 0x18u), c.S2);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V1 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801946D4;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x80190000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x35B0u));
        switch (c.V0)
        {
            case 0x801943DCu: goto L801943DC;
            case 0x801944A0u: goto L801944A0;
            case 0x80194530u: goto L80194530;
            case 0x80194570u: goto L80194570;
            case 0x801945E0u: goto L801945E0;
            case 0x80194604u: goto L80194604;
            case 0x80194688u: goto L80194688;
            case 0x801946D4u: goto L801946D4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801943DC:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x458u;
        c.S0 = 0u | 0x00C4u;
        c.RA = 0x801943ECu;
        SoTN.InitializeEntity_rbo0(c, m);
        c.S2 = 0x80070000u;
        c.S2 = c.S2 + 0x6E98u;
        c.A0 = 0u | 0x001Bu;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80194400u;
        SoTN.CreateEntityFromCurrentEntity_rbo0(c, m);
        c.S2 = c.S2 + 0x5E0u;
        c.A0 = 0u | 0x001Cu;
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x308Eu;
        c.V1 = m.ReadU16(c.S1);
        c.V0 = 0u | 0x0100u;
        c.V0 = c.V0 - c.V1;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6E9Au), (ushort)c.V0);
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x3092u));
        c.V0 = c.S0 - c.V0;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x6E9Eu), (ushort)c.V0);
        c.A1 = c.S2 + 0u;
        c.RA = 0x80194444u;
        SoTN.CreateEntityFromCurrentEntity_rbo0(c, m);
        c.A0 = 0u | 0x001Du;
        c.V0 = 0u | 0x00B8u;
        c.A2 = m.ReadU16(c.S1);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        c.V0 = c.V0 - c.A2;
        c.V1 = c.S0 - c.V1;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x747Au), (ushort)c.V0);
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x747Eu), (ushort)c.V1);
        c.A1 = c.S2 + 0x5E0u;
        c.RA = 0x80194478u;
        SoTN.CreateEntityFromCurrentEntity_rbo0(c, m);
        c.V0 = 0u | 0x0148u;
        c.V1 = m.ReadU16(c.S1);
        c.A0 = 0x80070000u;
        c.A0 = m.ReadU16((c.A0 + 0x3092u));
        c.V0 = c.V0 - c.V1;
        c.S0 = c.S0 - c.A0;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x7A5Au), (ushort)c.V0);
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x7A5Eu), (ushort)c.S0);
    L801944A0:;
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DAu));
        c.V1 = 0x80070000u;
        c.V1 = (uint)(short)m.ReadU16((c.V1 + 0x308Eu));
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 - 0xE1u;
        c.V0 = c.V0 < 0x0000005Fu ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x000Bu;
            goto L801946D4;
        }
        c.A0 = 0u | 0x000Bu;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x37C0u));
        c.A1 = 0u | 0x0002u;
        c.RA = 0x801944DCu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0u | 0x0001u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x6ACu), c.V0);
        c.V0 = 0u | 0x0140u;
        m.WriteU16((c.S3 + 0x80u), (ushort)c.V0);
        c.V0 = 0u | 0x031Du;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V0);
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x6B0u));
        c.V1 = 0x80040000u;
        c.V1 = m.ReadU32((c.V1 - 0x3824u));
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), 0u);
        c.V0 = c.V0 | 0x0001u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x6B0u), c.V0);
        c.A0 = 0u | 0x031Du;
        c.RA = 0x80194528u;
        Dispatcher.Call(c, m, c.V1);
        goto L801946C4;
    L80194530:;
        c.V0 = m.ReadU16((c.S3 + 0x80u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S3 + 0x80u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u)
        {
            goto L801946D4;
        }
        c.V1 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x6B0u));
        c.V1 = c.V1 + 0x1u;
        c.V0 = c.V0 | 0x0002u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x6B0u), c.V0);
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.V1);
        goto L801946D4;
    L80194570:;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x6B4u));
        c.V0 = (int)c.V0 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x000Bu;
            goto L801946D4;
        }
        c.A0 = 0u | 0x000Bu;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x37C0u));
        c.A1 = 0u | 0x0001u;
        c.RA = 0x8019459Cu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0u | 0x0080u;
        m.WriteU16((c.S3 + 0x80u), (ushort)c.V0);
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x6B0u));
        c.V1 = 0x80040000u;
        c.V1 = m.ReadU32((c.V1 - 0x3824u));
        c.V0 = c.V0 | 0x0004u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x6B0u), c.V0);
        c.A0 = 0u | 0x0090u;
        c.RA = 0x801945C8u;
        Dispatcher.Call(c, m, c.V1);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        //c.V1 = 0u | 0x0315u;
        c.V1 = 0u | m.ReadU16(0x801945CC);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V1);
        c.V0 = c.V0 + 0x1u;
        goto L801946D0;
    L801945E0:;
        c.V0 = m.ReadU16((c.S3 + 0x80u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S3 + 0x80u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 == 0u)
        {
            goto L801946C4;
        }
        goto L801946D4;
    L80194604:;
        c.A0 = 0x80080000u;
        c.A0 = c.A0 - 0x56A8u;
        c.A1 = c.A0 + 0x1780u;
        c.RA = 0x80194614u;
        SoTN.AllocEntity_rbo0(c, m);
        c.S2 = c.V0 + 0u;
        if (c.S2 == 0u)
        {
            c.A0 = 0u | 0x0018u;
            goto L801946D4;
        }
        c.A0 = 0u | 0x0018u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x8019462Cu;
        SoTN.CreateEntityFromEntity_rbo0(c, m);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x308Eu));
        c.V0 = 0u | 0x0001u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), c.V0);
        c.V0 = 0u | 0x0100u;
        c.At = 0x80180000u;
        m.WriteU32((c.At + 0x6ACu), 0u);
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S2 + 0x2u), (ushort)c.V0);
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x3092u));
        //c.V0 = 0u | 0x0002u;
        c.V0 = 0u | m.ReadU16(0x8019465c);
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V0);
        c.V0 = 0u | 0x0080u;
        c.V0 = c.V0 - c.V1;
        m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        //c.V1 = 0u | 0x0315u;
        c.V1 = 0u | m.ReadU16(0x80194674);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), c.V1);
        c.V0 = c.V0 + 0x1u;
        goto L801946D0;
    L80194688:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3808u));
        c.RA = 0x8019469Cu;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 != 0u)
        {
            goto L801946D4;
        }
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x7910u));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), 0u);
        c.RA = 0x801946C4u;
        Dispatcher.Call(c, m, c.V0);
    L801946C4:;
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
    L801946D0:;
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.V0);
    L801946D4:;
        c.RA = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    // Trio Life Max Up Spawn
    public static void EntityLifeUpSpawn_rbo0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xE8u;
        m.WriteU32((c.SP + 0xC8u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0xE4u), c.RA);
        m.WriteU32((c.SP + 0xE0u), c.FP);
        m.WriteU32((c.SP + 0xDCu), c.S7);
        m.WriteU32((c.SP + 0xD8u), c.S6);
        m.WriteU32((c.SP + 0xD4u), c.S5);
        m.WriteU32((c.SP + 0xD0u), c.S4);
        m.WriteU32((c.SP + 0xCCu), c.S3);
        m.WriteU32((c.SP + 0xC4u), c.S1);
        m.WriteU32((c.SP + 0xC0u), c.S0);
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801A6190;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x80190000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x37E0u));
        switch (c.V0)
        {
            case 0x801A596Cu: goto L801A596C;
            case 0x801A5BD4u: goto L801A5BD4;
            case 0x801A5C20u: goto L801A5C20;
            case 0x801A5FC0u: goto L801A5FC0;
            case 0x801A5F38u: goto L801A5F38;
            case 0x801A6050u: goto L801A6050;
            case 0x801A6080u: goto L801A6080;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801A596C:;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x458u;
        c.RA = 0x801A597Cu;
        SoTN.InitializeEntity_rbo0(c, m);
        c.A0 = 0u | 0x0004u;
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S2 + 0x54u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x56u), (ushort)0u);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3820u));
        c.A1 = 0u | 0x0181u;
        c.RA = 0x801A59A0u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 != c.V0)
        {
            c.V0 = c.A0 << 1;
            goto L801A59C0;
        }
        c.V0 = c.A0 << 1;
        c.V0 = 0u | 0x0006u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801A6190;
    L801A59C0:;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.S1 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S2 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S2 + 0x64u), c.A0);
        m.WriteU32((c.S2 + 0x80u), c.S1);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S2 + 0x34u), c.V0);
        c.V0 = 0u | 0x001Au;
        m.WriteU16((c.S1 + 0x1Au), (ushort)c.V0);
        c.V0 = 0u | 0x019Fu;
        m.WriteU16((c.S1 + 0xEu), (ushort)c.V0);
        c.V0 = 0u | 0x003Fu;
        m.WriteU8((c.S1 + 0x30u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x18u), (byte)c.V0);
        c.V0 = 0u | 0x00C0u;
        m.WriteU8((c.S1 + 0x19u), (byte)c.V0);
        m.WriteU8((c.S1 + 0xDu), (byte)c.V0);
        c.V0 = 0u | 0x00FFu;
        m.WriteU8((c.S1 + 0x24u), (byte)0u);
        m.WriteU8((c.S1 + 0xCu), (byte)0u);
        m.WriteU8((c.S1 + 0x31u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x25u), (byte)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2u));
        c.S0 = 0u + 0u;
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x20u), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x14u), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x8u), (ushort)c.V0);
        c.V1 = m.ReadU16((c.S2 + 0x2u));
        c.V0 = 0u | 0x00C0u;
        m.WriteU16((c.S1 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0033u;
        m.WriteU16((c.S1 + 0x32u), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x2Eu), (ushort)c.V1);
        m.WriteU16((c.S1 + 0x22u), (ushort)c.V1);
        m.WriteU16((c.S1 + 0x16u), (ushort)c.V1);
        m.WriteU16((c.S1 + 0xAu), (ushort)c.V1);
        c.S1 = m.ReadU32(c.S1);
        c.S3 = 0u | 0x0020u;
        m.WriteU32((c.S2 + 0x7Cu), c.S1);
        c.S6 = 0u + 0u;
    L801A5A7C:;
        c.S2 = 0u + 0u;
    L801A5A80:;
        c.S5 = 0u + 0u;
        c.S4 = c.S2 + 0u;
    L801A5A88:;
        c.A0 = c.S1 + 0u;
        c.RA = 0x801A5A90u;
        SoTN.UnkPolyFunc2_rbo0(c, m);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = 0u | 0x001Au;
        m.WriteU16((c.S1 + 0x1Au), (ushort)c.V0);
        c.V0 = 0u | 0x0194u;
        m.WriteU16((c.S1 + 0xEu), (ushort)c.V0);
        c.V0 = 0u | 0x0010u;
        m.WriteU8((c.S1 + 0x30u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x18u), (byte)c.V0);
        c.V0 = 0u | 0x0050u;
        m.WriteU8((c.S1 + 0x19u), (byte)c.V0);
        m.WriteU8((c.S1 + 0xDu), (byte)c.V0);
        c.V0 = 0u | 0x0060u;
        m.WriteU8((c.S1 + 0x31u), (byte)c.V0);
        m.WriteU8((c.S1 + 0x25u), (byte)c.V0);
        c.V0 = 0u | 0x1000u;
        m.WriteU8((c.S1 + 0x24u), (byte)0u);
        m.WriteU8((c.S1 + 0xCu), (byte)0u);
        m.WriteU8((c.S1 + 0x28u), (byte)c.S3);
        m.WriteU8((c.S1 + 0x1Cu), (byte)c.S3);
        m.WriteU8((c.S1 + 0x10u), (byte)c.S3);
        m.WriteU8((c.S1 + 0x4u), (byte)c.S3);
        m.WriteU8((c.S1 + 0x29u), (byte)0u);
        m.WriteU8((c.S1 + 0x1Du), (byte)0u);
        m.WriteU8((c.S1 + 0x11u), (byte)0u);
        m.WriteU8((c.S1 + 0x5u), (byte)0u);
        m.WriteU8((c.S1 + 0x2Au), (byte)0u);
        m.WriteU8((c.S1 + 0x1Eu), (byte)0u);
        m.WriteU8((c.S1 + 0x12u), (byte)0u);
        m.WriteU8((c.S1 + 0x6u), (byte)0u);
        m.WriteU16((c.V1 + 0x22u), (ushort)c.V0);
        m.WriteU16((c.V1 + 0x20u), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = c.S5 << 9;
        m.WriteU16((c.V1 + 0x1Au), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S1);
        m.WriteU16((c.V0 + 0x2Cu), (ushort)0u);
        c.V0 = m.ReadU32(c.S1);
        m.WriteU16((c.V0 + 0x2Eu), (ushort)c.S4);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = 0xFFFB0000u;
        m.WriteU32((c.V1 + 0xCu), c.V0);
        c.V0 = m.ReadU32(c.S1);
        m.WriteU32((c.V0 + 0x10u), 0u);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = 0u | 0x0080u;
        m.WriteU16((c.V1 + 0x14u), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S1);
        m.WriteU16((c.V0 + 0xAu), (ushort)0u);
        c.V0 = 0u | 0x00C0u;
        m.WriteU16((c.S1 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0073u;
        m.WriteU16((c.S1 + 0x32u), (ushort)c.V0);
        c.S1 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.S1 + 0x32u));
        c.S5 = c.S5 + 0x1u;
        c.V0 = c.V0 & 0xFFFDu;
        m.WriteU16((c.S1 + 0x32u), (ushort)c.V0);
        c.V0 = (int)c.S5 < 8 ? 1u : 0u;
        c.S1 = m.ReadU32(c.S1);
        if (c.V0 != 0u)
        {
            goto L801A5A88;
        }
        c.S6 = c.S6 + 0x1u;
        c.V0 = (int)c.S6 < 3 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S2 = c.S2 + 0x540u;
            goto L801A5A80;
        }
        c.S2 = c.S2 + 0x540u;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 8 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S6 = 0u + 0u;
            goto L801A5A7C;
        }
        c.S6 = 0u + 0u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07D2u;
        c.RA = 0x801A5BCCu;
        Dispatcher.Call(c, m, c.V0);
        goto L801A6190;
    L801A5BD4:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x86u));
        if (c.V0 != 0u)
        {
            c.V1 = 0u | 0x0002u;
            goto L801A5BF4;
        }
        c.V1 = 0u | 0x0002u;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
    L801A5BF4:;
        c.V0 = m.ReadU16((c.S2 + 0x86u));
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        c.V1 = (int)c.V1 < 8 ? 1u : 0u;
        if (c.V1 != 0u)
        {
            m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
            goto L801A5C20;
        }
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Cu));
        c.V1 = 0u | 0x0007u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
    L801A5C20:;
        c.A0 = 0u | 0x0200u;
        c.RA = 0x801A5C28u;
        SoTN.SetGeomScreen(c, m);
        c.A0 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        c.A1 = (uint)(short)m.ReadU16((c.S2 + 0x6u));
        c.S6 = 0u + 0u;
        m.WriteU32((c.SP + 0xA0u), 0u);
        c.RA = 0x801A5C3Cu;
        SoTN.SetGeomOffset(c, m);
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x88u));
        c.S1 = m.ReadU32((c.S2 + 0x7Cu));
        c.V0 = c.V0 + 0x1u;
        if ((int)c.V0 <= 0)
        {
            c.T0 = c.SP + 0x60u;
            goto L801A5F00;
        }
        c.T0 = c.SP + 0x60u;
        c.S3 = c.SP + 0x70u;
        m.WriteU32((c.SP + 0xA8u), c.T0);
    L801A5C58:;
        c.V1 = m.ReadU32(c.S1);
        c.V0 = (uint)(short)m.ReadU16((c.V1 + 0x14u));
        c.A0 = (uint)(short)m.ReadU16((c.V1 + 0x16u));
        c.A1 = m.ReadU32((c.V1 + 0xCu));
        c.V0 = c.V0 << 16;
        c.S4 = c.A0 + c.V0;
        c.S4 = c.S4 + c.A1;
        m.WriteU16((c.V1 + 0x16u), (ushort)c.S4);
        c.V0 = m.ReadU32(c.S1);
        c.V1 = (uint)((int)c.S4 >> 16);
        m.WriteU16((c.V0 + 0x14u), (ushort)c.V1);
        c.A1 = m.ReadU32(c.S1);
        c.A0 = m.ReadU32((c.A1 + 0xCu));
        c.V0 = (int)c.A0 < -16384 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.S4 = c.V1 + 0u;
            goto L801A5CAC;
        }
        c.S4 = c.V1 + 0u;
        c.V0 = c.A0 + 0x3800u;
        m.WriteU32((c.A1 + 0xCu), c.V0);
    L801A5CAC:;
        c.V0 = m.ReadU32(c.S1);
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x14u));
        c.V0 = (int)c.V0 < 8 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V1 = 0u | 0x0008u;
            goto L801A5D00;
        }
        c.V1 = 0u | 0x0008u;
        c.T0 = m.ReadU32((c.SP + 0xA0u));
        c.T0 = c.T0 + 0x1u;
        m.WriteU32((c.SP + 0xA0u), c.T0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.S0 = 0u | 0x002Fu;
        c.V0 = c.V0 + 0x4u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
    L801A5CE8:;
        m.WriteU16((c.S1 + 0x32u), (ushort)c.V1);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0)
        {
            c.S1 = c.S1 + 0x34u;
            goto L801A5CE8;
        }
        c.S1 = c.S1 + 0x34u;
        goto L801A5EE8;
    L801A5D00:;
        c.S5 = 0u + 0u;
        c.FP = c.SP + 0x98u;
        c.S7 = c.SP + 0x9Cu;
    L801A5D0C:;
        c.V0 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V0 + 0x2Cu));
        m.WriteU16((c.SP + 0x58u), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V0 + 0x2Eu));
        m.WriteU16((c.SP + 0x5Au), (ushort)c.V0);
        c.V0 = m.ReadU32(c.S1);
        c.A0 = c.SP + 0x58u;
        c.V0 = m.ReadU16((c.V0 + 0x1Au));
        c.A1 = c.S3 + 0u;
        m.WriteU16((c.SP + 0x5Cu), (ushort)c.V0);
        c.RA = 0x801A5D4Cu;
        SoTN.RotMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = m.ReadU32((c.SP + 0xA8u));
        c.V0 = 0u | 0x0200u;
        m.WriteU32((c.SP + 0x60u), 0u);
        m.WriteU32((c.SP + 0x64u), 0u);
        m.WriteU32((c.SP + 0x68u), c.V0);
        c.RA = 0x801A5D68u;
        SoTN.TransMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x801A5D70u;
        SoTN.SetRotMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x801A5D78u;
        SoTN.SetTransMatrix(c, m);
        c.A0 = c.SP + 0x90u;
        c.A1 = c.SP + 0x50u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S7 + 0u;
        m.WriteU16((c.SP + 0x90u), (ushort)c.S4);
        m.WriteU16((c.SP + 0x92u), (ushort)0u);
        m.WriteU16((c.SP + 0x94u), (ushort)0u);
        c.RA = 0x801A5D98u;
        SoTN.RotTransPers(c, m);
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x19B4u;
        c.A1 = c.S3 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x801A5DACu;
        SoTN.RotMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.S0 = c.S0 << 16;
        c.V0 = (uint)(short)m.ReadU16((c.SP + 0x50u));
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        c.A1 = m.ReadU32((c.SP + 0xA8u));
        c.V0 = c.V0 - c.V1;
        m.WriteU32((c.SP + 0x60u), c.V0);
        c.V0 = (uint)(short)m.ReadU16((c.SP + 0x52u));
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x6u));
        c.S0 = (uint)((int)c.S0 >> 14);
        m.WriteU32((c.SP + 0x68u), c.S0);
        c.V0 = c.V0 - c.V1;
        m.WriteU32((c.SP + 0x64u), c.V0);
        c.RA = 0x801A5DE4u;
        SoTN.TransMatrix(c, m);
        c.V0 = m.ReadU32(c.S1);
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x20u));
        c.A0 = c.S3 + 0u;
        m.WriteU32((c.SP + 0x60u), c.V0);
        c.V0 = m.ReadU32(c.S1);
        c.A1 = m.ReadU32((c.SP + 0xA8u));
        c.V1 = (uint)(short)m.ReadU16((c.V0 + 0x22u));
        c.V0 = 0u | 0x1000u;
        m.WriteU32((c.SP + 0x68u), c.V0);
        m.WriteU32((c.SP + 0x64u), c.V1);
        c.RA = 0x801A5E14u;
        SoTN.ScaleMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x801A5E1Cu;
        SoTN.SetRotMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x801A5E24u;
        SoTN.SetTransMatrix(c, m);
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x1968u;
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x1970u;
        c.A2 = 0x80180000u;
        c.A2 = c.A2 + 0x1978u;
        c.A3 = 0x80180000u;
        c.A3 = c.A3 + 0x1980u;
        c.V0 = c.S1 + 0x8u;
        m.WriteU32((c.SP + 0x10u), c.V0);
        c.V0 = c.S1 + 0x14u;
        m.WriteU32((c.SP + 0x14u), c.V0);
        c.V0 = c.S1 + 0x20u;
        m.WriteU32((c.SP + 0x18u), c.V0);
        c.V0 = c.S1 + 0x2Cu;
        m.WriteU32((c.SP + 0x1Cu), c.V0);
        m.WriteU32((c.SP + 0x20u), c.FP);
        m.WriteU32((c.SP + 0x24u), c.S7);
        c.RA = 0x801A5E70u;
        SoTN.RotTransPers4(c, m);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V1 + 0x22u));
        c.V0 = c.V0 - 0x10u;
        m.WriteU16((c.V1 + 0x22u), (ushort)c.V0);
        m.WriteU16((c.V1 + 0x20u), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V1 + 0x1Au));
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.V1 + 0x1Au), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V1 + 0x2Cu));
        c.V0 = c.V0 + 0x10u;
        m.WriteU16((c.V1 + 0x2Cu), (ushort)c.V0);
        c.V1 = m.ReadU32(c.S1);
        c.V0 = m.ReadU16((c.V1 + 0x2Eu));
        c.S5 = c.S5 + 0x1u;
        c.V0 = c.V0 + 0x20u;
        m.WriteU16((c.V1 + 0x2Eu), (ushort)c.V0);
        c.S1 = m.ReadU32(c.S1);
        c.V0 = (int)c.S5 < 24 ? 1u : 0u;
        c.S1 = m.ReadU32(c.S1);
        if (c.V0 != 0u)
        {
            goto L801A5D0C;
        }
    L801A5EE8:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x88u));
        c.S6 = c.S6 + 0x1u;
        c.V0 = c.V0 + 0x1u;
        c.V0 = (int)c.S6 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801A5C58;
        }
    L801A5F00:;
        c.T0 = m.ReadU32((c.SP + 0xA0u));
        c.V0 = 0u | 0x0008u;
        if (c.T0 != c.V0)
        {
            goto L801A5F20;
        }
        c.V0 = m.ReadU16((c.S2 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
    L801A5F20:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S1 = m.ReadU32((c.S2 + 0x80u));
        c.S4 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        m.WriteU32((c.SP + 0x98u), c.V0);
        c.V0 = (int)c.V0 < 257 ? 1u : 0u;
        goto L801A6000;
    L801A5F38:;
        c.RA = 0x801A5F40u;
        SoTN.MoveEntity_rbo0(c, m);
        c.A2 = c.SP + 0x28u;
        c.A3 = 0u + 0u;
        c.A0 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        c.V0 = m.ReadU32((c.S2 + 0xCu));
        c.A1 = m.ReadU16((c.S2 + 0x6u));
        c.V0 = c.V0 + 0x2000u;
        c.A1 = c.A1 + 0x4u;
        c.A1 = c.A1 << 16;
        m.WriteU32((c.S2 + 0xCu), c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3844u));
        c.A1 = (uint)((int)c.A1 >> 16);
        c.RA = 0x801A5F78u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU32((c.SP + 0x28u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            goto L801A5FC0;
        }
        c.V0 = m.ReadU16((c.S2 + 0x6u));
        m.WriteU32((c.S2 + 0xCu), 0u);
        c.A0 = m.ReadU16((c.SP + 0x40u));
        c.V1 = m.ReadU16((c.S2 + 0x84u));
        c.V0 = c.V0 + c.A0;
        c.V1 = c.V1 - 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V1);
        c.V1 = c.V1 << 16;
        if (c.V1 != 0u)
        {
            m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
            goto L801A5FC0;
        }
        m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
        c.V0 = 0u | 0x0005u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
        goto L801A6190;
    L801A5FC0:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        if ((int)c.V0 <= 0)
        {
            c.V1 = c.V0 + 0u;
            goto L801A5FDC;
        }
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 - 0x20u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        goto L801A5FEC;
    L801A5FDC:;
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        c.V0 = 0u | 0x0005u;
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V0);
    L801A5FEC:;
        c.V0 = (uint)(short)m.ReadU16((c.S2 + 0x84u));
        c.S1 = m.ReadU32((c.S2 + 0x80u));
        c.S4 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        m.WriteU32((c.SP + 0x98u), c.V0);
        c.V0 = (int)c.V0 < 225 ? 1u : 0u;
    L801A6000:;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x00E0u;
            goto L801A600C;
        }
        c.V0 = 0u | 0x00E0u;
        m.WriteU32((c.SP + 0x98u), c.V0);
    L801A600C:;
        c.V0 = m.ReadU16((c.SP + 0x98u));
        c.V1 = c.S4 - c.V0;
        c.A0 = c.V0 + c.S4;
        m.WriteU16((c.S1 + 0x20u), (ushort)c.V1);
        m.WriteU16((c.S1 + 0x8u), (ushort)c.V1);
        m.WriteU16((c.S1 + 0x2Cu), (ushort)c.A0);
        m.WriteU16((c.S1 + 0x14u), (ushort)c.A0);
        c.S4 = (uint)(short)m.ReadU16((c.S2 + 0x6u));
        c.V1 = c.S4 - c.V0;
        c.V0 = c.V0 + c.S4;
        m.WriteU16((c.S1 + 0x16u), (ushort)c.V1);
        m.WriteU16((c.S1 + 0xAu), (ushort)c.V1);
        m.WriteU16((c.S1 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.S1 + 0x22u), (ushort)c.V0);
        goto L801A6190;
    L801A6050:;
        c.A0 = m.ReadU32((c.S2 + 0x64u));
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x384Cu));
        c.RA = 0x801A6068u;
        Dispatcher.Call(c, m, c.V0);
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.V0 = m.ReadU16((c.S2 + 0x6u));
        c.V1 = c.V1 + 0x1u;
        c.V0 = c.V0 - 0x4u;
        m.WriteU16((c.S2 + 0x6u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x2Cu), (ushort)c.V1);
    L801A6080:;
        c.V1 = m.ReadU16((c.S2 + 0x30u));
        c.V0 = c.V1 < 0x00000011u ? 1u : 0u;
        // Added for Modified Instruction to happen if present.
        if(m.ReadU32(0x801a6088) == 0x34020000)
        {
            c.V0 = 0;
        }
        if (c.V0 != 0u)
        {
            c.V0 = c.V1 << 1;
            goto L801A611C;
        }
        c.V0 = c.V1 << 1;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3660u));
        if (c.V0 == 0u)
        {
            c.V1 = 0u | 0x0017u;
            goto L801A60D8;
        }
        c.V1 = 0u | 0x0017u;
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V1);
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V1);
        c.V1 = m.ReadU16((c.S2 + 0x30u));
        c.V0 = 0u | 0x0003u;
        m.WriteU16((c.S2 + 0x26u), (ushort)c.V0);
        c.V0 = 0x801A0000u;
        c.V0 = c.V0 - 0x1164u;
        m.WriteU32((c.S2 + 0x28u), c.V0);
        c.V0 = 0u | 0x0010u;
        m.WriteU16((c.S2 + 0x52u), (ushort)0u);
        m.WriteU16((c.S2 + 0x50u), (ushort)0u);
        goto L801A6180;
    L801A60D8:;
        c.V0 = 0u | 0x000Bu;
        m.WriteU16((c.S2 + 0x26u), (ushort)c.V0);
        c.V0 = 0x801A0000u;
        c.V0 = c.V0 + 0x148u;
        m.WriteU32((c.S2 + 0x28u), c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x30u));
        c.V1 = 0u | 0x0010u;
        m.WriteU16((c.S2 + 0x52u), (ushort)0u);
        m.WriteU16((c.S2 + 0x50u), (ushort)0u);
        m.WriteU8((c.S2 + 0x6Du), (byte)c.V1);
        c.V0 = c.V0 << 1;
        c.At = 0x80180000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU16((c.At + 0x1988u));
        m.WriteU16((c.S2 + 0x2Cu), (ushort)0u);
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V0);
        goto L801A6190;
    L801A611C:;
        c.At = 0x80180000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU16((c.At + 0x1988u));
        c.V1 = c.V0 & 0x0FFFu;
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V0);
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0003u;
            goto L801A615C;
        }
        c.V0 = 0u | 0x0003u;
        m.WriteU16((c.S2 + 0x26u), (ushort)c.V0);
        c.V0 = 0x801A0000u;
        c.V0 = c.V0 - 0x1164u;
        m.WriteU32((c.S2 + 0x28u), c.V0);
        m.WriteU16((c.S2 + 0x52u), (ushort)0u);
        m.WriteU16((c.S2 + 0x50u), (ushort)0u);
        goto L801A6174;
    L801A615C:;
        c.V1 = c.V1 - 0x80u;
        c.V0 = 0u | 0x000Au;
        m.WriteU16((c.S2 + 0x26u), (ushort)c.V0);
        c.V0 = 0x801A0000u;
        c.V0 = c.V0 - 0x750u;
        m.WriteU32((c.S2 + 0x28u), c.V0);
    L801A6174:;
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V1);
        c.V1 = m.ReadU16((c.S2 + 0x30u));
        c.V0 = 0u | 0x0010u;
    L801A6180:;
        m.WriteU8((c.S2 + 0x6Du), (byte)c.V0);
        m.WriteU16((c.S2 + 0x2Cu), (ushort)0u);
        c.V1 = c.V1 | 0x8000u;
        m.WriteU16((c.S2 + 0x30u), (ushort)c.V1);
    L801A6190:;
        c.RA = m.ReadU32((c.SP + 0xE4u));
        c.FP = m.ReadU32((c.SP + 0xE0u));
        c.S7 = m.ReadU32((c.SP + 0xDCu));
        c.S6 = m.ReadU32((c.SP + 0xD8u));
        c.S5 = m.ReadU32((c.SP + 0xD4u));
        c.S4 = m.ReadU32((c.SP + 0xD0u));
        c.S3 = m.ReadU32((c.SP + 0xCCu));
        c.S2 = m.ReadU32((c.SP + 0xC8u));
        c.S1 = m.ReadU32((c.SP + 0xC4u));
        c.S0 = m.ReadU32((c.SP + 0xC0u));
        c.SP = c.SP + 0xE8u;
        return;
    }

    // AntiFreeze
    // Rather than patching the whole function, this is a post function hook to get the same effect.
    public static void AntiFreeze(CpuContext c, IMemory m)
    {
        if(m.ReadU8(0x80121B74) == 0x00 && m.ReadU8(0x80097420) == 0x03)
        {
            m.WriteU8(0x80097420, 0);
        }
    }

    // Fast Warps
    // Rather than patching the whole function, this is a post function hook to get the same effect.
    public static void FastWarps(CpuContext c, IMemory m)
    {
        if (m.ReadU8(0x800974A0) == 0x0E && m.ReadU8(0x801878B8) == 0x02 && m.ReadU8(0x80076EC4) == 0x03)   // If StageId == WRP, AntiFreeze Byte Change Detected, and Warp Step == 3
        {
            m.WriteU8(0x80076EC4, 0x4); // Warp Step = 4
        }
        if (m.ReadU8(0x800974A0) == 0x2E && m.ReadU8(0x8018972C) == 0x02 && m.ReadU8(0x80076EC4) == 0x04)   // If StageId == WRP, AntiFreeze Byte Change Detected, and Warp Step == 4
        {
            m.WriteU8(0x80076EC4, 0x5); // Warp Step = 5
        }
    }

    // Clock Statue Always Open NO0
    public static void EntityClockRoomController_no0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x20u), c.RA);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.S3 = 0x80090000u;
        c.S3 = c.S3 + 0x7964u;
        if (c.V0 == 0u)
        {
            goto L801CCCFC;
        }
        c.V0 = m.ReadU16((c.S2 + 0x86u));
        if (c.V0 != 0u)
        {
            goto L801CCCEC;
        }
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A6u;
        c.RA = 0x801CCCC8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0040u;
            goto L801CCCFC;
        }
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
        goto L801CCCFC;
    L801CCCEC:;
        c.V0 = m.ReadU16((c.S2 + 0x86u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
    L801CCCFC:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x73FCu));
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x33D8u;
        if (c.V0 != 0u)
        {
            goto L801CCD3C;
        }
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DEu));
        c.V0 = (int)c.V0 < 129 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801CCD54;
        }
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB8u), (ushort)0u);
        goto L801CCD54;
    L801CCD3C:;
        c.V0 = m.ReadU16((c.S2 + 0x8Au));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801CCD54;
        }
        c.V0 = 0u | 0x0001u;
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB8u), (ushort)c.V0);
    L801CCD54:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU16((c.V0 + 0x73FCu));
        m.WriteU16((c.S2 + 0x8Au), (ushort)c.V0);
        c.V0 = m.ReadU32((c.S3 + 0x2D0u));
        if(m.ReadU32(0x801CCD64) != 0x8E6202D0)     // Clock Statue always Open
        {
            c.V0 = 0;
        }
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801CCD9C;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = (uint)(short)m.ReadU16((c.S1 + 0x6u));
        c.V0 = (int)c.V0 < 129 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            goto L801CCDA4;
        }
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB6u), (ushort)0u);
        goto L801CCDA4;
    L801CCD9C:;
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB6u), (ushort)c.V0);
    L801CCDA4:;
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.S1 = 0u | 0x0001u;
        if (c.V1 == c.S1)
        {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801CD0C4;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801CCDCC;
        }
        if (c.V1 == 0u)
        {
            c.V0 = 0x88880000u;
            goto L801CCDE8;
        }
        c.V0 = 0x88880000u;
        goto L801CD730;
    L801CCDCC:;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0)
        {
            c.V0 = 0u | 0x0003u;
            goto L801CD1F4;
        }
        c.V0 = 0u | 0x0003u;
        if (c.V1 == c.V0)
        {
            goto L801CD640;
        }
        goto L801CD730;
    L801CCDE8:;
        c.A0 = 0x80040000u;
        c.A0 = m.ReadU32((c.A0 - 0x3668u));
        c.V0 = c.V0 | 0x8889u;
        { var _r = (ulong)c.A0 * c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.HI;
        c.V1 = c.V1 >> 5;
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        if (c.A0 != c.V0)
        {
            c.A0 = 0u | 0x0003u;
            goto L801CCE2C;
        }
        c.A0 = 0u | 0x0003u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A9u;
        c.RA = 0x801CCE28u;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = 0u | 0x0003u;
    L801CCE2C:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0001u;
        c.RA = 0x801CCE40u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.S0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0)
        {
            goto L801CD730;
        }
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xAE8u;
        c.RA = 0x801CCE64u;
        SoTN.InitializeEntity_no0(c, m);
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S2 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S2 + 0x64u), c.S0);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S2 + 0x34u), c.V0);
        m.WriteU8((c.A3 + 0x6u), (byte)0u);
        m.WriteU8((c.A3 + 0x5u), (byte)0u);
        m.WriteU8((c.A3 + 0x4u), (byte)0u);
        c.V1 = m.ReadU32((c.A3 + 0x4u));
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = 0u | 0x0100u;
        m.WriteU16((c.A3 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x22u), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x14u), (ushort)c.V0);
        c.V0 = 0u | 0x01F0u;
        m.WriteU16((c.A3 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0008u;
        m.WriteU16((c.A3 + 0x16u), (ushort)0u);
        m.WriteU16((c.A3 + 0xAu), (ushort)0u);
        m.WriteU16((c.A3 + 0x20u), (ushort)0u);
        m.WriteU16((c.A3 + 0x8u), (ushort)0u);
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        m.WriteU32((c.A3 + 0x10u), c.V1);
        m.WriteU32((c.A3 + 0x1Cu), c.A0);
        m.WriteU32((c.A3 + 0x28u), c.A1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x000Au;
        c.RA = 0x801CCF04u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DEu));
        c.A0 = 0x801E0000u;
        c.A0 = c.A0 - 0xAB8u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), c.S1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), 0u);
        c.V0 = (int)c.V0 < 64 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            m.WriteU16(c.A0, (ushort)0u);
            goto L801CCF68;
        }
        m.WriteU16(c.A0, (ushort)0u);
        c.V1 = 0x80070000u;
        c.V1 = (uint)(short)m.ReadU16((c.V1 + 0x33DAu));
        c.V0 = (int)c.V1 < 64 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801CCF58;
        }
        c.V0 = 0u | 0x0001u;
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB6u), (ushort)c.V0);
        c.S1 = c.S2 + 0x3ACu;
        goto L801CCF6C;
    L801CCF58:;
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801CCF68;
        }
        c.V0 = 0u | 0x0001u;
        m.WriteU16(c.A0, (ushort)c.V0);
    L801CCF68:;
        c.S1 = c.S2 + 0x3ACu;
    L801CCF6C:;
        c.S0 = 0u + 0u;
        c.V0 = 0xFFFF8001u;
        m.WriteU16((c.S2 + 0x54u), (ushort)c.V0);
        c.V0 = 0u | 0x0017u;
        m.WriteU16((c.S2 + 0x56u), (ushort)c.V0);
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x24u), (ushort)c.V0);
    L801CCF88:;
        c.A0 = 0u | 0x001Au;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801CCF94u;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801CCF88;
        }
        c.S1 = c.S1 + 0xBCu;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x801CCFB8u;
        SoTN.UpdateClockHands(c, m);
        c.S1 = c.S2 + 0x524u;
        c.S0 = 0u + 0u;
    L801CCFC0:;
        c.A0 = 0u | 0x001Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801CCFCCu;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801CCFC0;
        }
        c.S1 = c.S1 + 0xBCu;
        c.A0 = c.S2 + 0u;
        c.S1 = c.S2 + 0xBCu;
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        c.S0 = 0u + 0u;
        c.RA = 0x801CCFF8u;
        SoTN.UpdateBirdcages(c, m);
        c.A0 = 0u | 0x0020u;
        c.A1 = c.S2 + 0x69Cu;
        c.RA = 0x801CD004u;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        c.V0 = 0xFFFF8001u;
        m.WriteU16((c.S2 + 0x6F0u), (ushort)c.V0);
        c.V0 = 0u | 0x0017u;
        m.WriteU16((c.S2 + 0x6F2u), (ushort)c.V0);
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x6C0u), (ushort)c.V0);
        c.V0 = 0u | 0x804Bu;
        m.WriteU16((c.S2 + 0x6B2u), (ushort)c.V0);
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.S2 + 0x6B5u), (byte)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x6A2u));
        c.V1 = 0u | 0x0010u;
        m.WriteU8((c.S2 + 0x6B4u), (byte)c.V1);
        c.V0 = c.V0 + 0x4u;
        m.WriteU16((c.S2 + 0x6A2u), (ushort)c.V0);
    L801CD040:;
        c.A0 = 0u | 0x001Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801CD04Cu;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801CD040;
        }
        c.S1 = c.S1 + 0xBCu;
        c.S1 = c.S2 + 0x8D0u;
        c.S0 = 0u + 0u;
    L801CD06C:;
        c.A0 = 0u | 0x001Du;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801CD078u;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801CD06C;
        }
        c.S1 = c.S1 + 0xBCu;
        c.S1 = c.S2 + 0xA48u;
        c.S0 = 0u + 0u;
    L801CD098:;
        c.A0 = 0u | 0x001Eu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801CD0A4u;
        SoTN.CreateEntityFromCurrentEntity_no0(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801CD098;
        }
        c.S1 = c.S1 + 0xBCu;
        goto L801CD730;
    L801CD0C4:;
        c.V0 = m.ReadU32((c.S3 + 0x2D8u));
        if (c.V0 != 0u)
        {
            c.A0 = c.S2 + 0u;
            goto L801CD0EC;
        }
        c.A0 = c.S2 + 0u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A9u;
        c.RA = 0x801CD0E8u;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = c.S2 + 0u;
    L801CD0EC:;
        c.A1 = c.S3 + 0u;
        c.RA = 0x801CD0F4u;
        SoTN.UpdateClockHands(c, m);
        c.V0 = m.ReadU32((c.S3 + 0x2D4u));
        if (c.V0 != 0u)
        {
            goto L801CD170;
        }
        c.V0 = m.ReadU32((c.S3 + 0x2D8u));
        if (c.V0 != 0u)
        {
            goto L801CD170;
        }
        c.V0 = m.ReadU32((c.S3 + 0x2D0u));
        if (c.V0 != 0u)
        {
            c.V0 = 0x2AAA0000u;
            goto L801CD170;
        }
        c.V0 = 0x2AAA0000u;
        c.A0 = m.ReadU32((c.S3 + 0x2CCu));
        c.V0 = c.V0 | 0xAAABu;
        c.A0 = c.A0 + 0xBu;
        { var _r = (long)(int)c.A0 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.A0 >> 31);
        c.V1 = c.HI;
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V1 = c.V1 - c.V0;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 + 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.A0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x000Cu;
            goto L801CD170;
        }
        c.V0 = 0u | 0x000Cu;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
    L801CD170:;
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        c.A0 = c.S2 + 0u;
        c.RA = 0x801CD17Cu;
        SoTN.UpdateBirdcages(c, m);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x4214u));
        if (c.V0 != 0u)
        {
            goto L801CD730;
        }
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x33DAu));
        c.V0 = c.V0 - 0x30u;
        c.V0 = c.V0 < 0x000000A1u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V1 = 0u | 0xFFB8u;
            goto L801CD730;
        }
        c.V1 = 0u | 0xFFB8u;
        c.S0 = 0x80090000u;
        c.S0 = m.ReadU16((c.S0 + 0x7C14u));
        c.V0 = c.S0 + c.V1;
        c.V0 = c.V0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801CD730;
        }
        c.S0 = 0x80090000u;
        c.S0 = m.ReadU16((c.S0 + 0x7C18u));
        c.V0 = c.S0 + c.V1;
        c.V0 = c.V0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.A0 = 0u | 0x0002u;
            goto L801CD730;
        }
        c.A0 = 0u | 0x0002u;
        goto L801CD728;
    L801CD1F4:;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.S1);
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x33D8u;
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB8u), (ushort)0u);
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB6u), (ushort)0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), 0u);
        c.A0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V1 = 0x80070000u;
        c.V1 = m.ReadU16((c.V1 + 0x33DAu));
        c.V0 = c.A0 < 0x0000000Au ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.A0 << 2;
            goto L801CD730;
        }
        c.V0 = c.A0 << 2;
        c.At = 0x801C0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x1468u));
        switch (c.V0)
        {
            case 0x801CD24Cu: goto L801CD24C;
            case 0x801CD2D8u: goto L801CD2D8;
            case 0x801CD324u: goto L801CD324;
            case 0x801CD388u: goto L801CD388;
            case 0x801CD3D8u: goto L801CD3D8;
            case 0x801CD450u: goto L801CD450;
            case 0x801CD4C8u: goto L801CD4C8;
            case 0x801CD4E8u: goto L801CD4E8;
            case 0x801CD5C0u: goto L801CD5C0;
            case 0x801CD5F8u: goto L801CD5F8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801CD24C:;
        c.S0 = 0x80070000u;
        c.S0 = m.ReadU32((c.S0 + 0x2F2Cu));
        c.V0 = c.S0 & 0x0007u;
        if (c.V0 == 0u)
        {
            goto L801CD2CC;
        }
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3668u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = c.S0 & 0x0001u;
            goto L801CD730;
        }
        c.V0 = c.S0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0008u;
            goto L801CD294;
        }
        c.V0 = 0u | 0x0008u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        goto L801CD730;
    L801CD294:;
        c.V0 = c.S0 & 0x0002u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0004u;
            goto L801CD2B0;
        }
        c.V0 = 0u | 0x0004u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        goto L801CD730;
    L801CD2B0:;
        c.V0 = c.S0 & 0x0004u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0002u;
            goto L801CD730;
        }
        c.V0 = 0u | 0x0002u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        goto L801CD730;
    L801CD2CC:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        goto L801CD5EC;
    L801CD2D8:;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x2F20u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            goto L801CD730;
        }
        c.A0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.A0 + 0x1u;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        c.V0 = c.V1 << 16;
        c.V1 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V1 < 73 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = (int)c.V1 < 184 ? 1u : 0u;
            goto L801CD730;
        }
        c.V0 = (int)c.V1 < 184 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = c.A0 + 0x2u;
            goto L801CD730;
        }
        c.V0 = c.A0 + 0x2u;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        goto L801CD730;
    L801CD324:;
        c.V0 = c.V1 - 0x41u;
        c.V0 = c.V0 < 0x0000003Fu ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x8040u;
            goto L801CD344;
        }
        c.V0 = 0u | 0x8040u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        goto L801CD730;
    L801CD344:;
        c.V0 = c.V1 - 0x80u;
        c.V0 = c.V0 < 0x00000040u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x2040u;
            goto L801CD364;
        }
        c.V0 = 0u | 0x2040u;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
        goto L801CD730;
    L801CD364:;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 + 0x2F20u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            goto L801CD730;
        }
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801CD5F0;
    L801CD388:;
        c.V0 = c.V1 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 73 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801CD3B4;
        }
        c.V0 = m.ReadU16((c.S1 + 0x14u));
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x2000u;
            goto L801CD3CC;
        }
        c.V0 = 0u | 0x2000u;
        goto L801CD3C4;
    L801CD3B4:;
        c.V0 = m.ReadU16((c.S1 + 0x14u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x8000u;
            goto L801CD3CC;
        }
        c.V0 = 0u | 0x8000u;
    L801CD3C4:;
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), c.V0);
    L801CD3CC:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801CD5F0;
    L801CD3D8:;
        c.V1 = m.ReadU32((c.S2 + 0x64u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU8((c.A3 + 0x6u));
        c.V0 = c.V0 + 0x10u;
        m.WriteU8((c.A3 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x5u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x4u), (byte)c.V0);
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.A2 = m.ReadU32((c.A3 + 0x4u));
        c.V1 = m.ReadU8((c.A3 + 0x4u));
        c.V0 = 0u | 0x0031u;
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        c.V1 = c.V1 < 0x000000C1u ? 1u : 0u;
        m.WriteU32((c.A3 + 0x10u), c.A0);
        m.WriteU32((c.A3 + 0x1Cu), c.A1);
        if (c.V1 != 0u)
        {
            m.WriteU32((c.A3 + 0x28u), c.A2);
            goto L801CD730;
        }
        m.WriteU32((c.A3 + 0x28u), c.A2);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801CD5F0;
    L801CD450:;
        c.V1 = m.ReadU32((c.S2 + 0x64u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU8((c.A3 + 0x6u));
        c.V0 = c.V0 - 0x4u;
        m.WriteU8((c.A3 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x5u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x4u), (byte)c.V0);
        c.V1 = m.ReadU32((c.A3 + 0x4u));
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = m.ReadU8((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = c.V0 < 0x00000008u ? 1u : 0u;
        m.WriteU32((c.A3 + 0x10u), c.V1);
        m.WriteU32((c.A3 + 0x1Cu), c.A0);
        if (c.V0 == 0u)
        {
            m.WriteU32((c.A3 + 0x28u), c.A1);
            goto L801CD730;
        }
        m.WriteU32((c.A3 + 0x28u), c.A1);
        c.V0 = 0u | 0x0008u;
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801CD5F0;
    L801CD4C8:;
        c.V1 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x5A4u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x660u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V1 = c.V1 + 0x1u;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V1);
        goto L801CD730;
    L801CD4E8:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            c.T0 = 0x91A20000u;
            goto L801CD730;
        }
        c.T0 = 0x91A20000u;
        c.T0 = c.T0 | 0xB3C5u;
        c.V1 = m.ReadU32((c.S2 + 0x428u));
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.A3 = c.V1 << 16;
        c.A1 = (uint)((int)c.A3 >> 16);
        { var _r = (long)(int)c.A1 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A2 = m.ReadU32((c.S2 + 0x4E4u));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        c.V0 = c.V0 + 0x1u;
        m.WriteU32((c.S2 + 0x4E8u), c.A2);
        c.A2 = c.A2 << 16;
        c.A0 = (uint)((int)c.A2 >> 16);
        c.A3 = (uint)((int)c.A3 >> 31);
        c.A2 = (uint)((int)c.A2 >> 31);
        m.WriteU32((c.S2 + 0x42Cu), c.V1);
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        c.V1 = c.HI;
        c.V1 = c.V1 + c.A1;
        c.V1 = (uint)((int)c.V1 >> 11);
        c.V1 = c.V1 - c.A3;
        { var _r = (long)(int)c.A0 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 5;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 4;
        c.A1 = c.A1 - c.V0;
        c.A1 = c.A1 << 16;
        c.A1 = (uint)((int)c.A1 >> 16);
        c.V0 = 0u | 0x1518u;
        c.V0 = c.V0 - c.A1;
        m.WriteU32((c.S2 + 0x430u), c.V0);
        c.V1 = c.HI;
        c.V1 = c.V1 + c.A0;
        c.V1 = (uint)((int)c.V1 >> 11);
        c.V1 = c.V1 - c.A2;
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 5;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 4;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 16);
        c.A0 = c.A0 + 0x708u;
        m.WriteU32((c.S2 + 0x4ECu), c.A0);
        goto L801CD730;
    L801CD5C0:;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801CD5C8u;
        SoTN.func_us_801CCAAC(c, m);
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 < 0x00000200u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x000Du;
            goto L801CD730;
        }
        c.V0 = 0u | 0x000Du;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V1 = 0u | 0x0380u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V1);
    L801CD5EC:;
        c.V0 = c.V0 + 0x1u;
    L801CD5F0:;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        goto L801CD730;
    L801CD5F8:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            c.V1 = 0u | 0x0001u;
            goto L801CD730;
        }
        c.V1 = 0u | 0x0001u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3794u));
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x4214u), (byte)c.V1);
        c.A0 = 0u + 0u;
        c.RA = 0x801CD62Cu;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = 0u | 0x0003u;
        c.RA = 0x801CD634u;
        SoTN.SetStep_no0(c, m);
        c.V0 = 0u | 0x0140u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        goto L801CD730;
    L801CD640:;
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB8u), (ushort)0u);
        c.At = 0x801E0000u;
        m.WriteU16((c.At - 0xAB6u), (ushort)0u);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        if (c.V0 == 0u)
        {
            goto L801CD670;
        }
        if (c.V0 == c.S1)
        {
            goto L801CD70C;
        }
        goto L801CD730;
    L801CD670:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            goto L801CD730;
        }
        c.V0 = m.ReadU32((c.S2 + 0x428u));
        m.WriteU32((c.S2 + 0x42Cu), c.V0);
        c.V0 = m.ReadU32((c.S2 + 0x4E4u));
        c.V1 = m.ReadU32((c.S3 + 0x2D0u));
        m.WriteU32((c.S2 + 0x4E8u), c.V0);
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 18;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = c.V0 + 0x708u;
        m.WriteU32((c.S2 + 0x430u), c.V0);
        c.A0 = m.ReadU32((c.S3 + 0x2CCu));
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        c.V0 = c.V0 + 0x1u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.A0;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = c.A1 << 2;
        c.V1 = c.V1 + c.A1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V1 = 0u | 0x1518u;
        c.V1 = c.V1 - c.V0;
        m.WriteU32((c.S2 + 0x4ECu), c.V1);
        goto L801CD730;
    L801CD70C:;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801CD714u;
        SoTN.func_us_801CCAAC(c, m);
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 < 0x00000200u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x0001u;
            goto L801CD730;
        }
        c.A0 = 0u | 0x0001u;
    L801CD728:;
        c.RA = 0x801CD730u;
        SoTN.SetStep_no0(c, m);
    L801CD730:;
        c.RA = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

    // Reverse Clock Statue
    public static void func_801C0E1C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x20u), c.RA);
        m.WriteU32((c.SP + 0x1Cu), c.S3);
        m.WriteU32((c.SP + 0x14u), c.S1);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.S3 = 0x80090000u;
        c.S3 = c.S3 + 0x7964u;
        if (c.V0 == 0u)
        {
            goto L801C0EA4;
        }
        c.V0 = m.ReadU16((c.S2 + 0x86u));
        if (c.V0 != 0u)
        {
            goto L801C0E94;
        }
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A6u;
        c.RA = 0x801C0E70u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0040u;
            goto L801C0EA4;
        }
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
        goto L801C0EA4;
    L801C0E94:;
        c.V0 = m.ReadU16((c.S2 + 0x86u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x86u), (ushort)c.V0);
    L801C0EA4:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU32((c.V0 + 0x73FCu));
        c.S1 = 0x80070000u;
        c.S1 = c.S1 + 0x33D8u;
        if (c.V0 != 0u)
        {
            goto L801C0EE4;
        }
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DEu));
        c.V0 = (int)c.V0 < 144 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801C0EFC;
        }
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B48u), (ushort)0u);
        goto L801C0EFC;
    L801C0EE4:;
        c.V0 = m.ReadU16((c.S2 + 0x8Au));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801C0EFC;
        }
        c.V0 = 0u | 0x0001u;
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B48u), (ushort)c.V0);
    L801C0EFC:;
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU16((c.V0 + 0x73FCu));
        m.WriteU16((c.S2 + 0x8Au), (ushort)c.V0);
        c.V0 = m.ReadU32((c.S3 + 0x2D0u));
        if (m.ReadU32(0x801C0F0C) != 0x8E6202D0)     // Clock Statue always Open
        {
            c.V0 = 0;
        }
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801C0F44;
        }
        c.V0 = 0u | 0x0001u;
        c.V0 = (uint)(short)m.ReadU16((c.S1 + 0x6u));
        c.V0 = (int)c.V0 < 144 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801C0F4C;
        }
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B4Au), (ushort)0u);
        goto L801C0F4C;
    L801C0F44:;
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B4Au), (ushort)c.V0);
    L801C0F4C:;
        c.V1 = m.ReadU16((c.S2 + 0x2Cu));
        c.S1 = 0u | 0x0001u;
        if (c.V1 == c.S1)
        {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801C1268;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            goto L801C0F74;
        }
        if (c.V1 == 0u)
        {
            c.V0 = 0x88880000u;
            goto L801C0F90;
        }
        c.V0 = 0x88880000u;
        goto L801C1790;
    L801C0F74:;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0)
        {
            c.V0 = 0u | 0x0003u;
            goto L801C139C;
        }
        c.V0 = 0u | 0x0003u;
        if (c.V1 == c.V0)
        {
            goto L801C16A0;
        }
        goto L801C1790;
    L801C0F90:;
        c.A0 = 0x80040000u;
        c.A0 = m.ReadU32((c.A0 - 0x3668u));
        c.V0 = c.V0 | 0x8889u;
        { var _r = (ulong)c.A0 * c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.HI;
        c.V1 = c.V1 >> 5;
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        if (c.A0 != c.V0)
        {
            c.A0 = 0u | 0x0003u;
            goto L801C0FD4;
        }
        c.A0 = 0u | 0x0003u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A9u;
        c.RA = 0x801C0FD0u;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = 0u | 0x0003u;
    L801C0FD4:;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0001u;
        c.RA = 0x801C0FE8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.S0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0)
        {
            goto L801C1790;
        }
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0xAB0u;
        c.RA = 0x801C100Cu;
        SoTN.func_801BB44C(c, m);
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S2 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S2 + 0x64u), c.S0);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S2 + 0x34u), c.V0);
        m.WriteU8((c.A3 + 0x6u), (byte)0u);
        m.WriteU8((c.A3 + 0x5u), (byte)0u);
        m.WriteU8((c.A3 + 0x4u), (byte)0u);
        c.V1 = m.ReadU32((c.A3 + 0x4u));
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = 0u | 0x0100u;
        m.WriteU16((c.A3 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x22u), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A3 + 0x14u), (ushort)c.V0);
        c.V0 = 0u | 0x01F0u;
        m.WriteU16((c.A3 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0008u;
        m.WriteU16((c.A3 + 0x16u), (ushort)0u);
        m.WriteU16((c.A3 + 0xAu), (ushort)0u);
        m.WriteU16((c.A3 + 0x20u), (ushort)0u);
        m.WriteU16((c.A3 + 0x8u), (ushort)0u);
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        m.WriteU32((c.A3 + 0x10u), c.V1);
        m.WriteU32((c.A3 + 0x1Cu), c.A0);
        m.WriteU32((c.A3 + 0x28u), c.A1);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x000Au;
        c.RA = 0x801C10ACu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DEu));
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 + 0x4B48u;
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7928u), c.S1);
        c.At = 0x80090000u;
        m.WriteU32((c.At + 0x7910u), 0u);
        c.V0 = (int)c.V0 < 193 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            m.WriteU16(c.A0, (ushort)0u);
            goto L801C110C;
        }
        m.WriteU16(c.A0, (ushort)0u);
        c.V1 = 0x80070000u;
        c.V1 = (uint)(short)m.ReadU16((c.V1 + 0x33DAu));
        c.V0 = (int)c.V1 < 64 ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801C10F8;
        }
        c.V0 = 0u | 0x0001u;
        m.WriteU16(c.A0, (ushort)c.V0);
        goto L801C110C;
    L801C10F8:;
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0001u;
            goto L801C110C;
        }
        c.V0 = 0u | 0x0001u;
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B4Au), (ushort)c.V0);
    L801C110C:;
        c.S1 = c.S2 + 0x3ACu;
        c.S0 = 0u + 0u;
        c.V0 = 0xFFFF8002u;
        m.WriteU16((c.S2 + 0x54u), (ushort)c.V0);
        c.V0 = 0u | 0x0017u;
        m.WriteU16((c.S2 + 0x56u), (ushort)c.V0);
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x24u), (ushort)c.V0);
    L801C112C:;
        c.A0 = 0u | 0x001Au;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801C1138u;
        SoTN.func_801B999C(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801C112C;
        }
        c.S1 = c.S1 + 0xBCu;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x801C115Cu;
        SoTN.func_801C0DD4(c, m);
        c.S1 = c.S2 + 0x524u;
        c.S0 = 0u + 0u;
    L801C1164:;
        c.A0 = 0u | 0x001Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801C1170u;
        SoTN.func_801B999C(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801C1164;
        }
        c.S1 = c.S1 + 0xBCu;
        c.A0 = c.S2 + 0u;
        c.S1 = c.S2 + 0xBCu;
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        c.S0 = 0u + 0u;
        c.RA = 0x801C119Cu;
        SoTN.func_801C0D8C(c, m);
        c.A0 = 0u | 0x0020u;
        c.A1 = c.S2 + 0x69Cu;
        c.RA = 0x801C11A8u;
        SoTN.func_801B999C(c, m);
        c.V0 = 0xFFFF8002u;
        m.WriteU16((c.S2 + 0x6F0u), (ushort)c.V0);
        c.V0 = 0u | 0x0017u;
        m.WriteU16((c.S2 + 0x6F2u), (ushort)c.V0);
        c.V0 = 0u | 0x0040u;
        m.WriteU16((c.S2 + 0x6C0u), (ushort)c.V0);
        c.V0 = 0u | 0x804Bu;
        m.WriteU16((c.S2 + 0x6B2u), (ushort)c.V0);
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.S2 + 0x6B5u), (byte)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x6A2u));
        c.V1 = 0u | 0x0010u;
        m.WriteU8((c.S2 + 0x6B4u), (byte)c.V1);
        c.V0 = c.V0 + 0x4u;
        m.WriteU16((c.S2 + 0x6A2u), (ushort)c.V0);
    L801C11E4:;
        c.A0 = 0u | 0x001Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801C11F0u;
        SoTN.func_801B999C(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801C11E4;
        }
        c.S1 = c.S1 + 0xBCu;
        c.S1 = c.S2 + 0x8D0u;
        c.S0 = 0u + 0u;
    L801C1210:;
        c.A0 = 0u | 0x001Du;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801C121Cu;
        SoTN.func_801B999C(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801C1210;
        }
        c.S1 = c.S1 + 0xBCu;
        c.S1 = c.S2 + 0xA48u;
        c.S0 = 0u + 0u;
    L801C123C:;
        c.A0 = 0u | 0x001Eu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x801C1248u;
        SoTN.func_801B999C(c, m);
        m.WriteU16((c.S1 + 0x30u), (ushort)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.S1 = c.S1 + 0xBCu;
            goto L801C123C;
        }
        c.S1 = c.S1 + 0xBCu;
        goto L801C1790;
    L801C1268:;
        c.V0 = m.ReadU32((c.S3 + 0x2D8u));
        if (c.V0 != 0u)
        {
            c.A0 = c.S2 + 0u;
            goto L801C1290;
        }
        c.A0 = c.S2 + 0u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3824u));
        c.A0 = 0u | 0x07A9u;
        c.RA = 0x801C128Cu;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = c.S2 + 0u;
    L801C1290:;
        c.A1 = c.S3 + 0u;
        c.RA = 0x801C1298u;
        SoTN.func_801C0DD4(c, m);
        c.V0 = m.ReadU32((c.S3 + 0x2D4u));
        if (c.V0 != 0u)
        {
            goto L801C1314;
        }
        c.V0 = m.ReadU32((c.S3 + 0x2D8u));
        if (c.V0 != 0u)
        {
            goto L801C1314;
        }
        c.V0 = m.ReadU32((c.S3 + 0x2D0u));
        if (c.V0 != 0u)
        {
            c.V0 = 0x2AAA0000u;
            goto L801C1314;
        }
        c.V0 = 0x2AAA0000u;
        c.A0 = m.ReadU32((c.S3 + 0x2CCu));
        c.V0 = c.V0 | 0xAAABu;
        c.A0 = c.A0 + 0xBu;
        { var _r = (long)(int)c.A0 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.A0 >> 31);
        c.V1 = c.HI;
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V1 = c.V1 - c.V0;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 + 0x1u;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.A0);
        c.V0 = m.ReadU16((c.S2 + 0x84u));
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x000Cu;
            goto L801C1314;
        }
        c.V0 = 0u | 0x000Cu;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
    L801C1314:;
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        c.A0 = c.S2 + 0u;
        c.RA = 0x801C1320u;
        SoTN.func_801C0D8C(c, m);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU8((c.V0 - 0x4130u));
        if (c.V0 != 0u)
        {
            goto L801C1790;
        }
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU16((c.V0 + 0x33DAu));
        c.V0 = c.V0 - 0x60u;
        c.V0 = c.V0 < 0x00000041u ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V1 = 0u + 0u;
            goto L801C1790;
        }
        c.V1 = 0u + 0u;
        c.S0 = 0u | 0x0019u;
        c.V0 = c.S0 & 0xFFFFu;
    L801C1358:;
        c.At = 0x80090000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU8((c.At + 0x7964u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u)
        {
            c.S0 = c.S0 + 0x1u;
            goto L801C1378;
        }
        c.S0 = c.S0 + 0x1u;
        c.V1 = c.V1 + 0x1u;
    L801C1378:;
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V0 < 0x0000001Eu ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = c.S0 & 0xFFFFu;
            goto L801C1358;
        }
        c.V0 = c.S0 & 0xFFFFu;
        c.V0 = c.V1 << 16;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x0002u;
            goto L801C1790;
        }
        c.A0 = 0u | 0x0002u;
        goto L801C1788;
    L801C139C:;
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x33DAu));
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B48u), (ushort)0u);
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B4Au), (ushort)0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EF4u), 0u);
        c.At = 0x80070000u;
        m.WriteU32((c.At + 0x2EFCu), c.S1);
        c.V0 = (int)c.V0 < 129 ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x0060u;
            goto L801C13D4;
        }
        c.V0 = 0u | 0x0060u;
        c.V0 = 0u | 0x00A0u;
    L801C13D4:;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x33DAu), (ushort)c.V0);
        c.V1 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V1 < 0x0000000Au ? 1u : 0u;
        if (c.V0 == 0u)
        {
            c.V0 = c.V1 << 2;
            goto L801C1790;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801B0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x5BECu));
        switch (c.V0)
        {
            case 0x801C1408u: goto L801C1408;
            case 0x801C1414u: goto L801C1414;
            case 0x801C1420u: goto L801C1420;
            case 0x801C142Cu: goto L801C142C;
            case 0x801C1438u: goto L801C1438;
            case 0x801C14B0u: goto L801C14B0;
            case 0x801C1528u: goto L801C1528;
            case 0x801C1548u: goto L801C1548;
            case 0x801C1620u: goto L801C1620;
            case 0x801C1658u: goto L801C1658;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
    L801C1408:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        goto L801C164C;
    L801C1414:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801C1650;
    L801C1420:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801C1650;
    L801C142C:;
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801C1650;
    L801C1438:;
        c.V1 = m.ReadU32((c.S2 + 0x64u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU8((c.A3 + 0x6u));
        c.V0 = c.V0 + 0x10u;
        m.WriteU8((c.A3 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x5u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x4u), (byte)c.V0);
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.A2 = m.ReadU32((c.A3 + 0x4u));
        c.V1 = m.ReadU8((c.A3 + 0x4u));
        c.V0 = 0u | 0x0031u;
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        c.V1 = c.V1 < 0x000000C1u ? 1u : 0u;
        m.WriteU32((c.A3 + 0x10u), c.A0);
        m.WriteU32((c.A3 + 0x1Cu), c.A1);
        if (c.V1 != 0u)
        {
            m.WriteU32((c.A3 + 0x28u), c.A2);
            goto L801C1790;
        }
        m.WriteU32((c.A3 + 0x28u), c.A2);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801C1650;
    L801C14B0:;
        c.V1 = m.ReadU32((c.S2 + 0x64u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A3 = c.V0 + c.V1;
        c.V0 = m.ReadU8((c.A3 + 0x6u));
        c.V0 = c.V0 - 0x4u;
        m.WriteU8((c.A3 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x5u), (byte)c.V0);
        m.WriteU8((c.A3 + 0x4u), (byte)c.V0);
        c.V1 = m.ReadU32((c.A3 + 0x4u));
        c.A0 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = m.ReadU8((c.A3 + 0x4u));
        c.A1 = m.ReadU32((c.A3 + 0x4u));
        c.V0 = c.V0 < 0x00000008u ? 1u : 0u;
        m.WriteU32((c.A3 + 0x10u), c.V1);
        m.WriteU32((c.A3 + 0x1Cu), c.A0);
        if (c.V0 == 0u)
        {
            m.WriteU32((c.A3 + 0x28u), c.A1);
            goto L801C1790;
        }
        m.WriteU32((c.A3 + 0x28u), c.A1);
        c.V0 = 0u | 0x0008u;
        m.WriteU16((c.A3 + 0x32u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = c.V0 + 0x1u;
        goto L801C1650;
    L801C1528:;
        c.V1 = m.ReadU16((c.S2 + 0x2Eu));
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S2 + 0x5A4u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x660u), (ushort)c.V0);
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V1 = c.V1 + 0x1u;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V1);
        goto L801C1790;
    L801C1548:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            c.T0 = 0x91A20000u;
            goto L801C1790;
        }
        c.T0 = 0x91A20000u;
        c.T0 = c.T0 | 0xB3C5u;
        c.V1 = m.ReadU32((c.S2 + 0x428u));
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.A3 = c.V1 << 16;
        c.A1 = (uint)((int)c.A3 >> 16);
        { var _r = (long)(int)c.A1 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A2 = m.ReadU32((c.S2 + 0x4E4u));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        c.V0 = c.V0 + 0x1u;
        m.WriteU32((c.S2 + 0x4E8u), c.A2);
        c.A2 = c.A2 << 16;
        c.A0 = (uint)((int)c.A2 >> 16);
        c.A3 = (uint)((int)c.A3 >> 31);
        c.A2 = (uint)((int)c.A2 >> 31);
        m.WriteU32((c.S2 + 0x42Cu), c.V1);
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        c.V1 = c.HI;
        c.V1 = c.V1 + c.A1;
        c.V1 = (uint)((int)c.V1 >> 11);
        c.V1 = c.V1 - c.A3;
        { var _r = (long)(int)c.A0 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 5;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 4;
        c.A1 = c.A1 - c.V0;
        c.A1 = c.A1 << 16;
        c.A1 = (uint)((int)c.A1 >> 16);
        c.V0 = 0u | 0x1518u;
        c.V0 = c.V0 - c.A1;
        m.WriteU32((c.S2 + 0x430u), c.V0);
        c.V1 = c.HI;
        c.V1 = c.V1 + c.A0;
        c.V1 = (uint)((int)c.V1 >> 11);
        c.V1 = c.V1 - c.A2;
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 5;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 4;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 16);
        c.A0 = c.A0 + 0x708u;
        m.WriteU32((c.S2 + 0x4ECu), c.A0);
        goto L801C1790;
    L801C1620:;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801C1628u;
        SoTN.func_801C0C54(c, m);
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 < 0x00000200u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.V0 = 0u | 0x000Du;
            goto L801C1790;
        }
        c.V0 = 0u | 0x000Du;
        m.WriteU16((c.S2 + 0x84u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.V1 = 0u | 0x0380u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V1);
    L801C164C:;
        c.V0 = c.V0 + 0x1u;
    L801C1650:;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        goto L801C1790;
    L801C1658:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            c.V1 = 0u | 0x0001u;
            goto L801C1790;
        }
        c.V1 = 0u | 0x0001u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3794u));
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x4130u), (byte)c.V1);
        c.A0 = 0u | 0x00E4u;
        c.RA = 0x801C168Cu;
        Dispatcher.Call(c, m, c.V0);
        c.A0 = 0u | 0x0003u;
        c.RA = 0x801C1694u;
        SoTN.func_801BB37C(c, m);
        c.V0 = 0u | 0x0140u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        goto L801C1790;
    L801C16A0:;
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B48u), (ushort)0u);
        c.At = 0x801D0000u;
        m.WriteU16((c.At + 0x4B4Au), (ushort)0u);
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        if (c.V0 == 0u)
        {
            goto L801C16D0;
        }
        if (c.V0 == c.S1)
        {
            goto L801C176C;
        }
        goto L801C1790;
    L801C16D0:;
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 - 0x1u;
        m.WriteU16((c.S2 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u)
        {
            goto L801C1790;
        }
        c.V0 = m.ReadU32((c.S2 + 0x428u));
        m.WriteU32((c.S2 + 0x42Cu), c.V0);
        c.V0 = m.ReadU32((c.S2 + 0x4E4u));
        c.V1 = m.ReadU32((c.S3 + 0x2D0u));
        m.WriteU32((c.S2 + 0x4E8u), c.V0);
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 18;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = c.V0 + 0x708u;
        m.WriteU32((c.S2 + 0x430u), c.V0);
        c.A0 = m.ReadU32((c.S3 + 0x2CCu));
        c.V0 = m.ReadU16((c.S2 + 0x2Eu));
        c.A1 = m.ReadU32((c.S3 + 0x2D0u));
        m.WriteU16((c.S2 + 0x88u), (ushort)0u);
        c.V0 = c.V0 + 0x1u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.A0;
        m.WriteU16((c.S2 + 0x2Eu), (ushort)c.V0);
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V1 = c.A1 << 2;
        c.V1 = c.V1 + c.A1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V1 = 0u | 0x1518u;
        c.V1 = c.V1 - c.V0;
        m.WriteU32((c.S2 + 0x4ECu), c.V1);
        goto L801C1790;
    L801C176C:;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801C1774u;
        SoTN.func_801C0C54(c, m);
        c.V0 = m.ReadU16((c.S2 + 0x88u));
        c.V0 = c.V0 < 0x00000200u ? 1u : 0u;
        if (c.V0 != 0u)
        {
            c.A0 = 0u | 0x0001u;
            goto L801C1790;
        }
        c.A0 = 0u | 0x0001u;
    L801C1788:;
        c.RA = 0x801C1790u;
        SoTN.func_801BB37C(c, m);
    L801C1790:;
        c.RA = m.ReadU32((c.SP + 0x20u));
        c.S3 = m.ReadU32((c.SP + 0x1Cu));
        c.S2 = m.ReadU32((c.SP + 0x18u));
        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }

}
