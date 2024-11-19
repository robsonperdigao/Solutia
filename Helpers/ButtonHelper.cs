using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;

namespace Solutia.Helpers
{
    public static class ButtonHelper
    {
        public static PushButton CreateButtonWithIcon(RibbonPanel panel, string buttonName, string buttonText, string tooltip, string commandClassName, string iconName)
        {
            // Cria o botão
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, assemblyPath, commandClassName);
            PushButton pushButton = panel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = tooltip;

            // Adiciona a imagem ao botão
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream iconStream = assembly.GetManifestResourceStream(iconName);
            BitmapImage iconImage = new BitmapImage();
            iconImage.BeginInit();
            iconImage.StreamSource = iconStream;
            iconImage.EndInit();
            pushButton.LargeImage = iconImage;

            return pushButton;
        }
    }
}
