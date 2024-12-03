using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Solutia.Helpers
{
    public static class FileHelper
    {
        public static (string comentario, double diametro, string pavimento, List<(double X, double Y, double Z)> pontos)[] ProcessarArquivoTubulacao(string filtro = "Text Files (*.txt)|*.txt", string titulo = "Selecione o arquivo que contém as coordenadas de tubulação")
        {
            OpenFileDialog selecionaArquivo = new OpenFileDialog
            {
                Filter = filtro,
                Title = titulo
            };

            if (selecionaArquivo.ShowDialog() != DialogResult.OK)
            {
                throw new OperationCanceledException("Seleção de arquivo cancelada pelo usuário.");
            }

            string filePath = selecionaArquivo.FileName;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("O arquivo selecionado não foi encontrado.");
            }

            string[] segmentosTubos = File.ReadAllLines(filePath);
            double pe = 3.2808398950; // Conversão para pés
            var resultados = new List<(string comentario, double diametro, string pavimento, List<(double X, double Y, double Z)> pontos)>();

            foreach (string segmentoTubo in segmentosTubos)
            {
                string[] segmentoInfo = segmentoTubo.Split('/');
                if (segmentoInfo.Length < 4)
                {
                    throw new Exception("Formato de dados inválido: " + segmentoTubo);
                }

                string comentario = segmentoInfo[0].Trim();
                double diametro = Convert.ToDouble(segmentoInfo[1].Trim());
                string pavimento = segmentoInfo[2].Trim();

                string[] coordenadasSegmento = segmentoInfo[3].Split(';');
                var pontos = new List<(double X, double Y, double Z)>();

                foreach (string segmento in coordenadasSegmento)
                {
                    string[] coordenadas = segmento.Split(' ');
                    if (coordenadas.Length == 3)
                    {
                        double x = Convert.ToDouble(coordenadas[0].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                        double y = Convert.ToDouble(coordenadas[1].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                        double z = Convert.ToDouble(coordenadas[2].Replace(',', '.'), CultureInfo.InvariantCulture) * pe;
                        pontos.Add((x, y, z));
                    }
                }

                resultados.Add((comentario, diametro, pavimento, pontos));
            }

            return resultados.ToArray();
        }
    }
}
