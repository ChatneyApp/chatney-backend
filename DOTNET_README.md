1. Install dotnet sdk 9.x
1. Install dotnet runtime 9.x
1. Run in the root:

```shell
dotnet restore
```

This will restore all NuGet packages.

1. Install dependencies: 
```shell
dotnet restore
```
2. Run project
```shell
dotnet run
```
3. Run project in watch mode
```shell
dotnet watch
```

# Ubuntu:

```shell
sudo snap install dotnet-sdk-90 --classic
```

add this to your `.bashrc`:

```
export DOTNET_ROOT=/snap/dotnet-sdk-90/current
```

add the symlink in case you can't execute `dotnet` command:

```shell
sudo ln -sf $DOTNET_ROOT/dotnet /usr/local/bin/dotnet
```
