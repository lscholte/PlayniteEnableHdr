function OnApplicationStarted()
{
    $__logger.Info("OnApplicationStarted")
    DetectAndActivateHdr
}

function OnLibraryUpdated()
{
    $__logger.Info("OnLibraryUpdated")
    DetectAndActivateHdr
}

function DetectAndActivateHdr()
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $PlayniteAPI.Database.Games.Where{$_.Features.Name -Contains "HDR"}.ForEach{$_.EnableSystemHdr = $true}
}

function GetMainMenuItems()
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem.Description = "Detect and Activate HDR"
    $menuItem.FunctionName = "DetectAndActivateHdr"
    $menuItem.MenuSection = "@"
    return $menuItem
}
