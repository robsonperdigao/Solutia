using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.Attributes;

namespace Solutia.Commands.ELE
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    // Classe do comando Fiac
    public class Fiac : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obter a aplicação e o documento do Revit
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Selecione um ou mais eletrodutos/conduítes");

            Element element = doc.GetElement(reference);

            // Verificar se o elemento é um conduite
            if (element is Conduit conduit)
            {
                // Obter o gerenciador de conectores do conduite
                ConnectorManager cm = conduit.ConnectorManager;

                // Criar uma lista para armazenar os elementos conectados
                List<Element> connectedElements = new List<Element>();

                // Iterar pelos conectores do conduite
                foreach (Connector conn in cm.Connectors)
                {
                    // Obter o elemento dono do conector conectado
                    Element connectedElem = doc.GetElement(conn.Owner.Id);

                    // Verificar se o elemento é um equipamento ou um dispositivo de iluminação
                    if (connectedElem is FamilyInstance fi &&
                        (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment ||
                            fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures))
                    {
                        // Adicionar o elemento à lista de elementos conectados
                        connectedElements.Add(connectedElem);
                    }

                }

                // Criar uma caixa de diálogo para mostrar os elementos conectados
                TaskDialog td = new TaskDialog("Elementos Conectados");

                // Adicionar um título e uma mensagem principal
                td.TitleAutoPrefix = false;
                td.MainInstruction = "Os seguintes elementos estão conectados ao conduite selecionado:";

                // Adicionar um botão para cada elemento conectado, mostrando o seu nome e o seu tipo
                foreach (Element e in connectedElements)
                {
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + connectedElements.IndexOf(e),
                                      e.Name + "\n" + e.GetType().Name);
                }


                // Mostrar a caixa de diálogo e obter o resultado
                TaskDialogResult tdr = td.Show();



                return Result.Succeeded;
            }

            else
            {
                // Mostrar uma mensagem de erro se o elemento não for um painel elétrico
                //TaskDialog.Show("Erro", "O elemento selecionado não é um painel elétrico válido.");
                return Result.Failed;
            }



        }
    }
}