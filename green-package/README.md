# Windows 绿色包（v2rayN 7.25.6 fork 版，含“不显示 -1”等改动）

GitHub 单个文件不能超过 100MB，所以把绿色包切成了 7 段，下载后拼回一个 zip 再解压。

## 下载

下载本目录里的 7 个文件：

- `v2rayN-windows-64-ci.zip.01` 到 `v2rayN-windows-64-ci.zip.07`（共 7 个文件）

## 合并（Windows）

把 7 个文件放在同一个文件夹里，在该文件夹按住 Shift 点右键 → “在此处打开 PowerShell 窗口”，执行：

```powershell
cmd /c copy /b v2rayN-windows-64-ci.zip.01+v2rayN-windows-64-ci.zip.02+v2rayN-windows-64-ci.zip.03+v2rayN-windows-64-ci.zip.04+v2rayN-windows-64-ci.zip.05+v2rayN-windows-64-ci.zip.06+v2rayN-windows-64-ci.zip.07 v2rayN-windows-64-ci.zip
```

合并后得到 `v2rayN-windows-64-ci.zip`（约 168MB），解压就能用，解压后双击 `v2rayN.exe`。

## 校验

合并后的 zip 大小应为 168,554,332 字节。
