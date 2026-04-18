using System.Text;
using UnityEngine;

public static class CategoryFormatter
{
    public static string ToDisplayName(ProductCategory category)
    {
        string raw = category.ToString();

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < raw.Length; i++)
        {
            char c = raw[i];

            if (i > 0 && char.IsUpper(c))
                result.Append(" ");

            result.Append(c);
        }

        return ApplyAccentFix(result.ToString());
    }

    private static string ApplyAccentFix(string text)
    {
        // Ajustes específicos do português
        return text
            .Replace("Maquina", "Máquina")
            .Replace("Tesoura", "Tesoura")
            .Replace("Pente", "Pente")
            .Replace("Navalha", "Navalha")
            .Replace("Secador", "Secador")
            .Replace("Produto Capilar", "Produto Capilar")
            .Replace("Decoracao", "Decoração")
            .Replace("Mobilia", "Mobília");
    }
}