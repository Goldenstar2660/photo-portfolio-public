using PhotoGallery.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace PhotoGallery.Permissions;

public class PhotoGalleryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(PhotoGalleryPermissions.GroupName);

        var albumsPermission = myGroup.AddPermission(PhotoGalleryPermissions.Albums.Default, L("Permission:Albums"));
        albumsPermission.AddChild(PhotoGalleryPermissions.Albums.Create, L("Permission:Albums.Create"));
        albumsPermission.AddChild(PhotoGalleryPermissions.Albums.Edit, L("Permission:Albums.Edit"));
        albumsPermission.AddChild(PhotoGalleryPermissions.Albums.Delete, L("Permission:Albums.Delete"));

        var photosPermission = myGroup.AddPermission(PhotoGalleryPermissions.Photos.Default, L("Permission:Photos"));
        photosPermission.AddChild(PhotoGalleryPermissions.Photos.Create, L("Permission:Photos.Create"));
        photosPermission.AddChild(PhotoGalleryPermissions.Photos.Edit, L("Permission:Photos.Edit"));
        photosPermission.AddChild(PhotoGalleryPermissions.Photos.Delete, L("Permission:Photos.Delete"));

        //Define your own permissions here. Example:
        //myGroup.AddPermission(PhotoGalleryPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PhotoGalleryResource>(name);
    }
}
