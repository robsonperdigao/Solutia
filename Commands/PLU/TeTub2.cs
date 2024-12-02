using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Linq;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.PLU
{
    [Transaction(TransactionMode.Manual)]
    public class TeTub2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtém o documento atual
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;

            // Obtém a lista de todas as tubulações visíveis na vista atual
            List<Pipe> tubulacoes = TubulacoesVistaAtual(doc, view);
            double margem = 0.01; // Margem padrão de 1 unidade

            // Coleta todos os conectores não conectados
            List<Connector> conectoresNaoConectados = new List<Connector>();
            foreach (var tubo in tubulacoes)
            {
                ConnectorSet conectores = tubo.ConnectorManager.Connectors;
                foreach (Connector conector in conectores)
                {
                    if (!conector.IsConnected)
                    {
                        conectoresNaoConectados.Add(conector);
                    }
                }
            }

            // Identifica combinações válidas de conectores para um tee
            List<(Connector, Connector, Connector)> combinacoesTees = IdentificarTees(conectoresNaoConectados, margem);

            int teesCriados = 0;
            int falhas = 0;

            // Mantém um controle dos conectores já usados
            HashSet<Connector> conectoresUsados = new HashSet<Connector>();

            using (Transaction trans = new Transaction(doc, "Conecta Tubulações com Tês"))
            {
                trans.Start();

                foreach (var (connector1, connector2, connector3) in combinacoesTees)
                {
                    if (conectoresUsados.Contains(connector1) || conectoresUsados.Contains(connector2) || conectoresUsados.Contains(connector3))
                    {
                        continue; // Ignora se os conectores já foram usados
                    }

                    try
                    {
                        // Insere o tee
                        doc.Create.NewTeeFitting(connector1, connector2, connector3);
                        teesCriados++;

                        // Marca os conectores como usados
                        conectoresUsados.Add(connector1);
                        conectoresUsados.Add(connector2);
                        conectoresUsados.Add(connector3);
                    }
                    catch
                    {
                        falhas++;
                    }
                }

                trans.Commit();
            }

            // Exibe um resumo ao final do processo
            TaskDialog.Show("Resumo do Processo",
                $"Foram criados {teesCriados} Tês\n{falhas} conexões não puderam ser inseridas com Tês");

            return Result.Succeeded;
        }

        private List<(Connector, Connector, Connector)> IdentificarTees(List<Connector> conectores, double margem)
        {
            var tees = new List<(Connector, Connector, Connector)>();

            // Loop para verificar todos os grupos de três conectores
            for (int i = 0; i < conectores.Count; i++)
            {
                for (int j = i + 1; j < conectores.Count; j++)
                {
                    for (int k = j + 1; k < conectores.Count; k++)
                    {
                        var c1 = conectores[i];
                        var c2 = conectores[j];
                        var c3 = conectores[k];

                        // Verifica se os três conectores estão dentro da margem de distância
                        if (EstaoDentroDaMargem(c1, c2, c3, margem))
                        {
                            // Verifica se os três conectores estão alinhados de forma que formem um Tê
                            if (EstaoAlinhadosParaTe(c1, c2, c3))
                            {
                                // Evita duplicação verificando se já há um Tê nas mesmas coordenadas
                                if (!ExisteTeDuplicado(c1, c2, c3))
                                {
                                    tees.Add((c1, c2, c3));
                                }
                            }
                        }
                    }
                }
            }

            return tees;
        }

        private bool ExisteTeDuplicado(Connector c1, Connector c2, Connector c3)
        {
            // Verifica se já existe um Tê nas mesmas coordenadas dos conectores
            var teesExistentes = new List<(XYZ, XYZ, XYZ)>();

            // Exemplo simples de verificação de duplicação, pode ser expandido conforme necessário
            foreach (var tee in teesExistentes)
            {
                if (TeeJaExiste(c1.Origin, c2.Origin, c3.Origin, tee))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EstaoDentroDaMargem(Connector c1, Connector c2, Connector c3, double margem)
        {
            // Verifica a distância entre todos os pares de conectores
            bool dentroDaMargem1 = c1.Origin.DistanceTo(c2.Origin) <= margem;
            bool dentroDaMargem2 = c1.Origin.DistanceTo(c3.Origin) <= margem;
            bool dentroDaMargem3 = c2.Origin.DistanceTo(c3.Origin) <= margem;

            // Retorna verdadeiro se todos os pares estão dentro da margem
            return dentroDaMargem1 && dentroDaMargem2 && dentroDaMargem3;
        }

        private bool EstaoAlinhadosParaTe(Connector c1, Connector c2, Connector c3)
        {
            // Calcula os vetores entre os conectores
            XYZ vetor1 = c2.Origin - c1.Origin;
            XYZ vetor2 = c3.Origin - c1.Origin;
            XYZ vetor3 = c3.Origin - c2.Origin;

            // Verifica se os conectores formam um ângulo de 90 graus entre eles (Tê perpendicular)
            double angulo1 = vetor1.AngleTo(vetor2);
            double angulo2 = vetor1.AngleTo(vetor3);
            double angulo3 = vetor2.AngleTo(vetor3);

            // Permite que os conectores formem um Tê se o ângulo for 90 graus ou alinhado
            bool alinhado90 = Math.Abs(angulo1 - Math.PI / 2) < 0.01 || Math.Abs(angulo2 - Math.PI / 2) < 0.01 || Math.Abs(angulo3 - Math.PI / 2) < 0.01;
            bool alinhado = Math.Abs(angulo1) < 0.01 || Math.Abs(angulo2) < 0.01 || Math.Abs(angulo3) < 0.01;

            // Ignora diâmetros e apenas verifica o alinhamento
            return alinhado90 || alinhado;
        }



        private bool TeeJaExiste(XYZ origem1, XYZ origem2, XYZ origem3, (XYZ, XYZ, XYZ) teeExistente)
        {
            return origem1.IsAlmostEqualTo(teeExistente.Item1) &&
                   origem2.IsAlmostEqualTo(teeExistente.Item2) &&
                   origem3.IsAlmostEqualTo(teeExistente.Item3);
        }

        private List<Pipe> TubulacoesVistaAtual(Document doc, View view)
        {
            // Coleta todas as tubulações visíveis na vista atual
            FilteredElementCollector filtro = new FilteredElementCollector(doc, view.Id);
            ICollection<Element> elementos = filtro.OfClass(typeof(Pipe)).ToElements();
            return elementos.Cast<Pipe>().ToList();
        }
    }
}
