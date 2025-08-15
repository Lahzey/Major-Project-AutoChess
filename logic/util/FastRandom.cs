namespace MPAutoChess.logic.util;

public class FastRandom {
    
    private static uint seed = 123456789; // can be anything
    
    public static float FastRandomFloat() {
        // Xorshift (very fast PRNG)
        seed ^= seed << 13;
        seed ^= seed >> 17;
        seed ^= seed << 5;

        // Convert to float in range [0,1)
        return (seed & 0xFFFFFF) / (float)0x1000000;
    }
}