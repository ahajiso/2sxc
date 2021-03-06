﻿using System;
using System.Reflection;
using ToSic.Eav.Apps;

namespace ToSic.SexyContent.Engines
{
    internal class EngineFactory
    {

        public enum EngineType
        {
            Token = 0,
            Razor = 1
        }

        public static IEngine CreateEngine(Template template)
        {
            Type engineType = null;

            switch (template.IsRazor)
            {
                case true:
                    // Load the Razor Engine via Reflection
                    var engineAssembly = Assembly.Load("ToSic.SexyContent.Razor");
                    engineType = engineAssembly.GetType("ToSic.SexyContent.Engines.RazorEngine");
                    break;
                case false:
                    // Load Token Engine
                    engineType = typeof(TokenEngine.TokenEngine);
                    break;
            }


            if (engineType == null)
                throw new Exception("Error: Could not find the template engine to parse this template.");

            return (IEngine)Activator.CreateInstance(engineType, null);
        }

    }
}