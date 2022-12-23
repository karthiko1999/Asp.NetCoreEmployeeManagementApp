namespace EmployeeManagementApp.Security
{
    // Inorder to provide purpose  strings for IDataProtectionProvider so the we get IDataProtector and do encryption and decryption
    // This purpose string we use it as EncryptionKey and this will combine with Master root key of IDataProtectionProvider to generate unique key used for Encrypt and Decrypt 
    public class DataProtectionPurposeStrings
    {
        // This is the pupose string EmployeeIdRouteValues which used to encrypt and decrypt the EmployeeId values thast we used in Route
        public readonly string EmployeeIdRouteValues = "EmployeeIdRouteValues";
    } 
}