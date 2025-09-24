
# To learn more about how to use Nix to configure your environment
# see: https://developers.google.com/idx/guides/customize-idx-env
{ pkgs, ... }: {
  # Which nixpkgs channel to use.
  channel = "stable-24.11"; # or "unstable"
  # Use https://search.nixos.org/packages to find packages
  packages = [ pkgs.dotnet-sdk_9 pkgs.nodejs_20 pkgs.docker-compose pkgs.dotnet-ef pkgs.docker pkgs.openssl pkgs.curl pkgs.gh pkgs.natscli pkgs.postgresql];
  # Sets environment variables in the workspace
  env = { };

  # Enable the Docker daemon
  services.docker.enable = true;

  idx = {
    # Search for the extensions you want on https://open-vsx.org/ and use "publisher.id"
    extensions = [ "muhammad-sammy.csharp" "rangav.vscode-thunder-client"  "ms-dotnettools.vscode-dotnet-runtime" "PKief.material-icon-theme" "peterj.proto"];
  };
}
