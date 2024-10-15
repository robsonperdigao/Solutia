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
            string panelNameCli = "Climatização";
            RibbonPanel ribbonPanelCli = app.CreateRibbonPanel(tabName, panelNameCli);

            //Cria painel Geral
            string panelNameGer = "Geral";
            RibbonPanel ribbonPanelGer = app.CreateRibbonPanel(tabName, panelNameGer);

            //Cria painel BIM
            string panelNameBim = "BIM";
            RibbonPanel ribbonPanelBim = app.CreateRibbonPanel(tabName, panelNameBim);

            //Cria botões
            string assemblyPath = Assembly.GetExecutingAssembly().Location;


            // Botões de Elétrica
            // Botão ConduitCAD
            PushButtonData buttonDataEle1 = new PushButtonData("ConduitCAD", "ConduitCAD", assemblyPath, "Solutia.Commands.ConduitCAD");
            PushButton pushButtonEle1 = ribbonPanelEle.AddItem(buttonDataEle1) as PushButton;
            pushButtonEle1.ToolTip = "Cria eletrodutos/conduítes horizontais a partir de um desenho CAD";
            // Adicionando imagem ao botão
            Uri uriEle1 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonConduitL.ico"));
            BitmapImage bitmapEle1 = new BitmapImage(uriEle1);
            pushButtonEle1.LargeImage = bitmapEle1;

            // Botão Furo ELE
            PushButtonData buttonDataEle2 = new PushButtonData("FuroELE", "Furo ELE", assemblyPath, "Solutia.Commands.FuroTeste");
            PushButton pushButtonEle2 = ribbonPanelEle.AddItem(buttonDataEle2) as PushButton;
            pushButtonEle2.ToolTip = "Insere família de furo onde há interferências entre eletrodutos/conduítes e vigas";
            // Adicionando imagem ao botão
            Uri uriEle2 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttomFuroL.ico"));
            BitmapImage bitmapEle2 = new BitmapImage(uriEle2);
            pushButtonEle2.LargeImage = bitmapEle2;

            // Botão Auto Prum ELE
            PushButtonData buttonDataEle3 = new PushButtonData("AutoPrumELE", "AutoPrum", assemblyPath, "Solutia.Commands.AutoPrumELE");
            PushButton pushButtonEle3 = ribbonPanelEle.AddItem(buttonDataEle3) as PushButton;
            pushButtonEle3.ToolTip = "Cria prumada";
            // Adicionando imagem ao botão
            Uri uriEle3 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonEleL.ico"));
            BitmapImage bitmapEle3 = new BitmapImage(uriEle3);
            pushButtonEle3.LargeImage = bitmapEle3;

            // Botão Fiac
            PushButtonData buttonDataEle4 = new PushButtonData("Fiac", "Fiac", assemblyPath, "Solutia.Commands.Fiac");
            PushButton pushButtonEle4 = ribbonPanelEle.AddItem(buttonDataEle4) as PushButton;
            pushButtonEle4.ToolTip = "Cria uma tabela com as informações dos painéis, circuitos e lança a fiação nos eletrodutos";
            // Adicionando imagem ao botão
            Uri uriEle4 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonFiacL.ico"));
            BitmapImage bitmapEle4 = new BitmapImage(uriEle4);
            pushButtonEle4.LargeImage = bitmapEle4;

            // Botão Coleta Dados FIac
            PushButtonData buttonDataEle5 = new PushButtonData("Coleta Dados Fiac", "Coleta Fiac", assemblyPath, "Solutia.Commands.ColetaDadosFiac");
            PushButton pushButtonEle5 = ribbonPanelEle.AddItem(buttonDataEle5) as PushButton;
            pushButtonEle5.ToolTip = "Cria uma tabela com as informações dos painéis, circuitos e lança a fiação nos eletrodutos";
            // Adicionando imagem ao botão
            Uri uriEle5 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonFiacL.ico"));
            BitmapImage bitmapEle5 = new BitmapImage(uriEle5);
            pushButtonEle5.LargeImage = bitmapEle5;


            // Botões de Hidráulica
            // Botão TubCAD
            PushButtonData buttonDataHid1 = new PushButtonData("TubCAD", "TubCAD", assemblyPath, "Solutia.Commands.HIDTub");
            PushButton pushButtonHid1 = ribbonPanelHid.AddItem(buttonDataHid1) as PushButton;
            pushButtonHid1.ToolTip = "Cria tubulações horizontais a partir de um desenho CAD";
            // Adicionando imagem ao botão
            Uri uriHid1 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonTubL.ico"));
            BitmapImage bitmapHid1 = new BitmapImage(uriHid1);
            pushButtonHid1.LargeImage = bitmapHid1;

            // Botão Furo HID
            PushButtonData buttonDataHid2 = new PushButtonData("FuroHID", "Furo HID", assemblyPath, "Solutia.Commands.Furo");
            PushButton pushButtonHid2 = ribbonPanelHid.AddItem(buttonDataHid2) as PushButton;
            pushButtonHid2.ToolTip = "Insere família de furo onde há interferências entre tubulações e vigas";
            // Adicionando imagem ao botão
            Uri uriHid2 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttomFuroL.ico"));
            BitmapImage bitmapHid2 = new BitmapImage(uriHid2);
            pushButtonHid2.LargeImage = bitmapHid2;

            // Botão Criar Conexões HID
            PushButtonData buttonDataHid3 = new PushButtonData("ConnectTubHID", "Criar Conexões", assemblyPath, "Solutia.Commands.ConnectTubHID");
            PushButton pushButtonHid3 = ribbonPanelHid.AddItem(buttonDataHid3) as PushButton;
            pushButtonHid3.ToolTip = "Cria conexões";
            // Adicionando imagem ao botão
            Uri uriHid3 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonHidL.ico"));
            BitmapImage bitmapHid3 = new BitmapImage(uriHid3);
            pushButtonHid3.LargeImage = bitmapHid3;

            // Botão Tê HID
            PushButtonData buttonDataHid4 = new PushButtonData("Tê", "Criar Tê", assemblyPath, "Solutia.Commands.TeTub");
            PushButton pushButtonHid4 = ribbonPanelHid.AddItem(buttonDataHid4) as PushButton;
            pushButtonHid4.ToolTip = "Tê";
            // Adicionando imagem ao botão
            Uri uriHid4 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonHidL.ico"));
            BitmapImage bitmapHid4 = new BitmapImage(uriHid4);
            pushButtonHid4.LargeImage = bitmapHid4;

            // Botão Intersec
            PushButtonData buttonDataHid5 = new PushButtonData("Interseções", "Interseções", assemblyPath, "Solutia.Commands.Intersec");
            PushButton pushButtonHid5 = ribbonPanelHid.AddItem(buttonDataHid5) as PushButton;
            pushButtonHid5.ToolTip = "Intersec";
            // Adicionando imagem ao botão
            Uri uriHid5 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonHidL.ico"));
            BitmapImage bitmapHid5 = new BitmapImage(uriHid5);
            pushButtonHid5.LargeImage = bitmapHid5;




            // Botões de BIM

            // Botão AutoFuro
            PushButtonData buttonDataBim1 = new PushButtonData("Furação", "Furação", assemblyPath, "Solutia.Commands.AutoFuro");
            PushButton pushButtonBim1 = ribbonPanelBim.AddItem(buttonDataBim1) as PushButton;
            pushButtonBim1.ToolTip = "Analisa interferências com elementos estruturais e insere a furação";
            // Adicionando imagem ao botão
            Uri uriBim1 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttomFuroL.ico"));
            BitmapImage bitmapBim1 = new BitmapImage(uriBim1);
            pushButtonBim1.LargeImage = bitmapBim1;

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

            ribbonPanelGer.AddSeparator();
            // Botão Configurações
            //PushButtonData buttonDataGer2 = new PushButtonData("Config", "Configurações", assemblyPath, "Solutia.Commands.Command");
            //PushButton pushButtonGer2 = ribbonPanelGer.AddItem(buttonDataGer2) as PushButton;
            //pushButtonGer2.ToolTip = "Abre as configurações de usuário";
            // Adicionando imagem ao botão
            //Uri uriGer2 = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "buttonConfigL.ico"));
            //BitmapImage bitmapGer2 = new BitmapImage(uriGer2);
            //pushButtonGer2.LargeImage = bitmapGer2;

            ribbonPanelGer.AddSeparator();
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