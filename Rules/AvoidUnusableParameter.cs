// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUnusableParameter: Check if a parameter has a unusable name.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUnusableParameter : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if a parameter has a unusable name.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            IEnumerable<ParameterAst> paramList = null;          

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                if (funcAst.Body != null && funcAst.Body.ParamBlock != null
                    && funcAst.Body.ParamBlock.Attributes != null && funcAst.Body.ParamBlock.Parameters != null)
                {
                    paramList.Concat(funcAst.Body.ParamBlock.Parameters);

                }
                if (funcAst.Parameters != null)
                {
                    paramList.Concat(funcAst.Parameters);
                }
            }

            foreach (var paramAst in paramList)
            {
                bool unusableName = false;
                string paramName = paramAst.Name.VariablePath.UserPath;

                if (string.IsNullOrEmpty(paramName))
                {
                    unusableName = true;
                }
                else if (char.IsDigit(paramName[0]))
                {
                    unusableName = true;
                }
                else if (paramName.StartsWith("|"))
                {
                    unusableName = true;
                }

                if (unusableName) 
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUnusableParameterError, paramName),
                    paramAst.Name.Extent, GetName(), DiagnosticSeverity.Warning, fileName, paramName);
                }

            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUnusableParameterName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUnusableParameterCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUnusableParameterDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
            // TODO: Check this?
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
