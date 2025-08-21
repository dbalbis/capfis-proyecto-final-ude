using System.Text.RegularExpressions;
using System.Web;
using Ganss.Xss;

namespace CAPFIS.Utils
{
    public static class InputSanitizer
    {
        /// <summary>
        /// Limpia un string de posibles etiquetas HTML y caracteres peligrosos.
        /// Permite letras, números, espacios, signos básicos de puntuación y emojis.
        /// </summary>
        public static string SanitizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Decodificar entidades HTML por seguridad
            input = HttpUtility.HtmlDecode(input);

            // Eliminar etiquetas HTML
            input = Regex.Replace(input, "<.*?>", string.Empty);

            // Eliminar caracteres no imprimibles
            input = Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);

            // Eliminar comillas y paréntesis que puedan ser usados en XSS
            input = input.Replace("\"", "").Replace("'", "").Replace("(", "").Replace(")", "");

            // Eliminar múltiples espacios
            input = Regex.Replace(input, @"\s+", " ");

            return input.Trim();
        }

        /// <summary>
        /// Sanitiza URL, eliminando espacios al inicio/final y caracteres extraños.
        /// </summary>
        public static string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            url = url.Trim();

            // Opcional: eliminar caracteres no válidos para URL
            url = Regex.Replace(url, @"[^\w\-.:/?&=#]", string.Empty);

            return url;
        }

        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("b");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("u");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedAttributes.Add("target");

            return sanitizer.Sanitize(html);
        }
    }
}