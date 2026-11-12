using System.Net;
using System.Text.RegularExpressions;

namespace ProgIIICazador.Web.Modules.CapturaOfertas.Extractors;

/// <summary>
/// Divide páginas oficiales que publican varias bases y condiciones en candidatos independientes.
/// Solo separa encabezados explícitos y conserva el texto fuente para su revisión.
/// </summary>
public static class SeparadorSeccionesPromocionales
{
    private static readonly Regex Encabezado = new(
        @"^(?:PROMOCIÓN|BASES Y CONDICIONES\s*:\s*ACCIÓN PROMOCIONAL)\b.*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EvidenciaBeneficio = new(
        @"\b(descuento|reintegro|ahorro|beneficio|cup[oó]n|2\s*x\s*1|gratis|bonificaci[oó]n|precio especial)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<SeccionPromocional> Separar(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return [];

        // Los cierres de bloques se convierten en saltos de línea antes de quitar etiquetas.
        // Así se conservan los títulos de las bases, incluso cuando el HTML está minificado.
        var conSaltos = Regex.Replace(
            html,
            @"</(?:p|div|h[1-6]|li|section|article|header|footer|tr)\s*>|<br\s*/?>",
            "\n",
            RegexOptions.IgnoreCase);
        conSaltos = Regex.Replace(conSaltos, @"<(script|style|noscript)[^>]*>.*?</\1>", " ",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var sinEtiquetas = WebUtility.HtmlDecode(Regex.Replace(conSaltos, "<[^>]+>", " "));
        var lineas = sinEtiquetas
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim())
            .Where(x => x.Length > 0)
            .ToList();

        var indices = lineas
            .Select((linea, indice) => (linea, indice))
            .Where(x => Encabezado.IsMatch(x.linea))
            .Select(x => x.indice)
            .ToList();

        if (indices.Count == 0) return [];

        var secciones = new List<SeccionPromocional>();
        for (var i = 0; i < indices.Count; i++)
        {
            var inicio = indices[i];
            var fin = i + 1 < indices.Count ? indices[i + 1] : lineas.Count;
            var bloque = string.Join(Environment.NewLine, lineas.Skip(inicio).Take(fin - inicio));
            if (!EvidenciaBeneficio.IsMatch(bloque)) continue;

            var titulo = lineas[inicio].Trim();
            secciones.Add(new SeccionPromocional(
                titulo.Length > 300 ? titulo[..300] : titulo,
                bloque.Length > 12000 ? bloque[..12000] : bloque));
        }

        return secciones;
    }
}

public sealed record SeccionPromocional(string Titulo, string Texto);
