﻿using System.Collections.Generic;
using JetBrains.Application;
using ReSharperExtension;

namespace YC.ReSharper.AbstractAnalysis.Plugin
{
    [ShellComponent]
    public class LanguageLoader
    {
        public LanguageLoader(IEnumerable<IReSharperLanguage> providers)
        {
            //At this point all languages must be found and loaded"
        }
    }
}
