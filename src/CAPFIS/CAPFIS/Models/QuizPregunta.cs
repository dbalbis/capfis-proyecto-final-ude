using System.Collections.Generic;

namespace CAPFIS.Models
{
    public class QuizPregunta
    {
        public string Pregunta { get; set; } = "";
        public List<string> Respuestas { get; set; } = new List<string>();
        public int Correcta { get; set; }
    }
}