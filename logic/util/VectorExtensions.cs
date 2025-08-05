using System;
using Godot;
// ReSharper disable InconsistentNaming

namespace MPAutoChess.logic.util;

public static class VectorEXtensions {

    public static Vector2 Uniform2D(float value) {
       return new Vector2(value, value);
    }
    
    public static Vector3 Uniform3D(float value) {
       return new Vector3(value, value, value);
    }
    
    public static Vector4 Uniform4D(float value) {
       return new Vector4(value, value, value, value);
    }
    
    #region Vector2
    public static Vector3 Extend(this Vector2 v, float Z) {
       return new Vector3(v.X, v.Y, Z);
    }
    public static Vector4 Extend(this Vector2 v, float Z, float w) {
       return new Vector4(v.X, v.Y, Z, w);
    }
    public static float Largest(this Vector2 v) {
         return Mathf.Max(v.X, v.Y);
    }
    public static float Smallest(this Vector2 v) {
        return Mathf.Min(v.X, v.Y);
    }
    
    public static float X(this Vector2 v) {
       return v.X;
    }
    public static float R(this Vector2 v) {
       return v.X;
    }
    public static float Y(this Vector2 v) {
       return v.Y;
    }
    public static float G(this Vector2 v) {
       return v.Y;
    }
    
    public static Vector2 XY(this Vector2 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 RG(this Vector2 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 YX(this Vector2 v) {
       return new Vector2(v.Y, v.X);
    }
    public static Vector2 GR(this Vector2 v) {
       return new Vector2(v.Y, v.X);
    }
    #endregion
    #region Vector3
    public static Vector4 EXtend(this Vector3 v, float w) {
       return new Vector4(v.X, v.Y, v.Z, w);
    }
    public static float Largest(this Vector3 v) {
        return Math.Max(v.X, Math.Max(v.Y, v.Z));
    }
    public static float Smallest(this Vector3 v) {
        return Math.Min(v.X, Math.Min(v.Y, v.Z));
    }
    
    public static float X(this Vector3 v) {
       return v.X;
    }
    public static float R(this Vector3 v) {
       return v.X;
    }
    public static float Y(this Vector3 v) {
       return v.Y;
    }
    public static float G(this Vector3 v) {
       return v.Y;
    }
    public static float Z(this Vector3 v) {
       return v.Z;
    }
    public static float B(this Vector3 v) {
       return v.Z;
    }
    
    public static Vector2 XY(this Vector3 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 RG(this Vector3 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 XZ(this Vector3 v) {
       return new Vector2(v.X, v.Z);
    }
    public static Vector2 RB(this Vector3 v) {
       return new Vector2(v.X, v.Z);
    }
    public static Vector2 YX(this Vector3 v) {
       return new Vector2(v.Y, v.X);
    }
    public static Vector2 GR(this Vector3 v) {
       return new Vector2(v.Y, v.X);
    }
    public static Vector2 YZ(this Vector3 v) {
       return new Vector2(v.Y, v.Z);
    }
    public static Vector2 GB(this Vector3 v) {
       return new Vector2(v.Y, v.Z);
    }
    public static Vector2 ZX(this Vector3 v) {
       return new Vector2(v.Z, v.X);
    }
    public static Vector2 BR(this Vector3 v) {
       return new Vector2(v.Z, v.X);
    }
    public static Vector2 ZY(this Vector3 v) {
       return new Vector2(v.Z, v.Y);
    }
    public static Vector2 BG(this Vector3 v) {
       return new Vector2(v.Z, v.Y);
    }
    public static Vector3 XYZ(this Vector3 v) {
       return new Vector3(v.X, v.Y, v.Z);
    }
    
    public static Vector3 RGB(this Vector3 v) {
       return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 XZY(this Vector3 v) {
       return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 RBG(this Vector3 v) {
       return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 YXZ(this Vector3 v) {
       return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 GRB(this Vector3 v) {
       return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 YZX(this Vector3 v) {
       return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 GBR(this Vector3 v) {
       return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 ZXY(this Vector3 v) {
       return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 BRG(this Vector3 v) {
       return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 ZYX(this Vector3 v) {
       return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 BGR(this Vector3 v) {
       return new Vector3(v.Z, v.Y, v.X);
    }
    #endregion
    #region Vector4
    public static float Largest(this Vector4 v) {
        return Math.Max(v.X, Math.Max(v.Y, Math.Max(v.Z, v.W)));
    }
    public static float Smallest(this Vector4 v) {
        return Math.Min(v.X, Math.Min(v.Y, Math.Min(v.Z, v.W)));
    }
    
    public static float X(this Vector4 v) {
       return v.X;
    }
    public static float R(this Vector4 v) {
       return v.X;
    }
    public static float Y(this Vector4 v) {
       return v.Y;
    }
    public static float G(this Vector4 v) {
       return v.Y;
    }
    public static float Z(this Vector4 v) {
       return v.Z;
    }
    public static float B(this Vector4 v) {
       return v.Z;
    }
    public static float W(this Vector4 v) {
       return v.W;
    }
    public static float A(this Vector4 v) {
       return v.W;
    }
    
    public static Vector2 XY(this Vector4 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 RG(this Vector4 v) {
       return new Vector2(v.X, v.Y);
    }
    public static Vector2 XZ(this Vector4 v) {
       return new Vector2(v.X, v.Z);
    }
    public static Vector2 RB(this Vector4 v) {
       return new Vector2(v.X, v.Z);
    }
    public static Vector2 XW(this Vector4 v) {
       return new Vector2(v.X, v.W);
    }
    public static Vector2 RA(this Vector4 v) {
       return new Vector2(v.X, v.W);
    }
    public static Vector2 YX(this Vector4 v) {
       return new Vector2(v.Y, v.X);
    }
    public static Vector2 GR(this Vector4 v) {
       return new Vector2(v.Y, v.X);
    }
    public static Vector2 YZ(this Vector4 v) {
       return new Vector2(v.Y, v.Z);
    }
    public static Vector2 GB(this Vector4 v) {
       return new Vector2(v.Y, v.Z);
    }
    public static Vector2 YW(this Vector4 v) {
       return new Vector2(v.Y, v.W);
    }
    public static Vector2 GA(this Vector4 v) {
       return new Vector2(v.Y, v.W);
    }
    public static Vector2 ZX(this Vector4 v) {
       return new Vector2(v.Z, v.X);
    }
    public static Vector2 BR(this Vector4 v) {
       return new Vector2(v.Z, v.X);
    }
    public static Vector2 ZY(this Vector4 v) {
       return new Vector2(v.Z, v.Y);
    }
    public static Vector2 BG(this Vector4 v) {
       return new Vector2(v.Z, v.Y);
    }
    public static Vector2 ZW(this Vector4 v) {
       return new Vector2(v.Z, v.W);
    }
    public static Vector2 BA(this Vector4 v) {
       return new Vector2(v.Z, v.W);
    }
    public static Vector2 WX(this Vector4 v) {
       return new Vector2(v.W, v.X);
    }
    public static Vector2 AR(this Vector4 v) {
       return new Vector2(v.W, v.X);
    }
    public static Vector2 WY(this Vector4 v) {
       return new Vector2(v.W, v.Y);
    }
    public static Vector2 AG(this Vector4 v) {
       return new Vector2(v.W, v.Y);
    }
    public static Vector2 WZ(this Vector4 v) {
       return new Vector2(v.W, v.Z);
    }
    public static Vector2 AB(this Vector4 v) {
       return new Vector2(v.W, v.Z);
    }
    
    public static Vector3 XYZ(this Vector4 v) {
       return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 RGB(this Vector4 v) {
       return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 XYW(this Vector4 v) {
       return new Vector3(v.X, v.Y, v.W);
    }
    public static Vector3 RGA(this Vector4 v) {
       return new Vector3(v.X, v.Y, v.W);
    }
    public static Vector3 XZY(this Vector4 v) {
       return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 RBG(this Vector4 v) {
       return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 XZW(this Vector4 v) {
       return new Vector3(v.X, v.Z, v.W);
    }
    public static Vector3 RBA(this Vector4 v) {
       return new Vector3(v.X, v.Z, v.W);
    }
    public static Vector3 XWY(this Vector4 v) {
       return new Vector3(v.X, v.W, v.Y);
    }
    public static Vector3 RAG(this Vector4 v) {
       return new Vector3(v.X, v.W, v.Y);
    }
    public static Vector3 XWZ(this Vector4 v) {
       return new Vector3(v.X, v.W, v.Z);
    }
    public static Vector3 RAB(this Vector4 v) {
       return new Vector3(v.X, v.W, v.Z);
    }
    public static Vector3 YXZ(this Vector4 v) {
       return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 GRB(this Vector4 v) {
       return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 YXW(this Vector4 v) {
       return new Vector3(v.Y, v.X, v.W);
    }
    public static Vector3 GRA(this Vector4 v) {
       return new Vector3(v.Y, v.X, v.W);
    }
    public static Vector3 YZX(this Vector4 v) {
       return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 GBR(this Vector4 v) {
       return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 YZW(this Vector4 v) {
       return new Vector3(v.Y, v.Z, v.W);
    }
    public static Vector3 GBA(this Vector4 v) {
       return new Vector3(v.Y, v.Z, v.W);
    }
    public static Vector3 YWX(this Vector4 v) {
       return new Vector3(v.Y, v.W, v.X);
    }
    public static Vector3 GAR(this Vector4 v) {
       return new Vector3(v.Y, v.W, v.X);
    }
    public static Vector3 YWZ(this Vector4 v) {
       return new Vector3(v.Y, v.W, v.Z);
    }
    public static Vector3 GAB(this Vector4 v) {
       return new Vector3(v.Y, v.W, v.Z);
    }
    public static Vector3 ZXY(this Vector4 v) {
       return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 BRG(this Vector4 v) {
       return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 ZXW(this Vector4 v) {
       return new Vector3(v.Z, v.X, v.W);
    }
    public static Vector3 BRA(this Vector4 v) {
       return new Vector3(v.Z, v.X, v.W);
    }
    public static Vector3 ZYX(this Vector4 v) {
       return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 BGR(this Vector4 v) {
       return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 ZYW(this Vector4 v) {
       return new Vector3(v.Z, v.Y, v.W);
    }
    public static Vector3 BGA(this Vector4 v) {
       return new Vector3(v.Z, v.Y, v.W);
    }
    public static Vector3 ZWX(this Vector4 v) {
       return new Vector3(v.Z, v.W, v.X);
    }
    public static Vector3 BAR(this Vector4 v) {
       return new Vector3(v.Z, v.W, v.X);
    }
    public static Vector3 ZWY(this Vector4 v) {
       return new Vector3(v.Z, v.W, v.Y);
    }
    public static Vector3 BAG(this Vector4 v) {
       return new Vector3(v.Z, v.W, v.Y);
    }
    public static Vector3 WXY(this Vector4 v) {
       return new Vector3(v.W, v.X, v.Y);
    }
    public static Vector3 ARG(this Vector4 v) {
       return new Vector3(v.W, v.X, v.Y);
    }
    public static Vector3 WXZ(this Vector4 v) {
       return new Vector3(v.W, v.X, v.Z);
    }
    public static Vector3 ARB(this Vector4 v) {
       return new Vector3(v.W, v.X, v.Z);
    }
    public static Vector3 WYX(this Vector4 v) {
       return new Vector3(v.W, v.Y, v.X);
    }
    public static Vector3 AGR(this Vector4 v) {
       return new Vector3(v.W, v.Y, v.X);
    }
    public static Vector3 WYZ(this Vector4 v) {
       return new Vector3(v.W, v.Y, v.Z);
    }
    public static Vector3 AGB(this Vector4 v) {
       return new Vector3(v.W, v.Y, v.Z);
    }
    public static Vector3 WZX(this Vector4 v) {
       return new Vector3(v.W, v.Z, v.X);
    }
    public static Vector3 ABR(this Vector4 v) {
       return new Vector3(v.W, v.Z, v.X);
    }
    public static Vector3 WZY(this Vector4 v) {
       return new Vector3(v.W, v.Z, v.Y);
    }
    public static Vector3 ABG(this Vector4 v) {
       return new Vector3(v.W, v.Z, v.Y);
    }
    
    public static Vector4 XYZW(this Vector4 v) {
       return new Vector4(v.X, v.Y, v.Z, v.W);
    }
    public static Vector4 RGBA(this Vector4 v) {
       return new Vector4(v.X, v.Y, v.Z, v.W);
    }
    public static Vector4 XYWZ(this Vector4 v) {
       return new Vector4(v.X, v.Y, v.W, v.Z);
    }
    public static Vector4 RGAB(this Vector4 v) {
       return new Vector4(v.X, v.Y, v.W, v.Z);
    }
    public static Vector4 XZYW(this Vector4 v) {
       return new Vector4(v.X, v.Z, v.Y, v.W);
    }
    public static Vector4 RBGA(this Vector4 v) {
       return new Vector4(v.X, v.Z, v.Y, v.W);
    }
    public static Vector4 XZWY(this Vector4 v) {
       return new Vector4(v.X, v.Z, v.W, v.Y);
    }
    public static Vector4 RBAG(this Vector4 v) {
       return new Vector4(v.X, v.Z, v.W, v.Y);
    }
    public static Vector4 XWYZ(this Vector4 v) {
       return new Vector4(v.X, v.W, v.Y, v.Z);
    }
    public static Vector4 RAGB(this Vector4 v) {
       return new Vector4(v.X, v.W, v.Y, v.Z);
    }
    public static Vector4 XWZY(this Vector4 v) {
       return new Vector4(v.X, v.W, v.Z, v.Y);
    }
    public static Vector4 RABG(this Vector4 v) {
       return new Vector4(v.X, v.W, v.Z, v.Y);
    }
    public static Vector4 YXZW(this Vector4 v) {
       return new Vector4(v.Y, v.X, v.Z, v.W);
    }
    public static Vector4 GRBA(this Vector4 v) {
       return new Vector4(v.Y, v.X, v.Z, v.W);
    }
    public static Vector4 YXWZ(this Vector4 v) {
       return new Vector4(v.Y, v.X, v.W, v.Z);
    }
    public static Vector4 GRAB(this Vector4 v) {
       return new Vector4(v.Y, v.X, v.W, v.Z);
    }
    public static Vector4 YZXW(this Vector4 v) {
       return new Vector4(v.Y, v.Z, v.X, v.W);
    }
    public static Vector4 GBRA(this Vector4 v) {
       return new Vector4(v.Y, v.Z, v.X, v.W);
    }
    public static Vector4 YZWX(this Vector4 v) {
       return new Vector4(v.Y, v.Z, v.W, v.X);
    }
    public static Vector4 GBAR(this Vector4 v) {
       return new Vector4(v.Y, v.Z, v.W, v.X);
    }
    public static Vector4 YWXZ(this Vector4 v) {
       return new Vector4(v.Y, v.W, v.X, v.Z);
    }
    public static Vector4 GARB(this Vector4 v) {
       return new Vector4(v.Y, v.W, v.X, v.Z);
    }
    public static Vector4 YWZX(this Vector4 v) {
       return new Vector4(v.Y, v.W, v.Z, v.X);
    }
    public static Vector4 GABR(this Vector4 v) {
       return new Vector4(v.Y, v.W, v.Z, v.X);
    }
    public static Vector4 ZXYW(this Vector4 v) {
       return new Vector4(v.Z, v.X, v.Y, v.W);
    }
    public static Vector4 BRGA(this Vector4 v) {
       return new Vector4(v.Z, v.X, v.Y, v.W);
    }
    public static Vector4 ZXWY(this Vector4 v) {
       return new Vector4(v.Z, v.X, v.W, v.Y);
    }
    public static Vector4 BRAG(this Vector4 v) {
       return new Vector4(v.Z, v.X, v.W, v.Y);
    }
    public static Vector4 ZYXW(this Vector4 v) {
       return new Vector4(v.Z, v.Y, v.X, v.W);
    }
    public static Vector4 BGRA(this Vector4 v) {
       return new Vector4(v.Z, v.Y, v.X, v.W);
    }
    public static Vector4 ZYWX(this Vector4 v) {
       return new Vector4(v.Z, v.Y, v.W, v.X);
    }
    public static Vector4 BGAR(this Vector4 v) {
       return new Vector4(v.Z, v.Y, v.W, v.X);
    }
    public static Vector4 ZWXY(this Vector4 v) {
       return new Vector4(v.Z, v.W, v.X, v.Y);
    }
    public static Vector4 BARG(this Vector4 v) {
       return new Vector4(v.Z, v.W, v.X, v.Y);
    }
    public static Vector4 ZWYX(this Vector4 v) {
       return new Vector4(v.Z, v.W, v.Y, v.X);
    }
    public static Vector4 BAGR(this Vector4 v) {
       return new Vector4(v.Z, v.W, v.Y, v.X);
    }
    public static Vector4 WXYZ(this Vector4 v) {
       return new Vector4(v.W, v.X, v.Y, v.Z);
    }
    public static Vector4 ARGB(this Vector4 v) {
       return new Vector4(v.W, v.X, v.Y, v.Z);
    }
    public static Vector4 WXZY(this Vector4 v) {
       return new Vector4(v.W, v.X, v.Z, v.Y);
    }
    public static Vector4 ARBG(this Vector4 v) {
       return new Vector4(v.W, v.X, v.Z, v.Y);
    }
    public static Vector4 WYXZ(this Vector4 v) {
       return new Vector4(v.W, v.Y, v.X, v.Z);
    }
    public static Vector4 AGRB(this Vector4 v) {
       return new Vector4(v.W, v.Y, v.X, v.Z);
    }
    public static Vector4 WYZX(this Vector4 v) {
       return new Vector4(v.W, v.Y, v.Z, v.X);
    }
    public static Vector4 AGBR(this Vector4 v) {
       return new Vector4(v.W, v.Y, v.Z, v.X);
    }
    public static Vector4 WZXY(this Vector4 v) {
       return new Vector4(v.W, v.Z, v.X, v.Y);
    }
    public static Vector4 ABRG(this Vector4 v) {
       return new Vector4(v.W, v.Z, v.X, v.Y);
    }
    public static Vector4 WZYX(this Vector4 v) {
       return new Vector4(v.W, v.Z, v.Y, v.X);
    }
    public static Vector4 ABGR(this Vector4 v) {
       return new Vector4(v.W, v.Z, v.Y, v.X);
    }
    #endregion
}