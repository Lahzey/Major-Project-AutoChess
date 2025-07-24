using Godot;

namespace MPAutoChess.logic.core.util;

public static class VectorExtensions {

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
    public static Vector3 Extend(this Vector2 v, float z) {
        return new Vector3(v.X, v.Y, z);
    }
    public static Vector4 Extend(this Vector2 v, float z, float w) {
        return new Vector4(v.X, v.Y, z, w);
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
    
    public static Vector2 Xy(this Vector2 v) {
        return new Vector2(v.X, v.Y);
    }
    public static Vector2 Rg(this Vector2 v) {
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
    public static Vector4 Extend(this Vector3 v, float w) {
        return new Vector4(v.X, v.Y, v.Z, w);
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
    
    public static Vector2 Xy(this Vector3 v) {
        return new Vector2(v.X, v.Y);
    }
    public static Vector2 RG(this Vector3 v) {
        return new Vector2(v.X, v.Y);
    }
    public static Vector2 Xz(this Vector3 v) {
        return new Vector2(v.X, v.Z);
    }
    public static Vector2 Rb(this Vector3 v) {
        return new Vector2(v.X, v.Z);
    }
    public static Vector2 YX(this Vector3 v) {
        return new Vector2(v.Y, v.X);
    }
    public static Vector2 Gr(this Vector3 v) {
        return new Vector2(v.Y, v.X);
    }
    public static Vector2 Yz(this Vector3 v) {
        return new Vector2(v.Y, v.Z);
    }
    public static Vector2 Gb(this Vector3 v) {
        return new Vector2(v.Y, v.Z);
    }
    public static Vector2 Zx(this Vector3 v) {
        return new Vector2(v.Z, v.X);
    }
    public static Vector2 Br(this Vector3 v) {
        return new Vector2(v.Z, v.X);
    }
    public static Vector2 Zy(this Vector3 v) {
        return new Vector2(v.Z, v.Y);
    }
    public static Vector2 Bg(this Vector3 v) {
        return new Vector2(v.Z, v.Y);
    }
    public static Vector3 Xyz(this Vector3 v) {
        return new Vector3(v.X, v.Y, v.Z);
    }
    
    public static Vector3 Rgb(this Vector3 v) {
        return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 Xzy(this Vector3 v) {
        return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 Rbg(this Vector3 v) {
        return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 YXZ(this Vector3 v) {
        return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 Grb(this Vector3 v) {
        return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 YZX(this Vector3 v) {
        return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 Gbr(this Vector3 v) {
        return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 Zxy(this Vector3 v) {
        return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 BRG(this Vector3 v) {
        return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 ZYX(this Vector3 v) {
        return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 Bgr(this Vector3 v) {
        return new Vector3(v.Z, v.Y, v.X);
    }
    #endregion
    #region Vector4
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
    public static Vector2 Rg(this Vector4 v) {
        return new Vector2(v.X, v.Y);
    }
    public static Vector2 XZ(this Vector4 v) {
        return new Vector2(v.X, v.Z);
    }
    public static Vector2 Rb(this Vector4 v) {
        return new Vector2(v.X, v.Z);
    }
    public static Vector2 Xw(this Vector4 v) {
        return new Vector2(v.X, v.W);
    }
    public static Vector2 Ra(this Vector4 v) {
        return new Vector2(v.X, v.W);
    }
    public static Vector2 Yx(this Vector4 v) {
        return new Vector2(v.Y, v.X);
    }
    public static Vector2 Gr(this Vector4 v) {
        return new Vector2(v.Y, v.X);
    }
    public static Vector2 Yz(this Vector4 v) {
        return new Vector2(v.Y, v.Z);
    }
    public static Vector2 Gb(this Vector4 v) {
        return new Vector2(v.Y, v.Z);
    }
    public static Vector2 Yw(this Vector4 v) {
        return new Vector2(v.Y, v.W);
    }
    public static Vector2 Ga(this Vector4 v) {
        return new Vector2(v.Y, v.W);
    }
    public static Vector2 Zx(this Vector4 v) {
        return new Vector2(v.Z, v.X);
    }
    public static Vector2 Br(this Vector4 v) {
        return new Vector2(v.Z, v.X);
    }
    public static Vector2 Zy(this Vector4 v) {
        return new Vector2(v.Z, v.Y);
    }
    public static Vector2 Bg(this Vector4 v) {
        return new Vector2(v.Z, v.Y);
    }
    public static Vector2 Zw(this Vector4 v) {
        return new Vector2(v.Z, v.W);
    }
    public static Vector2 Ba(this Vector4 v) {
        return new Vector2(v.Z, v.W);
    }
    public static Vector2 Wx(this Vector4 v) {
        return new Vector2(v.W, v.X);
    }
    public static Vector2 Ar(this Vector4 v) {
        return new Vector2(v.W, v.X);
    }
    public static Vector2 Wy(this Vector4 v) {
        return new Vector2(v.W, v.Y);
    }
    public static Vector2 Ag(this Vector4 v) {
        return new Vector2(v.W, v.Y);
    }
    public static Vector2 Wz(this Vector4 v) {
        return new Vector2(v.W, v.Z);
    }
    public static Vector2 Ab(this Vector4 v) {
        return new Vector2(v.W, v.Z);
    }
    
    public static Vector3 Xyz(this Vector4 v) {
        return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 Rgb(this Vector4 v) {
        return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3 Xyw(this Vector4 v) {
        return new Vector3(v.X, v.Y, v.W);
    }
    public static Vector3 Rga(this Vector4 v) {
        return new Vector3(v.X, v.Y, v.W);
    }
    public static Vector3 Xzy(this Vector4 v) {
        return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 Rbg(this Vector4 v) {
        return new Vector3(v.X, v.Z, v.Y);
    }
    public static Vector3 XZW(this Vector4 v) {
        return new Vector3(v.X, v.Z, v.W);
    }
    public static Vector3 Rba(this Vector4 v) {
        return new Vector3(v.X, v.Z, v.W);
    }
    public static Vector3 Xwy(this Vector4 v) {
        return new Vector3(v.X, v.W, v.Y);
    }
    public static Vector3 RAG(this Vector4 v) {
        return new Vector3(v.X, v.W, v.Y);
    }
    public static Vector3 Xwz(this Vector4 v) {
        return new Vector3(v.X, v.W, v.Z);
    }
    public static Vector3 Rab(this Vector4 v) {
        return new Vector3(v.X, v.W, v.Z);
    }
    public static Vector3 Yxz(this Vector4 v) {
        return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 Grb(this Vector4 v) {
        return new Vector3(v.Y, v.X, v.Z);
    }
    public static Vector3 YXW(this Vector4 v) {
        return new Vector3(v.Y, v.X, v.W);
    }
    public static Vector3 Gra(this Vector4 v) {
        return new Vector3(v.Y, v.X, v.W);
    }
    public static Vector3 Yzx(this Vector4 v) {
        return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 Gbr(this Vector4 v) {
        return new Vector3(v.Y, v.Z, v.X);
    }
    public static Vector3 YZW(this Vector4 v) {
        return new Vector3(v.Y, v.Z, v.W);
    }
    public static Vector3 Gba(this Vector4 v) {
        return new Vector3(v.Y, v.Z, v.W);
    }
    public static Vector3 Ywx(this Vector4 v) {
        return new Vector3(v.Y, v.W, v.X);
    }
    public static Vector3 Gar(this Vector4 v) {
        return new Vector3(v.Y, v.W, v.X);
    }
    public static Vector3 Ywz(this Vector4 v) {
        return new Vector3(v.Y, v.W, v.Z);
    }
    public static Vector3 GAB(this Vector4 v) {
        return new Vector3(v.Y, v.W, v.Z);
    }
    public static Vector3 Zxy(this Vector4 v) {
        return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 Brg(this Vector4 v) {
        return new Vector3(v.Z, v.X, v.Y);
    }
    public static Vector3 Zxw(this Vector4 v) {
        return new Vector3(v.Z, v.X, v.W);
    }
    public static Vector3 Bra(this Vector4 v) {
        return new Vector3(v.Z, v.X, v.W);
    }
    public static Vector3 Zyx(this Vector4 v) {
        return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 Bgr(this Vector4 v) {
        return new Vector3(v.Z, v.Y, v.X);
    }
    public static Vector3 Zyw(this Vector4 v) {
        return new Vector3(v.Z, v.Y, v.W);
    }
    public static Vector3 Bga(this Vector4 v) {
        return new Vector3(v.Z, v.Y, v.W);
    }
    public static Vector3 Zwx(this Vector4 v) {
        return new Vector3(v.Z, v.W, v.X);
    }
    public static Vector3 Bar(this Vector4 v) {
        return new Vector3(v.Z, v.W, v.X);
    }
    public static Vector3 ZWY(this Vector4 v) {
        return new Vector3(v.Z, v.W, v.Y);
    }
    public static Vector3 Bag(this Vector4 v) {
        return new Vector3(v.Z, v.W, v.Y);
    }
    public static Vector3 Wxy(this Vector4 v) {
        return new Vector3(v.W, v.X, v.Y);
    }
    public static Vector3 Arg(this Vector4 v) {
        return new Vector3(v.W, v.X, v.Y);
    }
    public static Vector3 Wxz(this Vector4 v) {
        return new Vector3(v.W, v.X, v.Z);
    }
    public static Vector3 Arb(this Vector4 v) {
        return new Vector3(v.W, v.X, v.Z);
    }
    public static Vector3 Wyx(this Vector4 v) {
        return new Vector3(v.W, v.Y, v.X);
    }
    public static Vector3 AGR(this Vector4 v) {
        return new Vector3(v.W, v.Y, v.X);
    }
    public static Vector3 Wyz(this Vector4 v) {
        return new Vector3(v.W, v.Y, v.Z);
    }
    public static Vector3 Agb(this Vector4 v) {
        return new Vector3(v.W, v.Y, v.Z);
    }
    public static Vector3 Wzx(this Vector4 v) {
        return new Vector3(v.W, v.Z, v.X);
    }
    public static Vector3 Abr(this Vector4 v) {
        return new Vector3(v.W, v.Z, v.X);
    }
    public static Vector3 WZY(this Vector4 v) {
        return new Vector3(v.W, v.Z, v.Y);
    }
    public static Vector3 Abg(this Vector4 v) {
        return new Vector3(v.W, v.Z, v.Y);
    }
    
    public static Vector4 XYZW(this Vector4 v) {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }
    public static Vector4 Rgba(this Vector4 v) {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }
    public static Vector4 Xywz(this Vector4 v) {
        return new Vector4(v.X, v.Y, v.W, v.Z);
    }
    public static Vector4 Rgab(this Vector4 v) {
        return new Vector4(v.X, v.Y, v.W, v.Z);
    }
    public static Vector4 XZYW(this Vector4 v) {
        return new Vector4(v.X, v.Z, v.Y, v.W);
    }
    public static Vector4 Rbga(this Vector4 v) {
        return new Vector4(v.X, v.Z, v.Y, v.W);
    }
    public static Vector4 Xzwy(this Vector4 v) {
        return new Vector4(v.X, v.Z, v.W, v.Y);
    }
    public static Vector4 Rbag(this Vector4 v) {
        return new Vector4(v.X, v.Z, v.W, v.Y);
    }
    public static Vector4 Xwyz(this Vector4 v) {
        return new Vector4(v.X, v.W, v.Y, v.Z);
    }
    public static Vector4 Ragb(this Vector4 v) {
        return new Vector4(v.X, v.W, v.Y, v.Z);
    }
    public static Vector4 XWZY(this Vector4 v) {
        return new Vector4(v.X, v.W, v.Z, v.Y);
    }
    public static Vector4 Rabg(this Vector4 v) {
        return new Vector4(v.X, v.W, v.Z, v.Y);
    }
    public static Vector4 Yxzw(this Vector4 v) {
        return new Vector4(v.Y, v.X, v.Z, v.W);
    }
    public static Vector4 GRBA(this Vector4 v) {
        return new Vector4(v.Y, v.X, v.Z, v.W);
    }
    public static Vector4 Yxwz(this Vector4 v) {
        return new Vector4(v.Y, v.X, v.W, v.Z);
    }
    public static Vector4 Grab(this Vector4 v) {
        return new Vector4(v.Y, v.X, v.W, v.Z);
    }
    public static Vector4 Yzxw(this Vector4 v) {
        return new Vector4(v.Y, v.Z, v.X, v.W);
    }
    public static Vector4 Gbra(this Vector4 v) {
        return new Vector4(v.Y, v.Z, v.X, v.W);
    }
    public static Vector4 Yzwx(this Vector4 v) {
        return new Vector4(v.Y, v.Z, v.W, v.X);
    }
    public static Vector4 Gbar(this Vector4 v) {
        return new Vector4(v.Y, v.Z, v.W, v.X);
    }
    public static Vector4 Ywxz(this Vector4 v) {
        return new Vector4(v.Y, v.W, v.X, v.Z);
    }
    public static Vector4 Garb(this Vector4 v) {
        return new Vector4(v.Y, v.W, v.X, v.Z);
    }
    public static Vector4 Ywzx(this Vector4 v) {
        return new Vector4(v.Y, v.W, v.Z, v.X);
    }
    public static Vector4 Gabr(this Vector4 v) {
        return new Vector4(v.Y, v.W, v.Z, v.X);
    }
    public static Vector4 Zxyw(this Vector4 v) {
        return new Vector4(v.Z, v.X, v.Y, v.W);
    }
    public static Vector4 Brga(this Vector4 v) {
        return new Vector4(v.Z, v.X, v.Y, v.W);
    }
    public static Vector4 Zxwy(this Vector4 v) {
        return new Vector4(v.Z, v.X, v.W, v.Y);
    }
    public static Vector4 BRAG(this Vector4 v) {
        return new Vector4(v.Z, v.X, v.W, v.Y);
    }
    public static Vector4 Zyxw(this Vector4 v) {
        return new Vector4(v.Z, v.Y, v.X, v.W);
    }
    public static Vector4 BGRA(this Vector4 v) {
        return new Vector4(v.Z, v.Y, v.X, v.W);
    }
    public static Vector4 Zywx(this Vector4 v) {
        return new Vector4(v.Z, v.Y, v.W, v.X);
    }
    public static Vector4 Bgar(this Vector4 v) {
        return new Vector4(v.Z, v.Y, v.W, v.X);
    }
    public static Vector4 Zwxy(this Vector4 v) {
        return new Vector4(v.Z, v.W, v.X, v.Y);
    }
    public static Vector4 Barg(this Vector4 v) {
        return new Vector4(v.Z, v.W, v.X, v.Y);
    }
    public static Vector4 ZWYX(this Vector4 v) {
        return new Vector4(v.Z, v.W, v.Y, v.X);
    }
    public static Vector4 Bagr(this Vector4 v) {
        return new Vector4(v.Z, v.W, v.Y, v.X);
    }
    public static Vector4 Wxyz(this Vector4 v) {
        return new Vector4(v.W, v.X, v.Y, v.Z);
    }
    public static Vector4 ARGB(this Vector4 v) {
        return new Vector4(v.W, v.X, v.Y, v.Z);
    }
    public static Vector4 Wxzy(this Vector4 v) {
        return new Vector4(v.W, v.X, v.Z, v.Y);
    }
    public static Vector4 Arbg(this Vector4 v) {
        return new Vector4(v.W, v.X, v.Z, v.Y);
    }
    public static Vector4 WYXZ(this Vector4 v) {
        return new Vector4(v.W, v.Y, v.X, v.Z);
    }
    public static Vector4 Agrb(this Vector4 v) {
        return new Vector4(v.W, v.Y, v.X, v.Z);
    }
    public static Vector4 Wyzx(this Vector4 v) {
        return new Vector4(v.W, v.Y, v.Z, v.X);
    }
    public static Vector4 Agbr(this Vector4 v) {
        return new Vector4(v.W, v.Y, v.Z, v.X);
    }
    public static Vector4 WZXY(this Vector4 v) {
        return new Vector4(v.W, v.Z, v.X, v.Y);
    }
    public static Vector4 Abrg(this Vector4 v) {
        return new Vector4(v.W, v.Z, v.X, v.Y);
    }
    public static Vector4 Wzyx(this Vector4 v) {
        return new Vector4(v.W, v.Z, v.Y, v.X);
    }
    public static Vector4 Abgr(this Vector4 v) {
        return new Vector4(v.W, v.Z, v.Y, v.X);
    }
    #endregion
}