using System.Text;

namespace MPAutoChess.logic.util;

public class StringUtil {
    
    public static string PascalToReadable(string pascalCase) {
        if (string.IsNullOrEmpty(pascalCase)) return pascalCase;
        
        StringBuilder result = new StringBuilder();
        result.Append(pascalCase[0]);
        for (int i = 1; i < pascalCase.Length; i++) {
            char currentChar = pascalCase[i];
            if (char.IsUpper(currentChar) && char.IsLower(pascalCase[i - 1])) {
                result.Append(' ');
            }
            result.Append(currentChar);
        }
        return result.ToString();
    }
    
}