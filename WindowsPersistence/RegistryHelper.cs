using Microsoft.Win32;

namespace RegistryHelper{
    public class RegistryHelper{
    public static void SetRegistryKey(string keyPath, string keyName, object keyValue, RegistryHive hive = RegistryHive.CurrentUser, RegistryView view = RegistryView.Default)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
            using (var subKey = baseKey.OpenSubKey(keyPath, writable: true) ?? baseKey.CreateSubKey(keyPath, writable: true))
            {
                if (subKey == null)
                {
                    throw new System.Exception("Failed to open or create registry key.");
                }

                subKey.SetValue(keyName, keyValue);
            }
        }
    
    public static bool DoesRegistryKeyMatch(string keyPath, string keyName, object expectedValue, RegistryHive hive = RegistryHive.CurrentUser, RegistryView view = RegistryView.Default)
    {
        using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
        using (var subKey = baseKey.OpenSubKey(keyPath))
        {
            if (subKey == null)
            {
                return false; // Key does not exist
            }

            var actualValue = subKey.GetValue(keyName);

            // Check if the actual value matches the expected value
            return actualValue != null && actualValue.Equals(expectedValue);
        }
    }
}
}

