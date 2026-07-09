using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled;

public static partial class WidescreenPatch
{
    public static void CloudsWide(CpuContext c, IMemory m)
    {
        int xMin = -StageMargin();
        int xMax = 257 + StageMargin();
        c.SP = c.SP - 0x40u;
        m.WriteU32((c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x3Cu), c.RA);
        m.WriteU32((c.SP + 0x38u), c.FP);
        m.WriteU32((c.SP + 0x34u), c.S7);
        m.WriteU32((c.SP + 0x30u), c.S6);
        m.WriteU32((c.SP + 0x2Cu), c.S5);
        m.WriteU32((c.SP + 0x28u), c.S4);
        m.WriteU32((c.SP + 0x24u), c.S3);
        m.WriteU32((c.SP + 0x1Cu), c.S1);
        m.WriteU32((c.SP + 0x18u), c.S0);
        c.V0 = m.ReadU16((c.S2 + 0x2Cu));
        if (c.V0 != 0u) {
            c.S3 = 0x1F800000u;
            goto L801BAC6C;
        }
        c.S3 = 0x1F800000u;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x5D4u;
        c.RA = 0x801BAB58u;
        SoTN.InitializeEntity_st0(c, m);
        c.A0 = 0u | 0x0004u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3820u));
        c.A1 = 0u | 0x0070u;
        c.RA = 0x801BAB70u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = c.A0 << 1;
            goto L801BB270;
        }
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.T1 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S2 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S2 + 0x64u), c.A0);
        m.WriteU32((c.S2 + 0x7Cu), c.T1);
        c.V0 = c.V0 | c.V1;
        m.WriteU32((c.S2 + 0x34u), c.V0);
        c.V0 = 0u | 0x000Fu;
        m.WriteU16((c.T1 + 0x1Au), (ushort)c.V0);
        c.V0 = 0u | 0x0046u;
        m.WriteU16((c.T1 + 0xEu), (ushort)c.V0);
        c.V0 = 0u | 0x0065u;
        m.WriteU8((c.T1 + 0x30u), (byte)c.V0);
        m.WriteU8((c.T1 + 0x18u), (byte)c.V0);
        c.V0 = 0u | 0x0080u;
        m.WriteU8((c.T1 + 0x19u), (byte)c.V0);
        m.WriteU8((c.T1 + 0xDu), (byte)c.V0);
        c.V0 = 0u | 0x00FBu;
        c.V1 = 0u | 0x004Bu;
        m.WriteU8((c.T1 + 0x31u), (byte)c.V0);
        m.WriteU8((c.T1 + 0x25u), (byte)c.V0);
        c.V0 = 0u | 0x00B0u;
        m.WriteU16((c.T1 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.T1 + 0x14u), (ushort)c.V0);
        c.V0 = 0u | 0x007Bu;
        m.WriteU16((c.T1 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.T1 + 0x22u), (ushort)c.V0);
        c.V0 = 0u | 0x0004u;
        m.WriteU8((c.T1 + 0x24u), (byte)0u);
        m.WriteU8((c.T1 + 0xCu), (byte)0u);
        m.WriteU16((c.T1 + 0x20u), (ushort)c.V1);
        m.WriteU16((c.T1 + 0x8u), (ushort)c.V1);
        m.WriteU16((c.T1 + 0x16u), (ushort)0u);
        m.WriteU16((c.T1 + 0xAu), (ushort)0u);
        m.WriteU16((c.S2 + 0x9Eu), (ushort)c.V1);
        m.WriteU16((c.S2 + 0xA2u), (ushort)0u);
        m.WriteU16((c.T1 + 0x26u), (ushort)c.V0);
        m.WriteU16((c.T1 + 0x32u), (ushort)0u);
        c.T1 = m.ReadU32(c.T1);
        if (c.T1 == 0u) {
            c.A0 = 0u | 0x000Fu;
            goto L801BAC68;
        }
        c.A0 = 0u | 0x000Fu;
        c.V1 = 0u | 0x0226u;
        c.V0 = 0u | 0x0008u;
        L801BAC4C: ;
        m.WriteU16((c.T1 + 0x1Au), (ushort)c.A0);
        m.WriteU16((c.T1 + 0xEu), (ushort)c.V1);
        m.WriteU16((c.T1 + 0x32u), (ushort)c.V0);
        c.T1 = m.ReadU32(c.T1);
        if (c.T1 != 0u) {
            goto L801BAC4C;
        }
        L801BAC68: ;
        c.S3 = 0x1F800000u;
        L801BAC6C: ;
        c.S0 = 0x1F800000u;
        c.S0 = c.S0 | 0x0020u;
        c.S5 = 0x1F800000u;
        c.S5 = c.S5 | 0x0060u;
        c.A2 = 0x1F800000u;
        c.A2 = c.A2 | 0x0080u;
        c.T8 = 0x80180000u;
        c.T8 = c.T8 + 0x2084u;
        c.T9 = 0u + 0u;
        c.V1 = 0x00030000u;
        c.V0 = m.ReadU32((c.S2 + 0x84u));
        c.A0 = 0x00080000u;
        c.V0 = c.V0 + c.V1;
        m.WriteU32((c.S2 + 0x84u), c.V0);
        c.V0 = m.ReadU32((c.S2 + 0x8Cu));
        c.V1 = m.ReadU32((c.S2 + 0x88u));
        c.V0 = c.V0 + c.A0;
        m.WriteU32((c.S2 + 0x8Cu), c.V0);
        c.V0 = m.ReadU32((c.S2 + 0x90u));
        c.V1 = c.V1 + c.A0;
        m.WriteU32((c.S2 + 0x88u), c.V1);
        c.V1 = 0x00040000u;
        c.V0 = c.V0 + c.V1;
        m.WriteU32((c.S2 + 0x90u), c.V0);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x20CCu;
        c.V0 = m.ReadWordLeft(c.V0, (c.A1 + 0x3u));
        c.V0 = m.ReadWordRight(c.V0, c.A1);
        c.V1 = m.ReadWordLeft(c.V1, (c.A1 + 0x7u));
        c.V1 = m.ReadWordRight(c.V1, (c.A1 + 0x4u));
        m.WriteWordLeft((c.S5 + 0x3u), c.V0);
        m.WriteWordRight(c.S5, c.V0);
        m.WriteWordLeft((c.S5 + 0x7u), c.V1);
        m.WriteWordRight((c.S5 + 0x4u), c.V1);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x20D4u;
        c.A0 = 0x1F800000u;
        c.A0 = c.A0 | 0x0068u;
        c.V0 = m.ReadWordLeft(c.V0, (c.A1 + 0x3u));
        c.V0 = m.ReadWordRight(c.V0, c.A1);
        c.V1 = m.ReadWordLeft(c.V1, (c.A1 + 0x7u));
        c.V1 = m.ReadWordRight(c.V1, (c.A1 + 0x4u));
        m.WriteWordLeft((c.A0 + 0x3u), c.V0);
        m.WriteWordRight(c.A0, c.V0);
        m.WriteWordLeft((c.A0 + 0x7u), c.V1);
        m.WriteWordRight((c.A0 + 0x4u), c.V1);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x20DCu;
        c.A0 = 0x1F800000u;
        c.A0 = c.A0 | 0x0070u;
        c.V0 = m.ReadWordLeft(c.V0, (c.A1 + 0x3u));
        c.V0 = m.ReadWordRight(c.V0, c.A1);
        c.V1 = m.ReadWordLeft(c.V1, (c.A1 + 0x7u));
        c.V1 = m.ReadWordRight(c.V1, (c.A1 + 0x4u));
        m.WriteWordLeft((c.A0 + 0x3u), c.V0);
        m.WriteWordRight(c.A0, c.V0);
        m.WriteWordLeft((c.A0 + 0x7u), c.V1);
        m.WriteWordRight((c.A0 + 0x4u), c.V1);
        c.A1 = 0x80180000u;
        c.A1 = c.A1 + 0x20E4u;
        c.A0 = 0x1F800000u;
        c.A0 = c.A0 | 0x0078u;
        c.V0 = m.ReadWordLeft(c.V0, (c.A1 + 0x3u));
        c.V0 = m.ReadWordRight(c.V0, c.A1);
        c.V1 = m.ReadWordLeft(c.V1, (c.A1 + 0x7u));
        c.V1 = m.ReadWordRight(c.V1, (c.A1 + 0x4u));
        m.WriteWordLeft((c.A0 + 0x3u), c.V0);
        m.WriteWordRight(c.A0, c.V0);
        m.WriteWordLeft((c.A0 + 0x7u), c.V1);
        m.WriteWordRight((c.A0 + 0x4u), c.V1);
        c.A0 = 0u | 0x0010u;
        c.V1 = 0u | 0x0008u;
        c.V0 = 0u | 0x0020u;
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x34DBu), (byte)c.A0);
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x34DAu), (byte)c.V1);
        c.At = 0x80040000u;
        m.WriteU8((c.At - 0x34D9u), (byte)c.V0);
        c.At = 0x80050000u;
        m.WriteU8((c.At + 0x4319u), (byte)c.A0);
        c.At = 0x80050000u;
        m.WriteU8((c.At + 0x431Au), (byte)c.V1);
        c.At = 0x80050000u;
        m.WriteU8((c.At + 0x431Bu), (byte)c.V0);
        L801BADC0: ;
        c.T9 = c.T9 + 0x1u;
        c.V0 = (uint)(short)m.ReadU16(c.T8);
        c.T8 = c.T8 + 0x2u;
        m.WriteU32(c.A2, c.V0);
        c.V0 = (int)c.T9 < 36 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A2 = c.A2 + 0x4u;
            goto L801BADC0;
        }
        c.A2 = c.A2 + 0x4u;
        c.S1 = 0x1F800000u;
        c.S1 = c.S1 | 0x0200u;
        c.V0 = 0u | 0x0004u;
        c.At = 0x1F800000u;
        m.WriteU8((c.At + 0x203u), (byte)c.V0);
        c.At = 0x1F800000u;
        m.WriteU8((c.At + 0x207u), (byte)c.V0);
        c.A0 = 0u | 0x0100u;
        c.RA = 0x801BAE00u;
        SoTN.SetGeomScreen(c, m);
        c.A0 = 0u | 0x0080u;
        c.A1 = 0u | 0x00A0u;
        c.RA = 0x801BAE0Cu;
        SoTN.SetGeomOffset(c, m);
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x217Cu;
        c.A1 = c.S3 + 0u;
        c.RA = 0x801BAE1Cu;
        SoTN.RotMatrix(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x801BAE24u;
        SoTN.SetRotMatrix(c, m);
        c.S4 = 0x80180000u;
        c.S4 = c.S4 + 0x216Cu;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3668u));
        c.T1 = m.ReadU32((c.S2 + 0x7Cu));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.T8 = c.S2 + 0x86u;
            goto L801BAE4C;
        }
        c.T8 = c.S2 + 0x86u;
        c.V0 = 0u | 0x0046u;
        goto L801BAE50;
        L801BAE4C: ;
        c.V0 = 0u | 0x0047u;
        L801BAE50: ;
        m.WriteU16((c.T1 + 0xEu), (ushort)c.V0);
        c.V0 = 0x2AAA0000u;
        c.V1 = 0x80090000u;
        c.V1 = m.ReadU32((c.V1 + 0x7908u));
        c.V0 = c.V0 | 0xAAABu;
        c.V1 = c.V1 << 16;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = 0x80090000u;
        c.A0 = m.ReadU32((c.A0 + 0x790Cu));
        c.A1 = c.HI;
        c.A0 = c.A0 << 16;
        { var _r = (long)(int)c.A0 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T9 = 0u + 0u;
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A1 = c.A1 - c.V1;
        c.V0 = m.ReadU32((c.S2 + 0x9Cu));
        c.A0 = (uint)((int)c.A0 >> 31);
        c.V0 = c.V0 - c.A1;
        m.WriteU32((c.S2 + 0x9Cu), c.V0);
        c.V0 = m.ReadU32((c.S2 + 0xA0u));
        c.V1 = c.HI;
        c.V1 = c.V1 - c.A0;
        c.A0 = m.ReadU16((c.S2 + 0x9Eu));
        c.V0 = c.V0 - c.V1;
        m.WriteU32((c.S2 + 0xA0u), c.V0);
        m.WriteU16((c.T1 + 0x20u), (ushort)c.A0);
        m.WriteU16((c.T1 + 0x8u), (ushort)c.A0);
        c.V0 = m.ReadU16((c.S2 + 0x9Eu));
        c.S6 = 0u | 0x0008u;
        c.V0 = c.V0 + 0x65u;
        m.WriteU16((c.T1 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.T1 + 0x14u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0xA2u));
        c.FP = 0x80040000u;
        c.FP = c.FP - 0x3668u;
        m.WriteU16((c.T1 + 0x16u), (ushort)c.V0);
        m.WriteU16((c.T1 + 0xAu), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S2 + 0xA2u));
        c.S3 = c.S4 + 0x4u;
        c.V0 = c.V0 + 0x7Bu;
        m.WriteU16((c.T1 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.T1 + 0x22u), (ushort)c.V0);
        c.T1 = m.ReadU32(c.T1);
        L801BAF00: ;
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x2u));
        c.V0 = (uint)(short)m.ReadU16(c.T8);
        c.T6 = c.V1 + c.V0;
        if ((int)c.T6 >= 0) {
            c.V0 = c.T6 + 0u;
            goto L801BAF1C;
        }
        c.V0 = c.T6 + 0u;
        c.V0 = c.T6 + 0x7FFu;
        L801BAF1C: ;
        c.V0 = (uint)((int)c.V0 >> 11);
        c.V0 = c.V0 << 11;
        c.T6 = c.T6 - c.V0;
        c.A2 = (uint)(short)m.ReadU16(c.S3);
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x3092u));
        c.S7 = m.ReadU16((c.S3 + 0x2u));
        if ((int)c.V0 >= 0) {
            c.V1 = c.A2 + 0x40u;
            goto L801BAF44;
        }
        c.V1 = c.A2 + 0x40u;
        c.V0 = c.V0 + 0x3u;
        L801BAF44: ;
        c.V0 = (uint)((int)c.V0 >> 2);
        c.A2 = c.V1 - c.V0;
        RecompOne.Runtime.Gte.WriteControl(6, c.A2);
        c.T7 = 0u + 0u;
        L801BAF54: ;
        c.T2 = m.ReadU16((c.T8 + 0x4u));
        c.V0 = c.T7 << 8;
        c.A2 = c.V0 + 0x1C0u;
        if ((int)c.T2 >= 0) {
            c.V1 = c.T2 + 0u;
            goto L801BAF6C;
        }
        c.V1 = c.T2 + 0u;
        c.V1 = c.T2 + 0xFFu;
        L801BAF6C: ;
        c.V1 = (uint)((int)c.V1 >> 8);
        c.V0 = c.V1 << 8;
        c.V0 = c.T2 - c.V0;
        c.A2 = c.A2 - c.V0;
        RecompOne.Runtime.Gte.WriteControl(7, c.A2);
        c.V1 = c.T7 + c.V1;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V1 + 0u;
            goto L801BAF90;
        }
        c.V0 = c.V1 + 0u;
        c.V0 = c.V1 + 0x7u;
        L801BAF90: ;
        c.T2 = (uint)((int)c.V0 >> 3);
        c.V0 = c.T2 << 3;
        c.T2 = c.V1 - c.V0;
        c.A2 = c.A2 - 0x1C0u;
        c.V1 = c.A2 << 1;
        c.V1 = c.V1 + c.A2;
        c.V1 = c.V1 << 2;
        c.V0 = c.V1 + c.A2;
        c.V0 = c.V0 << 5;
        c.A3 = 0x000F0000u;
        c.T0 = c.A3 - c.V0;
        c.A0 = 0xFFFE0000u;
        c.A0 = c.A0 | 0x6000u;
        c.A3 = (uint)((int)c.T0 >> 12);
        c.A0 = c.T0 + c.A0;
        c.V0 = c.A2 << 3;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.V0 << 6;
        m.WriteU32((c.SP + 0x10u), c.A3);
        c.A3 = 0x000F0000u;
        c.T0 = c.A3 - c.V0;
        c.V0 = 0xFFFE0000u;
        c.V0 = c.V0 | 0x4000u;
        c.A1 = (uint)((int)c.T0 >> 12);
        c.V0 = c.T0 + c.V0;
        c.V1 = c.V1 - c.A2;
        c.V1 = c.V1 << 5;
        c.T0 = c.A3 - c.V1;
        c.V1 = 0xFFFE0000u;
        c.V1 = c.V1 | 0xA000u;
        c.T5 = c.T6 + 0u;
        c.T3 = 0u + 0u;
        c.T4 = 0xFFFFFFFFu;
        c.A0 = (uint)((int)c.A0 >> 12);
        c.A2 = m.ReadU32(c.S4);
        c.A3 = m.ReadU8((c.SP + 0x10u));
        c.V0 = (uint)((int)c.V0 >> 12);
        m.WriteU8((c.S1 + 0x5u), (byte)c.V0);
        c.V0 = (uint)((int)c.T0 >> 12);
        c.V1 = c.T0 + c.V1;
        c.V1 = (uint)((int)c.V1 >> 12);
        m.WriteU8((c.S1 + 0x2u), (byte)c.V0);
        c.V0 = c.T2 << 3;
        m.WriteU8((c.S1 + 0x4u), (byte)c.A0);
        m.WriteU8((c.S1 + 0x1u), (byte)c.A1);
        m.WriteU8((c.S1 + 0x6u), (byte)c.V1);
        c.A0 = c.V0 + c.A2;
        m.WriteU8(c.S1, (byte)c.A3);
        L801BB050: ;
        c.T3 = c.T3 + c.T4;
        L801BB054: ;
        c.T3 = c.T3 & 0x0007u;
        c.V0 = c.A0 + c.T3;
        c.A2 = m.ReadU8(c.V0);
        c.V0 = c.T4 << 8;
        if (c.A2 == 0u) {
            c.T5 = c.T5 + c.V0;
            goto L801BB050;
        }
        c.T5 = c.T5 + c.V0;
        RecompOne.Runtime.Gte.WriteControl(5, c.T5);
        c.V0 = c.S5 + 0x18u;
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.V0));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.V0 + 0x4u)));
        RecompOne.Runtime.Gte.Execute(0x4A180001u);
        c.V1 = 0x1F800000u;
        c.V1 = c.V1 | 0x0080u;
        c.V0 = 0u | 0x0015u;
        m.WriteU16((c.T1 + 0x32u), (ushort)c.V0);
        c.V0 = c.A2 << 4;
        c.V1 = c.V0 + c.V1;
        c.V0 = m.ReadU16(c.V1);
        m.WriteU16((c.T1 + 0xCu), (ushort)c.V0);
        c.V0 = c.S0 + 0x2Cu;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(14));
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.S5));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.S5 + 0x4u)));
        RecompOne.Runtime.Gte.LoadWord(2, m.ReadU32((c.S5 + 0x8u)));
        RecompOne.Runtime.Gte.LoadWord(3, m.ReadU32((c.S5 + 0xCu)));
        RecompOne.Runtime.Gte.LoadWord(4, m.ReadU32((c.S5 + 0x10u)));
        RecompOne.Runtime.Gte.LoadWord(5, m.ReadU32((c.S5 + 0x14u)));
        RecompOne.Runtime.Gte.Execute(0x4A280030u);
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x2Eu));
        if ((int)c.V0 >= 0) {
            goto L801BB0F0;
        }
        m.WriteU16((c.T1 + 0x32u), (ushort)c.S6);
        goto L801BB208;
        L801BB0F0: ;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x2Cu));
        if ((int)c.V0 >= xMin) {
            goto L801BB118;
        }
        if ((int)c.T4 >= 0) {
            m.WriteU16((c.T1 + 0x32u), (ushort)c.S6);
            goto L801BB050;
        }
        m.WriteU16((c.T1 + 0x32u), (ushort)c.S6);
        c.T4 = c.T4 + 0x2u;
        c.T5 = c.T6 - 0x100u;
        c.T3 = 0u | 0x0007u;
        goto L801BB050;
        L801BB118: ;
        c.V0 = (int)c.A2 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L801BB138;
        }
        c.V0 = m.ReadU16(c.FP);
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 + 0x4Au;
        goto L801BB148;
        L801BB138: ;
        c.V0 = m.ReadU16(c.FP);
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 + 0x48u;
        L801BB148: ;
        m.WriteU16((c.T1 + 0xEu), (ushort)c.V0);
        c.V0 = m.ReadU16((c.V1 + 0x4u));
        m.WriteU16((c.T1 + 0x18u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.V1 + 0x8u));
        m.WriteU16((c.T1 + 0x24u), (ushort)c.V0);
        m.WriteU32((c.S0 + 0x8u), RecompOne.Runtime.Gte.StoreWord(12));
        m.WriteU32((c.S0 + 0x14u), RecompOne.Runtime.Gte.StoreWord(13));
        m.WriteU32((c.S0 + 0x20u), RecompOne.Runtime.Gte.StoreWord(14));
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x20u));
        c.V0 = (int)c.V0 < xMax ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L801BB194;
        }
        if ((int)c.T4 > 0) {
            m.WriteU16((c.T1 + 0x32u), (ushort)c.S6);
            goto L801BB208;
        }
        m.WriteU16((c.T1 + 0x32u), (ushort)c.S6);
        c.T3 = c.T3 + c.T4;
        goto L801BB054;
        L801BB194: ;
        c.V0 = m.ReadU16((c.V1 + 0xCu));
        m.WriteU16((c.T1 + 0x30u), (ushort)c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x8u));
        m.WriteU32((c.T1 + 0x8u), c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x14u));
        m.WriteU32((c.T1 + 0x14u), c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x20u));
        m.WriteU32((c.T1 + 0x20u), c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x2Cu));
        m.WriteU32((c.T1 + 0x2Cu), c.V0);
        c.V0 = m.ReadU32(c.S1);
        m.WriteU32((c.T1 + 0x10u), c.V0);
        m.WriteU32((c.T1 + 0x4u), c.V0);
        c.V0 = m.ReadU32((c.S1 + 0x4u));
        m.WriteU16((c.T1 + 0x26u), (ushort)c.S7);
        m.WriteU32((c.T1 + 0x28u), c.V0);
        m.WriteU32((c.T1 + 0x1Cu), c.V0);
        c.T1 = m.ReadU32(c.T1);
        if (c.T1 == 0u) {
            c.T3 = c.T3 + c.T4;
            goto L801BB278;
        }
        c.T3 = c.T3 + c.T4;
        goto L801BB054;
        L801BB208: ;
        c.T7 = c.T7 + 0x1u;
        c.V0 = (int)c.T7 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L801BAF54;
        }
        c.T9 = c.T9 + 0x1u;
        c.S3 = c.S3 + 0x8u;
        c.S4 = c.S4 + 0x8u;
        c.V0 = (int)c.T9 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.T8 = c.T8 + 0x8u;
            goto L801BAF00;
        }
        c.T8 = c.T8 + 0x8u;
        if (c.T1 == 0u) {
            c.V0 = 0u | 0x0008u;
            goto L801BB24C;
        }
        c.V0 = 0u | 0x0008u;
        L801BB238: ;
        m.WriteU16((c.T1 + 0x32u), (ushort)c.V0);
        c.T1 = m.ReadU32(c.T1);
        if (c.T1 != 0u) {
            goto L801BB238;
        }
        L801BB24C: ;
        c.V0 = 0x80070000u;
        c.V0 = (uint)(short)m.ReadU16((c.V0 + 0x308Eu));
        c.V1 = 0x80070000u;
        c.V1 = (uint)(short)m.ReadU16((c.V1 + 0x33DAu));
        c.T5 = c.V0 + c.V1;
        c.V0 = (int)c.T5 < 256 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L801BB278;
        }
        L801BB270: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801BB278u;
        SoTN.DestroyEntity_st0(c, m);
        L801BB278: ;
        c.RA = m.ReadU32((c.SP + 0x3Cu));
        c.FP = m.ReadU32((c.SP + 0x38u));
        c.S7 = m.ReadU32((c.SP + 0x34u));
        c.S6 = m.ReadU32((c.SP + 0x30u));
        c.S5 = m.ReadU32((c.SP + 0x2Cu));
        c.S4 = m.ReadU32((c.SP + 0x28u));
        c.S3 = m.ReadU32((c.SP + 0x24u));
        c.S2 = m.ReadU32((c.SP + 0x20u));
        c.S1 = m.ReadU32((c.SP + 0x1Cu));
        c.S0 = m.ReadU32((c.SP + 0x18u));
        c.SP = c.SP + 0x40u;
        return;
    }

    // vortex eff
    public static void VortexWide(CpuContext c, IMemory m)
    {
        int xMin = -StageMargin();
        int xMax = 257 + StageMargin();
        c.SP = c.SP - 0x100u;
        m.WriteU32((c.SP + 0xE4u), c.S3);
        c.S3 = c.A0 + 0u;
        m.WriteU32((c.SP + 0xFCu), c.RA);
        m.WriteU32((c.SP + 0xF8u), c.FP);
        m.WriteU32((c.SP + 0xF4u), c.S7);
        m.WriteU32((c.SP + 0xF0u), c.S6);
        m.WriteU32((c.SP + 0xECu), c.S5);
        m.WriteU32((c.SP + 0xE8u), c.S4);
        m.WriteU32((c.SP + 0xE0u), c.S2);
        m.WriteU32((c.SP + 0xDCu), c.S1);
        m.WriteU32((c.SP + 0xD8u), c.S0);
        c.V1 = m.ReadU16((c.S3 + 0x2Cu));
        c.A0 = 0u | 0x0001u;
        if (c.V1 == c.A0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L801BE2E0;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L801BE20C;
        }
        if (c.V1 == 0u) {
            goto L801BE228;
        }
        goto L801BEAD0;
        L801BE20C: ;
        c.V0 = 0u | 0x0002u;
        if (c.V1 == c.V0) {
            c.V0 = 0u | 0x0003u;
            goto L801BE3DC;
        }
        c.V0 = 0u | 0x0003u;
        if (c.V1 == c.V0) {
            goto L801BE578;
        }
        goto L801BEAD0;
        L801BE228: ;
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x5D4u;
        c.RA = 0x801BE238u;
        SoTN.InitializeEntity_st0(c, m);
        c.A0 = 0u | 0x0004u;
        c.V0 = 0u | 0x0080u;
        m.WriteU16((c.S3 + 0x2u), (ushort)c.V0);
        c.V0 = 0u | 0x01A0u;
        m.WriteU16((c.S3 + 0x6u), (ushort)c.V0);
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3820u));
        c.A1 = 0u | 0x0110u;
        c.RA = 0x801BE260u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = c.A0 << 1;
            goto L801BE544;
        }
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.S0 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S3 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S3 + 0x64u), c.A0);
        m.WriteU32((c.S3 + 0x7Cu), c.S0);
        c.V0 = c.V0 | c.V1;
        if (c.S0 == 0u) {
            m.WriteU32((c.S3 + 0x34u), c.V0);
            goto L801BE2C4;
        }
        m.WriteU32((c.S3 + 0x34u), c.V0);
        c.V0 = 0u | 0x0008u;
        L801BE2B0: ;
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        c.S0 = m.ReadU32(c.S0);
        if (c.S0 != 0u) {
            goto L801BE2B0;
        }
        L801BE2C4: ;
        c.V0 = 0u | 0x05F0u;
        m.WriteU16((c.S3 + 0x9Cu), (ushort)c.V0);
        c.V0 = 0u | 0x00B0u;
        m.WriteU16((c.S3 + 0x9Eu), (ushort)c.V0);
        c.V0 = 0u | 0x00D0u;
        m.WriteU16((c.S3 + 0xA0u), (ushort)0u);
        m.WriteU16((c.S3 + 0xAEu), (ushort)c.V0);
        L801BE2E0: ;
        c.S7 = 0u | 0x0060u;
        c.S6 = 0u | 0x0010u;
        c.S5 = 0u + 0u;
        c.S4 = 0u + 0u;
        m.WriteU16((c.SP + 0xC8u), (ushort)0u);
        m.WriteU32((c.SP + 0xD0u), 0u);
        L801BE2F8: ;
        c.A0 = m.ReadU32((c.SP + 0xD0u));
        c.FP = 0u + 0u;
        c.RA = 0x801BE304u;
        SoTN.SquareRoot0(c, m);
        c.V0 = c.V0 << 2;
        c.T3 = m.ReadU16((c.SP + 0xC8u));
        c.S1 = c.S5 << 3;
        m.WriteU32((c.SP + 0xC0u), c.V0);
        c.V0 = c.T3 << 16;
        c.S2 = (uint)((int)c.V0 >> 16);
        L801BE31C: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x801BE324u;
        SoTN.rcos(c, m);
        { var _r = (long)(int)c.S7 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S2 + 0u;
        c.S0 = c.LO;
        c.S0 = (uint)((int)c.S0 >> 12);
        c.RA = 0x801BE338u;
        SoTN.rsin(c, m);
        { var _r = (long)(int)c.S7 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.S5 = c.S5 + 0x1u;
        c.T3 = m.ReadU32((c.SP + 0xC0u));
        c.S2 = c.S2 + 0x100u;
        m.WriteU32((c.SP + 0xB8u), c.T3);
        c.V0 = m.ReadU16((c.SP + 0xB8u));
        c.FP = c.FP + 0x1u;
        c.At = 0x801C0000u;
        c.At = c.At + c.S1;
        m.WriteU16((c.At + 0x1BC8u), (ushort)c.S0);
        c.At = 0x801C0000u;
        c.At = c.At + c.S1;
        m.WriteU16((c.At + 0x1BCCu), (ushort)c.V0);
        c.V0 = c.LO;
        c.V0 = (uint)((int)c.V0 >> 12);
        c.At = 0x801C0000u;
        c.At = c.At + c.S1;
        m.WriteU16((c.At + 0x1BCAu), (ushort)c.V0);
        c.V0 = (int)c.FP < 16 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x8u;
            goto L801BE31C;
        }
        c.S1 = c.S1 + 0x8u;
        c.S7 = c.S7 + 0x1Cu;
        c.At = 0x801C0000u;
        c.At = c.At + c.S4;
        m.WriteU8((c.At + 0x23C8u), (byte)c.S6);
        c.S6 = c.S6 + 0x28u;
        c.V0 = c.S6 & 0x00FFu;
        c.T3 = m.ReadU16((c.SP + 0xC8u));
        c.V0 = c.V0 < 0x000000A1u ? 1u : 0u;
        c.T3 = c.T3 + 0x80u;
        if (c.V0 != 0u) {
            m.WriteU16((c.SP + 0xC8u), (ushort)c.T3);
            goto L801BE3BC;
        }
        m.WriteU16((c.SP + 0xC8u), (ushort)c.T3);
        c.S6 = 0u | 0x00A0u;
        L801BE3BC: ;
        c.S4 = c.S4 + 0x1u;
        c.T3 = m.ReadU32((c.SP + 0xD0u));
        c.V0 = (int)c.S4 < 16 ? 1u : 0u;
        c.T3 = c.T3 + 0x120u;
        if (c.V0 != 0u) {
            m.WriteU32((c.SP + 0xD0u), c.T3);
            goto L801BE2F8;
        }
        m.WriteU32((c.SP + 0xD0u), c.T3);
        goto L801BE564;
        L801BE3DC: ;
        c.S0 = m.ReadU32((c.S3 + 0x7Cu));
        if (c.S0 == 0u) {
            c.A1 = 0u | 0x0016u;
            goto L801BE438;
        }
        c.A1 = 0u | 0x0016u;
        c.A0 = 0u | 0x019Eu;
        c.V0 = 0u | 0x0040u;
        c.V1 = 0u | 0x0080u;
        L801BE3F8: ;
        m.WriteU16((c.S0 + 0x1Au), (ushort)c.A1);
        m.WriteU16((c.S0 + 0xEu), (ushort)c.A0);
        m.WriteU8((c.S0 + 0x24u), (byte)c.V0);
        m.WriteU8((c.S0 + 0xCu), (byte)c.V0);
        m.WriteU8((c.S0 + 0x30u), (byte)c.V1);
        m.WriteU8((c.S0 + 0x18u), (byte)c.V1);
        m.WriteU8((c.S0 + 0x19u), (byte)0u);
        m.WriteU8((c.S0 + 0xDu), (byte)0u);
        m.WriteU8((c.S0 + 0x31u), (byte)c.V0);
        m.WriteU8((c.S0 + 0x25u), (byte)c.V0);
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V1);
        m.WriteU16((c.S0 + 0x32u), (ushort)0u);
        c.S0 = m.ReadU32(c.S0);
        if (c.S0 != 0u) {
            goto L801BE3F8;
        }
        L801BE438: ;
        c.A3 = c.SP + 0x48u;
        c.V0 = 0x80070000u;
        c.V0 = m.ReadU32((c.V0 - 0x3C84u));
        c.S0 = m.ReadU32((c.S3 + 0x7Cu));
        c.A2 = c.V0 + 0x4u;
        c.T0 = c.V0 + 0x54u;
        L801BE450: ;
        c.V0 = m.ReadU32(c.A2);
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x8u));
        c.A1 = m.ReadU32((c.A2 + 0xCu));
        m.WriteU32(c.A3, c.V0);
        m.WriteU32((c.A3 + 0x4u), c.V1);
        m.WriteU32((c.A3 + 0x8u), c.A0);
        m.WriteU32((c.A3 + 0xCu), c.A1);
        c.A2 = c.A2 + 0x10u;
        if (c.A2 != c.T0) {
            c.A3 = c.A3 + 0x10u;
            goto L801BE450;
        }
        c.A3 = c.A3 + 0x10u;
        c.V0 = m.ReadU32(c.A2);
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x8u));
        m.WriteU32(c.A3, c.V0);
        m.WriteU32((c.A3 + 0x4u), c.V1);
        m.WriteU32((c.A3 + 0x8u), c.A0);
        c.S2 = 0x80040000u;
        c.S2 = c.S2 - 0x3804u;
        c.V0 = m.ReadU32(c.S2);
        c.A0 = c.S0 + 0u;
        c.RA = 0x801BE4ACu;
        Dispatcher.Call(c, m, c.V0);
        c.A2 = c.V0 + 0u;
        if (c.A2 == 0u) {
            c.S1 = 0u | 0x0007u;
            goto L801BE544;
        }
        c.S1 = 0u | 0x0007u;
        c.V0 = 0u | 0x0040u;
        m.WriteU8((c.S0 + 0x7u), (byte)c.S1);
        m.WriteU8((c.SP + 0x60u), (byte)0u);
        m.WriteU8((c.SP + 0x61u), (byte)0u);
        m.WriteU8((c.SP + 0x62u), (byte)0u);
        m.WriteU8((c.SP + 0x63u), (byte)0u);
        m.WriteU8((c.SP + 0x5Eu), (byte)0u);
        m.WriteU16((c.SP + 0x48u), (ushort)0u);
        m.WriteU16((c.SP + 0x50u), (ushort)0u);
        m.WriteU16((c.SP + 0xA8u), (ushort)0u);
        m.WriteU16((c.SP + 0xAAu), (ushort)0u);
        m.WriteU16((c.SP + 0xACu), (ushort)c.V0);
        m.WriteU16((c.SP + 0xAEu), (ushort)c.V0);
        c.V0 = m.ReadWordLeft(c.V0, (c.SP + 0xABu));
        c.V0 = m.ReadWordRight(c.V0, (c.SP + 0xA8u));
        c.V1 = m.ReadWordLeft(c.V1, (c.SP + 0xAFu));
        c.V1 = m.ReadWordRight(c.V1, (c.SP + 0xACu));
        m.WriteWordLeft((c.SP + 0x57u), c.V0);
        m.WriteWordRight((c.SP + 0x54u), c.V0);
        m.WriteWordLeft((c.SP + 0x5Bu), c.V1);
        m.WriteWordRight((c.SP + 0x58u), c.V1);
        c.A0 = c.A2 + 0u;
        c.A1 = c.SP + 0x48u;
        c.RA = 0x801BE518u;
        SoTN.SetDrawEnv(c, m);
        c.V0 = 0u | 0x0002u;
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x1000u;
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        c.S0 = m.ReadU32(c.S0);
        c.V0 = m.ReadU32(c.S2);
        c.A0 = c.S0 + 0u;
        c.RA = 0x801BE53Cu;
        Dispatcher.Call(c, m, c.V0);
        if (c.V0 != 0u) {
            c.V0 = 0u | 0x0059u;
            goto L801BE554;
        }
        c.V0 = 0u | 0x0059u;
        L801BE544: ;
        c.A0 = c.S3 + 0u;
        c.RA = 0x801BE54Cu;
        SoTN.DestroyEntity_st0(c, m);
        goto L801BEAD0;
        L801BE554: ;
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0800u;
        m.WriteU8((c.S0 + 0x7u), (byte)c.S1);
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        L801BE564: ;
        c.V0 = m.ReadU16((c.S3 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S3 + 0x2Cu), (ushort)c.V0);
        goto L801BEAD0;
        L801BE578: ;
        c.V0 = m.ReadU16((c.S3 + 0x2Eu));
        if (c.V0 == 0u) {
            c.V1 = 0xFFFF0000u;
            goto L801BE598;
        }
        c.V1 = 0xFFFF0000u;
        if (c.V0 == c.A0) {
            goto L801BE5D0;
        }
        goto L801BE660;
        L801BE598: ;
        c.V0 = m.ReadU32((c.S3 + 0x4u));
        c.V1 = c.V1 | 0x4000u;
        c.V0 = c.V0 + c.V1;
        m.WriteU32((c.S3 + 0x4u), c.V0);
        c.V0 = (uint)(short)m.ReadU16((c.S3 + 0x6u));
        c.V0 = (int)c.V0 < 160 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0u | 0x00A0u;
            goto L801BE660;
        }
        c.V1 = 0u | 0x00A0u;
        c.V0 = m.ReadU16((c.S3 + 0x2Eu));
        m.WriteU16((c.S3 + 0x6u), (ushort)c.V1);
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S3 + 0x2Eu), (ushort)c.V0);
        goto L801BE660;
        L801BE5D0: ;
        c.V0 = m.ReadU16((c.S3 + 0xA4u));
        c.V1 = m.ReadU16((c.S3 + 0xAAu));
        c.V0 = c.V0 + 0x10u;
        m.WriteU16((c.S3 + 0xA4u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S3 + 0xA6u));
        c.A0 = (uint)(short)m.ReadU16((c.S3 + 0xA4u));
        c.V0 = c.V0 + 0x8u;
        m.WriteU16((c.S3 + 0xA6u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S3 + 0xA8u));
        c.V1 = c.V1 + 0x2u;
        m.WriteU16((c.S3 + 0xAAu), (ushort)c.V1);
        c.V0 = c.V0 + 0x20u;
        m.WriteU16((c.S3 + 0xA8u), (ushort)c.V0);
        c.RA = 0x801BE608u;
        SoTN.rsin(c, m);
        c.V0 = c.V0 << 6;
        c.V0 = (uint)((int)c.V0 >> 12);
        c.A0 = (uint)(short)m.ReadU16((c.S3 + 0xA6u));
        c.V0 = c.V0 + 0x5F0u;
        m.WriteU16((c.S3 + 0x9Cu), (ushort)c.V0);
        c.RA = 0x801BE620u;
        SoTN.rsin(c, m);
        c.V0 = c.V0 << 7;
        c.V0 = (uint)((int)c.V0 >> 12);
        c.A0 = (uint)(short)m.ReadU16((c.S3 + 0xA8u));
        c.V0 = c.V0 + 0xB0u;
        m.WriteU16((c.S3 + 0x9Eu), (ushort)c.V0);
        c.RA = 0x801BE638u;
        SoTN.rsin(c, m);
        c.A0 = (uint)(short)m.ReadU16((c.S3 + 0xAAu));
        c.V0 = c.V0 >> 8;
        m.WriteU16((c.S3 + 0xA0u), (ushort)c.V0);
        c.RA = 0x801BE648u;
        SoTN.rsin(c, m);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = (uint)((int)c.V1 >> 12);
        c.V1 = c.V1 + 0xD0u;
        m.WriteU16((c.S3 + 0xAEu), (ushort)c.V1);
        L801BE660: ;
        c.S0 = m.ReadU32((c.S3 + 0x7Cu));
        c.S0 = m.ReadU32(c.S0);
        c.S0 = m.ReadU32(c.S0);
        c.A1 = m.ReadU8((c.S0 + 0xDu));
        c.A0 = m.ReadU8((c.S0 + 0x25u));
        c.A1 = c.A1 + 0x2u;
        c.A2 = c.A0 + 0x2u;
        c.A0 = c.A2 + 0u;
        c.V0 = c.A0 & 0x00FFu;
        c.V1 = c.A1 & 0x00FFu;
        c.V0 = c.V0 < c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L801BE6A8;
        }
        c.A1 = c.A0 + 0u;
        c.A0 = c.A2 + 0x40u;
        L801BE6A8: ;
        if (c.S0 == 0u) {
            goto L801BE6D0;
        }
        L801BE6B0: ;
        m.WriteU8((c.S0 + 0x19u), (byte)c.A1);
        m.WriteU8((c.S0 + 0xDu), (byte)c.A1);
        m.WriteU8((c.S0 + 0x31u), (byte)c.A0);
        m.WriteU8((c.S0 + 0x25u), (byte)c.A0);
        c.S0 = m.ReadU32(c.S0);
        if (c.S0 != 0u) {
            goto L801BE6B0;
        }
        L801BE6D0: ;
        c.V0 = m.ReadU16((c.S3 + 0x80u));
        c.A0 = 0u | 0x00C0u;
        c.V0 = c.V0 - 0x4u;
        m.WriteU16((c.S3 + 0x80u), (ushort)c.V0);
        c.RA = 0x801BE6E4u;
        SoTN.SetGeomScreen(c, m);
        c.A0 = (uint)(short)m.ReadU16((c.S3 + 0x2u));
        c.A1 = (uint)(short)m.ReadU16((c.S3 + 0x6u));
        c.FP = 0u + 0u;
        c.RA = 0x801BE6F4u;
        SoTN.SetGeomOffset(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x801BE704u;
        SoTN.SetFarColor(c, m);
        c.A0 = 0u | 0x0128u;
        c.A1 = 0u | 0x00C0u;
        c.RA = 0x801BE710u;
        SoTN.SetFogNear(c, m);
        m.WriteU16((c.SP + 0x10u), (ushort)0u);
        m.WriteU16((c.SP + 0x12u), (ushort)0u);
        c.V1 = m.ReadU16((c.S3 + 0x80u));
        m.WriteU16((c.SP + 0x14u), (ushort)c.V1);
        c.V0 = m.ReadU16((c.S3 + 0x9Cu));
        c.A0 = c.SP + 0x10u;
        m.WriteU16((c.SP + 0x10u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S3 + 0x9Eu));
        c.S0 = c.SP + 0x28u;
        m.WriteU16((c.SP + 0x12u), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S3 + 0xA0u));
        c.A1 = c.S0 + 0u;
        c.V1 = c.V1 + c.V0;
        m.WriteU16((c.SP + 0x14u), (ushort)c.V1);
        c.RA = 0x801BE750u;
        SoTN.RotMatrix(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x801BE758u;
        SoTN.SetRotMatrix(c, m);
        c.A0 = c.S0 + 0u;
        c.V0 = 0u | 0x00C0u;
        m.WriteU32((c.SP + 0x18u), 0u);
        m.WriteU32((c.SP + 0x1Cu), 0u);
        m.WriteU32((c.SP + 0x20u), c.V0);
        c.V0 = (uint)(short)m.ReadU16((c.S3 + 0xAEu));
        c.A1 = c.SP + 0x18u;
        c.V0 = c.V0 + 0xC0u;
        m.WriteU32((c.SP + 0x20u), c.V0);
        c.RA = 0x801BE780u;
        SoTN.TransMatrix(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x801BE788u;
        SoTN.SetTransMatrix(c, m);
        c.A2 = 0x1F800000u;
        c.A3 = 0u | 0x0004u;
        c.A0 = 0x1F800000u;
        c.A0 = c.A0 | 0x0003u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x23C8u;
        L801BE7A0: ;
        c.V1 = c.FP & 0x0001u;
        c.V1 = c.V1 << 1;
        c.V0 = m.ReadU8(c.A1);
        c.S5 = c.V1 + 0x1u;
        if (c.S5 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.S5 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.S5); c.HI = (uint)((int)c.V0 % (int)c.S5); } }
        if (c.S5 != 0u) {
            goto L801BE7C0;
        }
        Bios.Break(c, m);
        L801BE7C0: ;
        c.At = 0xFFFFFFFFu;
        if (c.S5 != c.At) {
            c.At = 0x80000000u;
            goto L801BE7D8;
        }
        c.At = 0x80000000u;
        if (c.V0 != c.At) {
            goto L801BE7D8;
        }
        Bios.Break(c, m);
        L801BE7D8: ;
        c.V0 = c.LO;
        m.WriteU8((c.A0 - 0x1u), (byte)c.V0);
        c.V0 = m.ReadU8(c.A1);
        c.V0 = c.V0 >> 1;
        if (c.S5 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.S5 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.S5); c.HI = (uint)((int)c.V0 % (int)c.S5); } }
        if (c.S5 != 0u) {
            goto L801BE800;
        }
        Bios.Break(c, m);
        L801BE800: ;
        c.At = 0xFFFFFFFFu;
        if (c.S5 != c.At) {
            c.At = 0x80000000u;
            goto L801BE818;
        }
        c.At = 0x80000000u;
        if (c.V0 != c.At) {
            goto L801BE818;
        }
        Bios.Break(c, m);
        L801BE818: ;
        c.V0 = c.LO;
        c.FP = c.FP + 0x1u;
        c.A1 = c.A1 + 0x1u;
        m.WriteU8((c.A0 - 0x2u), (byte)c.V0);
        m.WriteU8(c.A2, (byte)c.V0);
        m.WriteU8(c.A0, (byte)c.A3);
        c.A0 = c.A0 + 0x4u;
        c.V0 = (int)c.FP < 16 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A2 = c.A2 + 0x4u;
            goto L801BE7A0;
        }
        c.A2 = c.A2 + 0x4u;
        c.FP = 0u + 0u;
        c.T0 = 0x801C0000u;
        c.T0 = c.T0 + 0x1BC8u;
        c.T2 = c.T0 + 0x80u;
        c.S0 = m.ReadU32((c.S3 + 0x7Cu));
        c.T1 = 0u | 0x0004u;
        c.S0 = m.ReadU32(c.S0);
        c.A3 = 0x1F800000u;
        c.S0 = m.ReadU32(c.S0);
        c.V0 = 0u | 0x0080u;
        m.WriteU8((c.SP + 0xB0u), (byte)c.V0);
        m.WriteU8((c.SP + 0xB1u), (byte)c.V0);
        m.WriteU8((c.SP + 0xB2u), (byte)c.V0);
        c.V0 = 0u | 0x0004u;
        m.WriteU8((c.SP + 0xB3u), (byte)c.V0);
        L801BE87C: ;
        c.S4 = 0u + 0u;
        c.S5 = c.FP << 4;
        c.A2 = c.A3 + 0u;
        c.A1 = c.A3 + 0x4u;
        c.V0 = c.S5 + c.S4;
        L801BE890: ;
        c.V1 = c.S4 + 0x1u;
        c.V1 = c.V1 & 0x000Fu;
        c.V1 = c.S5 + c.V1;
        c.A0 = c.V0 << 3;
        c.V0 = c.A0 + c.T0;
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.V0));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.V0 + 0x4u)));
        RecompOne.Runtime.Gte.Execute(0x4A180001u);
        c.V0 = c.S0 + 0x8u;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(14));
        RecompOne.Runtime.Gte.LoadWord(6, m.ReadU32(c.A2));
        RecompOne.Runtime.Gte.Execute(0x4A780010u);
        c.V0 = c.S0 + 0x4u;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(22));
        c.V1 = c.V1 << 3;
        c.V0 = c.V1 + c.T0;
        m.WriteU8((c.S0 + 0x7u), (byte)c.T1);
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.V0));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.V0 + 0x4u)));
        RecompOne.Runtime.Gte.Execute(0x4A180001u);
        c.V0 = c.S0 + 0x14u;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(14));
        RecompOne.Runtime.Gte.LoadWord(6, m.ReadU32(c.A2));
        RecompOne.Runtime.Gte.Execute(0x4A780010u);
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.V0 = c.S0 + 0x10u;
            goto L801BE930;
        }
        c.V0 = c.S0 + 0x10u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x16u));
        if ((int)c.V0 < 0) {
            c.V0 = c.S0 + 0x10u;
            goto L801BEA94;
        }
        c.V0 = c.S0 + 0x10u;
        L801BE930: ;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(22));
        c.V0 = c.A0 + c.T2;
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.V0));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.V0 + 0x4u)));
        RecompOne.Runtime.Gte.Execute(0x4A180001u);
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0xAu));
        c.V0 = (int)c.V0 < 257 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0x20u;
            goto L801BE974;
        }
        c.V0 = c.S0 + 0x20u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x16u));
        c.V0 = (int)c.V0 < 257 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + 0x20u;
            goto L801BEA94;
        }
        c.V0 = c.S0 + 0x20u;
        L801BE974: ;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(14));
        RecompOne.Runtime.Gte.LoadWord(6, m.ReadU32(c.A1));
        RecompOne.Runtime.Gte.Execute(0x4A780010u);
        c.V0 = c.S0 + 0x1Cu;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(22));
        c.V0 = c.V1 + c.T2;
        RecompOne.Runtime.Gte.LoadWord(0, m.ReadU32(c.V0));
        RecompOne.Runtime.Gte.LoadWord(1, m.ReadU32((c.V0 + 0x4u)));
        RecompOne.Runtime.Gte.Execute(0x4A180001u);
        c.V0 = c.S0 + 0x2Cu;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(14));
        RecompOne.Runtime.Gte.LoadWord(6, m.ReadU32(c.A1));
        RecompOne.Runtime.Gte.Execute(0x4A780010u);
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x8u));
        c.V0 = (int)c.V0 < xMax ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0x28u;
            goto L801BEA10;
        }
        c.V0 = c.S0 + 0x28u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x14u));
        c.V0 = (int)c.V0 < xMax ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0x28u;
            goto L801BEA10;
        }
        c.V0 = c.S0 + 0x28u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x20u));
        c.V0 = (int)c.V0 < xMax ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0x28u;
            goto L801BEA10;
        }
        c.V0 = c.S0 + 0x28u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = (int)c.V0 < xMax ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + 0x28u;
            goto L801BEA94;
        }
        c.V0 = c.S0 + 0x28u;
        L801BEA10: ;
        m.WriteU32(c.V0, RecompOne.Runtime.Gte.StoreWord(22));
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x8u));
        if ((int)c.V0 >= xMin) {
            c.V0 = c.SP + 0xB8u;
            goto L801BEA54;
        }
        c.V0 = c.SP + 0xB8u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x14u));
        if ((int)c.V0 >= xMin) {
            c.V0 = c.SP + 0xB8u;
            goto L801BEA54;
        }
        c.V0 = c.SP + 0xB8u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x20u));
        if ((int)c.V0 >= xMin) {
            c.V0 = c.SP + 0xB8u;
            goto L801BEA54;
        }
        c.V0 = c.SP + 0xB8u;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x2Cu));
        if ((int)c.V0 < xMin) {
            c.V0 = c.SP + 0xB8u;
            goto L801BEA94;
        }
        c.V0 = c.SP + 0xB8u;
        L801BEA54: ;
        c.T4 = RecompOne.Runtime.Gte.Read(19);
        c.T4 = (uint)((int)c.T4 >> 2);
        m.WriteU32(c.V0, c.T4);
        c.V0 = m.ReadU32((c.SP + 0xB8u));
        c.V0 = (int)c.V0 < 85 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0u | 0x0058u;
            goto L801BEA84;
        }
        c.V0 = 0u | 0x0058u;
        c.V1 = m.ReadU16((c.SP + 0xB8u));
        c.V0 = c.V0 - c.V1;
        goto L801BEA88;
        L801BEA84: ;
        c.V0 = 0u | 0x0003u;
        L801BEA88: ;
        m.WriteU16((c.S0 + 0x26u), (ushort)c.V0);
        m.WriteU16((c.S0 + 0x32u), (ushort)c.T1);
        c.S0 = m.ReadU32(c.S0);
        L801BEA94: ;
        c.S4 = c.S4 + 0x1u;
        c.V0 = (int)c.S4 < 16 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S5 + c.S4;
            goto L801BE890;
        }
        c.V0 = c.S5 + c.S4;
        c.FP = c.FP + 0x1u;
        c.V0 = (int)c.FP < 15 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A3 = c.A3 + 0x4u;
            goto L801BE87C;
        }
        c.A3 = c.A3 + 0x4u;
        if (c.S0 == 0u) {
            c.V0 = 0u | 0x0008u;
            goto L801BEAD0;
        }
        c.V0 = 0u | 0x0008u;
        L801BEABC: ;
        m.WriteU16((c.S0 + 0x32u), (ushort)c.V0);
        c.S0 = m.ReadU32(c.S0);
        if (c.S0 != 0u) {
            goto L801BEABC;
        }
        L801BEAD0: ;
        c.RA = m.ReadU32((c.SP + 0xFCu));
        c.FP = m.ReadU32((c.SP + 0xF8u));
        c.S7 = m.ReadU32((c.SP + 0xF4u));
        c.S6 = m.ReadU32((c.SP + 0xF0u));
        c.S5 = m.ReadU32((c.SP + 0xECu));
        c.S4 = m.ReadU32((c.SP + 0xE8u));
        c.S3 = m.ReadU32((c.SP + 0xE4u));
        c.S2 = m.ReadU32((c.SP + 0xE0u));
        c.S1 = m.ReadU32((c.SP + 0xDCu));
        c.S0 = m.ReadU32((c.SP + 0xD8u));
        c.SP = c.SP + 0x100u;
        return;
    }

    // title card fade fx
    public static void TitleFadeoutWide(CpuContext c, IMemory m)
    {
        int margin = StageMargin();
        c.SP = c.SP - 0x20u;
        m.WriteU32((c.SP + 0x18u), c.S0);
        c.S0 = c.A0 + 0u;
        m.WriteU32((c.SP + 0x1Cu), c.RA);
        c.V1 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 << 2;
            goto L801AB5D0;
        }
        c.V0 = c.V1 << 2;
        c.At = 0x801A0000u;
        c.At = c.At + c.V0;
        c.V0 = m.ReadU32((c.At + 0x7AE0u));
        switch (c.V0)
        {
            case 0x801AB104u: goto L801AB104;
            case 0x801AB20Cu: goto L801AB20C;
            case 0x801AB268u: goto L801AB268;
            case 0x801AB2ECu: goto L801AB2EC;
            case 0x801AB344u: goto L801AB344;
            case 0x801AB3D4u: goto L801AB3D4;
            case 0x801AB4E4u: goto L801AB4E4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L801AB104: ;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x908u));
        if (c.V0 != 0u) {
            goto L801AB5B8;
        }
        c.A0 = 0x80180000u;
        c.A0 = c.A0 + 0x5D4u;
        c.RA = 0x801AB128u;
        SoTN.InitializeEntity_st0(c, m);
        c.A0 = 0u | 0x0003u;
        c.V0 = 0x80040000u;
        c.V0 = m.ReadU32((c.V0 - 0x3848u));
        c.A1 = 0u | 0x0005u;
        c.RA = 0x801AB140u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 << 16;
        c.A0 = (uint)((int)c.V0 >> 16);
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = c.A0 << 1;
            goto L801AB5B8;
        }
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80080000u;
        c.V1 = c.V1 + 0x6FECu;
        c.A2 = c.V0 + c.V1;
        c.V0 = m.ReadU32((c.S0 + 0x34u));
        c.V1 = 0x00800000u;
        m.WriteU32((c.S0 + 0x64u), c.A0);
        m.WriteU32((c.S0 + 0x7Cu), c.A2);
        c.V0 = c.V0 | c.V1;
        if (c.A2 == 0u) {
            m.WriteU32((c.S0 + 0x34u), c.V0);
            goto L801AB1A4;
        }
        m.WriteU32((c.S0 + 0x34u), c.V0);
        c.V0 = 0u | 0x0008u;
        L801AB190: ;
        m.WriteU16((c.A2 + 0x32u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        if (c.A2 != 0u) {
            goto L801AB190;
        }
        L801AB1A4: ;
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        m.WriteU8((c.A2 + 0x6u), (byte)0u);
        m.WriteU8((c.A2 + 0x5u), (byte)0u);
        m.WriteU8((c.A2 + 0x4u), (byte)0u);
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x4u));
        c.A1 = m.ReadU32((c.A2 + 0x4u));
        c.V0 = (uint)(0x100 + margin);
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V0);
        c.V0 = 0u | 0x00F0u;
        m.WriteU16((c.A2 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x22u), (ushort)c.V0);
        c.V0 = 0u | 0x01FDu;
        m.WriteU16((c.A2 + 0x26u), (ushort)c.V0);
        c.V0 = 0u | 0x0020u;
        m.WriteU16((c.A2 + 0x20u), (ushort)(short)(-margin));
        m.WriteU16((c.A2 + 0x8u), (ushort)(short)(-margin));
        m.WriteU16((c.A2 + 0x16u), (ushort)0u);
        m.WriteU16((c.A2 + 0xAu), (ushort)0u);
        m.WriteU16((c.A2 + 0x32u), (ushort)0u);
        m.WriteU32((c.A2 + 0x10u), c.V1);
        m.WriteU32((c.A2 + 0x1Cu), c.A0);
        m.WriteU32((c.A2 + 0x28u), c.A1);
        m.WriteU16((c.S0 + 0x88u), (ushort)c.V0);
        L801AB20C: ;
        c.V0 = 0x80180000u;
        c.V0 = m.ReadU32((c.V0 + 0x908u));
        if (c.V0 == 0u) {
            c.V0 = 0u | 0x00F7u;
            goto L801AB5D0;
        }
        c.V0 = 0u | 0x00F7u;
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        m.WriteU8((c.A2 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A2 + 0x5u), (byte)c.V0);
        m.WriteU8((c.A2 + 0x4u), (byte)c.V0);
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x4u));
        c.A1 = m.ReadU32((c.A2 + 0x4u));
        c.V0 = 0u | 0x0051u;
        m.WriteU16((c.A2 + 0x32u), (ushort)c.V0);
        m.WriteU32((c.A2 + 0x10u), c.V1);
        m.WriteU32((c.A2 + 0x1Cu), c.A0);
        m.WriteU32((c.A2 + 0x28u), c.A1);
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        goto L801AB5D0;
        L801AB268: ;
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        c.A2 = m.ReadU32(c.A2);
        m.WriteU8((c.A2 + 0x6u), (byte)0u);
        m.WriteU8((c.A2 + 0x5u), (byte)0u);
        m.WriteU8((c.A2 + 0x4u), (byte)0u);
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x4u));
        c.A1 = m.ReadU32((c.A2 + 0x4u));
        c.V0 = 0u | 0x00C0u;
        m.WriteU16((c.A2 + 0x26u), (ushort)c.V0);
        c.V0 = (uint)(0x100 + margin);
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V0);
        c.V0 = 0u | 0x00F0u;
        m.WriteU16((c.A2 + 0x2Eu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x22u), (ushort)c.V0);
        c.V0 = 0u | 0x00D0u;
        m.WriteU16((c.A2 + 0x32u), (ushort)0u);
        m.WriteU16((c.A2 + 0x20u), (ushort)(short)(-margin));
        m.WriteU16((c.A2 + 0x8u), (ushort)(short)(-margin));
        m.WriteU16((c.A2 + 0x16u), (ushort)0u);
        m.WriteU16((c.A2 + 0xAu), (ushort)0u);
        m.WriteU32((c.A2 + 0x10u), c.V1);
        m.WriteU32((c.A2 + 0x1Cu), c.A0);
        m.WriteU32((c.A2 + 0x28u), c.A1);
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x33FCu), (ushort)c.V0);
        c.V0 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = c.V0 + 0x1u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        L801AB2EC: ;
        c.V0 = 0u | 0x00D0u;
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x33FCu), (ushort)c.V0);
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        c.V0 = m.ReadU8((c.A2 + 0x4u));
        c.V0 = c.V0 + 0xF8u;
        m.WriteU8((c.A2 + 0x4u), (byte)c.V0);
        c.V1 = m.ReadU8((c.A2 + 0x4u));
        c.V0 = c.V1 < 0x000000F9u ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 + 0u;
            goto L801AB3AC;
        }
        c.V0 = c.V1 + 0u;
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.A2 + 0x4u), (byte)0u);
        m.WriteU16((c.A2 + 0x32u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        c.V0 = 0u | 0x0005u;
        m.WriteU16((c.A2 + 0x32u), (ushort)0u);
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        goto L801AB5D0;
        L801AB344: ;
        c.V1 = 0x80070000u;
        c.V1 = c.V1 + 0x33FCu;
        c.V0 = 0u | 0x00D0u;
        m.WriteU16(c.V1, (ushort)c.V0);
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        c.A2 = m.ReadU32(c.A2);
        c.V0 = m.ReadU8((c.A2 + 0x4u));
        c.V0 = c.V0 + 0xF8u;
        m.WriteU8((c.A2 + 0x4u), (byte)c.V0);
        c.V0 = m.ReadU8((c.A2 + 0x4u));
        c.V0 = c.V0 < 0x000000F9u ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0u | 0x0008u;
            goto L801AB3A8;
        }
        c.V0 = 0u | 0x0008u;
        m.WriteU8((c.A2 + 0x4u), (byte)0u);
        m.WriteU16((c.A2 + 0x32u), (ushort)c.V0);
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU16((c.V0 + 0x7408u));
        m.WriteU16(c.V1, (ushort)c.V0);
        c.V0 = 0u | 0x0001u;
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V0);
        L801AB3A8: ;
        c.V0 = m.ReadU8((c.A2 + 0x4u));
        L801AB3AC: ;
        m.WriteU8((c.A2 + 0x6u), (byte)c.V0);
        m.WriteU8((c.A2 + 0x5u), (byte)c.V0);
        c.V0 = m.ReadU32((c.A2 + 0x4u));
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x4u));
        m.WriteU32((c.A2 + 0x10u), c.V0);
        m.WriteU32((c.A2 + 0x1Cu), c.V1);
        m.WriteU32((c.A2 + 0x28u), c.A0);
        goto L801AB5D0;
        L801AB3D4: ;
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        if (c.A2 == 0u) {
            goto L801AB444;
        }
        c.A3 = 0u | 0x00F0u;
        c.T1 = 0u | 0x00B0u;
        c.T0 = 0u | 0x0055u;
        c.A1 = 0u | 0x00FFu;
        L801AB3F4: ;
        m.WriteU8((c.A2 + 0x6u), (byte)c.A1);
        m.WriteU8((c.A2 + 0x5u), (byte)c.A1);
        m.WriteU8((c.A2 + 0x4u), (byte)c.A1);
        c.V0 = m.ReadU32((c.A2 + 0x4u));
        c.V1 = m.ReadU32((c.A2 + 0x4u));
        c.A0 = m.ReadU32((c.A2 + 0x4u));
        m.WriteU16((c.A2 + 0x16u), (ushort)0u);
        m.WriteU16((c.A2 + 0xAu), (ushort)0u);
        m.WriteU16((c.A2 + 0x2Eu), (ushort)c.A3);
        m.WriteU16((c.A2 + 0x22u), (ushort)c.A3);
        m.WriteU16((c.A2 + 0x26u), (ushort)c.T1);
        m.WriteU16((c.A2 + 0x32u), (ushort)c.T0);
        m.WriteU32((c.A2 + 0x10u), c.V0);
        m.WriteU32((c.A2 + 0x1Cu), c.V1);
        m.WriteU32((c.A2 + 0x28u), c.A0);
        c.A2 = m.ReadU32(c.A2);
        if (c.A2 != 0u) {
            goto L801AB3F4;
        }
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        L801AB444: ;
        m.WriteU16((c.A2 + 0x20u), (ushort)(short)(-margin));
        m.WriteU16((c.A2 + 0x8u), (ushort)(short)(-margin));
        c.A2 = m.ReadU32(c.A2);
        c.A0 = 0u | 0x00FFu;
        m.WriteU8((c.A2 + 0x6u), (byte)c.A0);
        m.WriteU8((c.A2 + 0x5u), (byte)c.A0);
        m.WriteU8((c.A2 + 0x4u), (byte)c.A0);
        c.V0 = m.ReadU32((c.A2 + 0x4u));
        m.WriteU8((c.A2 + 0x12u), (byte)0u);
        m.WriteU8((c.A2 + 0x11u), (byte)0u);
        m.WriteU8((c.A2 + 0x10u), (byte)0u);
        c.V1 = m.ReadU32((c.A2 + 0x10u));
        m.WriteU32((c.A2 + 0x1Cu), c.V0);
        m.WriteU32((c.A2 + 0x28u), c.V1);
        c.A2 = m.ReadU32(c.A2);
        c.V0 = (uint)(0x100 + margin);
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        m.WriteU8((c.A2 + 0x6u), (byte)0u);
        m.WriteU8((c.A2 + 0x5u), (byte)0u);
        m.WriteU8((c.A2 + 0x4u), (byte)0u);
        c.V0 = m.ReadU32((c.A2 + 0x4u));
        m.WriteU8((c.A2 + 0x12u), (byte)c.A0);
        m.WriteU8((c.A2 + 0x11u), (byte)c.A0);
        m.WriteU8((c.A2 + 0x10u), (byte)c.A0);
        c.V1 = m.ReadU32((c.A2 + 0x10u));
        m.WriteU32((c.A2 + 0x1Cu), c.V0);
        m.WriteU32((c.A2 + 0x28u), c.V1);
        c.A0 = 0x80070000u;
        c.A0 = (uint)(short)m.ReadU16((c.A0 + 0x33DAu));
        c.V1 = m.ReadU16((c.S0 + 0x2Cu));
        c.V0 = 0u | 0x0020u;
        m.WriteU16((c.S0 + 0x88u), (ushort)c.V0);
        c.V1 = c.V1 + 0x1u;
        m.WriteU32((c.S0 + 0x84u), c.A0);
        m.WriteU32((c.S0 + 0x80u), c.A0);
        m.WriteU16((c.S0 + 0x2Cu), (ushort)c.V1);
        L801AB4E4: ;
        c.V0 = (uint)(short)m.ReadU16((c.S0 + 0x88u));
        if (c.V0 == 0u) {
            c.V1 = c.V0 + 0u;
            goto L801AB518;
        }
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 - 0x1u;
        m.WriteU16((c.S0 + 0x88u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            goto L801AB518;
        }
        c.V0 = 0x80090000u;
        c.V0 = m.ReadU16((c.V0 + 0x7408u));
        c.At = 0x80070000u;
        m.WriteU16((c.At + 0x33FCu), (ushort)c.V0);
        L801AB518: ;
        c.A0 = m.ReadU32((c.S0 + 0x80u));
        c.A1 = m.ReadU32((c.S0 + 0x84u));
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        c.A0 = c.A0 - 0x4u;
        c.A1 = c.A1 + 0x4u;
        c.V0 = c.A0 + 0u;
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        c.V1 = c.A0 + 0x40u;
        m.WriteU16((c.A2 + 0x20u), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x8u), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V1);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V1);
        c.A2 = m.ReadU32(c.A2);
        c.V0 = c.A1 + 0u;
        m.WriteU16((c.A2 + 0x20u), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x8u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        c.V1 = c.A1 - 0x40u;
        m.WriteU16((c.A2 + 0x2Cu), (ushort)c.V0);
        m.WriteU16((c.A2 + 0x14u), (ushort)c.V0);
        c.V0 = (int)c.A0 < -64 - margin ? 1u : 0u;
        m.WriteU16((c.A2 + 0x20u), (ushort)c.V1);
        if (c.V0 == 0u) {
            m.WriteU16((c.A2 + 0x8u), (ushort)c.V1);
            goto L801AB5C8;
        }
        m.WriteU16((c.A2 + 0x8u), (ushort)c.V1);
        c.V0 = (int)c.A1 < 321 + margin ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L801AB5C8;
        }
        c.A2 = m.ReadU32((c.S0 + 0x7Cu));
        if (c.A2 == 0u) {
            c.V0 = 0u | 0x0008u;
            goto L801AB5B0;
        }
        c.V0 = 0u | 0x0008u;
        L801AB59C: ;
        m.WriteU16((c.A2 + 0x32u), (ushort)c.V0);
        c.A2 = m.ReadU32(c.A2);
        if (c.A2 != 0u) {
            goto L801AB59C;
        }
        L801AB5B0: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x801AB5B8u;
        SoTN.PreventEntityFromRespawning_st0(c, m);
        L801AB5B8: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x801AB5C0u;
        SoTN.DestroyEntity_st0(c, m);
        goto L801AB5D0;
        L801AB5C8: ;
        m.WriteU32((c.S0 + 0x80u), c.A0);
        m.WriteU32((c.S0 + 0x84u), c.A1);
        L801AB5D0: ;
        c.RA = m.ReadU32((c.SP + 0x1Cu));
        c.S0 = m.ReadU32((c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
}
