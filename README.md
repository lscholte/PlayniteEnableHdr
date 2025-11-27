# HDR Manager
An extension to automatically detect games with HDR support and enable HDR for those games. It is strongly recommended to use this extension with the [PCGamingWiki Metadata Provider](https://github.com/sharkusmanch/playnite-pcgamingwiki-metadata-provider) extension, which is capable of determining which games support HDR by adding an "HDR" feature to those games.

## Deploying a Release
Run the [Generate Extension Release](https://github.com/lscholte/PlayniteHdrManager/actions/workflows/generate-release.yaml) action, which will generate a release in the [GitHub releases](https://github.com/lscholte/PlayniteHdrManager/releases). This will automatically bump the version of [extenion.yaml](https://github.com/lscholte/PlayniteHdrManager/blob/main/extension.yaml) and [manifest.yaml](https://github.com/lscholte/PlayniteHdrManager/blob/main/manifest.yaml).

Note that a version bump will only succeed if there are user-facing changes made (e.g. `feat`/`fix` commits).
