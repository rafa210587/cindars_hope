using System.Collections.Generic;
using CindarsHope.City.Services;

namespace CindarsHope.City.Validation
{
    public class CityServiceValidationIssue
    {
        public string ServiceId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class CityServiceValidator
    {
        public List<CityServiceValidationIssue> Validate(CityServiceDefinition service)
        {
            var issues = new List<CityServiceValidationIssue>();
            if (service == null) { issues.Add(new CityServiceValidationIssue { Code = "SERVICE_NULL", Message = "Service is null", IsBlocker = true }); return issues; }

            if (string.IsNullOrEmpty(service.ServiceId))
                issues.Add(new CityServiceValidationIssue { ServiceId = service.ServiceId, Code = "SERVICE_NO_ID", Message = "Service has no ServiceId", IsBlocker = true });

            // Anya temple/altar is explicitly blocked
            if (service.ServiceId != null && (service.ServiceId.Contains("anya_temple") || service.ServiceId.Contains("altar_anya")))
                issues.Add(new CityServiceValidationIssue { ServiceId = service.ServiceId, Code = "SERVICE_ANYA_TEMPLE", Message = "Anya city temple/altar is not an active urban service per canon", IsBlocker = true });

            // Shop service must have ShopInventoryId
            if ((service.ServiceType == CityServiceType.ShopGeneral ||
                 service.ServiceType == CityServiceType.ShopSeeds ||
                 service.ServiceType == CityServiceType.ShopNight) &&
                string.IsNullOrEmpty(service.ShopInventoryId))
                issues.Add(new CityServiceValidationIssue { ServiceId = service.ServiceId, Code = "SHOP_NO_INVENTORY", Message = $"Shop service '{service.ServiceId}' has no ShopInventoryId", IsBlocker = false });

            // Night shop must have condition
            if (service.IsNightShop && string.IsNullOrEmpty(service.NightShopConditionFlag) &&
                service.ServiceType != CityServiceType.ShopNight)
                issues.Add(new CityServiceValidationIssue { ServiceId = service.ServiceId, Code = "NIGHT_SHOP_NO_CONDITION", Message = $"Night shop '{service.ServiceId}' has no condition flag", IsBlocker = false });

            // License service must have LicenseDefinitionId
            if (service.ServiceType == CityServiceType.TownHallLicense && string.IsNullOrEmpty(service.LicenseDefinitionId))
                issues.Add(new CityServiceValidationIssue { ServiceId = service.ServiceId, Code = "LICENSE_SERVICE_NO_DEF", Message = $"License service '{service.ServiceId}' has no LicenseDefinitionId", IsBlocker = false });

            return issues;
        }

        public List<CityServiceValidationIssue> ValidateLicense(LicenseDefinition license)
        {
            var issues = new List<CityServiceValidationIssue>();
            if (license == null) { issues.Add(new CityServiceValidationIssue { Code = "LICENSE_NULL", Message = "License is null", IsBlocker = true }); return issues; }
            if (string.IsNullOrEmpty(license.GrantedFlag))
                issues.Add(new CityServiceValidationIssue { ServiceId = license.LicenseId, Code = "LICENSE_NO_GRANT_FLAG", Message = $"License '{license.LicenseId}' has no GrantedFlag", IsBlocker = true });
            if (string.IsNullOrEmpty(license.GrantedPermission))
                issues.Add(new CityServiceValidationIssue { ServiceId = license.LicenseId, Code = "LICENSE_NO_PERMISSION", Message = $"License '{license.LicenseId}' has no GrantedPermission", IsBlocker = false });
            return issues;
        }
    }
}
