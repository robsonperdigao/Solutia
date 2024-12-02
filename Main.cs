using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;
using Solutia.Helpers;

namespace Solutia
{
    public class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            //Cria uma nova aba na Ribbon
            string tabName = "Solutia";
            app.CreateRibbonTab(tabName);

            //Cria painel ELE
            string panelNameEle = "Elétrica";
            RibbonPanel ribbonPanelEle = app.CreateRibbonPanel(tabName, panelNameEle);

            //Cria painel HID
            string panelNameHid = "Hidráulica";
            RibbonPanel ribbonPanelHid = app.CreateRibbonPanel(tabName, panelNameHid);

            //Cria painel CLI
            //string panelNameCli = "Climatização";
            //RibbonPanel ribbonPanelCli = app.CreateRibbonPanel(tabName, panelNameCli);

            //Cria painel Geral
            //string panelNameGer = "Geral";
            //RibbonPanel ribbonPanelGer = app.CreateRibbonPanel(tabName, panelNameGer);

            //Cria painel BIM
            string panelNameBim = "BIM";
            RibbonPanel ribbonPanelBim = app.CreateRibbonPanel(tabName, panelNameBim);

            string panelNameTest = "Teste";
            RibbonPanel ribbonPanelTest = app.CreateRibbonPanel(tabName, panelNameTest);


            //ButtonHelper.CreateButtonWithIcon(ribbonPanelEle, "ConduitCAD", "ConduitCAD", "Cria eletrodutos/conduítes horizontais a partir de um desenho CAD", "Solutia.Commands.SHA.CADtoBIM", "Solutia.Resources.buttonConduitL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelEle, "FuroELE", "Furo ELE", "Insere família de furo onde há interferências entre eletrodutos/conduítes e vigas", "Solutia.Commands.SHA.FuroTeste", "Solutia.Resources.buttomFuroL.ico");
            //ButtonHelper.CreateButtonWithIcon(ribbonPanelEle, "AutoPrumELE", "AutoPrum", "Cria eletrodutos/conduítes verticais a partir de um desenho CAD", "Solutia.Commands.AutoPrumELE", "Solutia.Resources.buttonEleL.ico");
            //ButtonHelper.CreateButtonWithIcon(ribbonPanelEle, "Fiac", "Fiac", "Cria uma tabela com as informações dos painéis, circuitos e lança a fiação nos eletrodutos", "Solutia.Commands.Fiac", "Solutia.Resources.buttonFiacL.ico");
            //ButtonHelper.CreateButtonWithIcon(ribbonPanelEle, "Coleta Dados Fiac", "Coleta Fiac", "Cria uma tabela com as informações dos painéis, circuitos e lança a fiação nos eletrodutos", "Solutia.Commands.ColetaDadosFiac", "Solutia.Resources.buttonFiacL.ico");

            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "TubCAD", "TubCAD", "Cria tubulações horizontais a partir de um desenho CAD", "Solutia.Commands.PLU.HIDTub", "Solutia.Resources.buttonTubL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "FuroHID", "Furo HID", "Insere família de furo onde há interferências entre tubulações e vigas", "Solutia.Commands.SHA.Furo", "Solutia.Resources.buttomFuroL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "ConnectTubHID", "Criar Conexões", "Cria conexões", "Solutia.Commands.PLU.ConnectTubHID", "Solutia.Resources.buttonHidL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "Tê", "Criar Tê", "Tê", "Solutia.Commands.PLU.TeTub2", "Solutia.Resources.buttonHidL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "Interseções", "Interseções", "Interseções", "Solutia.Commands.SHA.IntersectTub", "Solutia.Resources.buttonHidL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelHid, "TÊÊÊÊ", "TÊÊÊÊ", "TÊÊÊÊ", "Solutia.Commands.PLU.InsertTe", "Solutia.Resources.buttonHidL.ico");

            ButtonHelper.CreateButtonWithIcon(ribbonPanelBim, "Furação", "Furação", "Analisa interferências com elementos estruturais e insere a furação", "Solutia.Commands.SHA.AutoFuro", "Solutia.Resources.buttomFuroL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelBim, "Exporta RVT", "Exporta RVT", "Apaga todas as vistas e solicita ao usuário para salvar o arquivo", "Solutia.Commands.GEN.ExportRVT", "Solutia.Resources.buttonSaveL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelBim, "Exporta IFC", "Exporta IFC", "Apaga todas as vistas e exporta IFC", "Solutia.Commands.GEN.ExportIFC", "Solutia.Resources.buttonSaveL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelBim, "Exporta Famílias", "Exporta Famílias", "Exporta todas as famílias do modelo/template para a pasta selecionada pelo usuário", "Solutia.Commands.GEN.ExportFamilies", "Solutia.Resources.buttonSaveL.ico");

            ButtonHelper.CreateButtonWithIcon(ribbonPanelTest, "FuroTeste", "FuroTeste", "FuroTeste", "Solutia.Commands.SHA.FuroTeste", "Solutia.Resources.buttomFuroL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelTest, "FuroELE", "FuroELE", "FuroELE", "Solutia.Commands.SHA.FuroELE", "Solutia.Resources.buttomFuroL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelTest, "AutoFuro", "AutoFuro", "AutoFuro", "Solutia.Commands.SHA.AutoFuro", "Solutia.Resources.buttomFuroL.ico");
            ButtonHelper.CreateButtonWithIcon(ribbonPanelTest, "AutoFuroOrig", "AutoFuroOrig", "AutoFuroOrig", "Solutia.Commands.SHA.AutoFuroOrig", "Solutia.Resources.buttomFuroL.ico");

            // Botão Rotinas Dynamo
            //PushButtonData buttonDataBim1 = new PushButtonData("Rotinas", "Rotinas \nDynamo", assemblyPath, "Solutia.Commands.Command");
            //PushButton pushButtonBim1 = ribbonPanelBim.AddItem(buttonDataBim1) as PushButton;
            //pushButtonBim1.ToolTip = "Executa as rotinas de Dynamo";
            // Adicionando imagem ao botão
            //Uri uriBim1 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonBIML.ico"));
            //BitmapImage bitmapBim1 = new BitmapImage(uriBim1);
            //pushButtonBim1.LargeImage = bitmapBim1;

            // Botão da Biblioteca de Famílias
            //PushButtonData buttonDataBim2 = new PushButtonData("Biblioteca", "Biblioteca \nde Famílias", assemblyPath, "Solutia.Commands.Lib");
            //PushButton pushButtonBim2 = ribbonPanelBim.AddItem(buttonDataBim2) as PushButton;
            //pushButtonBim2.ToolTip = "Abre a biblioteca de famílias de todas as disciplinas";
            // Adicionando imagem ao botão
            //Uri uriBim2 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonCloudL.ico"));
            //BitmapImage bitmapBim2 = new BitmapImage(uriBim2);
            //pushButtonBim2.LargeImage = bitmapBim2;


            // Botões de Configurações
            // Conjunto Furo Viga
            //PulldownButton pulldownFuroViga = ribbonPanelGer.AddItem(new PulldownButtonData("Furo", "Furo Viga")) as PulldownButton;
            //Uri uriFuroViga = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonBIML.ico"));
            //BitmapImage bitmapFuroViga = new BitmapImage(uriFuroViga);
            //pulldownFuroViga.LargeImage = bitmapFuroViga;

            // Botão Furo HID
            //PushButton buttonDataFuroHid = pulldownFuroViga.AddPushButton(new PushButtonData("FuroHID", "Furo HID", assemblyPath, "Solutia.Commands.FuroHID"));
            // Adicionando imagem ao botão
            //Uri uriFuroHid = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonHidL.ico"));
            //BitmapImage bitmapFuroHid = new BitmapImage(uriFuroHid);
            //buttonDataFuroHid.LargeImage = bitmapFuroHid;

            // Botão Furo ELE
            //PushButton buttonDataFuroEle = pulldownFuroViga.AddPushButton(new PushButtonData("FuroELE", "Furo ELE", assemblyPath, "Solutia.Commands.FuroELE"));
            // Adicionando imagem ao botão
            //Uri uriFuroEle = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonEleL.ico"));
            //BitmapImage bitmapFuroEle = new BitmapImage(uriFuroEle);
            //buttonDataFuroEle.LargeImage = bitmapFuroEle;

            // Botão Furo CLI
            //PushButton buttonDataFuroCli = pulldownFuroViga.AddPushButton(new PushButtonData("FuroCLI", "Furo CLI", assemblyPath, "Solutia.Commands.FuroCLI"));
            // Adicionando imagem ao botão
            //Uri uriFuroCli = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonEleL.ico"));
            //BitmapImage bitmapFuroCli = new BitmapImage(uriFuroCli);
            //buttonDataFuroCli.LargeImage = bitmapFuroCli;

            // Botão AutoSave
            //PushButtonData buttonDataGer1 = new PushButtonData("AutoSave", "AutoSave", assemblyPath, "Solutia.Commands.AutoSave");
            //PushButton pushButtonGer1 = ribbonPanelGer.AddItem(buttonDataGer1) as PushButton;
            //pushButtonGer1.ToolTip = "AutoSave";
            // Adicionando imagem ao botão
            //Uri uriGer1 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonSaveL.ico"));
            //BitmapImage bitmapGer1 = new BitmapImage(uriGer1);
            //pushButtonGer1.LargeImage = bitmapGer1;

            //ribbonPanelGer.AddSeparator();
            // Botão Configurações
            //PushButtonData buttonDataGer2 = new PushButtonData("Config", "Configurações", assemblyPath, "Solutia.Commands.Command");
            //PushButton pushButtonGer2 = ribbonPanelGer.AddItem(buttonDataGer2) as PushButton;
            //pushButtonGer2.ToolTip = "Abre as configurações de usuário";
            // Adicionando imagem ao botão
            //Uri uriGer2 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonConfigL.ico"));
            //BitmapImage bitmapGer2 = new BitmapImage(uriGer2);
            //pushButtonGer2.LargeImage = bitmapGer2;

            //ribbonPanelGer.AddSeparator();
            // Stacked Buttons
            //PushButtonData buttonRed = new PushButtonData("buttonRed", "Botão Vermelho", assemblyPath, "Solutia.Commands.Command");
            //PushButtonData buttonBlue = new PushButtonData("buttonBlue", "Botão Azul", assemblyPath, "Solutia.Commands.Command");
            //PushButtonData buttonGreen = new PushButtonData("buttonGreen", "Botão Verde", assemblyPath, "Solutia.Commands.Command");
            //ribbonPanelGer.AddStackedItems(buttonRed, buttonBlue, buttonGreen);



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }

    }

}