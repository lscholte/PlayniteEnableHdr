# HDR Manager
An extension to automatically detect games with HDR support and enable HDR for those games. It is strongly recommended to use this extension with the [PCGamingWiki Metadata Provider](https://github.com/sharkusmanch/playnite-pcgamingwiki-metadata-provider) extension, which is capable of determining which games support HDR by adding an "HDR" feature to those games.

## Deploying a Release
1. Update the extension version in [extension.yaml](https://github.com/lscholte/PlayniteHdrManager/blob/main/extension.yaml) and push to GitHub.
1. Push a new version tag (e.g. `v1.2`) to run the [Generate Extension Release](https://github.com/lscholte/PlayniteHdrManager/actions/workflows/generate-release.yaml) action, which will generate a draft release in the [GitHub releases](https://github.com/lscholte/PlayniteHdrManager/releases).
1. Update the extension release [manifest.yaml](https://github.com/lscholte/PlayniteHdrManager/blob/main/manifest.yaml) with a new version, including the `PackageUrl` that references the `.pext` file from the draft release. Push the changes to GitHub.
1. Modify the draft release description in GitHub to contain relevant release notes.
1. Publish the relase by un-drafting it.
