using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sambar;

public static class Extensions
{
    public static bool ContainsFlag(this uint flag, uint flagToCheck)
    {
       if((flag & flagToCheck) != 0)
       {
            return true;
       }
        return false;
    } 
}
