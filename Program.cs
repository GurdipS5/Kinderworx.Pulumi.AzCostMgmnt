using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Collections.Generic;
using Pulumi.AzureNative.CostManagement;

return await Pulumi.Deployment.RunAsync(() =>
{
    //
    string budgetName = "";
    BudgetArgs budgetArgs = new BudgetArgs();

    budgetArgs.BudgetName = "";
    budgetArgs.Amount = "";
    // Export the primary key of the Storage Account
    return new Dictionary<string, object?>
    {
        ["primaryStorageKey"] = primaryStorageKey
    };
});