// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.CoreThresholdProcessor
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.ExpressionEvaluator;
using SolarWinds.Orion.Core.Common.ExpressionEvaluator.Functions;
using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.Thresholds;
using SolarWinds.Orion.Core.Models;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  [Export(typeof (IThresholdDataProcessor))]
  public class CoreThresholdProcessor : ExprEvaluationEngine, IThresholdDataProcessor
  {
    private const string MeanVariableName = "mean";
    private const string StdDevVariableName = "std_dev";
    private const string MinVariableName = "min";
    private const string MaxVariableName = "max";
    private Dictionary<string, Variable> _variables;
    private readonly IFunctionsDefinition _functions;
    private readonly CoreThresholdPreProcessor _preProcessor;

    public CoreThresholdProcessor()
    {
      base.\u002Ector();
      this.set_VariableConvertor(new Func<string, Variable>(this.ConvertVariable));
    }

    protected virtual IEnumerable<string> AllowedVariables
    {
      get
      {
        return (IEnumerable<string>) this._variables.Keys;
      }
    }

    protected virtual IFunctionsDefinition Functions
    {
      get
      {
        return this._functions;
      }
    }

    public ValidationResult IsFormulaValid(
      string formula,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (this.get_Log().get_IsDebugEnabled())
        this.get_Log().DebugFormat("Validating formula: {0} ...", (object) formula);
      ValidationResult validationResult;
      try
      {
        validationResult = this._preProcessor.PreValidateFormula(formula, level, thresholdOperator);
        if (validationResult.get_IsValid())
        {
          formula = this._preProcessor.PreProcessFormula(formula, level, thresholdOperator);
          this._variables = CoreThresholdProcessor.CreateVariables(CoreThresholdProcessor.CreateDefaultBaselineValues());
          this.TryParse(formula, true);
          validationResult = ValidationResult.CreateValid();
        }
      }
      catch (InvalidInputException ex)
      {
        validationResult = !ex.get_HasError() ? new ValidationResult(false, ((Exception) ex).Message) : new ValidationResult(false, (IEnumerable<string>) ((IEnumerable<ExprEvalErrorDescription>) ex.get_Errors()).Select<ExprEvalErrorDescription, string>((Func<ExprEvalErrorDescription, string>) (er => CoreThresholdProcessor.GetErrorMessage(er))).ToArray<string>());
      }
      catch (Exception ex)
      {
        this.get_Log().Error((object) string.Format("Unexpected error when validating formula: {0} ", (object) formula), ex);
        validationResult = new ValidationResult(false, ex.Message);
      }
      return validationResult;
    }

    public double ComputeThreshold(
      string formula,
      BaselineValues baselineValues,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (this.get_Log().get_IsVerboseEnabled())
        this.get_Log().VerboseFormat("Computing formula: {0}, values: [{1}]", new object[2]
        {
          (object) formula,
          (object) baselineValues
        });
      if (string.IsNullOrEmpty(formula))
        return 0.0;
      try
      {
        formula = this._preProcessor.PreProcessFormula(formula, level, thresholdOperator);
        this._variables = CoreThresholdProcessor.CreateVariables(baselineValues);
        return this.EvaluateDynamic(formula, (IDictionary<string, Variable>) this._variables, (object) null);
      }
      catch (InvalidInputException ex)
      {
        string message = !ex.get_HasError() ? ((Exception) ex).Message : string.Join(" ", ((IEnumerable<ExprEvalErrorDescription>) ex.get_Errors()).Select<ExprEvalErrorDescription, string>(new Func<ExprEvalErrorDescription, string>(CoreThresholdProcessor.GetErrorMessage)).ToArray<string>());
        if (this.get_Log().get_IsInfoEnabled())
          this.get_Log().Info((object) string.Format("Parsing error: {0} when evaluating formula: {1}, values: {2}", (object) message, (object) formula, (object) baselineValues), (Exception) ex);
        throw new Exception(message, (Exception) ex);
      }
      catch (Exception ex)
      {
        this.get_Log().Error((object) string.Format("Unexpected error when evaluating formula: {0}, values: {1}", (object) formula, (object) baselineValues), ex);
        throw;
      }
    }

    public virtual bool IsBaselineValuesValid(BaselineValues baselineValues)
    {
      if (baselineValues == null)
        throw new ArgumentNullException(nameof (baselineValues));
      return baselineValues.get_Mean().HasValue && baselineValues.get_StdDev().HasValue && (baselineValues.get_Max().HasValue && baselineValues.get_Min().HasValue) && (baselineValues.get_MinDateTime().HasValue && baselineValues.get_MaxDateTime().HasValue) && baselineValues.get_Timestamp().HasValue;
    }

    private Variable ConvertVariable(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new Exception("Variable name can't be null or empty.");
      string lowerInvariant = name.ToLowerInvariant();
      if (this._variables.ContainsKey(lowerInvariant))
        return this._variables[lowerInvariant];
      throw new Exception(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Can't convert variable {0}.", (object) CoreThresholdProcessor.FormatVariable(name)));
    }

    private static Dictionary<string, Variable> CreateVariables(
      BaselineValues baselineValues)
    {
      Dictionary<string, Variable> dictionary = new Dictionary<string, Variable>();
      Variable variable1 = new Variable();
      variable1.set_Type(typeof (double));
      variable1.set_Name("mean");
      variable1.set_Value((object) baselineValues.get_Mean());
      dictionary.Add("mean", variable1);
      Variable variable2 = new Variable();
      variable2.set_Type(typeof (double));
      variable2.set_Name("std_dev");
      variable2.set_Value((object) baselineValues.get_StdDev());
      dictionary.Add("std_dev", variable2);
      Variable variable3 = new Variable();
      variable3.set_Type(typeof (double));
      variable3.set_Name("min");
      variable3.set_Value((object) baselineValues.get_Min());
      dictionary.Add("min", variable3);
      Variable variable4 = new Variable();
      variable4.set_Type(typeof (double));
      variable4.set_Name("max");
      variable4.set_Value((object) baselineValues.get_Max());
      dictionary.Add("max", variable4);
      return dictionary;
    }

    private static BaselineValues CreateDefaultBaselineValues()
    {
      BaselineValues baselineValues = new BaselineValues();
      baselineValues.set_Count(1);
      baselineValues.set_Max(new double?(1.0));
      baselineValues.set_Mean(new double?(1.0));
      baselineValues.set_Min(new double?(1.0));
      baselineValues.set_StdDev(new double?(1.0));
      return baselineValues;
    }

    private static string GetErrorMessage(ExprEvalErrorDescription err)
    {
      switch ((int) err.get_Type())
      {
        case 0:
          return err.get_Message();
        case 1:
        case 2:
          return string.IsNullOrEmpty(err.get_InvalidText()) ? string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources.get_LIBCODE_ZT0_12(), (object) err.get_CharPosition()) : string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources.get_LIBCODE_ZT0_13(), (object) err.get_InvalidText(), (object) err.get_CharPosition());
        case 3:
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources.get_LIBCODE_ZT0_14(), (object) CoreThresholdProcessor.FormatVariable(err.get_InvalidText()));
        case 4:
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources.get_LIBCODE_ZT0_15(), (object) err.get_InvalidText());
        case 5:
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources.get_LIBCODE_ZT0_16(), (object) err.get_InvalidText());
        default:
          return string.Empty;
      }
    }

    private static string FormatVariable(string name)
    {
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "${0}{1}{2}", (object) "{", (object) name, (object) "}");
    }
  }
}
