using System;
using UnityEngine;

// #define EXTEND_TO_ERROR_AND_WARNING 
namespace HightLightDebug
{
    public static class DebugPro
    {
        public static void Aqua(object message) => Debug.Log("<color=Aqua>" + message + "</color>");
        public static void Black(object message) => Debug.Log("<color=Black>" + message + "</color>");
        public static void Blue(object message) => Debug.Log("<color=Blue>" + message + "</color>");
        public static void Brown(object message) => Debug.Log("<color=Brown>" + message + "</color>");
        public static void Cyan(object message) => Debug.Log("<color=Cyan>" + message + "</color>");
        public static void DarkBlue(object message) => Debug.Log("<color=DarkBlue>" + message + "</color>");
        public static void Fuchsia(object message) => Debug.Log("<color=Fuchsia>" + message + "</color>");
        public static void Green(object message) => Debug.Log("<color=Green>" + message + "</color>");
        public static void Grey(object message) => Debug.Log("<color=Grey>" + message + "</color>");
        public static void LightBlue(object message) => Debug.Log("<color=LightBlue>" + message + "</color>");
        public static void Lime(object message) => Debug.Log("<color=Lime>" + message + "</color>");
        public static void Magenta(object message) => Debug.Log("<color=Magenta>" + message + "</color>");
        public static void Maroon(object message) => Debug.Log("<color=Maroon>" + message + "</color>");
        public static void Navy(object message) => Debug.Log("<color=Navy>" + message + "</color>");
        public static void Olive(object message) => Debug.Log("<color=Olive>" + message + "</color>");
        public static void Orange(object message) => Debug.Log("<color=Orange>" + message + "</color>");
        public static void Purple(object message) => Debug.Log("<color=Purple>" + message + "</color>");
        public static void Red(object message) => Debug.Log("<color=red>" + message + "</color>");
        public static void Silver(object message) => Debug.Log("<color=Silver>" + message + "</color>");
        public static void Teal(object message) => Debug.Log("<color=Teal>" + message + "</color>");
        public static void White(object message) => Debug.Log("<color=White>" + message + "</color>");
        public static void Yellow(object message) => Debug.Log("<color=Yellow>" + message + "</color>");
        public static void Pink(object message) => Debug.Log("<color=Magenta>" + message + "</color>");

        public static void Italic(object message) => Debug.Log("<i>" + message + "</i>");
        public static void Bold(object message) => Debug.Log("<b>" + message + "</b>");
        public static void BoldItalic(object message) => Debug.Log("<b><i>" + message + "</i></b>");

        public static void AquaBold(object message) => Debug.Log("<b><color=Aqua>" + message + "</color></b>");
        public static void BlackBold(object message) => Debug.Log("<b><color=Black>" + message + "</color></b>");
        public static void BlueBold(object message) => Debug.Log("<b><color=Blue>" + message + "</color></b>");
        public static void BrownBold(object message) => Debug.Log("<b><color=Brown>" + message + "</color></b>");
        public static void CyanBold(object message) => Debug.Log("<b><color=Cyan>" + message + "</color></b>");
        public static void DarkBlueBold(object message) => Debug.Log("<b><color=DarkBlue>" + message + "</color></b>");
        public static void FuchsiaBold(object message) => Debug.Log("<b><color=Fuchsia>" + message + "</color></b>");
        public static void GreenBold(object message) => Debug.Log("<b><color=Green>" + message + "</color></b>");
        public static void GreyBold(object message) => Debug.Log("<b><color=Grey>" + message + "</color></b>");
        public static void LightBlueBold(object message) => Debug.Log("<b><color=LightBlue>" + message + "</color></b>");
        public static void LimeBold(object message) => Debug.Log("<b><color=Lime>" + message + "</color></b>");
        public static void MagentaBold(object message) => Debug.Log("<b><color=Magenta>" + message + "</color></b>");
        public static void MaroonBold(object message) => Debug.Log("<b><color=Maroon>" + message + "</color></b>");
        public static void NavyBold(object message) => Debug.Log("<b><color=Navy>" + message + "</color></b>");
        public static void OliveBold(object message) => Debug.Log("<b><color=Olive>" + message + "</color></b>");
        public static void OrangeBold(object message) => Debug.Log("<b><color=Orange>" + message + "</color></b>");
        public static void PurpleBold(object message) => Debug.Log("<b><color=Purple>" + message + "</color></b>");
        public static void RedBold(object message) => Debug.Log("<b><color=red>" + message + "</color></b>");
        public static void SilverBold(object message) => Debug.Log("<b><color=Silver>" + message + "</color></b>");
        public static void TealBold(object message) => Debug.Log("<b><color=Teal>" + message + "</color></b>");
        public static void WhiteBold(object message) => Debug.Log("<b><color=White>" + message + "</color></b>");
        public static void YellowBold(object message) => Debug.Log("<b><color=Yellow>" + message + "</color></b>");
        public static void PinkBold(object message) => Debug.Log("<b><color=Magenta>" + message + "</color></b>");

        public static void AquaItalic(object message) => Debug.Log("<i><color=Aqua>" + message + "</color></i>");
        public static void BlackItalic(object message) => Debug.Log("<i><color=Black>" + message + "</color></i>");
        public static void BlueItalic(object message) => Debug.Log("<i><color=Blue>" + message + "</color></i>");
        public static void BrownItalic(object message) => Debug.Log("<i><color=Brown>" + message + "</color></i>");
        public static void CyanItalic(object message) => Debug.Log("<i><color=Cyan>" + message + "</color></i>");
        public static void DarkBlueItalic(object message) => Debug.Log("<i><color=DarkBlue>" + message + "</color></i>");
        public static void FuchsiaItalic(object message) => Debug.Log("<i><color=Fuchsia>" + message + "</color></i>");
        public static void GreenItalic(object message) => Debug.Log("<i><color=Green>" + message + "</color></i>");
        public static void GreyItalic(object message) => Debug.Log("<i><color=Grey>" + message + "</color></i>");
        public static void LightBlueItalic(object message) => Debug.Log("<i><color=LightBlue>" + message + "</color></i>");
        public static void LimeItalic(object message) => Debug.Log("<i><color=Lime>" + message + "</color></i>");
        public static void MagentaItalic(object message) => Debug.Log("<i><color=Magenta>" + message + "</color></i>");
        public static void MaroonItalic(object message) => Debug.Log("<i><color=Maroon>" + message + "</color></i>");
        public static void NavyItalic(object message) => Debug.Log("<i><color=Navy>" + message + "</color></i>");
        public static void OliveItalic(object message) => Debug.Log("<i><color=Olive>" + message + "</color></i>");
        public static void OrangeItalic(object message) => Debug.Log("<i><color=Orange>" + message + "</color></i>");
        public static void PurpleItalic(object message) => Debug.Log("<i><color=Purple>" + message + "</color></i>");
        public static void RedItalic(object message) => Debug.Log("<i><color=red>" + message + "</color></i>");
        public static void SilverItalic(object message) => Debug.Log("<i><color=Silver>" + message + "</color></i>");
        public static void TealItalic(object message) => Debug.Log("<i><color=Teal>" + message + "</color></i>");
        public static void WhiteItalic(object message) => Debug.Log("<i><color=White>" + message + "</color></i>");
        public static void YellowItalic(object message) => Debug.Log("<i><color=Yellow>" + message + "</color></i>");
        public static void PinkItalic(object message) => Debug.Log("<i><color=Magenta>" + message + "</color></i>");

        public static void AquaBoldItalic(object message) => Debug.Log("<b><i><color=Aqua>" + message + "</color></i></b>");
        public static void BlackBoldItalic(object message) => Debug.Log("<b><i><color=Black>" + message + "</color></i></b>");
        public static void BlueBoldItalic(object message) => Debug.Log("<b><i><color=Blue>" + message + "</color></i></b>");
        public static void BrownBoldItalic(object message) => Debug.Log("<b><i><color=Brown>" + message + "</color></i></b>");
        public static void CyanBoldItalic(object message) => Debug.Log("<b><i><color=Cyan>" + message + "</color></i></b>");
        public static void DarkBlueBoldItalic(object message) => Debug.Log("<b><i><color=DarkBlue>" + message + "</color></i></b>");
        public static void FuchsiaBoldItalic(object message) => Debug.Log("<b><i><color=Fuchsia>" + message + "</color></i></b>");
        public static void GreenBoldItalic(object message) => Debug.Log("<b><i><color=Green>" + message + "</color></i></b>");
        public static void GreyBoldItalic(object message) => Debug.Log("<b><i><color=Grey>" + message + "</color></i></b>");
        public static void LightBlueBoldItalic(object message) => Debug.Log("<b><i><color=LightBlue>" + message + "</color></i></b>");
        public static void LimeBoldItalic(object message) => Debug.Log("<b><i><color=Lime>" + message + "</color></i></b>");
        public static void MagentaBoldItalic(object message) => Debug.Log("<b><i><color=Magenta>" + message + "</color></i></b>");
        public static void MaroonBoldItalic(object message) => Debug.Log("<b><i><color=Maroon>" + message + "</color></i></b>");
        public static void NavyBoldItalic(object message) => Debug.Log("<b><i><color=Navy>" + message + "</color></i></b>");
        public static void OliveBoldItalic(object message) => Debug.Log("<b><i><color=Olive>" + message + "</color></i></b>");
        public static void OrangeBoldItalic(object message) => Debug.Log("<b><i><color=Orange>" + message + "</color></i></b>");
        public static void PurpleBoldItalic(object message) => Debug.Log("<b><i><color=Purple>" + message + "</color></i></b>");
        public static void RedBoldItalic(object message) => Debug.Log("<b><i><color=red>" + message + "</color></i></b>");
        public static void SilverBoldItalic(object message) => Debug.Log("<b><i><color=Silver>" + message + "</color></i></b>");
        public static void TealBoldItalic(object message) => Debug.Log("<b><i><color=Teal>" + message + "</color></i></b>");
        public static void WhiteBoldItalic(object message) => Debug.Log("<b><i><color=White>" + message + "</color></i></b>");
        public static void YellowBoldItalic(object message) => Debug.Log("<b><i><color=Yellow>" + message + "</color></i></b>");
        public static void PinkBoldItalic(object message) => Debug.Log("<b><i><color=Magenta>" + message + "</color></i></b>");

        public static void ErrorAqua(object message) => Debug.LogError("<color=Aqua>" + message + "</color>");
        public static void ErrorBlack(object message) => Debug.LogError("<color=Black>" + message + "</color>");
        public static void ErrorBlue(object message) => Debug.LogError("<color=Blue>" + message + "</color>");
        public static void ErrorBrown(object message) => Debug.LogError("<color=Brown>" + message + "</color>");
        public static void ErrorCyan(object message) => Debug.LogError("<color=Cyan>" + message + "</color>");
        public static void ErrorDarkBlue(object message) => Debug.LogError("<color=DarkBlue>" + message + "</color>");
        public static void ErrorFuchsia(object message) => Debug.LogError("<color=Fuchsia>" + message + "</color>");
        public static void ErrorGreen(object message) => Debug.LogError("<color=Green>" + message + "</color>");
        public static void ErrorGrey(object message) => Debug.LogError("<color=Grey>" + message + "</color>");
        public static void ErrorLightBlue(object message) => Debug.LogError("<color=LightBlue>" + message + "</color>");
        public static void ErrorLime(object message) => Debug.LogError("<color=Lime>" + message + "</color>");
        public static void ErrorMagenta(object message) => Debug.LogError("<color=Magenta>" + message + "</color>");
        public static void ErrorMaroon(object message) => Debug.LogError("<color=Maroon>" + message + "</color>");
        public static void ErrorNavy(object message) => Debug.LogError("<color=Navy>" + message + "</color>");
        public static void ErrorOlive(object message) => Debug.LogError("<color=Olive>" + message + "</color>");
        public static void ErrorOrange(object message) => Debug.LogError("<color=Orange>" + message + "</color>");
        public static void ErrorPurple(object message) => Debug.LogError("<color=Purple>" + message + "</color>");
        public static void ErrorRed(object message) => Debug.LogError("<color=red>" + message + "</color>");
        public static void ErrorSilver(object message) => Debug.LogError("<color=Silver>" + message + "</color>");
        public static void ErrorTeal(object message) => Debug.LogError("<color=Teal>" + message + "</color>");
        public static void ErrorWhite(object message) => Debug.LogError("<color=White>" + message + "</color>");
        public static void ErrorYellow(object message) => Debug.LogError("<color=Yellow>" + message + "</color>");
        public static void ErrorPink(object message) => Debug.LogError("<color=Magenta>" + message + "</color>");

        public static void ErrorItalic(object message) => Debug.LogError("<i>" + message + "</i>");
        public static void ErrorBold(object message) => Debug.LogError("<b>" + message + "</b>");
        public static void ErrorBoldItalic(object message) => Debug.LogError("<b><i>" + message + "</i></b>");

        public static void ErrorAquaBold(object message) => Debug.LogError("<b><color=Aqua>" + message + "</color></b>");
        public static void ErrorBlackBold(object message) => Debug.LogError("<b><color=Black>" + message + "</color></b>");
        public static void ErrorBlueBold(object message) => Debug.LogError("<b><color=Blue>" + message + "</color></b>");
        public static void ErrorBrownBold(object message) => Debug.LogError("<b><color=Brown>" + message + "</color></b>");
        public static void ErrorCyanBold(object message) => Debug.LogError("<b><color=Cyan>" + message + "</color></b>");
        public static void ErrorDarkBlueBold(object message) => Debug.LogError("<b><color=DarkBlue>" + message + "</color></b>");
        public static void ErrorFuchsiaBold(object message) => Debug.LogError("<b><color=Fuchsia>" + message + "</color></b>");
        public static void ErrorGreenBold(object message) => Debug.LogError("<b><color=Green>" + message + "</color></b>");
        public static void ErrorGreyBold(object message) => Debug.LogError("<b><color=Grey>" + message + "</color></b>");
        public static void ErrorLightBlueBold(object message) => Debug.LogError("<b><color=LightBlue>" + message + "</color></b>");
        public static void ErrorLimeBold(object message) => Debug.LogError("<b><color=Lime>" + message + "</color></b>");
        public static void ErrorMagentaBold(object message) => Debug.LogError("<b><color=Magenta>" + message + "</color></b>");
        public static void ErrorMaroonBold(object message) => Debug.LogError("<b><color=Maroon>" + message + "</color></b>");
        public static void ErrorNavyBold(object message) => Debug.LogError("<b><color=Navy>" + message + "</color></b>");
        public static void ErrorOliveBold(object message) => Debug.LogError("<b><color=Olive>" + message + "</color></b>");
        public static void ErrorOrangeBold(object message) => Debug.LogError("<b><color=Orange>" + message + "</color></b>");
        public static void ErrorPurpleBold(object message) => Debug.LogError("<b><color=Purple>" + message + "</color></b>");
        public static void ErrorRedBold(object message) => Debug.LogError("<b><color=red>" + message + "</color></b>");
        public static void ErrorSilverBold(object message) => Debug.LogError("<b><color=Silver>" + message + "</color></b>");
        public static void ErrorTealBold(object message) => Debug.LogError("<b><color=Teal>" + message + "</color></b>");
        public static void ErrorWhiteBold(object message) => Debug.LogError("<b><color=White>" + message + "</color></b>");
        public static void ErrorYellowBold(object message) => Debug.LogError("<b><color=Yellow>" + message + "</color></b>");
        public static void ErrorPinkBold(object message) => Debug.LogError("<b><color=Magenta>" + message + "</color></b>");

        public static void ErrorAquaItalic(object message) => Debug.LogError("<i><color=Aqua>" + message + "</color></i>");
        public static void ErrorBlackItalic(object message) => Debug.LogError("<i><color=Black>" + message + "</color></i>");
        public static void ErrorBlueItalic(object message) => Debug.LogError("<i><color=Blue>" + message + "</color></i>");
        public static void ErrorBrownItalic(object message) => Debug.LogError("<i><color=Brown>" + message + "</color></i>");
        public static void ErrorCyanItalic(object message) => Debug.LogError("<i><color=Cyan>" + message + "</color></i>");
        public static void ErrorDarkBlueItalic(object message) => Debug.LogError("<i><color=DarkBlue>" + message + "</color></i>");
        public static void ErrorFuchsiaItalic(object message) => Debug.LogError("<i><color=Fuchsia>" + message + "</color></i>");
        public static void ErrorGreenItalic(object message) => Debug.LogError("<i><color=Green>" + message + "</color></i>");
        public static void ErrorGreyItalic(object message) => Debug.LogError("<i><color=Grey>" + message + "</color></i>");
        public static void ErrorLightBlueItalic(object message) => Debug.LogError("<i><color=LightBlue>" + message + "</color></i>");
        public static void ErrorLimeItalic(object message) => Debug.LogError("<i><color=Lime>" + message + "</color></i>");
        public static void ErrorMagentaItalic(object message) => Debug.LogError("<i><color=Magenta>" + message + "</color></i>");
        public static void ErrorMaroonItalic(object message) => Debug.LogError("<i><color=Maroon>" + message + "</color></i>");
        public static void ErrorNavyItalic(object message) => Debug.LogError("<i><color=Navy>" + message + "</color></i>");
        public static void ErrorOliveItalic(object message) => Debug.LogError("<i><color=Olive>" + message + "</color></i>");
        public static void ErrorOrangeItalic(object message) => Debug.LogError("<i><color=Orange>" + message + "</color></i>");
        public static void ErrorPurpleItalic(object message) => Debug.LogError("<i><color=Purple>" + message + "</color></i>");
        public static void ErrorRedItalic(object message) => Debug.LogError("<i><color=red>" + message + "</color></i>");
        public static void ErrorSilverItalic(object message) => Debug.LogError("<i><color=Silver>" + message + "</color></i>");
        public static void ErrorTealItalic(object message) => Debug.LogError("<i><color=Teal>" + message + "</color></i>");
        public static void ErrorWhiteItalic(object message) => Debug.LogError("<i><color=White>" + message + "</color></i>");
        public static void ErrorYellowItalic(object message) => Debug.LogError("<i><color=Yellow>" + message + "</color></i>");
        public static void ErrorPinkItalic(object message) => Debug.LogError("<i><color=Magenta>" + message + "</color></i>");

        public static void ErrorAquaBoldItalic(object message) => Debug.LogError("<b><i><color=Aqua>" + message + "</color></i></b>");
        public static void ErrorBlackBoldItalic(object message) => Debug.LogError("<b><i><color=Black>" + message + "</color></i></b>");
        public static void ErrorBlueBoldItalic(object message) => Debug.LogError("<b><i><color=Blue>" + message + "</color></i></b>");
        public static void ErrorBrownBoldItalic(object message) => Debug.LogError("<b><i><color=Brown>" + message + "</color></i></b>");
        public static void ErrorCyanBoldItalic(object message) => Debug.LogError("<b><i><color=Cyan>" + message + "</color></i></b>");
        public static void ErrorDarkBlueBoldItalic(object message) => Debug.LogError("<b><i><color=DarkBlue>" + message + "</color></i></b>");
        public static void ErrorFuchsiaBoldItalic(object message) => Debug.LogError("<b><i><color=Fuchsia>" + message + "</color></i></b>");
        public static void ErrorGreenBoldItalic(object message) => Debug.LogError("<b><i><color=Green>" + message + "</color></i></b>");
        public static void ErrorGreyBoldItalic(object message) => Debug.LogError("<b><i><color=Grey>" + message + "</color></i></b>");
        public static void ErrorLightBlueBoldItalic(object message) => Debug.LogError("<b><i><color=LightBlue>" + message + "</color></i></b>");
        public static void ErrorLimeBoldItalic(object message) => Debug.LogError("<b><i><color=Lime>" + message + "</color></i></b>");
        public static void ErrorMagentaBoldItalic(object message) => Debug.LogError("<b><i><color=Magenta>" + message + "</color></i></b>");
        public static void ErrorMaroonBoldItalic(object message) => Debug.LogError("<b><i><color=Maroon>" + message + "</color></i></b>");
        public static void ErrorNavyBoldItalic(object message) => Debug.LogError("<b><i><color=Navy>" + message + "</color></i></b>");
        public static void ErrorOliveBoldItalic(object message) => Debug.LogError("<b><i><color=Olive>" + message + "</color></i></b>");
        public static void ErrorOrangeBoldItalic(object message) => Debug.LogError("<b><i><color=Orange>" + message + "</color></i></b>");
        public static void ErrorPurpleBoldItalic(object message) => Debug.LogError("<b><i><color=Purple>" + message + "</color></i></b>");
        public static void ErrorRedBoldItalic(object message) => Debug.LogError("<b><i><color=red>" + message + "</color></i></b>");
        public static void ErrorSilverBoldItalic(object message) => Debug.LogError("<b><i><color=Silver>" + message + "</color></i></b>");
        public static void ErrorTealBoldItalic(object message) => Debug.LogError("<b><i><color=Teal>" + message + "</color></i></b>");
        public static void ErrorWhiteBoldItalic(object message) => Debug.LogError("<b><i><color=White>" + message + "</color></i></b>");
        public static void ErrorYellowBoldItalic(object message) => Debug.LogError("<b><i><color=Yellow>" + message + "</color></i></b>");
        public static void ErrorPinkBoldItalic(object message) => Debug.LogError("<b><i><color=Magenta>" + message + "</color></i></b>");
        
        public static void WarningAqua(object message) => Debug.LogWarning("<color=Aqua>" + message + "</color>");
        public static void WarningBlack(object message) => Debug.LogWarning("<color=Black>" + message + "</color>");
        public static void WarningBlue(object message) => Debug.LogWarning("<color=Blue>" + message + "</color>");
        public static void WarningBrown(object message) => Debug.LogWarning("<color=Brown>" + message + "</color>");
        public static void WarningCyan(object message) => Debug.LogWarning("<color=Cyan>" + message + "</color>");
        public static void WarningDarkBlue(object message) => Debug.LogWarning("<color=DarkBlue>" + message + "</color>");
        public static void WarningFuchsia(object message) => Debug.LogWarning("<color=Fuchsia>" + message + "</color>");
        public static void WarningGreen(object message) => Debug.LogWarning("<color=Green>" + message + "</color>");
        public static void WarningGrey(object message) => Debug.LogWarning("<color=Grey>" + message + "</color>");
        public static void WarningLightBlue(object message) => Debug.LogWarning("<color=LightBlue>" + message + "</color>");
        public static void WarningLime(object message) => Debug.LogWarning("<color=Lime>" + message + "</color>");
        public static void WarningMagenta(object message) => Debug.LogWarning("<color=Magenta>" + message + "</color>");
        public static void WarningMaroon(object message) => Debug.LogWarning("<color=Maroon>" + message + "</color>");
        public static void WarningNavy(object message) => Debug.LogWarning("<color=Navy>" + message + "</color>");
        public static void WarningOlive(object message) => Debug.LogWarning("<color=Olive>" + message + "</color>");
        public static void WarningOrange(object message) => Debug.LogWarning("<color=Orange>" + message + "</color>");
        public static void WarningPurple(object message) => Debug.LogWarning("<color=Purple>" + message + "</color>");
        public static void WarningRed(object message) => Debug.LogWarning("<color=red>" + message + "</color>");
        public static void WarningSilver(object message) => Debug.LogWarning("<color=Silver>" + message + "</color>");
        public static void WarningTeal(object message) => Debug.LogWarning("<color=Teal>" + message + "</color>");
        public static void WarningWhite(object message) => Debug.LogWarning("<color=White>" + message + "</color>");
        public static void WarningYellow(object message) => Debug.LogWarning("<color=Yellow>" + message + "</color>");
        public static void WarningPink(object message) => Debug.LogWarning("<color=Magenta>" + message + "</color>");

        public static void WarningItalic(object message) => Debug.LogWarning("<i>" + message + "</i>");
        public static void WarningBold(object message) => Debug.LogWarning("<b>" + message + "</b>");
        public static void WarningBoldItalic(object message) => Debug.LogWarning("<b><i>" + message + "</i></b>");

        public static void WarningAquaBold(object message) => Debug.LogWarning("<b><color=Aqua>" + message + "</color></b>");
        public static void WarningBlackBold(object message) => Debug.LogWarning("<b><color=Black>" + message + "</color></b>");
        public static void WarningBlueBold(object message) => Debug.LogWarning("<b><color=Blue>" + message + "</color></b>");
        public static void WarningBrownBold(object message) => Debug.LogWarning("<b><color=Brown>" + message + "</color></b>");
        public static void WarningCyanBold(object message) => Debug.LogWarning("<b><color=Cyan>" + message + "</color></b>");
        public static void WarningDarkBlueBold(object message) => Debug.LogWarning("<b><color=DarkBlue>" + message + "</color></b>");
        public static void WarningFuchsiaBold(object message) => Debug.LogWarning("<b><color=Fuchsia>" + message + "</color></b>");
        public static void WarningGreenBold(object message) => Debug.LogWarning("<b><color=Green>" + message + "</color></b>");
        public static void WarningGreyBold(object message) => Debug.LogWarning("<b><color=Grey>" + message + "</color></b>");
        public static void WarningLightBlueBold(object message) => Debug.LogWarning("<b><color=LightBlue>" + message + "</color></b>");
        public static void WarningLimeBold(object message) => Debug.LogWarning("<b><color=Lime>" + message + "</color></b>");
        public static void WarningMagentaBold(object message) => Debug.LogWarning("<b><color=Magenta>" + message + "</color></b>");
        public static void WarningMaroonBold(object message) => Debug.LogWarning("<b><color=Maroon>" + message + "</color></b>");
        public static void WarningNavyBold(object message) => Debug.LogWarning("<b><color=Navy>" + message + "</color></b>");
        public static void WarningOliveBold(object message) => Debug.LogWarning("<b><color=Olive>" + message + "</color></b>");
        public static void WarningOrangeBold(object message) => Debug.LogWarning("<b><color=Orange>" + message + "</color></b>");
        public static void WarningPurpleBold(object message) => Debug.LogWarning("<b><color=Purple>" + message + "</color></b>");
        public static void WarningRedBold(object message) => Debug.LogWarning("<b><color=red>" + message + "</color></b>");
        public static void WarningSilverBold(object message) => Debug.LogWarning("<b><color=Silver>" + message + "</color></b>");
        public static void WarningTealBold(object message) => Debug.LogWarning("<b><color=Teal>" + message + "</color></b>");
        public static void WarningWhiteBold(object message) => Debug.LogWarning("<b><color=White>" + message + "</color></b>");
        public static void WarningYellowBold(object message) => Debug.LogWarning("<b><color=Yellow>" + message + "</color></b>");
        public static void WarningPinkBold(object message) => Debug.LogWarning("<b><color=Magenta>" + message + "</color></b>");

        public static void WarningAquaItalic(object message) => Debug.LogWarning("<i><color=Aqua>" + message + "</color></i>");
        public static void WarningBlackItalic(object message) => Debug.LogWarning("<i><color=Black>" + message + "</color></i>");
        public static void WarningBlueItalic(object message) => Debug.LogWarning("<i><color=Blue>" + message + "</color></i>");
        public static void WarningBrownItalic(object message) => Debug.LogWarning("<i><color=Brown>" + message + "</color></i>");
        public static void WarningCyanItalic(object message) => Debug.LogWarning("<i><color=Cyan>" + message + "</color></i>");
        public static void WarningDarkBlueItalic(object message) => Debug.LogWarning("<i><color=DarkBlue>" + message + "</color></i>");
        public static void WarningFuchsiaItalic(object message) => Debug.LogWarning("<i><color=Fuchsia>" + message + "</color></i>");
        public static void WarningGreenItalic(object message) => Debug.LogWarning("<i><color=Green>" + message + "</color></i>");
        public static void WarningGreyItalic(object message) => Debug.LogWarning("<i><color=Grey>" + message + "</color></i>");
        public static void WarningLightBlueItalic(object message) => Debug.LogWarning("<i><color=LightBlue>" + message + "</color></i>");
        public static void WarningLimeItalic(object message) => Debug.LogWarning("<i><color=Lime>" + message + "</color></i>");
        public static void WarningMagentaItalic(object message) => Debug.LogWarning("<i><color=Magenta>" + message + "</color></i>");
        public static void WarningMaroonItalic(object message) => Debug.LogWarning("<i><color=Maroon>" + message + "</color></i>");
        public static void WarningNavyItalic(object message) => Debug.LogWarning("<i><color=Navy>" + message + "</color></i>");
        public static void WarningOliveItalic(object message) => Debug.LogWarning("<i><color=Olive>" + message + "</color></i>");
        public static void WarningOrangeItalic(object message) => Debug.LogWarning("<i><color=Orange>" + message + "</color></i>");
        public static void WarningPurpleItalic(object message) => Debug.LogWarning("<i><color=Purple>" + message + "</color></i>");
        public static void WarningRedItalic(object message) => Debug.LogWarning("<i><color=red>" + message + "</color></i>");
        public static void WarningSilverItalic(object message) => Debug.LogWarning("<i><color=Silver>" + message + "</color></i>");
        public static void WarningTealItalic(object message) => Debug.LogWarning("<i><color=Teal>" + message + "</color></i>");
        public static void WarningWhiteItalic(object message) => Debug.LogWarning("<i><color=White>" + message + "</color></i>");
        public static void WarningYellowItalic(object message) => Debug.LogWarning("<i><color=Yellow>" + message + "</color></i>");
        public static void WarningPinkItalic(object message) => Debug.LogWarning("<i><color=Magenta>" + message + "</color></i>");

        public static void WarningAquaBoldItalic(object message) => Debug.LogWarning("<b><i><color=Aqua>" + message + "</color></i></b>");
        public static void WarningBlackBoldItalic(object message) => Debug.LogWarning("<b><i><color=Black>" + message + "</color></i></b>");
        public static void WarningBlueBoldItalic(object message) => Debug.LogWarning("<b><i><color=Blue>" + message + "</color></i></b>");
        public static void WarningBrownBoldItalic(object message) => Debug.LogWarning("<b><i><color=Brown>" + message + "</color></i></b>");
        public static void WarningCyanBoldItalic(object message) => Debug.LogWarning("<b><i><color=Cyan>" + message + "</color></i></b>");
        public static void WarningDarkBlueBoldItalic(object message) => Debug.LogWarning("<b><i><color=DarkBlue>" + message + "</color></i></b>");
        public static void WarningFuchsiaBoldItalic(object message) => Debug.LogWarning("<b><i><color=Fuchsia>" + message + "</color></i></b>");
        public static void WarningGreenBoldItalic(object message) => Debug.LogWarning("<b><i><color=Green>" + message + "</color></i></b>");
        public static void WarningGreyBoldItalic(object message) => Debug.LogWarning("<b><i><color=Grey>" + message + "</color></i></b>");
        public static void WarningLightBlueBoldItalic(object message) => Debug.LogWarning("<b><i><color=LightBlue>" + message + "</color></i></b>");
        public static void WarningLimeBoldItalic(object message) => Debug.LogWarning("<b><i><color=Lime>" + message + "</color></i></b>");
        public static void WarningMagentaBoldItalic(object message) => Debug.LogWarning("<b><i><color=Magenta>" + message + "</color></i></b>");
        public static void WarningMaroonBoldItalic(object message) => Debug.LogWarning("<b><i><color=Maroon>" + message + "</color></i></b>");
        public static void WarningNavyBoldItalic(object message) => Debug.LogWarning("<b><i><color=Navy>" + message + "</color></i></b>");
        public static void WarningOliveBoldItalic(object message) => Debug.LogWarning("<b><i><color=Olive>" + message + "</color></i></b>");
        public static void WarningOrangeBoldItalic(object message) => Debug.LogWarning("<b><i><color=Orange>" + message + "</color></i></b>");
        public static void WarningPurpleBoldItalic(object message) => Debug.LogWarning("<b><i><color=Purple>" + message + "</color></i></b>");
        public static void WarningRedBoldItalic(object message) => Debug.LogWarning("<b><i><color=red>" + message + "</color></i></b>");
        public static void WarningSilverBoldItalic(object message) => Debug.LogWarning("<b><i><color=Silver>" + message + "</color></i></b>");
        public static void WarningTealBoldItalic(object message) => Debug.LogWarning("<b><i><color=Teal>" + message + "</color></i></b>");
        public static void WarningWhiteBoldItalic(object message) => Debug.LogWarning("<b><i><color=White>" + message + "</color></i></b>");
        public static void WarningYellowBoldItalic(object message) => Debug.LogWarning("<b><i><color=Yellow>" + message + "</color></i></b>");
        public static void WarningPinkBoldItalic(object message) => Debug.LogWarning("<b><i><color=Magenta>" + message + "</color></i></b>");

        public static void Log(object message,
            TextColor color = TextColor.silver,
            TextStyle style = TextStyle.none,
            int size = 12)
        {
            string styledMessage = $"<size={size}><color={Enum.GetName(typeof(TextColor), color)}>" + message +
                                   "</color></size>";

            if (style is TextStyle.italic or TextStyle.boldItalic)
                styledMessage = "<i>" + styledMessage + "</i>";
            if (style is TextStyle.bold or TextStyle.boldItalic)
                styledMessage = "<b>" + styledMessage + "</b>";

            Debug.Log(styledMessage);
        }
    }

    public enum TextColor
    {
        aqua,
        black,
        blue,
        brown,
        cyan,
        darkblue,
        fuchsia,
        green,
        grey,
        lightblue,
        lime,
        magent,
        maroon,
        navy,
        olive,
        orange,
        purple,
        red,
        silver,
        teal,
        white,
        yellow
    }

    public enum TextStyle
    {
        bold,
        italic,
        boldItalic,
        none
    }
}