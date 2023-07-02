using System.Text;

namespace JWT_Implementation.Helpers;

public static class CodeGenerator
{
    private const int CodeLength = 6;
    
    public static string GenerateCode()
    {
        Random random = new Random();
        StringBuilder codeBuilder = new StringBuilder(CodeLength);
        
        for (int i = 0; i < CodeLength; i++)
        {
            codeBuilder.Append(random.Next(0, 10));
        }

        return codeBuilder.ToString();
    }
}
