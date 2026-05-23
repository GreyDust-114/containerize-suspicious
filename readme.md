**提示：本分支项目完全由AI生成，如果您对AI非常抵触、厌恶、不愿意看到一点AI生的程序，那本项目不适合您（）**

# Containerize Suspicious

本项目致力于将一些Windows软件进行隔离（虚拟化、容器化），以提供更高的安全性，保护敏感数据。

目前采用的主要思路是利用Windows Sandbox沙箱机制。但是Windows Sandbox并不支持数据持久化，因此我们需要利用 Sandbox 的路径映射机制将部分路径映射到Sandbox中以支持持久化。

## 本项目
本项目 Fork 自 [Sansui233/containerize-suspicious](https://github.com/Sansui233/containerize-suspicious)（2026/5/22：原项目最后更新于 11 个月前），并在此基础上做了以下改进：

- **脚本语言迁移**：完全使用 AI（DeepSeek v3 pro，通过 Claude Code 编辑）将原 Ruby 脚本改写为 **C# (.NET)** 版本，无需安装 Ruby 环境即可运行。（因为我不会Ruby（逃））
- **数据目录统一**：所有持久化文件统一收纳到 `.\data\ ` 子目录，不再散落在项目根目录。
- **扩展映射覆盖**：迅雷等应用的映射更加完整（如安装目录、公共文件路径等）。
- **Bug 修复**：修复了原 Ruby 脚本中缺少 `require 'set'` 以及 XML 布尔值大小写不规范等问题。

## 使用方法

首先需要确保你的系统是**Windows10以上**并且是**专业版**（因为需要用到Windows 沙箱）

- 安装 [.NET 8+ SDK](https://dotnet.microsoft.com/download)
- 在当前目录下运行 `dotnet run` 准备环境
- 点击生成的 `Containerize.wsb`，即可打开沙盒，在其中安装可持久化的应用程序。

## 可持久化的应用程序

可持久化的程序目录见 [directory_mappings](./directory_mappings/)。此仓库 fork 时，应用程序数据映射的目录几乎为用户目录。对于安装到管理员权限目录的应用程序，需要自行添加对应的目录映射。

## 拓展适用App

如果需要将适用范围拓展到其他应用程序，需要自己把软件安装需要的目录添加进 [directory_mappings](./directory_mappings/)。

*以下是添加目录映射参考步骤，也是本仓库目前各个应用程序目录映射的获取办法*

1. 创建一个空白沙箱，准备好安装程序和文件修改监视器（可用
[ProcMon](https://learn.microsoft.com/zh-cn/sysinternals/downloads/procmon)/
[FileActivityWatch](https://www.nirsoft.net/utils/file_activity_watch.html)）。
> *Note: ProcMon在沙箱中运行似乎会有性能等导致的bug，容易导致沙箱闪退，建议使用
  FileActivityWatch。*
2. 打开文件修改监视器，运行安装程序，安装完成后运行一次程序。
3. 导出修改的文件列表，并据此确定需要的目录映射（通常包括`AppData/Local`、
  `AppData/Roaming`等目录，不包括各种`Temp`目录）。
4. 创建应用程序的目录映射文件，并放置到directory_mappings目录。

如果你创建了其他应用程序的目录映射，并测试通过可以正常使用，欢迎贡献到本仓库！

## 致谢
最初我原本是想用Sandboxie-plus的，但是在找资料的时候我看到了这个项目，看到这里说其运行qq微信啥的有issue还要付费，于是就研究起了这个项目。我将其源码下载到本地，连接进claude code，尝试让它帮我解读原理。然后用cc将源码改成了C#式的并再上传到GitHub上。
- 本项目的启发来自于 [tencent-sandbox](https://github.com/empty-233/tencent-sandbox)，沙箱主XML的模板也是参考这一项目完成的。
- Fork 自 [Sansui233/containerize-suspicious](https://github.com/Sansui233/containerize-suspicious)，感谢原作者提供宝贵思路。
- Ruby → C# 脚本迁移由 DeepSeek-v3-pro在 Claude Code 中完成。

### Windows权限访问控制

Windows提供了完备的权限控制机制，对于文件系统等资源的访问提供了精细的控制。基于Windows和NTFS
的访问控制策略，我们可以创建一个受限用户，控制其文件系统访问权限，再利用runas运行应用程序以实现
访问控制。可惜，腾讯QQ等程序上来就要求提权，因此此方式对其无效。

注：可能有方式可以模拟UAC提权或绕过UAC提权，但是我没有进行进一步的探索，欢迎补充。

### 容器化

Docker等容器化机制显然是理想的选择。可惜，Docker等容器不支持Windows GUI程序的容器化。

### Sandboxie-Plus

Sandboxie提供的数据保护型沙箱机制可以提供良好的保护，而且安装、配置运行均较为方便，是较理想的
解决方案。但是该方案目前存在两个问题：
1. Sandboxie在运行腾讯QQ和微信时会报错，尽管似乎并没有影响程序正常功能，但是可能存在没有测试
   到的不兼容性；
2. Sandboxie实质上是付费软件，其免费模式提供的沙箱并不能实现数据的隔离，需要通过Supporter
   Certificate（实质上就是订阅机制）来获取增强沙箱，从而实现隔离。

*注：尽管Sandboxie是开源软件，但是由于其涉及驱动程序代码，需要取得Windows认证的企业开发者数字
证书方可部署，该证书价格不菲，因此阻碍了普通用户通过自行编译获得免费版本。当然，可以选用测试证书
在本地安装部署，但是对于非专业用户的要求较高，且可能导致额外的风险。欢迎具有企业开发者资质的朋友
编译并分享二进制！*

### Comodo虚拟桌面

Comodo安全软件提供的虚拟桌面功能可以实现对应用程序的隔离运行，同时开箱即用，操作方便。但是我在
前期使用过程中注意到其资源开销较大，会拖慢系统运行速度。

### 虚拟机

虚拟机方案的最大问题在于空间占用。我尝试过各种精简版，以及自己精简的Win10、Win11，但是其磁盘空
间占用均在6GB以上，对于配置紧张的用户还是相当可观的开销。同时虚拟机中运行需要首先启动虚拟机，再
运行程序，启动开销较高。使用Win7等更早期的操作系统或许是可行的解决方案，但是由于过于老旧，在未
来可能会逐渐不兼容大部分应用程序。

### 安全软件的HIPS功能

火绒等安全软件提供了高度可配置的安全防护规则，是一种比较可行的解决方案。其缺点在于规则过于复杂，
可能对普通用户不太友好。
