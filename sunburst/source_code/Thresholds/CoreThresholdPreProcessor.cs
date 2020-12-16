// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.CoreThresholdPreProcessor
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Models;
using SolarWinds.Orion.Core.Strings;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class CoreThresholdPreProcessor
  {
    public string PreProcessFormula(
      string formula,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (string.IsNullOrEmpty(formula) || !this.IsUseBaselineMacro(formula))
        return formula;
      if (this.IsMacro(BusinessLayerSettings.Instance.ThresholdsUseBaselineCriticalCalculationMacro, formula))
        level = (ThresholdLevel) 2;
      if (this.IsMacro(BusinessLayerSettings.Instance.ThresholdsUseBaselineWarningCalculationMacro, formula))
        level = (ThresholdLevel) 1;
      return CoreThresholdPreProcessor.IsGreaterOperator(thresholdOperator) ? (level != 2 ? BusinessLayerSettings.Instance.ThresholdsDefaultWarningFormulaForGreater : BusinessLayerSettings.Instance.ThresholdsDefaultCriticalFormulaForGreater) : (level != 2 ? BusinessLayerSettings.Instance.ThresholdsDefaultWarningFormulaForLess : BusinessLayerSettings.Instance.ThresholdsDefaultCriticalFormulaForLess);
    }

    public ValidationResult PreValidateFormula(
      string formula,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (string.IsNullOrEmpty(formula))
        return new ValidationResult(false, string.Format(Resources.get_LIBCODE_PC0_01()));
      if (this.IsUseBaselineMacro(formula))
      {
        if (thresholdOperator == 2 || thresholdOperator == 5)
          return new ValidationResult(false, string.Format(Resources.get_LIBCODE_ZT0_11(), (object) formula));
      }
      else
      {
        if (this.ContainsMacro(BusinessLayerSettings.Instance.ThresholdsUseBaselineCalculationMacro, formula))
          return new ValidationResult(false, string.Format(Resources.get_LIBCODE_ZT0_17(), (object) BusinessLayerSettings.Instance.ThresholdsUseBaselineCalculationMacro));
        if (this.ContainsMacro(BusinessLayerSettings.Instance.ThresholdsUseBaselineWarningCalculationMacro, formula))
          return new ValidationResult(false, string.Format(Resources.get_LIBCODE_ZT0_17(), (object) BusinessLayerSettings.Instance.ThresholdsUseBaselineWarningCalculationMacro));
        if (this.ContainsMacro(BusinessLayerSettings.Instance.ThresholdsUseBaselineCriticalCalculationMacro, formula))
          return new ValidationResult(false, string.Format(Resources.get_LIBCODE_ZT0_17(), (object) BusinessLayerSettings.Instance.ThresholdsUseBaselineCriticalCalculationMacro));
      }
      return ValidationResult.CreateValid();
    }

    private static bool IsGreaterOperator(ThresholdOperatorEnum thresholdOperator)
    {
      return thresholdOperator == null || thresholdOperator == 1;
    }

    private bool IsUseBaselineMacro(string formula)
    {
      string str = formula.Trim();
      return str.Equals(BusinessLayerSettings.Instance.ThresholdsUseBaselineCalculationMacro, StringComparison.InvariantCultureIgnoreCase) || str.Equals(BusinessLayerSettings.Instance.ThresholdsUseBaselineWarningCalculationMacro, StringComparison.InvariantCultureIgnoreCase) || str.Equals(BusinessLayerSettings.Instance.ThresholdsUseBaselineCriticalCalculationMacro, StringComparison.InvariantCultureIgnoreCase);
    }

    private bool IsMacro(string macro, string formula)
    {
      return formula.Trim().ToUpper().Equals(macro, StringComparison.InvariantCultureIgnoreCase);
    }

    private bool ContainsMacro(string macro, string formula)
    {
      return !string.IsNullOrEmpty(macro) && !string.IsNullOrEmpty(formula) && formula.IndexOf(macro, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }
  }
}
