namespace PhotoGallery.Permissions;

public static class PhotoGalleryPermissions
{
    public const string GroupName = "PhotoGallery";

    public static class Albums
    {
        public const string Default = GroupName + ".Albums";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Photos
    {
        public const string Default = GroupName + ".Photos";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    
    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
