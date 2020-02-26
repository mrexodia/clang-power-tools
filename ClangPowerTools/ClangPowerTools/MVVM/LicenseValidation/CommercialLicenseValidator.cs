﻿using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  /// <summary>
  /// Contains the logic for the commercial license verification
  /// </summary>
  public class CommercialLicenseValidator : PersonalLicenseValidator
  {
    #region ILicenseValidator Implementation

    /// <summary>
    /// Verify if the user license is active.
    /// </summary>
    /// <returns>True if the user license is active. False otherwise.</returns>
    public override async Task<bool> ValidateAsync()
    {
      try
      {
        if (GetToken(out string token) == false)
          return false;

        KeyValuePair<bool, HttpResponseMessage> httpResponse = await CheckUserAccountAsync(token);

        if (httpResponse.Key == false)
          return false;

        List<LicenseModel> licenses = await httpResponse.Value.Content.ReadAsAsync<List<LicenseModel>>();
        return licenses.Count > 0 ? VerifyLicense(licenses) : false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Verify if at least one of the user licenses is active. 
    /// </summary>
    /// <param name="userTokenHttpResponse">The HTTP server response for user token</param>
    /// <returns>True if at least one of the user licenses is active. False otherwise.</returns>
    private bool VerifyLicense(List<LicenseModel> licenses)
    {
      return licenses.Any(license => CheckExpirationDate(license.expires)) == false;
    }

    /// <summary>
    /// Check if the license expired
    /// </summary>
    /// <param name="expirationDate">The expiration date as string</param>
    /// <returns>True if the license expired. False otherwise.</returns>
    private bool CheckExpirationDate(string expirationDate)
    {
      DateTime.TryParse(expirationDate, out DateTime expirationDateTime);
      return DateTime.Compare(DateTime.Now, expirationDateTime) > 0;
    }

    #endregion

  }
}