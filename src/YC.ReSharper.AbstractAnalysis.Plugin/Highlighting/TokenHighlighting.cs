﻿using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Psi.Tree;
using YC.ReSharper.AbstractAnalysis.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(TokenHighlighting.SEVERITY_ID, null, HighlightingGroupIds.LanguageUsage, TokenHighlighting.SEVERITY_ID, TokenHighlighting.SEVERITY_ID, 
    Severity.INFO, false, Internal = false)]
namespace YC.ReSharper.AbstractAnalysis.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SEVERITY_ID, LANGUAGE_NAME, OverlapResolve = OverlapResolveKind.NONE, ToolTipFormatString = null)]
    internal class TokenHighlighting : ICustomAttributeIdHighlighting, IHighlightingWithRange
    {
        //public const string HIGHLIGHTING_ID = "YC.SEL.SDK Highlighting";
        public const string SEVERITY_ID = "YC.SEL.SDK Highlighting";
        public const string LANGUAGE_NAME = "Embedded language";

        private readonly string attributeId;
        private readonly ITreeNode myElement;

        public TokenHighlighting(ITreeNode element)
        {
            myElement = element;
            string lang = element.UserData.GetData(Constants.YcLanguage);
            string tokenName = element.UserData.GetData(Constants.YcTokenName);
            attributeId = LanguageHelper.GetColor(lang, tokenName);
        }

        #region ICustomAttributeIdHighlighting Members

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return null; }
        }

        public string ErrorStripeToolTip
        {
            get { return null; }
        }

        public int NavigationOffsetPatch
        {
            get { return 0; }
        }

        public string AttributeId
        {
            get { return attributeId; }
        }

        #endregion

        #region IHighlightingWithRange Members

        public DocumentRange CalculateRange()
        {
            return myElement.GetNavigationRange();
        }
        
        #endregion
    }
}