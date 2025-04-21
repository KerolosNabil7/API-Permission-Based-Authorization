using ApiPermissionBasedAuthorization.Constants;

namespace ApiPermissionBasedAuthorization.Constants
{
    public static class Permissions
    {
        public static List<string> GeneratePermissionsList(string Module)
        {
            return new List<string>
            {
                $"Permissions.{Module}.View",
                $"Permissions.{Module}.Create",
                $"Permissions.{Module}.Edit",
                $"Permissions.{Module}.Delete"
            };
        }

        //To generate the permissions for all Modules
        public static List<string> GenerateAllPermissions()
        {
            var allPermissions = new List<string>();

            var modules = Enum.GetValues(typeof(Modules));

            foreach(var module in modules)
            {
                var modulePermissions = GeneratePermissionsList(module.ToString());
                allPermissions.AddRange(modulePermissions);
            }

            return allPermissions;
        }

        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Edit = "Permissions.Products.Edit";
            public const string Delete = "Permissions.Products.Delete";
        }
    }
}
